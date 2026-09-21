using Domain.Models.Acuerdos.Enums;
using Domain.Models.ProcesoLiquidacion;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.AcuerdosPago;

public partial class AcuerdoPagoService
{
    public class ActualizacionAcuerdosVencidosACoactivo(
        IServiceScopeFactory scopeFactory,
        ILogger<AcuerdoPagoService> logger
    ) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromDays(1));

            do
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var acuerdoPagoService = scope.ServiceProvider.GetRequiredService<AcuerdoPagoService>();
                    await acuerdoPagoService.ActualizarVencidosToCoactivo();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error ejecutando la actualización periódica de acuerdos vencidos a coactivo.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
    }

    public async Task ActualizarVencidosToCoactivo()
    {
        try
        {
            var fechaLimite = DateTime.UtcNow.AddDays(-25);
            var fechaActual = DateTime.UtcNow;

            var acuerdosVencidos = await _context.AcuerdosDePago
                .Include(a => a.Cuotas)
                .Where(a => a.Estado == EstadoAcuerdoPago.Vigente &&
                            a.Cuotas.Any(c => c.FechaVencimiento < fechaLimite && c.Estado == EstadoCuotaAcuerdo.Pendiente))
                .ToListAsync();

            foreach (var acuerdo in acuerdosVencidos)
            {
                acuerdo.Estado = EstadoAcuerdoPago.Incumplido;

                foreach (var cuota in acuerdo.Cuotas.Where(c => c.Estado == EstadoCuotaAcuerdo.Pendiente))
                {
                    cuota.Estado = EstadoCuotaAcuerdo.Vencida;
                }

                // Carteras (vigencias) que quedaron congeladas dentro de este acuerdo.
                var carterasDelAcuerdo = await _context.Cartera
                    .Where(c => c.AcuerdoPagoId == acuerdo.Id)
                    .ToListAsync();

                // Deuda original sin el beneficio/tasa especial: se ignora el Descuento
                // porque ese beneficio se pierde al incumplir el acuerdo.
                var totalSinBeneficio = carterasDelAcuerdo.Sum(c => c.Valor + c.ValorInteres);

                var abonado = acuerdo.Cuotas
                    .Where(c => c.Estado == EstadoCuotaAcuerdo.Pagada)
                    .Sum(c => c.ValorTotalCuota);

                // Consejo de Estado: el abono se aplica primero a interés y luego a capital.
                // Como Proceso.Valor es un único total, el orden de aplicación no cambia el resultado.
                var saldoPendiente = Math.Max(0m, totalSinBeneficio - abonado);

                var proceso = new Proceso
                {
                    VehiculoId = acuerdo.VehiculoId,
                    Fecha = fechaActual,
                    FechaMandamiento = fechaActual,
                    FechaProceso = fechaActual,
                    Valor = saldoPendiente,
                    EstadoProceso = EstadoProceso.Coactivo
                };

                _context.Procesos.Add(proceso);
                acuerdo.Proceso = proceso;

                // Se liberan las Cartera del acuerdo roto (quedan como historial;
                // Valor/ValorInteres/Descuento no se modifican).
                await _context.Cartera
                    .Where(c => c.AcuerdoPagoId == acuerdo.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(c => c.AcuerdoPagoId, (int?)null));
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error al actualizar acuerdos vencidos a coactivo.");
            throw;
        }
    }
}