using Domain.Models.Acuerdos;
using Domain.Models.Acuerdos.Enums;
using Domain.Models.Acuerdos.Requests;
using Domain.Models.Acuerdos.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Infrastructure.Services.AcuerdosPago;

public partial class AcuerdoPagoService
{
    public async Task<AcuerdoPagoDto?> CrearAcuerdoPago(
        CreateAcuerdoPagoRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null || request.CarteraIds == null || request.CarteraIds.Count == 0 || request.NumeroCuotas <= 0)
            return null;

        // Convertir explícitamente a UTC al inicio para asegurar Kind = Utc en todas las referencias
        var fechaSuscripcionUtc = DateTime.SpecifyKind(request.FechaSuscripcion, DateTimeKind.Utc);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var carteras = await _context.Cartera
                .Where(c => request.CarteraIds.Contains(c.Id) && c.VehiculoId == request.VehiculoId)
                .ToListAsync(cancellationToken);

            // Validar que existan y que ninguna esté ya en un acuerdo activo o pagada/anulada
            if (carteras.Count != request.CarteraIds.Count)
                throw new InvalidOperationException("Algunas carteras seleccionadas no existen o no pertenecen al vehículo.");

            if (carteras.Any(c => c.AcuerdoPagoId.HasValue || c.IsPagado || c.IsAnulled))
                throw new InvalidOperationException("Una o más carteras seleccionadas ya se encuentran en otro acuerdo o están pagadas/anuladas.");

            // 1. Calcular el Interés por Mora dinámicamente usando LiquidacionService para cada cartera
            foreach (var cartera in carteras)
            {
                // Usamos la lógica de tu LiquidacionService pasándole capital, vigencia y fecha de suscripción
                cartera.ValorInteres = await liquidacionService.CalcularInteresMora(
                    cartera.Valor, 
                    cartera.Vigencia, 
                    fechaSuscripcionUtc
                );
            }

            // 2. Consolidar totales exactos con los intereses calculados
            decimal totalCapital = carteras.Sum(c => c.Valor);
            decimal totalInteres = carteras.Sum(c => c.ValorInteres);
            decimal totalFinanciado = totalCapital + totalInteres;

            if (request.ValorCuotaInicial >= totalFinanciado)
                throw new InvalidOperationException("La cuota inicial no puede ser mayor o igual al total adeudado.");

            // 3. Generar Consecutivo de Acuerdo (Ej: AP-2026-00001)
            int anioActual = fechaSuscripcionUtc.Year;
            int contadorAnio = await _context.AcuerdosDePago
                .CountAsync(a => a.FechaSuscripcion.Year == anioActual, cancellationToken) + 1;

            string numeroAcuerdo = $"AP-{anioActual}-{contadorAnio:D5}";

            // 4. Instanciar el Acuerdo de Pago
            var acuerdo = new AcuerdosDePago
            {
                NumeroAcuerdo = numeroAcuerdo,
                FechaSuscripcion = fechaSuscripcionUtc,
                NumeroCuotas = request.NumeroCuotas,
                ValorCapitalFinanciado = totalCapital,
                ValorInteresFinanciado = totalInteres,
                ValorCuotaInicial = request.ValorCuotaInicial,
                ValorTotalFinanciado = totalFinanciado,
                Estado = EstadoAcuerdoPago.Vigente,
                Observaciones = request.Observaciones,
                VehiculoId = request.VehiculoId,
                ProcesoId = request.ProcesoId
            };

            // 5. Calcular la amortización lineal y generar las cuotas
            decimal capitalAFinanciar = totalCapital;
            decimal interesAFinanciar = totalInteres;

            // Si pagó cuota inicial, descontamos proporcionalmente de capital e interés
            if (request.ValorCuotaInicial > 0)
            {
                decimal proporcionCapital = totalFinanciado > 0 ? totalCapital / totalFinanciado : 0;
                decimal inicialCapital = Math.Round(request.ValorCuotaInicial * proporcionCapital, 2);
                decimal inicialInteres = request.ValorCuotaInicial - inicialCapital;

                capitalAFinanciar -= inicialCapital;
                interesAFinanciar -= inicialInteres;

                // Cuota 0: Cuota Inicial (Vence el mismo día)
                acuerdo.Cuotas.Add(new CuotaAcuerdoPago
                {
                    NumeroCuota = 0,
                    FechaVencimiento = fechaSuscripcionUtc,
                    ValorCapital = inicialCapital,
                    ValorInteres = inicialInteres,
                    ValorTotalCuota = request.ValorCuotaInicial,
                    Estado = EstadoCuotaAcuerdo.Pendiente
                });
            }

            // Reparto lineal entre el número de cuotas
            decimal capitalPorCuotaBase = Math.Round(capitalAFinanciar / request.NumeroCuotas, 2);
            decimal interesPorCuotaBase = Math.Round(interesAFinanciar / request.NumeroCuotas, 2);

            // Control de residuos para ajuste exacto en la última cuota
            decimal capitalAcumulado = 0;
            decimal interesAcumulado = 0;

            for (int i = 1; i <= request.NumeroCuotas; i++)
            {
                decimal cCapital = capitalPorCuotaBase;
                decimal cInteres = interesPorCuotaBase;

                // Ajuste de residuo por redondeo en la última cuota
                if (i == request.NumeroCuotas)
                {
                    cCapital = capitalAFinanciar - capitalAcumulado;
                    cInteres = interesAFinanciar - interesAcumulado;
                }
                else
                {
                    capitalAcumulado += cCapital;
                    interesAcumulado += cInteres;
                }

                DateTime fechaVencimiento = DateTime.SpecifyKind(
                    fechaSuscripcionUtc.AddMonths(i),
                    DateTimeKind.Utc
                );

                acuerdo.Cuotas.Add(new CuotaAcuerdoPago
                {
                    NumeroCuota = i,
                    FechaVencimiento = fechaVencimiento,
                    ValorCapital = cCapital,
                    ValorInteres = cInteres,
                    ValorTotalCuota = cCapital + cInteres,
                    Estado = EstadoCuotaAcuerdo.Pendiente
                });
            }

            // 6. Guardar el acuerdo en BD para obtener su ID
            _context.AcuerdosDePago.Add(acuerdo);
            await _context.SaveChangesAsync(cancellationToken);

            // 7. Bloquear/Asignar las carteras y actualizar su ValorInteres con la mora calculada
            foreach (var cartera in carteras)
            {
                cartera.AcuerdoPagoId = acuerdo.Id;
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return await GetAcuerdosQuery()
                .FirstOrDefaultAsync(a => a.Id == acuerdo.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Error al crear el acuerdo de pago para el vehículo ID {VehiculoId}", request.VehiculoId);

            throw;
        }
    }
}