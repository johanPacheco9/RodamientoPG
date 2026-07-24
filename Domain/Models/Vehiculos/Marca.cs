using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Domain.Models.Vehiculos;

public class Marca
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Nombre { get; set; } = null!;
}