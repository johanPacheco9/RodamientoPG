using Domain.Responses.Recibo.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Models.Acuerdos.Enums;

namespace Infrastructure.Services.Pagos;

public partial class PagoService
{
    public async Task<bool> AplicarPago(int reciboId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. Cargar el recibo con sus detalles de cartera Y la relación con la cuota del acuerdo
            var recibo = await context.Recibos
                .Include(r => r.Detalles)
                .Include(r => r.CuotaAcuerdoPago)
                .ThenInclude(c => c.AcuerdoPago)
                .FirstOrDefaultAsync(r => r.Id == reciboId, cancellationToken);

            if (recibo == null)
                throw new InvalidOperationException($"El recibo N° {reciboId} no existe.");

            if (recibo.Estado == EstadoRecibo.Pagado)
                throw new InvalidOperationException($"El recibo N° {reciboId} ya fue pagado anteriormente.");

            var fechaPagoUtc = DateTime.UtcNow;

            // ==========================================
            // CASO A: Es un Recibo de Acuerdo de Pago
            // ==========================================
            if (recibo.CuotaAcuerdoPagoId.HasValue && recibo.CuotaAcuerdoPago != null)
            {
                var cuota = recibo.CuotaAcuerdoPago;

                // 1. Marcar la cuota como pagada
                cuota.Estado = EstadoCuotaAcuerdo.Pagada;
                cuota.FechaPago = fechaPagoUtc;
                cuota.NumeroRecibo = recibo.Id.ToString();

                // 2. Marcar el recibo como pagado
                recibo.Estado = EstadoRecibo.Pagado;
                recibo.FechaPago = fechaPagoUtc;
                recibo.FechaAplica = fechaPagoUtc;

                // 3. (Opcional) Verificar si con esta cuota se liquidó TODO el acuerdo de pago
                var cuotasAcuerdo = await context.CuotasAcuerdoDePagos
                    .Where(c => c.AcuerdoPagoId == cuota.AcuerdoPagoId)
                    .ToListAsync(cancellationToken);

                // Si todas las cuotas (incluida la actual que ya se marcó Pagada en memoria) están pagadas
                bool todasPagadas = cuotasAcuerdo
                    .All(c => c.Id == cuota.Id || c.Estado == EstadoCuotaAcuerdo.Pagada);

                if (todasPagadas)
                {
                    cuota.AcuerdoPago.Estado = EstadoAcuerdoPago.Finalizado; // O EstadoAcuerdoPago.Pagado

                    // Marcar la cartera original del acuerdo como pagada definitivamente
                    var carterasAcuerdo = await context.Cartera
                        .Where(c => c.AcuerdoPagoId == cuota.AcuerdoPagoId)
                        .ToListAsync(cancellationToken);

                    foreach (var car in carterasAcuerdo)
                    {
                        car.IsPagado = true;
                        car.FechaPago = fechaPagoUtc;
                    }
                }
            }
            // ======================= ===================
            // CASO B: Es un Recibo Normal (Liquidación Directa)
            // ==========================================
            else if (recibo.Detalles != null && recibo.Detalles.Any())
            {
                // Validar/procesar la cartera normal
                recibo.Estado = EstadoRecibo.Pagado;
                recibo.FechaPago = fechaPagoUtc;
                recibo.FechaAplica = fechaPagoUtc;

                var carteraIds = recibo.Detalles.Select(d => d.CarteraId).ToList();
                var carteras = await context.Cartera
                    .Where(c => carteraIds.Contains(c.Id))
                    .ToListAsync(cancellationToken);

                foreach (var cartera in carteras)
                {
                    cartera.IsPagado = true;
                    cartera.FechaPago = fechaPagoUtc;
                }
            }
            else
            {
                // Si no tiene ni cuota de acuerdo ni detalles de cartera
                throw new InvalidOperationException($"El recibo N° {reciboId} no contiene detalles de cartera ni está asociado a una cuota de acuerdo de pago.");
            }

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error al aplicar el pago del recibo N° {ReciboId}", reciboId);

            throw;
        }
    }
}