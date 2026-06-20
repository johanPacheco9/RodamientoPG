using Domain.Models.ProcesoLiquidacion;
using Domain.Responses.Proceso.Enums;
using Domain.Responses.Proceso.Responses;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.Procesos.Coactivo;

public partial class CoactivoService
{
    public async Task<List<ConsolidadoCoactivosDto>> ConsolidadosPorLote()
    {
        try
        {
            return await context.Procesos
                .Where(p => p.EstadoProceso == EstadoProceso.Coactivo)
                .GroupBy(p => new { p.NumeroProceso, p.FechaProceso })
                .Select(g => new ConsolidadoCoactivosDto
                (
                    g.Key.NumeroProceso,
                    g.Key.FechaProceso,
                    g.Count()
                ))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error al generar el reporte consolidado de procesos coactivos por lote.", ex);
        }
    }
}