using Domain.Models;
using Domain.Models.Acuerdos.Enums;
using Domain.Models.Carteras;
using Domain.Models.Carteras.Enums;
using Domain.Models.Vehiculos;
using Domain.Responses.Liquidacion.Enums;
using Microsoft.EntityFrameworkCore;
using System;

namespace Infrastructure.Services.Carteras;

/// <summary>
/// Genera los registros de deuda en cartera para un vehículo determinado en un rango de vigencias fiscales.
/// </summary>
/// <param name="vehiculo">Entidad del vehículo a liquidar.</param>
/// <param name="desde">Año inicial de la consulta de vigencia.</param>
/// <param name="hasta">Año final de la consulta de vigencia.</param>
/// <param name="parametro">Configuración global del sistema con valores de contingencia y costos adicionales.</param>
/// <param name="guardarCambios">Indica si aplica el <c>SaveChangesAsync</c> al finalizar la ejecución.</param>
/// <param name="vehiculoEsNuevo">Indica si el registro es nuevo para omitir la eliminación previa de cartera no pagada.</param>
/// <returns>El número de registros creados o afectados en la base de datos.</returns>
/// <remarks>
/// <para><strong>Normativa y Reglas de Negocio (Colombia - Ley 488 de 1998):</strong></para>
/// <list type="bullet">
///   <item>
///     <description><strong>Exención de Motocicletas:</strong> Las motocicletas con cilindraje menor o igual a 125 cc están exentas del impuesto sobre vehículos (Art. 141).</description>
///   </item>
///   <item>
///     <description><strong>Determinación de Tarifa:</strong> El rango de evaluación para la consulta de tarifa se determina siempre a partir del avalúo comercial oficial en pesos colombianos (<c>BaseGravableVehiculo</c>).</description>
///   </item>
///   <item>
///     <description><strong>Cálculo Porcentual:</strong> Si la tarifa obtenida es un valor decimal menor a 1 (ej. <c>0.015</c> para 1.5%), el impuesto de rodamiento se calcula como <c>Avalúo Comercial × Tarifa</c>. Si es mayor o igual a 1, se interpreta como una tarifa plana/fija.</description>
///   </item>
///   <item>
///     <description><strong>Conceptos Adicionales:</strong> Liquida sistematización vehicular, estampillas territoriales (2% sobre rodamiento + carga), conceptos por carga/pasajeros y costos persuasivos/adicionales según parámetros.</description>
///   </item>
///   <item>
///     <description><strong>Restricciones de Modelo y Acuerdos:</strong> Ignora vigencias anteriores al año modelo del vehículo y omite vigencias comprometidas en un acuerdo de pago vigente.</description>
///   </item>
/// </list>
/// </remarks>
public partial class CarteraService
{
    private const int TipoVehiculoIdMotocicleta = 5;

