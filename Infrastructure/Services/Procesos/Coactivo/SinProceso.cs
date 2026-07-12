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
                !context.Procesos.Any(p => p.VehiculoId == l.VehiculoId && p.EstadoProceso != EstadoProceso.SinProceso))
            .ToListAsync();
    }

}