using Domain.Models.ProcesoLiquidacion;
using Domain.Responses.Proceso.Enums;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.Procesos.Persuasivo;

public partial class PersuasivoService
{
    public async Task<List<Proceso>> List(int opcion, int ptran, string placa)
    {
        try
        {
            if (opcion == 5)
            {
                return await context.Procesos
                    .Include(p => p.Vehiculo)
                    .Where(p => p.Vehiculo.Placa == placa)
                    .ToListAsync();
            }

            return await context.Procesos
                .Include(p => p.Vehiculo)
                .Where(p => p.EstadoProceso == EstadoProceso.Persuasivo)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error al consultar los procesos coactivos en la base de datos.", ex);
        }
    }
    
}