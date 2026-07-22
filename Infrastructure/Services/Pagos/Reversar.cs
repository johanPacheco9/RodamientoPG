using Domain.Responses.Recibo.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Infrastructure.Services.Pagos;

public partial class PagoService
{
    public async Task<int> ReversarPago(int numeroRecibo)
    {
        try
        {
            // 1. Buscamos el recibo encabezado
            var recibo = await context.Recibos.FirstOrDefaultAsync(s => s.Id == numeroRecibo);

            if (recibo == null) return 0; // Guardrail por si el recibo no existe

            // 2. Guardrail: solo se puede reversar un recibo que esté efectivamente PAGADO
            if (recibo.Estado != EstadoRecibo.Pagado) return 0;

            // 3. Traemos los IDs de cartera relacionados al detalle de este recibo
            var carteraIdsPagos = await context.ReciboDetalle
                .Where(s => s.ReciboId == numeroRecibo)
                .Select(c => c.CarteraId)
                .ToListAsync();

            if (!carteraIdsPagos.Any()) return 0;

            // 4. Obtenemos las entidades físicas de Cartera
            var carterasAReversar = await context.Cartera
                .Where(c => carteraIdsPagos.Contains(c.Id))
                .ToListAsync();

            // 5. Revertimos el estado de la cartera (vuelve a quedar como deuda pendiente)
            foreach (var itemCartera in carterasAReversar)
            {
                itemCartera.IsPagado = false;
            }

            // 6. 🔥 Revertimos el estado y fechas del recibo
            recibo.Estado = EstadoRecibo.Pendiente;
            recibo.FechaPago = null;
            recibo.FechaAplica = null;

            // 7. Impactamos la base de datos en una sola transacción atómica
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Fallo crítico en ReversarPago para el recibo N° {numeroRecibo}: {ex.Message}");

            return 0;
        }
    }
}