using Domain.Models.Recibos;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.Pagos;

public partial class PagoService
{
    public async Task<Recibo> GetRecibo(int nrecibo)
    {
        var recibo = await context.Recibos
            .Include(r => r.Vehiculo)
            .ThenInclude(v => v.Propietario)
            .Include(r => r.Vehiculo)
            .ThenInclude(v => v.Marca)
            .Include(r => r.Vehiculo)
            .ThenInclude(v => v.Linea)
            .FirstOrDefaultAsync(s => s.Id == nrecibo);

        if (recibo == null)
            throw new KeyNotFoundException($"El recibo con ID {nrecibo} no existe.");

        return recibo;
    }
}