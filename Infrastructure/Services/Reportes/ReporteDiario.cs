using Infrastructure.Services.Reportes.Responses;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Reportes;

public partial class ReportesManager
{
    public async Task<ReporteDiarioDto> ObtenerInformeDiario(DateTime fechaSeleccionada)
    {
        // 1. 🚀 SOLUCIÓN POSTGRESQL: Especificar explícitamente el Kind como UTC
        var fechaInicio = DateTime.SpecifyKind(fechaSeleccionada.Date, DateTimeKind.Utc);
        var fechaFin = DateTime.SpecifyKind(fechaInicio.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        var recibosDelDia = await context.Recibos
            .AsNoTracking()
            .Where(r => r.FechaPago >= fechaInicio && r.FechaPago <= fechaFin)
            .Select(r => new DetalleReciboReporteDto
            {
                ReciboId = r.Id,
                Placa = r.Vehiculo.Placa,             
                PropietarioNombre = r.Vehiculo.Propietario.Nombre,
                Documento = r.Vehiculo.Propietario.Documento,     
                FechaPago = r.FechaPago,
                ValorCapital = r.ValorCapital,
                InteresMora = r.InteresMora,
                Descuento = r.Descuento,
                TotalPagado = r.ValorTotalSistema
            })
            .ToListAsync();

        // 3. Consultamos sumatorias específicas por conceptos directamente en la BD
        var sumatorias = await context.Recibos
            .AsNoTracking()
            .Where(r => r.FechaPago >= fechaInicio && r.FechaPago <= fechaFin)
            .GroupBy(r => 1)
            .Select(g => new
            {
                TotalRodamiento = g.Sum(r => r.ValorRodamiento),
                TotalEstampillas = g.Sum(r => r.Estampillas),
                TotalCargaDatos = g.Sum(r => r.ValorCargaDatos),
                TotalCapital = g.Sum(r => r.ValorCapital),
                TotalInteres = g.Sum(r => r.InteresMora),
                TotalDescuento = g.Sum(r => r.Descuento),
                TotalTotal = g.Sum(r => r.ValorTotalSistema)
            })
            .FirstOrDefaultAsync();

        // 4. Mapear al DTO final consolidado
        return new ReporteDiarioDto
        {
            FechaReporte = fechaSeleccionada,
            CantidadRecibos = recibosDelDia.Count,
            TotalRecaudado = sumatorias?.TotalTotal ?? 0,
            TotalCapital = sumatorias?.TotalCapital ?? 0,
            TotalIntereses = sumatorias?.TotalInteres ?? 0,
            TotalDescuentos = sumatorias?.TotalDescuento ?? 0,
            TotalRodamiento = sumatorias?.TotalRodamiento ?? 0,
            TotalEstampillas = sumatorias?.TotalEstampillas ?? 0,
            TotalCargaDatos = sumatorias?.TotalCargaDatos ?? 0,
            Transacciones = recibosDelDia
        };
    }
}