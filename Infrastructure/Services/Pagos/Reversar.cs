using Domain.Models.Acuerdos.Enums;
using Domain.Responses.Recibo.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services.Pagos;

public partial class PagoService
{
    public async Task<bool> ReversarPago(int numeroRecibo, CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. Cargar el recibo con sus detalles de cartera Y la relación con la cuota del acuerdo
            var recibo = await context.Recibos
                .Include(r => r.Detalles)
                .Include(r => r.CuotaAcuerdoPago)
                .ThenInclude(c => c.AcuerdoPago)
                .FirstOrDefaultAsync(r => r.Id == numeroRecibo, cancellationToken);

            if (recibo == null)
                throw new InvalidOperationException($"El recibo N° {numeroRecibo} no existe.");

            // Guardrail: Solo se puede reversar si está Pagado
            if (recibo.Estado != EstadoRecibo.Pagado)
                throw new InvalidOperationException($"El recibo N° {numeroRecibo} no está pagado (Estado actual: {recibo.Estado}).");

            // ==========================================
            // CASO A: Es un Recibo de Acuerdo de Pago
            // ==========================================
            if (recibo.CuotaAcuerdoPagoId.HasValue && recibo.CuotaAcuerdoPago != null)
            {
                var cuota = recibo.CuotaAcuerdoPago;

                // 1. Revertir la cuota a Pendiente
                cuota.Estado = EstadoCuotaAcuerdo.Pendiente;
                cuota.FechaPago = null;
                cuota.NumeroRecibo = null;

                // 2. Si el Acuerdo de Pago estaba Finalizado, reactivarlo (Ajusta la enum según tu modelo)
                if (cuota.AcuerdoPago != null && cuota.AcuerdoPago.Estado == EstadoAcuerdoPago.Finalizado)
                {
                    cuota.AcuerdoPago.Estado = EstadoAcuerdoPago.Vigente;

                    // Revertir la cartera asociada al acuerdo para que vuelva a figurar como no pagada
                    var carterasAcuerdo = await context.Cartera
                        .Where(c => c.AcuerdoPagoId == cuota.AcuerdoPagoId)
                        .ToListAsync(cancellationToken);

                    foreach (var car in carterasAcuerdo)
                    {
                        car.IsPagado = false;
                        car.FechaPago = null;
                    }
                }
            }
            // ==========================================
            // CASO B: Es un Recibo Normal (Liquidación Directa)
            // ==========================================
            else if (recibo.Detalles != null && recibo.Detalles.Any())
            {
                var carteraIds = recibo.Detalles.Select(d => d.CarteraId).ToList();

                var carteras = await context.Cartera
                    .Where(c => carteraIds.Contains(c.Id))
                    .ToListAsync(cancellationToken);

                foreach (var itemCartera in carteras)
                {
                    itemCartera.IsPagado = false;
                    itemCartera.FechaPago = null;
                }
            }
            else
            {
                throw new InvalidOperationException($"El recibo N° {numeroRecibo} no contiene detalles de cartera ni está asociado a una cuota de acuerdo de pago.");
            }

            // 3. Revertir el estado del Recibo
            recibo.Estado = EstadoRecibo.Pendiente; // O EstadoRecibo.Anulado según tu lógica de negocio
            recibo.FechaPago = null;
            recibo.FechaAplica = null;

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error al reversar el pago del recibo N° {NumeroRecibo}", numeroRecibo);

            throw;
        }
    }
}