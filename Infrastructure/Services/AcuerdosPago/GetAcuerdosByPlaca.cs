using Domain.Models.Acuerdos.Enums;
using Domain.Models.Acuerdos.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.AcuerdosPago;

public partial class AcuerdoPagoService
{
    public async Task<List<AcuerdoPagoDto>> GetAcuerdosByPlaca(
        string placa, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(placa))
                return [];

            placa = placa.Trim().ToUpper();

            var acuerdos = await _context.AcuerdosDePago
                .AsNoTracking()
                .Where(a => a.Vehiculo.Placa == placa)
                .OrderByDescending(a => a.FechaSuscripcion)
                .Select(a => new AcuerdoPagoDto
                {
                    Id = a.Id,
                    NumeroAcuerdo = a.NumeroAcuerdo,
                    Placa = a.Vehiculo.Placa,
                    FechaSuscripcion = a.FechaSuscripcion,
                    NumeroCuotas = a.NumeroCuotas,
                    ValorCapitalFinanciado = a.ValorCapitalFinanciado,
                    ValorInteresFinanciado = a.ValorInteresFinanciado,
                    ValorCuotaInicial = a.ValorCuotaInicial,
                    ValorTotalFinanciado = a.ValorTotalFinanciado,
                    Estado = a.Estado,
                    Observaciones = a.Observaciones,
                    VehiculoId = a.VehiculoId,
                    ProcesoId = a.ProcesoId,

                    // --- Mapeo de la relación Proceso -> Liquidaciones ---
                    NumeroProceso = a.Proceso != null ? a.Proceso.NumeroProceso : null,
                    
                    VigenciaDesde = a.Proceso != null && a.Proceso.Liquidaciones.Any() 
                        ? a.Proceso.Liquidaciones.Min(l => l.VigenciaDesde) 
                        : null,

                    VigenciaHasta = a.Proceso != null && a.Proceso.Liquidaciones.Any() 
                        ? a.Proceso.Liquidaciones.Max(l => l.VigenciaHasta) 
                        : null,

                    // Cálculo del saldo pendiente
                    SaldoPendiente = a.Cuotas
                        .Where(c => c.Estado != EstadoCuotaAcuerdo.Pagada)
                        .Sum(c => c.ValorTotalCuota),

                    // Mapeo del plan de cuotas
                    Cuotas = a.Cuotas
                        .OrderBy(c => c.NumeroCuota)
                        .Select(c => new CuotaAcuerdoDto
                        {
                            Id = c.Id,
                            NumeroCuota = c.NumeroCuota,
                            FechaVencimiento = c.FechaVencimiento,
                            FechaPago = c.FechaPago,
                            ValorCuota = c.ValorCapital,
                            InteresFinanciacion = c.ValorInteres,
                            EstadoCuota = c.Estado,
                            IsPagado = c.Estado == EstadoCuotaAcuerdo.Pagada
                        })
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            return acuerdos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consultando acuerdos de pago para la placa {Placa}", placa);
            return [];
        }
    }
}