using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.Liquidaciones;

public partial class LiquidacionService
{
    private async Task<decimal> ObtenerPorcentajeDescuentoVigente()
    {
        var hoy = DateTime.UtcNow;

        return await context.Descuentos
            .Where(d => hoy >= d.Desde && hoy <= d.Hasta)
            .Select(d => d.Porcentaje)
            .FirstOrDefaultAsync();
    }
}