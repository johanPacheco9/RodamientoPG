using Domain.Models.Carteras.Enums;
using Domain.Models.Resoluciones;
using Infrastructure.Services.Reportes.Responses;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Reportes;
public partial class ReportesManager
{
    public async Task<List<CarteraClaseVehiculoDto>> ObtenerCarteraPorClaseVehiculo()
    {
        var queryBase = context.Cartera
            .Where(c => !c.IsPagado
                        && !c.IsAnulled
                        && (c.ResolucionId == null
                            || (c.Resolucion!.TipoResolucion != TipoResolucion.AnulacionDeuda
                                && c.Resolucion.TipoResolucion != TipoResolucion.Traslado)));

        var balancePorClase = await queryBase
            .GroupBy(c => new 
            { 
                c.Vehiculo.TipoVehiculoId, 
                NombreClase = c.Vehiculo.TipoVehiculo.Nombre 
            })
            .Select(g => new CarteraClaseVehiculoDto
            {
                TipoVehiculoId = g.Key.TipoVehiculoId,
                ClaseVehiculo = g.Key.NombreClase ?? "SIN ESPECIFICAR",
                ValorRodamiento = g.Where(c => c.Concepto == TipoConceptoCartera.Rodamiento).Sum(c => c.ValorTotal),
                ValorCarga = g.Where(c => c.Concepto == TipoConceptoCartera.Carga).Sum(c => c.ValorTotal),
                ValorEstampillas = g.Where(c => c.Concepto == TipoConceptoCartera.Estampillas).Sum(c => c.ValorTotal),
                ValorCostas = g.Where(c => c.Concepto == TipoConceptoCartera.Costas).Sum(c => c.ValorTotal),
                ValorInteres = g.Sum(c => c.ValorInteres)
            })
            .OrderBy(x => x.ClaseVehiculo)
            .ToListAsync();

        return balancePorClase;
    }
}