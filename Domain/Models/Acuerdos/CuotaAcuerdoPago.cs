using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.Acuerdos.Enums;
namespace Domain.Models.Acuerdos;

public class CuotaAcuerdoPago : EntityWithTraceability
{
    [Key]
    public int Id { get; set; }

    public int NumeroCuota { get; set; } // 0 = Cuota Inicial, 1 = Cuota 1, 2 = Cuota 2...

    public DateTime FechaVencimiento { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorCapital { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorInteres { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorTotalCuota { get; set; }

    public EstadoCuotaAcuerdo Estado { get; set; } = EstadoCuotaAcuerdo.Pendiente;

    // --- Datos del Recaudo / Pago efectuado ---
    public DateTime? FechaPago { get; set; }

    [StringLength(50)]
    public string? NumeroRecibo { get; set; }

    // --- Relación Padre ---
    public int AcuerdoPagoId { get; set; }
    
    public virtual AcuerdosDePago AcuerdoPago { get; set; } = null!;
}