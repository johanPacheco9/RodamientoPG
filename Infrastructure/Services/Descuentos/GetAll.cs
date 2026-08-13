using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Infrastructure.Services.Descuentos;

public partial class DescuentosManager
{
    /// <summary>
    /// Obtiene todos los descuentos registrados, ordenados por fecha de inicio descendente.
    /// </summary>
    public async Task<List<Descuento>> GetAll()
    {
        try
        {
            return await _context.Set<Descuento>()
                .AsNoTracking()
                .OrderByDescending(d => d.Desde)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el listado de descuentos.");
            return [];
        }
    }
    
}