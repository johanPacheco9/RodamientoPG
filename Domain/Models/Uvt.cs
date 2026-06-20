using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models;

public class Uvt
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("indesde")]
    public DateTime FechaDesde { get; set; }

    [Column("inhasta")]
    public DateTime FechaHasta { get; set; }

    // =========================================================
    // 💰 PROTECCIÓN FINANCIERA: De float a decimal
    // =========================================================
    [Column("valor", TypeName = "decimal(18,2)")]
    public decimal Valor { get; set; }
}