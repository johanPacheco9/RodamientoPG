using Domain.Models.ProcesoLiquidacion;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.Procesos.Coactivo;

public partial class CoactivoService
{
    public async Task<List<Proceso>> ReporteProcesos(int opcion, string pcomp, DateTime pdesde, DateTime phasta, int pvigencia, int ptran, EstadoProceso ptipo)
    {
        var query = context.Procesos
            .Include(p => p.Vehiculo)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(pcomp))
            query = query.Where(p => p.Vehiculo.Placa == pcomp);

        if (pvigencia > 0)
            query = query.Where(p => p.Desde <= pvigencia && p.Hasta >= pvigencia);

        if (ptran > 0)
            query = query.Where(p => p.NumeroProceso == ptran);

        if (ptipo > 0)
            query = query.Where(p => p.EstadoProceso == ptipo);

        if (pdesde != default && phasta != default)
            query = query.Where(p => p.FechaProceso >= pdesde && p.FechaProceso <= phasta);

        return await query
            .OrderBy(p => p.Id)
            .ToListAsync();
    }
}