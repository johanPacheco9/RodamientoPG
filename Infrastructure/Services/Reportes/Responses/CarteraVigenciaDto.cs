namespace Infrastructure.Services.Reportes.Responses;

public class CarteraVigenciaDto
{
    public int Vigencia { get; set; }
    public decimal ValorRodamiento { get; set; }
    public decimal ValorCarga { get; set; }
    public decimal ValorEstampillas { get; set; }
    public decimal ValorCostas { get; set; }
    public decimal ValorInteres { get; set; }
    public decimal ValorTotal => ValorRodamiento + ValorCarga + ValorEstampillas + ValorCostas + ValorInteres;
}