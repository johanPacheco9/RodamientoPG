using System.ComponentModel.DataAnnotations;
namespace Domain.Models.Vehiculos;

public class Color
{
    [Required]
    [Key]
    public int Id { get; set; }

    public int Codigo { get; set; }

    public string Nombre { get; set; } = null!;
}