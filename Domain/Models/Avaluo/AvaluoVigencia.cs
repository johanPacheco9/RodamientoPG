namespace Domain.Models.Avaluo;

public class AvaluoVigencia
{
    public int Id { get; set; }
    public int AvaluoVehiculoId { get; set; }   
    public int AnioVigencia { get; set; }       
    public decimal ValorComercial { get; set; }

    public virtual AvaluoVehiculo AvaluoVehiculo { get; set; } = null!;
}