using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.Vehiculos;
using Domain.Responses.Recibo.Enums;

namespace Domain.Models;

[Table("Recibos")]
public class Recibo
{
    [Key]
    public int Id { get; set; }

    [Required]
    public EstadoRecibo Estado { get; set; } = EstadoRecibo.Pendiente;

    public DateTime Fecha { get; set; }

    public DateTime? FechaAplica { get; set; }

    public DateTime? FechaProceso { get; set; }

    public DateTime? FechaPago { get; set; }

    public int Desde { get; set; }

    public int Hasta { get; set; }

    // ====================================================================
    // 💰 BLOQUE FINANCIERO DESCIFRADO Y REAL
    // ====================================================================
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorCapital { get; set; } 

    [Column(TypeName = "decimal(18,2)")]
    public decimal InteresMora { get; set; } 

    [Column(TypeName = "decimal(18,2)")]
    public decimal Descuento { get; set; } 

    [Column(TypeName = "decimal(18,2)")]
    public decimal Estampillas { get; set; } 

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorTotalSistema { get; set; } // Lo que el sistema viejo calculaba y guardaba en 'vlr_sistem'

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorCargaDatos { get; set; } 

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorRodamiento { get; set; } 

    // ====================================================================
    // 🧮 PROPIEDAD CALCULADA EN C# (No persiste en la Base de Datos)
    // ====================================================================
    /// <summary>
    /// Devuelve de forma dinámica la sumatoria real y matemática de todos los conceptos del recibo aplicando el descuento.
    /// Al no tener un setter, Entity Framework Core no intentará mapearla como columna en Postgres.
    /// </summary>
    [NotMapped] // Opcional: Refuerza a EF Core que ignore por completo esta propiedad
    public decimal ValorTotalCalculado => (ValorCapital + InteresMora + Estampillas + ValorCargaDatos + ValorRodamiento) - Descuento;


    // ====================================================================
    // 🔗 CLAVE FORÁNEA (Normalización Absoluta)
    // ====================================================================
    public int VehiculoId { get; set; }
    
    public virtual Vehiculo Vehiculo { get; set; } = null!;
}