using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.ProcesoLiquidacion;
using Domain.Models.Vehiculos;
using Domain.Responses.Resolucion.Enums;
namespace Domain.Models.Resoluciones;

public class Resolucion : EntityWithTraceability
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El número de resolución es obligatorio.")]
    [StringLength(50)]
    public string NumeroResolucion { get; set; } = string.Empty; 

    public DateTime Fecha { get; set; }

    public DateTime? FechaProceso { get; set; }

    public TipoResolucion TipoResolucion { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Valor { get; set; }

    [Required]
    public EstadoResolucion Estado { get; set; } = EstadoResolucion.Activa;

    public string? Observaciones { get; set; }
    
    public int VehiculoId { get; set; }

    public int UsuarioId { get; set; } 

    public int? ProcesoId { get; set; }
    
    // 🚀 La relación clave: Aquí es donde se guardarán explícitamente los años/carteras seleccionados
    public virtual ICollection<Cartera> Carteras { get; set; } = new List<Cartera>();
    
    public virtual Vehiculo Vehiculo { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;

    public virtual Proceso? Proceso { get; set; }
}