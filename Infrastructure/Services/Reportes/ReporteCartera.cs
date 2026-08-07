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
        var queryBase = context.Cartera
            .Where(c => !c.IsPagado
                        && !c.IsAnulled
                        && (c.ResolucionId == null
                            || (c.Resolucion!.TipoResolucion != TipoResolucion.AnulacionDeuda
                                && c.Resolucion.TipoResolucion != TipoResolucion.Traslado)));

        // Agrupamos por Vigencia utilizando evaluadores ternarios (SUM CASE WHEN)
        var balancePorVigencia = await queryBase
            .GroupBy(c => c.Vigencia)
            .Select(g => new CarteraVigenciaDto
            {
                Vigencia = g.Key,
                ValorRodamiento = g.Sum(c => c.Concepto == TipoConceptoCartera.Rodamiento ? c.ValorTotal : 0),
                ValorCarga = g.Sum(c => c.Concepto == TipoConceptoCartera.Carga ? c.ValorTotal : 0),
                ValorEstampillas = g.Sum(c => c.Concepto == TipoConceptoCartera.Estampillas ? c.ValorTotal : 0),
                ValorCostas = g.Sum(c => c.Concepto == TipoConceptoCartera.Costas ? c.ValorTotal : 0),
                ValorInteres = g.Sum(c => c.ValorInteres)
            })
            .OrderByDescending(x => x.Vigencia)
            .ToListAsync();

        return balancePorVigencia;
    }
}