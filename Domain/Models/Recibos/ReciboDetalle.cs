using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.Carteras.Enums;

namespace Domain.Models.Recibos;

public class ReciboDetalle
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ReciboId { get; set; }
    
    [ForeignKey(nameof(ReciboId))]
    public virtual Recibo Recibo { get; set; } = null!;

    // 🔗 Vínculo clave a la cartera seleccionada
    [Required]
    public int CarteraId { get; set; }
    
    [ForeignKey(nameof(CarteraId))]
    public virtual Cartera Cartera { get; set; } = null!;

    // Copia histórica de los valores en el momento de generar el recibo
    public int Vigencia { get; set; }
    public TipoConceptoCartera Concepto { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Valor { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorInteres { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Descuento { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorTotal { get; set; }
}