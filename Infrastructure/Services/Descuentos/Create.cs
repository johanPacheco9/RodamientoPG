using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Descuentos;

public partial class DescuentosManager
{
    /// <summary>
    /// Crea un nuevo descuento validando fechas, porcentaje y solapamiento con otros descuentos existentes.
    /// </summary>
    /// <param name="desde">Fecha y hora inicial de vigencia del descuento.</param>
    /// <param name="hasta">Fecha y hora final de vigencia del descuento.</param>
    /// <param name="porcentaje">Porcentaje de descuento a aplicar (0-100).</param>
    /// <returns>Tupla indicando éxito, mensaje descriptivo y el descuento creado (si aplica).</returns>
    public async Task<(bool Exito, string Mensaje, Descuento? Descuento)> Create(DateTime desde, DateTime hasta, decimal porcentaje)
    {
        // Normalizar a UTC apenas entran, antes de cualquier uso (validación, consulta o inserción)
        desde = DateTime.SpecifyKind(desde, DateTimeKind.Utc);
        hasta = DateTime.SpecifyKind(hasta, DateTimeKind.Utc);

        // Validación: rango de fechas coherente
        if (hasta <= desde)
        {
            _logger.LogWarning("Intento de creación de descuento con rango de fechas inválido: Desde={Desde}, Hasta={Hasta}", desde, hasta);

            return (false, "La fecha final debe ser posterior a la fecha inicial.", null);
        }

        // Validación: porcentaje dentro de un rango razonable
        if (porcentaje <= 0 || porcentaje > 100)
        {
            _logger.LogWarning("Intento de creación de descuento con porcentaje inválido: {Porcentaje}", porcentaje);

            return (false, "El porcentaje debe estar entre 0 y 100.", null);
        }

        // Validación: que no exista otro descuento activo que se solape en el mismo rango de fechas
        bool existeSolapamiento = await _context.Set<Descuento>()
            .AnyAsync(d => d.Desde < hasta && d.Hasta > desde);

        if (existeSolapamiento)
        {
            _logger.LogWarning("Intento de creación de descuento solapado con uno existente. Desde={Desde}, Hasta={Hasta}", desde, hasta);

            return (false, "Ya existe un descuento vigente que se solapa con el rango de fechas indicado.", null);
        }

        var descuento = new Descuento
        {
            Desde = desde,
            Hasta = hasta,
            Porcentaje = porcentaje
        };

        try
        {
            _context.Set<Descuento>().Add(descuento);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Descuento creado correctamente. Id={Id}, Desde={Desde}, Hasta={Hasta}, Porcentaje={Porcentaje}",
                descuento.Id, descuento.Desde, descuento.Hasta, descuento.Porcentaje);

            return (true, "Descuento creado correctamente.", descuento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar el descuento en la base de datos.");

            return (false, "Ocurrió un error al guardar el descuento.", null);
        }
    }
}