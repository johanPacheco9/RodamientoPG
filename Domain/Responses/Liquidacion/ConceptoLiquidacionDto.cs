namespace Domain.Responses.Liquidacion;

public class ConceptoLiquidacionDto
{
    public int Vigencia { get; set; }
    public decimal ValorRodamiento { get; set; }
    public decimal ValorCarga { get; set; }
    public decimal ValorEstampillas { get; set; }
    public decimal ValorRecibo { get; set; }
    public decimal ValorInteres { get; set; }
    public decimal Descuento { get; set; }
    public decimal ValorSistema { get; set; }
    public decimal ValorTotal { get; set; }
}