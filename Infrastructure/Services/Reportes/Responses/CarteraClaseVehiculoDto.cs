namespace Infrastructure.Services.Reportes.Responses;

public class CarteraClaseVehiculoDto
{
    public int TipoVehiculoId { get; set; }
    public string ClaseVehiculo { get; set; } = string.Empty;
    public decimal ValorRodamiento { get; set; }
    public decimal ValorCarga { get; set; }
    public decimal ValorEstampillas { get; set; }
    public decimal ValorCostas { get; set; }
    public decimal ValorInteres { get; set; }
    
    public decimal ValorTotal => ValorRodamiento + ValorCarga + ValorEstampillas + ValorCostas + ValorInteres;
}