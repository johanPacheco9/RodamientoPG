using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.Notificaciones;
using Domain.Models.Vehiculos;
namespace Domain.Models.ProcesoLiquidacion;
/// <summary>
/// Antiguo Coactivo
/// </summary>
public class Proceso
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string? Resolucion { get; set; }

    public DateTime Fecha { get; set; }

    [Column(TypeName = "decimal(18,2)")] 
    public decimal Valor { get; set; }

    public DateTime FechaMandamiento { get; set; }
    public int? NumeroProceso { get; set; }
    
    public DateTime FechaProceso { get; set; }

    public int? Desde { get; set; }
    public int? Hasta { get; set; }

    [StringLength(50)]
    public string? ResolucionSancion { get; set; }

    public int VehiculoId { get; set; }
    public virtual Vehiculo Vehiculo { get; set; } = null!;

    public  EstadoProceso EstadoProceso { get; set; }

    // =========================================================
    // 📂 COLECCIÓN NATIVA: Lo que este proceso agrupó y congeló
    // =========================================================
    public virtual ICollection<Liquidacion> Liquidaciones { get; set; } = new List<Liquidacion>();
    
    public virtual ICollection<Aviso> Avisos { get; set; } = new List<Aviso>();
    
    public virtual ICollection<HistorialEstadoProceso> Historial { get; set; } = new List<HistorialEstadoProceso>();
}