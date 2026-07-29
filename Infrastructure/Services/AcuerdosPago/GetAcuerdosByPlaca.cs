using Domain.Models.Acuerdos.Enums;
using Domain.Models.Acuerdos.Responses;
using Domain.Models.Recibos;
using Domain.Responses.Recibo.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.AcuerdosPago;

public partial class AcuerdoPagoService
{
    public async Task<List<AcuerdoPagoDto>> GetAllAcuerdos(CancellationToken cancellationToken = default)
    {
        return await GetAcuerdosQuery()
            .OrderByDescending(a => a.FechaSuscripcion)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AcuerdoPagoDto>> GetAcuerdosByPlaca(
        string placa,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(placa))
                return [];

            placa = placa.Trim().ToUpper();

            var acuerdos = await GetAcuerdosQuery()
                .Where(a => a.Placa == placa)
                .OrderByDescending(a => a.FechaSuscripcion)
                .ToListAsync(cancellationToken);

            return acuerdos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consultando acuerdos de pago para la placa {Placa}", placa);

            return [];
        }
    }

    public async Task<bool> RegistrarPagoCuota(int cuotaId, string? numeroRecibo)
    {
        var cuota = await _context.CuotasAcuerdoDePagos
            .Include(c => c.AcuerdoPago)
            .ThenInclude(a => a.Cuotas)
            .FirstOrDefaultAsync(c => c.Id == cuotaId);

        if (cuota is null || cuota.Estado == EstadoCuotaAcuerdo.Pagada)
            return false;

        var fechaActualUtc = DateTime.UtcNow;

        // 1. Marcar la cuota como pagada (Tu lógica exacta)
        cuota.Estado = EstadoCuotaAcuerdo.Pagada;
        cuota.FechaPago = fechaActualUtc;
        cuota.NumeroRecibo = numeroRecibo;

        var acuerdo = cuota.AcuerdoPago;

        // 2. Verificar si era la última cuota para paz y salvo del acuerdo (Tu lógica exacta)
        if (acuerdo.Cuotas.All(c => c.Id == cuotaId || c.Estado == EstadoCuotaAcuerdo.Pagada))
        {
            acuerdo.Estado = EstadoAcuerdoPago.Pagado;

            await _context.Cartera
                .Where(c => c.AcuerdoPagoId == acuerdo.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsPagado, true));
        }

        // 3. 🔥 EL PUENTE CONTABLE: Generar el Recibo pagado para el Informe Diario
        var recibo = new Recibo
        {
            VehiculoId = acuerdo.VehiculoId,
            CuotaAcuerdoPagoId = cuota.Id,
            Estado = EstadoRecibo.Pagado, // Nace Pagado directamente
            Fecha = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc),
            FechaPago = fechaActualUtc,                // Captura la hora exacta de entrada del pago
            ValorCapital = cuota.ValorCapital,         // Suma a TotalCapital en ObtenerInformeDiario
            InteresMora = cuota.ValorInteres,          // Suma a TotalIntereses en ObtenerInformeDiario
            ValorTotalSistema = cuota.ValorTotalCuota, // Suma a TotalRecaudado en ObtenerInformeDiario
            Descuento = 0,
            Estampillas = 0,
            ValorCargaDatos = 0,
            ValorRodamiento = 0
        };

        _context.Recibos.Add(recibo);

        // 4. Guardar los cambios de la cuota, el acuerdo y el recibo de forma atómica
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> MarcarIncumplido(int acuerdoId)
    {
        return await CambiarEstadoAcuerdo(acuerdoId, EstadoAcuerdoPago.Incumplido, liberarCartera: true);
    }

    public async Task<bool> AnularAcuerdo(int acuerdoId)
    {
        return await CambiarEstadoAcuerdo(acuerdoId, EstadoAcuerdoPago.Anulado, liberarCartera: true);
    }

    private async Task<bool> CambiarEstadoAcuerdo(int acuerdoId, EstadoAcuerdoPago estado, bool liberarCartera)
    {
        var acuerdo = await _context.AcuerdosDePago.FindAsync(acuerdoId);

        if (acuerdo is null)
            return false;

        acuerdo.Estado = estado;

        if (liberarCartera)
        {
            await _context.Cartera
                .Where(c => c.AcuerdoPagoId == acuerdoId && !c.IsPagado)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.AcuerdoPagoId, (int?)null));
        }

        return await _context.SaveChangesAsync() > 0;
    }

    private IQueryable<AcuerdoPagoDto> GetAcuerdosQuery()
    {
        return _context.AcuerdosDePago
            .AsNoTracking()
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
                NumeroProceso = a.Proceso != null ? a.Proceso.NumeroProceso : null,
                VigenciaDesde = _context.Cartera.Any(c => c.AcuerdoPagoId == a.Id)
                    ? _context.Cartera.Where(c => c.AcuerdoPagoId == a.Id).Min(c => (int?)c.Vigencia)
                    : a.Proceso != null && a.Proceso.Liquidaciones.Any()
                        ? a.Proceso.Liquidaciones.Min(l => (int?)l.VigenciaDesde)
                        : null,
                VigenciaHasta = _context.Cartera.Any(c => c.AcuerdoPagoId == a.Id)
                    ? _context.Cartera.Where(c => c.AcuerdoPagoId == a.Id).Max(c => (int?)c.Vigencia)
                    : a.Proceso != null && a.Proceso.Liquidaciones.Any()
                        ? a.Proceso.Liquidaciones.Max(l => (int?)l.VigenciaHasta)
                        : null,
                SaldoPendiente = a.Cuotas
                    .Where(c => c.Estado != EstadoCuotaAcuerdo.Pagada)
                    .Sum(c => c.ValorTotalCuota),
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
                        IsPagado = c.Estado == EstadoCuotaAcuerdo.Pagada,
                        NumeroRecibo = c.NumeroRecibo
                    })
                    .ToList()
            });
    }
}