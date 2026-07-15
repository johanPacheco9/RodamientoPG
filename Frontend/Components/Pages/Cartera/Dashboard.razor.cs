using Domain.Responses.Carteras;
using Infrastructure.Services.Carteras; // Asegúrate de que este namespace apunte a tu servicio
using Microsoft.AspNetCore.Components;

namespace Frontend.Components.Pages.Cartera;

public partial class Dashboard : ComponentBase
{
    // ── Inyecciones de Dependencias ──────────────────────────────────
    [Inject] private CarteraService CarteraService { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;

    // ── Variables de Estado ──────────────────────────────────────────
    private List<DeudorDto> _deudores = [];
    private FiltroCarteraDto _filtro = new();
    private int _pagina = 1;
    private int _porPagina = 20;
    
    private int _totalDeudores;
    private int _totalPaginas;
    private int _conProceso;
    private decimal _totalCartera;
    private bool _cargando;

    // ── Ciclo de Vida y Flujo ────────────────────────────────────────
    protected override async Task OnInitializedAsync() 
    {
        await Buscar();
    }

    private async Task Buscar()
    {
        _pagina = 1; // Siempre resetea a la primera página al filtrar
        await CargarDatos();
    }

    private async Task Limpiar()
    {
        _filtro = new FiltroCarteraDto();
        await Buscar();
    }

    private async Task CambiarPagina(int p)
    {
        // Validación para evitar peticiones redundantes o fuera de rango
        if (p < 1 || p > _totalPaginas || p == _pagina || _cargando) return;

        _pagina = p;
        await CargarDatos();
    }

    private async Task CargarDatos()
    {
        if (_cargando) return;

        _cargando = true;
        StateHasChanged(); // Renderiza el estado de carga (ej. spinners, deshabilitar botones)

        try
        {
            var resultado = await CarteraService.GetDeudores(_filtro, _pagina, _porPagina);
            
            _deudores       = resultado.Items;
            _totalDeudores  = resultado.Total;
            _totalPaginas   = (int)Math.Ceiling((double)_totalDeudores / _porPagina);
            _totalCartera   = resultado.TotalCartera;
            _conProceso     = resultado.ConProceso;
        }
        catch (Exception ex)
        {
            // Aquí puedes registrar el error usando ILogger si lo requieres
            Console.WriteLine($"Error al cargar la cartera: {ex.Message}");
        }
        finally
        {
            _cargando = false;
            StateHasChanged(); // Refresca la UI con los nuevos resultados
        }
    }

    // ── Utilidades de UI ─────────────────────────────────────────────
    private void VerDetalle(string placa) =>
        Nav.NavigateTo($"/Consulta/{placa}");

    private IEnumerable<int> ObtenerRangoPaginas()
    {
        if (_totalPaginas <= 1) yield break;

        const int maxBotones = 5;
        int inicio = Math.Max(1, _pagina - (maxBotones / 2));
        int fin = Math.Min(_totalPaginas, inicio + maxBotones - 1);

        // Ajuste dinámico en caso de estar cerca del final del catálogo
        if (fin - inicio + 1 < maxBotones)
        {
            inicio = Math.Max(1, fin - maxBotones + 1);
        }

        for (int i = inicio; i <= fin; i++)
        {
            yield return i;
        }
    }

    private static string BadgeVigencias(int v) => v switch
    {
        >= 5 => "badge bg-danger",
        >= 3 => "badge bg-warning text-dark",
        _    => "badge bg-secondary"
    };

    private static string BadgeProceso(string? proceso) => proceso?.ToLower() switch
    {
        "coactivo"   => "badge bg-danger",
        "persuasivo" => "badge bg-warning text-dark",
        _            => "badge bg-secondary"
    };
}