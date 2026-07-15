namespace Infrastructure.Services.Reportes.Responses;
public class ReporteDiarioDto
{
    public DateTime FechaReporte { get; set; }
    
    // Métricas de Control
    public int CantidadRecibos { get; set; }
    public decimal TotalRecaudado { get; set; }
    public decimal TotalCapital { get; set; }
    public decimal TotalIntereses { get; set; }
    public decimal TotalDescuentos { get; set; }

    // Desglose por Conceptos de Tránsito
    public decimal TotalRodamiento { get; set; }
    public decimal TotalEstampillas { get; set; }
    public decimal TotalCargaDatos { get; set; }

    // Listado detallado de transacciones del día
    public List<DetalleReciboReporteDto> Transacciones { get; set; } = new();
}
