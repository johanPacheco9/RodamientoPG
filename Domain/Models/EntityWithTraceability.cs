using System.ComponentModel.DataAnnotations.Schema;
namespace Domain.Models;

public abstract class EntityWithTraceability
{
    public int CreatedBy { get; set; }

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public int? UpdatedBy { get; set; }
    
    public DateTime? FechaModificacion { get; set; }
}