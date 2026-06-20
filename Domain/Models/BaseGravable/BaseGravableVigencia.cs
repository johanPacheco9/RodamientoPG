namespace Domain.Models.BaseGravable;

public class BaseGravableVigencia
{
    public int Id { get; set; }
    public int BaseGravableVehiculoId { get; set; } // Clave foránea hacia el vehículo base
    public int AnioVigencia { get; set; }           // Ej: 1999, 2024, 2025, 2026... hasta el año que quieras
    public decimal ValorComercial { get; set; }     // El avalúo/base gravable asignado para ese año

    // Propiedad de navegación inversa
    public virtual BaseGravableVehiculo BaseGravableVehiculo { get; set; } = null!;
}