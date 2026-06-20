using Domain.Models.ProcesoLiquidacion;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.Procesos.Coactivo;

public partial class CoactivoService
{
    public async Task<List<Proceso>> List(int nTransaccion)
    {
        try
        {
            return await context.Procesos
                .Where(t => t.NumeroProceso == nTransaccion)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener transacciones: {ex.Message}");
            return new List<Proceso>();
        }
    }
}
