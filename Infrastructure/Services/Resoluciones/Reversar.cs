using Domain.Responses.Resolucion.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Infrastructure.Services.Resoluciones;

public partial class ResolucionService
{
    public async Task<bool> ReversarResolucion(int resolucionId, int usuarioId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var resolucion = await _context.Resolucion
                .Include(r => r.Carteras)
                .FirstOrDefaultAsync(r => r.Id == resolucionId);

            if (resolucion == null)
            {
                _logger.LogWarning("No se encontró la resolución con ID {Id} para ser reversada.", resolucionId);
                return false;
            }

            if (resolucion.Estado == EstadoResolucion.Revocada || resolucion.Estado == EstadoResolucion.Anulada)
            {
                _logger.LogWarning("La resolución N° {Numero} ya se encuentra inactiva o anulada.", resolucion.NumeroResolucion);
                return false;
            }

            // 3. Desvincular las carteras afectadas y restaurar su trazabilidad
            foreach (var cartera in resolucion.Carteras.ToList())
            {
                cartera.UsuarioModifico = usuarioId;
                cartera.FechaModificacion = DateTime.UtcNow;
                resolucion.Carteras.Remove(cartera);
            }
            resolucion.Estado = EstadoResolucion.Revocada;
            resolucion.UsuarioModifico = usuarioId;
            resolucion.FechaModificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Resolución N° {Numero} (ID: {Id}) reversada exitosamente.", resolucion.NumeroResolucion, resolucion.Id);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al intentar reversar la resolución ID {Id}.", resolucionId);
            return false;
        }
    }
}