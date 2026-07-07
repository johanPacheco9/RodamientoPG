using Domain.Responses.Liquidacion;
namespace Frontend.Components.Pages.Consulta;

public class VigenciaForm
{
    public int Vigencia { get; set; }
    
    // Lista de conceptos detallados (IDs reales de la BD) para este año
    public List<ConceptoCarteraDto> Conceptos { get; set; } = [];
    
    // Suma dinámica de todo lo que compone este año específico
    public decimal TotalVigencia => Conceptos.Sum(c => c.ValorTotal);

    // El checkbox de la interfaz se amarra a esta propiedad
    public bool Seleccionado { get; set; }
}