    public async Task<int> GenerarCarteraVehiculo(string placa, int desde, int hasta)
    {
        var vehiculo = await context.Vehiculos
            .Include(v => v.Marca)
            .Include(v => v.Linea)
            .Include(v=>v.TipoVehiculo)
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
        // Fix bug #2: si TipoVehiculo no fue incluido por el caller, la exención de motos
        // y la tarifa por tipo/servicio quedan mal calculadas de forma silenciosa.
        // Es mejor fallar explícito aquí que liquidar impuesto sobre datos incompletos.
        if (vehiculo.TipoVehiculo is null)
        {
            throw new InvalidOperationException(
                $"No se puede generar cartera para el vehículo {vehiculo.Placa}: TipoVehiculo no fue cargado. " +
                "Verifique que el caller incluya .Include(v => v.TipoVehiculo).");
        }

        parametro ??= await context.Parametros.AsNoTracking().FirstOrDefaultAsync();

        var placa = vehiculo.Placa;

        if (!vehiculoEsNuevo)
        {
            await context.Cartera
                .Where(c => c.Placa == placa && !c.IsPagado && c.Vigencia >= desde && c.Vigencia <= hasta)
                .ExecuteDeleteAsync();
        }

        // La cartera no debe liquidarse para vigencias anteriores al modelo del vehículo
        int inicioVigencia = Math.Max(desde, vehiculo.Modelo);

        var nuevasDeudas = new List<Cartera>();

        bool cobraAdicional = parametro?.CobraAdicional == true;
        decimal valorCostas = parametro?.ValorCostasPersuasivo ?? 0m;
        
        // 🚀 Valor base parametrizado para derechos de sistematización vehicular
        decimal valorSistematizacion = parametro?.ValorSistema ?? parametro?.ValorSistema ?? 15_000m;

        var baseGravableVehiculo = await context.BaseGravableVehiculos
            .AsNoTracking()
            .Include(b => b.Vigencias)
            .FirstOrDefaultAsync(b => b.MarcaId == vehiculo.MarcaId && b.LineaId == vehiculo.LineaId);
        
        var vigenciasBloqueadas = await context.Cartera
            .Where(c => c.VehiculoId == vehiculo.Id && 
                        c.AcuerdoPago != null && 
                        c.AcuerdoPago.Estado == EstadoAcuerdoPago.Vigente)
            .Select(c => c.Vigencia)
            .Distinct()
            .ToListAsync();
        
        for (var vigencia = inicioVigencia; vigencia <= hasta; vigencia++)
        {
            if (vigenciasBloqueadas.Contains(vigencia))
            {
                continue;
            }

            decimal valorRodamiento = 0m;

            // Regla Legal: Motocicletas hasta 125 cc exentas de impuesto de rodamiento
            // Fix bug #2: se compara por TipoVehiculoId (estable, según seed) en vez de
            // por nombre, evitando falsos negativos si el nombre en catálogo cambia o difiere.
            bool esMotoExenta = vehiculo.TipoVehiculoId == TipoVehiculoIdMotocicleta && vehiculo.Cilindraje <= 125;

            if (!esMotoExenta)
            {
                // Obtener el avalúo comercial oficial o contingencia
                decimal valorComercial = baseGravableVehiculo?.Vigencias
                    .FirstOrDefault(v => v.Vigencia == vigencia && v.Modelo == vehiculo.Modelo)?.Valor ?? 0m;

                if (valorComercial == 0m)
                {
                    valorComercial = ObtenerValorComercialFallback(vehiculo, vigencia);
                }

                var valorRango = (int)Math.Round(valorComercial, 0, MidpointRounding.AwayFromZero);

                var tarifaPorcentaje = await tarifaService.ObtenerTarifa(
                    TipoConceptoTarifa.Rodamiento,
                    vigencia,
                    vehiculo.TipoVehiculoId,
                    vehiculo.TipoServicio,
                    valorRango);

                if (tarifaPorcentaje > 0)
                {
                    // Cálculo porcentual o valor fijo
                    decimal calculoBruto = tarifaPorcentaje < 1m
                        ? tarifaPorcentaje * valorComercial
                        : tarifaPorcentaje;

                    // Redondeo tributario al mil ($1.000) más cercano
                    valorRodamiento = Math.Round(calculoBruto / 1000m, MidpointRounding.AwayFromZero) * 1000m;
                }

                // Fix bug #1: se elimina el fallback que asignaba ValorSistema (tarifa de
                // Sistematización) como si fuera el impuesto de Rodamiento cuando la tarifa
                // daba 0. Si no hay tarifa configurada, valorRodamiento debe quedar en 0 —
                // cobrar un valor inventado aquí generaba deuda indebida (caso AKT NKD 125).
            }

            // 1. Concepto: Impuesto de Rodamiento
            if (valorRodamiento > 0)
            {
                nuevasDeudas.Add(CrearCartera(vehiculo.Id, placa, vigencia, TipoConceptoCartera.Rodamiento, valorRodamiento, tieneInteres: true));
            }

            // 2. Concepto: Sistematización / Derechos de Trámite
            if (valorSistematizacion > 0)
            {
                nuevasDeudas.Add(CrearCartera(vehiculo.Id, placa, vigencia, TipoConceptoCartera.Sistematizacion, valorSistematizacion, tieneInteres: true));
            }

            // 3. Concepto: Costas / Gastos Persuasivos
            if (cobraAdicional && valorCostas > 0 && vigencia != DateTime.UtcNow.Year)
            {
                nuevasDeudas.Add(CrearCartera(vehiculo.Id, placa, vigencia, TipoConceptoCartera.Costas, valorCostas, tieneInteres: true));
            }

            // 4. Concepto: Tarifa por Capacidad de Carga o Pasajeros
            var valorCargaPasajeros = await liquidacionService.ObtenerValorCargaOPasajero(vehiculo, vigencia);
            if (valorCargaPasajeros > 0)
            {
                nuevasDeudas.Add(CrearCartera(vehiculo.Id, placa, vigencia, TipoConceptoCartera.Carga, valorCargaPasajeros, tieneInteres: true));
            }

            // 5. Concepto: Estampillas Territoriales (2% sobre rodamiento + carga) redondeado al mil más cercano
            decimal estampillasBrutas = (valorRodamiento + valorCargaPasajeros) * 0.02m;
            decimal valorEstampillas = Math.Round(estampillasBrutas / 1000m, MidpointRounding.AwayFromZero) * 1000m;

            if (valorEstampillas > 0)
            {
                nuevasDeudas.Add(CrearCartera(vehiculo.Id, placa, vigencia, TipoConceptoCartera.Estampillas, valorEstampillas, tieneInteres: true));
            }
        }

        context.Cartera.AddRange(nuevasDeudas);

        return guardarCambios ? await context.SaveChangesAsync() : nuevasDeudas.Count;
    }

    private static decimal ObtenerValorComercialFallback(Vehiculo vehiculo, int vigencia)
    {
        // Normalizar nombre del tipo de vehículo para evitar fallos por ID desalineado
        string tipoVehiculo = vehiculo.TipoVehiculo?.Nombre?.ToUpperInvariant() 
                              ?? string.Empty;

        // 1. Manejo para Motocicletas (por nombre de tipo)
        if (tipoVehiculo.Contains("MOTO"))
        {
            if (vehiculo.Cilindraje <= 125) return 0m;
            return vehiculo.Cilindraje <= 250 ? 8_000_000m : 18_000_000m;
        }

        // 2. Estimación base según el nombre de la clase de vehículo
        decimal valorBaseNuevo = tipoVehiculo switch
        {
            var t when t.Contains("AUTOMOVIL") || t.Contains("AUTO") => 55_000_000m,
            var t when t.Contains("CAMIONETA") || t.Contains("CAMPERO") => 85_000_000m,
            var t when t.Contains("TRACTO") || t.Contains("CABEZOTE") => 220_000_000m,
            var t when t.Contains("CAMION") || t.Contains("CARGA") => 140_000_000m,
            var t when t.Contains("BUS") || t.Contains("BUSETA") => 160_000_000m,
            _ => 45_000_000m
        };

        // Depreciación del 8% anual por año de antigüedad respecto a la vigencia cobrada
        int antiguedad = Math.Max(0, vigencia - vehiculo.Modelo);
        decimal factorDepreciacion = (decimal)Math.Pow(0.92, antiguedad);
    
        decimal valorEstimado = valorBaseNuevo * factorDepreciacion;

        // Piso mínimo comercial en Colombia para vehículos particulares
        return Math.Max(valorEstimado, 8_000_000m);
    }
}