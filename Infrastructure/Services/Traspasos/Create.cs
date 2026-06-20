using Domain.Models.Vehiculos;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.Traspasos;

public partial class TraspasoManager
{
    public async Task<(bool Success, string Message)> TraspasoVehiculoAsync(
        string placa, int nuevoPropietarioId, DateTime fechaTraspaso)
    {
        var vehiculo = await context.Vehiculos
            .Include(v => v.Propietario)
            .FirstOrDefaultAsync(v => v.Placa == placa);

        if (vehiculo == null)
            return (false, "Vehículo no encontrado.");

        var tieneDeuda = await context.Cartera
            .AnyAsync(c => c.Placa == placa && !c.IsPagado);

        // Cerrar registro del propietario anterior
        var historialActual = await context.HistorialPropietarios
            .FirstOrDefaultAsync(h => h.VehiculoId == vehiculo.Id && h.FechaFin == null);

        if (historialActual != null)
            historialActual.FechaFin = DateTime.UtcNow;

        context.HistorialPropietarios.Add(new HistorialPropietario
        {
            VehiculoId = vehiculo.Id,
            PropietarioId = nuevoPropietarioId,
            FechaInicio = DateTime.UtcNow
        });

        // Actualizar propietario actual en el vehículo
        vehiculo.PropietarioId = nuevoPropietarioId;

        await context.SaveChangesAsync();

        var mensaje = tieneDeuda
            ? "Traspaso registrado. ⚠️ El vehículo tiene deuda pendiente que asume el nuevo propietario."
            : "Traspaso registrado exitosamente.";

        return (true, mensaje);
    }
}