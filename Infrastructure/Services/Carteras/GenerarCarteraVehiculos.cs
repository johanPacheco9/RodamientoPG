using Domain.Models;
using Domain.Models.Carteras.Enums;
using Domain.Responses.Liquidacion.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Carteras;

public partial class CarteraService
{
    private const int TipoVehiculoIdMotocicleta = 3;

    public async Task<int> GenerarCarteraVehiculo(string placa, int desde, int hasta)
    {
        var vehiculo = await context.Vehiculos
            .Include(v => v.Marca)
            .Include(v => v.Linea)
            .FirstOrDefaultAsync(v => v.Placa == placa);

        if (vehiculo == null) return 0;

        var parametro = await context.Parametros.AsNoTracking().FirstOrDefaultAsync();

        return await GenerarCarteraVehiculo(vehiculo, desde, hasta, parametro);
    }

    public async Task<int> GenerarCarteraVehiculo(
        Domain.Models.Vehiculos.Vehiculo vehiculo,
        int desde,
        int hasta,
        Parametro? parametro = null,
        bool guardarCambios = true,
        bool vehiculoEsNuevo = false)
    {
        parametro ??= await context.Parametros.AsNoTracking().FirstOrDefaultAsync();

        var placa = vehiculo.Placa;

        if (!vehiculoEsNuevo)
        {
            await context.Cartera
                .Where(c => c.Placa == placa && !c.IsPagado && c.Vigencia >= desde && c.Vigencia <= hasta)
                .ExecuteDeleteAsync();
        }

        // 💡 CORRECCIÓN 1: La cartera no debe liquidarse para vigencias anteriores al modelo del vehículo
        int inicioVigencia = Math.Max(desde, vehiculo.Modelo);
        vehiculo.PagoHasta = inicioVigencia;

        var nuevasDeudas = new List<Cartera>();

        bool cobraAdicional = parametro?.CobraAdicional == true;
        decimal valorCostas = parametro?.ValorCostasPersuasivo ?? 0m;

        var baseGravableVehiculo = await context.BaseGravableVehiculos
            .AsNoTracking()
            .Include(b => b.Vigencias)
            .FirstOrDefaultAsync(b => b.MarcaId == vehiculo.MarcaId && b.LineaId == vehiculo.LineaId);

        for (var vigencia = inicioVigencia; vigencia <= hasta; vigencia++)
        {
            // 💡 CORRECCIÓN 2: Obtener el avalúo comercial o asignar un estimado si no existe en la BD
            decimal valorComercial = baseGravableVehiculo?.Vigencias
                .FirstOrDefault(v => v.Vigencia == vigencia && v.Modelo == vehiculo.Modelo)?.Valor ?? 0m;

            // Si no hay avalúo comercial registrado en la tabla para esa línea/modelo
            if (valorComercial == 0m)
            {
                // Para motos o vehículos sin avalúo oficial, asignamos un valor base por defecto
                // según el cilindraje o un estándar para que la tarifa no se vuelva 0.
                valorComercial = ObtenerValorComercialFallback(vehiculo);
            }

            var valorRango = vehiculo.TipoVehiculoId == TipoVehiculoIdMotocicleta
                ? vehiculo.Cilindraje
                : (int)Math.Round(valorComercial, 0, MidpointRounding.AwayFromZero);

            var tarifaPorcentaje = await tarifaService.ObtenerTarifa(
                TipoConceptoTarifa.Rodamiento,
                vigencia,
                vehiculo.TipoVehiculoId,
                vehiculo.TipoServicioVehiculo,
                valorRango);

            // 💡 CORRECCIÓN 3: Si tarifaPorcentaje es un valor fijo (ej. motos) o un porcentaje (ej. 0.015 para 1.5%)
            decimal valorRodamiento = 0m;
            if (tarifaPorcentaje > 0)
            {
                // Si la tarifa es mayor a 1, asume tarifa fija en pesos; si es decimal (< 1), aplica el porcentaje
                valorRodamiento = tarifaPorcentaje > 1m
                    ? tarifaPorcentaje
                    : Math.Round(tarifaPorcentaje * valorComercial, 0, MidpointRounding.AwayFromZero);
            }

            // Si después del cálculo sigue en 0 (ej. exento o falta tarifa), se puede forzar un valor mínimo de impuesto urbano/rodamiento
            if (valorRodamiento == 0m && parametro != null)
            {
                valorRodamiento = parametro.ValorSistema; // Usa un valor base de contingencia si aplica
            }

            if (valorRodamiento > 0)
            {
                nuevasDeudas.Add(CrearCartera(vehiculo.Id, placa, vigencia, TipoConceptoCartera.Rodamiento, valorRodamiento, tieneInteres: true));
            }

            if (cobraAdicional && valorCostas > 0 && vigencia != DateTime.UtcNow.Year)
            {
                nuevasDeudas.Add(CrearCartera(vehiculo.Id, placa, vigencia, TipoConceptoCartera.Costas, valorCostas, tieneInteres: true));
            }

            var valorCargaPasajeros = await liquidacionService.ObtenerValorCargaOPasajero(vehiculo, vigencia);
            if (valorCargaPasajeros > 0)
            {
                nuevasDeudas.Add(CrearCartera(vehiculo.Id, placa, vigencia, TipoConceptoCartera.Carga, valorCargaPasajeros, tieneInteres: true));
            }

            var valorEstampillas = Math.Round(((valorRodamiento + valorCargaPasajeros) * 2m) / 100m, 0, MidpointRounding.AwayFromZero);
            if (valorEstampillas > 0)
            {
                nuevasDeudas.Add(CrearCartera(vehiculo.Id, placa, vigencia, TipoConceptoCartera.Estampillas, valorEstampillas, tieneInteres: true));
            }
        }

        context.Cartera.AddRange(nuevasDeudas);

        return guardarCambios ? await context.SaveChangesAsync() : nuevasDeudas.Count;
    }

    // Helper para evitar que la liquidación colapse en $0 si la tabla BaseGravableVehiculos está incompleta
    private static decimal ObtenerValorComercialFallback(Domain.Models.Vehiculos.Vehiculo vehiculo)
    {
        if (vehiculo.TipoVehiculoId == TipoVehiculoIdMotocicleta)
        {
            return vehiculo.Cilindraje switch
            {
                <= 125 => 5000000m,
                <= 250 => 12000000m,
                _ => 25000000m
            };
        }

        // Estimado base general para automóviles/camiones si no hay tabla de avalúo cargada
        return 35000000m;
    }
}