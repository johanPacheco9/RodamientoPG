using System.ComponentModel.DataAnnotations;
using Domain.Models.Vehiculos.Enums;
namespace Domain.Models.Vehiculos;

public class TipoVehiculo
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El código de homologación RUNT es obligatorio.")]
    public int Codigo { get; set; } // El código oficial del RUNT (Ej: 10 para Moto, 42 para Volqueta)

    [Required(ErrorMessage = "El nombre de la clase de vehículo es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    public int ModalidadServicio { get; set; } // Modalidad de servicio para el RUNT

    public ClaseAgrupacionVehiculo Tipo { get; set;}

    // ====================================================================
    // 📈 UVT ASOCIADA: Nullable porque no todas las clases aplican para UVT
    // ====================================================================
    public decimal? Uvt { get; set; } 
}