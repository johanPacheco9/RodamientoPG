namespace Rodamiento.Shared.Components.Pages.PConsulta;

public class ConceptoResumenDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal ValorIntereses { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
}