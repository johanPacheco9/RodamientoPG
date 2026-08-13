using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Infrastructure.Services.Descuentos;

public partial class DescuentosManager
{
    /// <summary>
    /// Elimina un descuento por su Id.
    /// </summary>
    public async Task<(bool Exito, string Mensaje)> Delete(int id)
    {
        var descuento = await _context.Set<Descuento>().FirstOrDefaultAsync(d => d.Id == id);

        if (descuento is null)
        {
            _logger.LogWarning("Intento de eliminación de descuento inexistente. Id={Id}", id);
            return (false, "El descuento indicado no existe.");
        }

        try
        {
            _context.Set<Descuento>().Remove(descuento);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Descuento eliminado correctamente. Id={Id}", id);

            return (true, "Descuento eliminado correctamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el descuento. Id={Id}", id);
            return (false, "Ocurrió un error al eliminar el descuento.");
        }
    }
}