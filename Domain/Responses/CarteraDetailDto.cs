namespace Rodamiento.Shared.Components.Pages.PConsulta;

public class CarteraDetailDto
{
    public int Vigencia { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal ValorInteres { get; set; }
    public decimal Descuento { get; set; }
    public decimal ValorTotal { get; set; }
    public bool Seleccionado { get; set; } = true;
}