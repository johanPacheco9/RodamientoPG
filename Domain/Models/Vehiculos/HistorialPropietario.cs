namespace Domain.Models.Vehiculos;

public class HistorialPropietario
{
    public int Id { get; set; }
    public int VehiculoId { get; set; }
    public int PropietarioId { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public Vehiculo Vehiculo { get; set; } = null!;
    public Propietario Propietario { get; set; } = null!;
}