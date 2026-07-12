using Domain.Responses.Carteras;
using Microsoft.AspNetCore.Components;
namespace Frontend.Components.Pages.Cartera;

public partial class Dashboard : ComponentBase
{
    private List<DeudorDto> _deudores = [];
    private FiltroCarteraDto _filtro = new();
    private int _pagina = 1;
    private int _porPagina = 20;
    private int _totalDeudores;
    private int _totalPaginas;
    private int _conProceso;
    private decimal _totalCartera;

    protected async override Task OnInitializedAsync() => await Buscar();
    private IEnumerable<int> ObtenerRangoPaginas()
    {
        int maxBotones = 5;
        int inicio = Math.Max(1, _pagina - (maxBotones / 2));
        int fin = Math.Min(_totalPaginas, inicio + maxBotones - 1);

        // Ajuste en caso de estar cerca del final del catálogo
        if (fin - inicio + 1 < maxBotones)
        {
            inicio = Math.Max(1, fin - maxBotones + 1);
        }

        for (int i = inicio; i <= fin; i++)
        {
            yield return i;
        }
    }
    private async Task Buscar()
    {
        _pagina = 1;
        await CargarDatos();
    }

    private async Task Limpiar()
    {
        _filtro = new();
        await Buscar();
    }

    private async Task CambiarPagina(int p)
    {
        _pagina = p;
        await CargarDatos();
    }

    private async Task CargarDatos()
    {
        var resultado = await CarteraService.GetDeudores(_filtro, _pagina, _porPagina);
        _deudores       = resultado.Items;
        _totalDeudores  = resultado.Total;
        _totalPaginas   = (int)Math.Ceiling((double)_totalDeudores / _porPagina);
        _totalCartera   = resultado.TotalCartera;
        _conProceso     = resultado.ConProceso;
        StateHasChanged();
    }

    private void VerDetalle(string placa) =>
        Nav.NavigateTo($"/Consulta/{placa}");

    private static string BadgeVigencias(int v) => v >= 5
        ? "badge bg-danger"
        : v >= 3 ? "badge bg-warning text-dark"
            : "badge bg-secondary";

    private static string BadgeProceso(string? proceso) => proceso?.ToLower() switch
    {
        "coactivo"   => "badge bg-danger",
        "persuasivo" => "badge bg-warning text-dark",
        _            => "badge bg-secondary"
    };
}
