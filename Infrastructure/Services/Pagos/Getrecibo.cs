using Domain.Models.Recibos.Responses;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Pagos;

public partial class PagoService
{
    public async Task<ReciboDto?> GetRecibo(int nrecibo)
    {
        var reciboDto = await context.Recibos
            .AsNoTracking()
            .Where(r => r.Id == nrecibo)
            .Select(r => new ReciboDto(
                r.Id,
                r.Estado,
                r.Fecha,
                r.FechaPago,
                r.ValorCapital,
                r.InteresMora,
                r.Descuento,
                r.Estampillas,
                r.ValorTotalSistema,
                r.ValorCargaDatos,
                r.ValorRodamiento,
                // Fórmula de cálculo total explicita para EF Core
                (r.ValorCapital + r.InteresMora + r.Estampillas + r.ValorCargaDatos + r.ValorRodamiento) - r.Descuento,
                
                // Extremas (Si no hay detalles asigna 0)
                r.Detalles.Any() ? r.Detalles.Min(d => d.Vigencia) : 0,
                r.Detalles.Any() ? r.Detalles.Max(d => d.Vigencia) : 0,
                
                // Evaluación de consistencia entre sistema y calculados
                r.ValorTotalSistema == ((r.ValorCapital + r.InteresMora + r.Estampillas + r.ValorCargaDatos + r.ValorRodamiento) - r.Descuento),
                
                // Placa extraída desde la relación Vehiculo
                r.Vehiculo != null ? r.Vehiculo.Placa : string.Empty,
                
                // Mapeo de la colección de detalles pasando el Enum Concepto a String (o .ToString())
                r.Detalles.Select(d => new ReciboDetalleDto(
                    d.Id,
                    d.CarteraId,
                    d.Vigencia,
                    d.Concepto.ToString(), // O d.Concepto.GetDisplayName() si tienes extensiones de Enum
                    d.ValorTotal
                )).ToList()
            ))
            .FirstOrDefaultAsync();

        if (reciboDto == null)
            throw new KeyNotFoundException($"El recibo con ID {nrecibo} no existe.");

        return reciboDto;
    }
}