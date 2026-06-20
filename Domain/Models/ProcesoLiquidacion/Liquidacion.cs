using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.Vehiculos;

namespace Domain.Models.ProcesoLiquidacion;

public class Liquidacion
{
    [Key]
    public int Id { get; set; }

    public int VehiculoId { get; set; }

    [ForeignKey(nameof(VehiculoId))]
    public virtual Vehiculo Vehiculo { get; set; } = null!;

    public DateTime FechaLiquidacion { get; set; }

    public int VigenciaDesde { get; set; }

    public int VigenciaHasta { get; set; }

    public int? UltimoPagoVigencia { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalDeuda { get; set; }

    public int? ProcesoId { get; set; }

    [ForeignKey(nameof(ProcesoId))]
    public virtual Proceso? Proceso { get; set; }

    public virtual ICollection<LiquidacionDetalle> Detalles { get; set; }
        = new List<LiquidacionDetalle>();
}