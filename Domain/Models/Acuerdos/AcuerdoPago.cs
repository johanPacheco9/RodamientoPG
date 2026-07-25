using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.Acuerdos.Enums;
using Domain.Models.ProcesoLiquidacion;
using Domain.Models.Vehiculos;
namespace Domain.Models.Acuerdos;

public class AcuerdosDePago : EntityWithTraceability
{
 
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string NumeroAcuerdo { get; set; } = null!; // Ej: "AP-2026-00012"

    //Fecha se firma el acuerdo.
    public DateTime FechaSuscripcion { get; set; }

    public int NumeroCuotas { get; set; }

    // --- Totales liquidados al momento del acuerdo ---
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorCapitalFinanciado { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorInteresFinanciado { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorCuotaInicial { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorTotalFinanciado { get; set; }

    
    public EstadoAcuerdoPago Estado { get; set; } = EstadoAcuerdoPago.Vigente;

    [StringLength(500)]
    public string? Observaciones { get; set; }

    // --- Relaciones ---
    public int VehiculoId { get; set; }
    public virtual Vehiculo Vehiculo { get; set; } = null!;

    // Si el acuerdo nació dentro de un Proceso Coactivo existente (Opcional/Nullable)
    public int? ProcesoId { get; set; }
    public virtual Proceso? Proceso { get; set; }

    // Colección de cuotas
    public virtual ICollection<CuotaAcuerdoPago> Cuotas { get; set; } = new List<CuotaAcuerdoPago>();
}