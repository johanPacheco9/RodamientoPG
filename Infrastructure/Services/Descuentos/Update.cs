using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Infrastructure.Services.Descuentos;

public partial class DescuentosManager
{
/// <summary>
    /// Actualiza un descuento existente, validando fechas, porcentaje y solapamiento con otros descuentos.
    /// </summary>
    public async Task<(bool Exito, string Mensaje, Descuento? Descuento)> Update(int id, DateTime desde, DateTime hasta, decimal porcentaje)
    {
        // Normalizar a UTC apenas entran, antes de cualquier uso
        desde = DateTime.SpecifyKind(desde, DateTimeKind.Utc);
        hasta = DateTime.SpecifyKind(hasta, DateTimeKind.Utc);

        var descuento = await _context.Set<Descuento>().FirstOrDefaultAsync(d => d.Id == id);

        if (descuento is null)
        {
            _logger.LogWarning("Intento de actualización de descuento inexistente. Id={Id}", id);
            return (false, "El descuento indicado no existe.", null);
        }

        // Validación: rango de fechas coherente
        if (hasta <= desde)
        {
            _logger.LogWarning("Intento de actualización de descuento con rango de fechas inválido. Id={Id}, Desde={Desde}, Hasta={Hasta}", id, desde, hasta);
            return (false, "La fecha final debe ser posterior a la fecha inicial.", null);
        }

        // Validación: porcentaje dentro de un rango razonable
        if (porcentaje <= 0 || porcentaje > 100)
        {
            _logger.LogWarning("Intento de actualización de descuento con porcentaje inválido. Id={Id}, Porcentaje={Porcentaje}", id, porcentaje);
            return (false, "El porcentaje debe estar entre 0 y 100.", null);
        }

        // Validación: que no exista OTRO descuento (Id distinto) que se solape con el nuevo rango
        bool existeSolapamiento = await _context.Set<Descuento>()
            .AnyAsync(d => d.Id != id && d.Desde < hasta && d.Hasta > desde);

        if (existeSolapamiento)
        {
            _logger.LogWarning("Intento de actualización de descuento solapado con uno existente. Id={Id}, Desde={Desde}, Hasta={Hasta}", id, desde, hasta);
            return (false, "Ya existe otro descuento vigente que se solapa con el rango de fechas indicado.", null);
        }

        try
        {
            descuento.Desde = desde;
            descuento.Hasta = hasta;
            descuento.Porcentaje = porcentaje;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Descuento actualizado correctamente. Id={Id}, Desde={Desde}, Hasta={Hasta}, Porcentaje={Porcentaje}",
                descuento.Id, descuento.Desde, descuento.Hasta, descuento.Porcentaje);

            return (true, "Descuento actualizado correctamente.", descuento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el descuento. Id={Id}", id);
            return (false, "Ocurrió un error al actualizar el descuento.", null);
        }
    }
}