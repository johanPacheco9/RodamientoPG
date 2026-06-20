using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.ProcesoLiquidacion;
using Domain.Models.Vehiculos;
using Domain.Responses.Resolucion.Enums;

namespace Domain.Models;

public class Resolucion
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El número de resolución es obligatorio.")]
    [StringLength(50)]
    public string NumeroResolucion { get; set; } = string.Empty; // Tu columna 'resolucion' (Ej: "188", "004")

    public DateTime Fecha { get; set; }

    public DateTime? FechaProceso { get; set; }

    public int TipoResolucionId { get; set; } // Tu columna 'tipo' (Mapea a la naturaleza jurídica)

    public int PeriodoDesde { get; set; } // 'desde' (Año de cartera inicial que afecta)

    public int PeriodoHasta { get; set; } // 'hasta' (Año de cartera final que afecta)

    [Column(TypeName = "decimal(18,2)")]
    public decimal Valor { get; set; }

    [Required]
    public EstadoResolucion Estado { get; set; } = EstadoResolucion.Activa;

    public string? Observaciones { get; set; } // Tu columna 'obs1'

    // ====================================================================
    // 🔗 CLAVES FORÁNEAS (El corazón de la Arquitectura Limpia)
    // ====================================================================

    public int VehiculoId { get; set; }

    public int UsuarioId { get; set; } // El funcionario/abogado que proyectó la resolución

    public int? ProcesoId { get; set; } // Tu columna 'no_proc' (Conecta directo al cobro coactivo)

    // ====================================================================
    // PROPIEDADES DE NAVEGACIÓN (Entity Framework Core)
    // ====================================================================

    public virtual Vehiculo Vehiculo { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;

    public virtual Proceso? Proceso { get; set; }
}