using Domain.Responses.Recibo.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services.Pagos;

public partial class PagoService
{
    public async Task<int> AplicarPago(int numeroRecibo)
    {
        try
        {
            // 1. Buscamos el recibo encabezado
            var recibo = await context.Recibos.FirstOrDefaultAsync(s => s.Id == numeroRecibo);
            if (recibo == null) return 0; // Guardrail por si el recibo no existe

            // 2. Traemos los IDs de cartera relacionados al detalle de este recibo
            var carteraIdsPagos = await context.ReciboDetalle
                .Where(s => s.ReciboId == numeroRecibo)
                .Select(c => c.CarteraId)
                .ToListAsync();

            if (!carteraIdsPagos.Any()) return 0;
            
            // 3. Obtenemos las entidades físicas de Cartera
            var carterasAAplicar = await context.Cartera
                .Where(c => carteraIdsPagos.Contains(c.Id))
                .ToListAsync();
            
            // 4. Actualizamos la cartera (Paz y salvo y fin de coactivo)
            foreach (var itemCartera in carterasAAplicar)
            { 
                itemCartera.IsPagado = true;
                itemCartera.EstaEnProcesoCoactivo = false; 
            }

            // 5. 🔥 CORREGIDO: Asignación del estado del recibo usando el operador '='
            recibo.Estado = EstadoRecibo.Pagado; 
            recibo.FechaPago = DateTime.UtcNow;
            
            // 6. Impactamos la base de datos en una sola transacción atómica
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Fallo crítico en AplicarPago para el recibo N° {numeroRecibo}: {ex.Message}");
            return 0;
        }
    }
}