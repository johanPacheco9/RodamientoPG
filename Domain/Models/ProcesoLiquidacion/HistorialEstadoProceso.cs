using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Domain.Models.ProcesoLiquidacion;

public class HistorialEstadoProceso : EntityWithTraceability
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ProcesoId { get; set; }

    [ForeignKey(nameof(ProcesoId))]
    public virtual Proceso Proceso { get; set; } = null!;

    public EstadoProceso EstadoAnterior { get; set; }
    
    public EstadoProceso EstadoNuevo { get; set; }

    public bool EsAutomatico { get; set; } // true = disparado por 4 avisos completos, false = manual

    [StringLength(200)]
    public string? Motivo { get; set; } // justificación, sobre todo relevante en el caso manual
}