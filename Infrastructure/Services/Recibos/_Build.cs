using Domain.Generics;
using Domain.Models.Recibos.Responses;
using Infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Infrastructure.Services.Rec2ibos;

public partial class ReciboService(MainDataContext context, ILogger<ReciboService> logger)
{
    private readonly MainDataContext _context = context;
    private readonly ILogger<ReciboService> _logger = logger;
    
    public async Task<List<ReciboDto>> GetRecibosByPlaca(string placa)
    {
        return await _context.Recibos
            .Where(r => r.Vehiculo.Placa == placa)
            .OrderByDescending(r => r.Fecha) // Los más recientes primero
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
                // ValorTotalCalculado matemático
                (r.ValorCapital + r.InteresMora + r.Estampillas + r.ValorCargaDatos + r.ValorRodamiento) - r.Descuento,
        
                // Cálculos dinámicos basados en la nueva relación de Detalles
                r.Detalles.Any() ? r.Detalles.Min(d => d.Vigencia) : 0,
                r.Detalles.Any() ? r.Detalles.Max(d => d.Vigencia) : 0,
        
                // EstaCompleto: Evalúa si todas las carteras asociadas a este recibo pasaron a estar pagadas
                r.Detalles.Any() && r.Detalles.All(d => d.Cartera.IsPagado),
        
                r.Vehiculo.Placa,
                r.Detalles.Select(d => new ReciboDetalleDto(
                    d.Id,
                    d.CarteraId,
                    d.Vigencia,
                    d.Concepto.GetDisplayName(),
                    d.ValorTotal
                )).ToList()
            ))
            .ToListAsync(); // 2. Cambiamos FirstOrDefaultAsync() por ToListAsync()
    }
}