using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.Notificaciones;
using Domain.Models.Vehiculos;

namespace Domain.Models;

/// <summary>
/// Clase que representa la deuda de una persona. Tiene relacion con el vehiculo y avisos.
/// </summary>
public class Cartera
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "La placa es obligatoria.")]
    [StringLength(6, ErrorMessage = "La placa en Colombia no supera los 6 caracteres.")]
    public string Placa { get; set; } = string.Empty;

    public int Vigencia { get; set; }

    public string Concepto { get; set; } = string.Empty;

    public bool IsPagado { get; set; }
    
    // 🔥 CORRECCIÓN: Este es el campo que DEBE ser la Clave Foránea porque coincide en tipo (int) con Vehiculo.Id
    [Required]
    public int VehiculoId { get; set; }

    public bool TieneInteres { get; set; }

    public bool EstaEnProcesoCoactivo { get; set; }

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
    [ForeignKey(nameof(VehiculoId))]
    public virtual Vehiculo Vehiculo { get; set; } = null!;
    
    public virtual ICollection<Aviso> Avisos { get; set; } = new List<Aviso>();
}