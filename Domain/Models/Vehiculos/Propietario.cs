using System.ComponentModel.DataAnnotations;
using Domain.Responses.Users.Enums;

namespace Domain.Models.Vehiculos;

public class Propietario
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El documento es obligatorio.")]
    public string Documento { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;

    public string Direccion { get; set; } = string.Empty;

    public string Telefono { get; set; } = string.Empty;
    
    public string? Correo { get; set; }
    
    public TipoDocumento TipoDocumento { get; set; }

    // =========================================================
    // 🚗 PROPIEDAD DE NAVEGACIÓN: Un propietario puede tener muchos vehículos
    // =========================================================
    public virtual ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}