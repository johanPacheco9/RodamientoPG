namespace Domain.Responses.Reportes;

public class ReporteDiarioDto
{
    public string NumeroRecibo { get; set; } = null!;
    public string Placa { get; set; } = null!;
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaPago { get; set; }
    public int AnioDesde { get; set; }
    public int AnioHasta { get; set; }

    public decimal ValorTotal { get; set; }
    public decimal ValorImpuesto { get; set; }
    public decimal ValorCarga { get; set; }
    public decimal ValorCostas { get; set; }
    public decimal ValorSistematizacion { get; set; }
    public decimal ValorInteres { get; set; }
    public decimal ValorEstampillas { get; set; }
    public decimal ValorSancion { get; set; }
    public decimal Descuento { get; set; }
    public string NombrePropietario { get; set; }
}