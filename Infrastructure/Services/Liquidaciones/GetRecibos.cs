using Domain.Models;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.Liquidaciones;

public partial class LiquidacionService 
{
    public async Task<List<Recibo>> GetRecibos(string placa)
    {
        return await context.Recibos
            .Where(r => r.Vehiculo.Placa == placa)
            .ToListAsync();
    }
}