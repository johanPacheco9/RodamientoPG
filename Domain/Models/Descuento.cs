using System.ComponentModel.DataAnnotations;

namespace Domain.Models;

public class Descuento
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "La fecha y hora inicial es obligatoria.")]
    public DateTime Desde { get; set; }

    [Required(ErrorMessage = "La fecha y hora final es obligatoria.")]
    public DateTime Hasta { get; set; }

    public decimal Porcentaje { get; set; } 
}