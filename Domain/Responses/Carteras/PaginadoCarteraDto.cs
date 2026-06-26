namespace Domain.Responses.Carteras;

public class DeudorDto
{
    public string TipoDocumento     { get; set; } = "";
    public string Documento         { get; set; } = "";
    public string NombrePropietario { get; set; } = "";
    public string Placa             { get; set; } = "";
    public int    VigenciasPendientes { get; set; }
    public decimal TotalDeuda       { get; set; }
    public string? Proceso          { get; set; }
}

public class FiltroCarteraDto
{
    public string?  Busqueda      { get; set; }
    public int?     VigenciaDesde { get; set; }
    public decimal? DeudaMinima   { get; set; }
    public string?  Proceso       { get; set; }
}

public class PaginadoCarteraDto
{
    public List<DeudorDto> Items { get; set; } = [];
    public int     Total         { get; set; }
    public decimal TotalCartera  { get; set; }
    public int     ConProceso    { get; set; }
}