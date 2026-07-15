namespace Infrastructure.Services.Reportes.Responses;

public class DetalleReciboReporteDto
{
    public int ReciboId { get; set; }
    public string Placa { get; set; } = null!;
    public string PropietarioNombre { get; set; } = null!;
    public string Documento { get; set; } = null!;
    public DateTime? FechaPago { get; set; }
    public decimal ValorCapital { get; set; }
    public decimal InteresMora { get; set; }
    public decimal Descuento { get; set; }
    public decimal TotalPagado { get; set; }
}