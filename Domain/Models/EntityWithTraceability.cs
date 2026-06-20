using System.ComponentModel.DataAnnotations.Schema;
namespace Domain.Models;

public abstract class EntityWithTraceability
{
    [Column("usuario_creo")]
    public int UsuarioCreo { get; set; }

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    [Column("usuario_modifico")]
    public int? UsuarioModifico { get; set; }

    [Column("fecha_modificacion")]
    public DateTime? FechaModificacion { get; set; }
}