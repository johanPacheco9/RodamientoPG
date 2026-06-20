using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models;

[Table("Intereses")] // Convención en plural para la tabla física
public class Interes
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal Porcentaje { get; set; }

    [Required(ErrorMessage = "La fecha inicial del interés es obligatoria.")]
    public DateTime Desde { get; set; }

    [Required(ErrorMessage = "La fecha final del interés es obligatoria.")]
    public DateTime Hasta { get; set; }
}