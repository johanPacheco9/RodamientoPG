using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.Carteras.Enums;
using Domain.Models.Notificaciones;
using Domain.Models.Resoluciones;
using Domain.Models.Vehiculos;

namespace Domain.Models;

/// <summary>
/// Clase que representa la deuda de una persona. Tiene relacion con el vehiculo y avisos.
/// </summary>
public class Cartera : EntityWithTraceability
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "La placa es obligatoria.")]
    [StringLength(6, ErrorMessage = "La placa en Colombia no supera los 6 caracteres.")]
    public string Placa { get; set; } = string.Empty;

    public int Vigencia { get; set; }

    public TipoConceptoCartera Concepto { get; set; }

    public bool IsPagado { get; set; }
    
    public bool IsAnulled { get; set; }
    
    [Required]
    public int VehiculoId { get; set; }

    public bool TieneInteres { get; set; }
    
    public string Tipo { get; set; } = string.Empty;

    // =========================================================
    // 💰 VALORES MONETARIOS
    // =========================================================

    [Column(TypeName = "decimal(18,2)")] 
    public decimal Valor { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Descuento { get; set; }

    [Column(TypeName = "decimal(18,2)")] 
    public decimal ValorInteres { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorTotal { get; set; }

    // =========================================================
    //  RELACIONES
    // =========================================================
    
    public int? ResolucionId { get; set; }

    [ForeignKey(nameof(ResolucionId))]
    public virtual Resolucion? Resolucion { get; set; }
    
    [ForeignKey(nameof(VehiculoId))]
    public virtual Vehiculo Vehiculo { get; set; } = null!;
}