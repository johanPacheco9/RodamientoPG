using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.ProcesoLiquidacion;

public class LiquidacionDetalle
{
    [Key]
    public int Id { get; set; }

    // ==========================================
    // Relación con Liquidación
    // ==========================================

    [Required]
    public int LiquidacionId { get; set; }

    [ForeignKey(nameof(LiquidacionId))]
    public virtual Liquidacion Liquidacion { get; set; } = null!;

    // ==========================================
    // Referencia opcional a la cartera original
    // ==========================================

    public int? CarteraId { get; set; }

    [ForeignKey(nameof(CarteraId))]
    public virtual Cartera? Cartera { get; set; }

    // ==========================================
    // Snapshot de la deuda
    // ==========================================

    public int Vigencia { get; set; }

    [Required]
    [StringLength(50)]
    public string Concepto { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Valor { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorInteres { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Descuento { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorTotal { get; set; }
}