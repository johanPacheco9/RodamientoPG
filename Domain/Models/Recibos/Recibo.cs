using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.Vehiculos;
using Domain.Responses.Recibo.Enums;
namespace Domain.Models.Recibos;

public class Recibo
{
    [Key]
    public int Id { get; set; }

    public EstadoRecibo Estado { get; set; } = EstadoRecibo.Pendiente;
    
    public DateTime Fecha { get; set; }
    
    public DateTime? FechaAplica { get; set; }
    
    public DateTime? FechaProceso { get; set; }
    
    public DateTime? FechaPago { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorCapital { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal InteresMora { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Descuento { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Estampillas { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorTotalSistema { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorCargaDatos { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorRodamiento { get; set; }

    [NotMapped]
    public decimal ValorTotalCalculado =>
        (ValorCapital + InteresMora + Estampillas + ValorCargaDatos + ValorRodamiento) - Descuento;

    // Vigencias individuales pagadas en este recibo
    public virtual ICollection<ReciboDetalle> Detalles { get; set; } = [];
    

    public int VehiculoId { get; set; }
    public virtual Vehiculo Vehiculo { get; set; } = null!;
}