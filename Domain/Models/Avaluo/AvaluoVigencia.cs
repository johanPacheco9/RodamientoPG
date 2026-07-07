using System.ComponentModel.DataAnnotations.Schema;
namespace Domain.Models.Avaluo;

public class AvaluoVigencia
{
    public int Id { get; set; }
    public int AvaluoVehiculoId { get; set; }   
    public int AnioVigencia { get; set; }       
    public decimal ValorComercial { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorUvtVigencia { get; set; }
    
    public virtual AvaluoVehiculo AvaluoVehiculo { get; set; } = null!;
}