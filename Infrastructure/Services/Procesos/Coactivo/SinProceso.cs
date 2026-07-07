using Domain.Models.ProcesoLiquidacion;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.Procesos.Coactivo;

public partial class CoactivoService
{
    public async Task<List<Liquidacion>> SinProcesos(int yearVigencia)
    {
        return await context.Liquidacion
            .Include(l => l.Vehiculo)
            .Where(l =>
                l.ProcesoId == null &&
                l.VigenciaHasta <= yearVigencia &&
                l.Vehiculo.EstadoProcesoId == EstadoProceso.SinProceso)
            .ToListAsync();
    }

}