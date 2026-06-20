using System.ComponentModel.DataAnnotations;
namespace Domain.Models;
/// <summary>
/// Antiguo RCON
/// </summary>
public class Concepto
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del concepto es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;

    public decimal Valor { get; set; }

    public decimal ValorIntereses { get; set; }

    public decimal Descuento { get; set; }

    public decimal Total { get; set; }
}