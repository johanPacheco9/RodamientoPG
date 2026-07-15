using Domain.Models.Carteras.Enums;
using Domain.Models.Resoluciones;
using Infrastructure.Services.Reportes.Responses;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.Reportes;

public partial class ReportesManager
{
    public async Task<List<CarteraVigenciaDto>> ObtenerCarteraPorVigencias()
    {
        // Consultamos la cartera pendiente que no esté anulada, trasladada ni pagada
        var queryBase =  context.Cartera
            .Where(c => !c.IsPagado
                        && !c.IsAnulled
                        && (c.ResolucionId == null
                            || (c.Resolucion!.TipoResolucion != TipoResolucion.AnulacionDeuda
                                && c.Resolucion.TipoResolucion != TipoResolucion.Traslado)));

       
        
        
        // Agrupamos por Vigencia (Año) directamente en la Base de Datos (PostgreSQL)
        var balancePorVigencia = await queryBase
            .GroupBy(c => c.Vigencia)
            .Select(g => new CarteraVigenciaDto
            {
                Vigencia = g.Key,
                ValorRodamiento = g.Where(c => c.Concepto == TipoConceptoCartera.Rodamiento).Sum(c => c.ValorTotal),
                ValorCarga = g.Where(c => c.Concepto == TipoConceptoCartera.Carga).Sum(c => c.ValorTotal),
                ValorEstampillas = g.Where(c => c.Concepto == TipoConceptoCartera.Estampillas).Sum(c => c.ValorTotal),
                ValorCostas = g.Where(c => c.Concepto == TipoConceptoCartera.Costas).Sum(c => c.ValorTotal),
                ValorInteres = g.Sum(c => c.ValorInteres)
            })
            .OrderByDescending(x => x.Vigencia)
            .ToListAsync();

        return balancePorVigencia;
    }
}