namespace Frontend.Components;

public class BusquedaService
{
    private string _ultimaPlaca = string.Empty;

    // 💡 ESTA ES LA LÍNEA CRÍTICA: Define el evento que Consulta.razor.cs está buscando
    public event Action<string>? OnComparendoCapturado;

    public async Task ComparendoCapturado(string placa)
    {
        _ultimaPlaca = placa;
        
        // Notifica de forma reactiva al componente padre (Consulta) en memoria
        OnComparendoCapturado?.Invoke(placa); 
    }

    public string ObtenerUltimoComparendoCapturado()
    {
        return _ultimaPlaca;
    }
}