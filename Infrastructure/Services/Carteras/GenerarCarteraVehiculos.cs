using Domain.Models;
using Domain.Models.Carteras.Enums;
using Domain.Responses.Liquidacion.Enums;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.Carteras;

public partial class CarteraService
{
    public async Task<int> GenerarCarteraVehiculo(string placa, int desde, int hasta)
        {
            var vehiculo = await context.Vehiculos.FirstOrDefaultAsync(v => v.Placa == placa);
    
            if (vehiculo == null) return 0;
    
            var parametro = await context.Parametros.AsNoTracking().FirstOrDefaultAsync();
    
            await context.Cartera
                .Where(c => c.Placa == placa && !c.IsPagado && c.Vigencia >= desde && c.Vigencia <= hasta)
                .ExecuteDeleteAsync();
    
            vehiculo.PagoHasta = desde;
    
            var nuevasDeudas = new List<Cartera>();
    
            for (var vigencia = desde; vigencia <= hasta; vigencia++)
            {
                var valorRodamiento = await tarifaService.ObtenerTarifa(TipoConceptoTarifa.Rodamiento, vigencia);
                if (valorRodamiento > 0)
                {
                    nuevasDeudas.Add(CrearCartera(vehiculo.Id, placa, vigencia, TipoConceptoCartera.Rodamiento, valorRodamiento, tieneInteres: true));
                }
    
                if (parametro?.CobraAdicional == true && parametro.ValorCostasPersuasivo > 0 && vigencia != 2026)
                {
                    nuevasDeudas.Add(CrearCartera(vehiculo.Id, placa, vigencia, TipoConceptoCartera.Costas, parametro.ValorCostasPersuasivo, tieneInteres: true));
                }
    
                var valorCargaPasajeros = await  liquidacionService.ObtenerValorCargaOPasajero(vehiculo, vigencia);
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
    
            return await context.SaveChangesAsync();
        }
}