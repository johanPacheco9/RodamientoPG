using Domain.Responses.Vehiculos;
using Domain.Responses.Vehiculos.Enums;
using Domain.Generics;
using Infrastructure.Services.Parametros;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Frontend.Components.Pages;

public partial class Index : ComponentBase
{
    [Inject] private ParametroService ParametroService { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;

    private List<EstadisticaVehiculoDto> Itemlist { get; set; } = [];

    private int    TotalVehiculos => Itemlist.Sum(x => x.CantidadVehiculos);
    private int    ClasesDistintas => Itemlist.Select(x => x.TipoVehiculo).Distinct().Count();
    private int    TiposServicio  => Itemlist.Select(x => x.TipoServicio).Distinct().Count();
    private string ClaseMasComun  => Itemlist
        .GroupBy(x => x.TipoVehiculo)
        .OrderByDescending(g => g.Sum(x => x.CantidadVehiculos))
        .FirstOrDefault()?.Key ?? "—";

    private List<string> Clases => Itemlist
        .Select(x => x.TipoVehiculo)
        .Distinct()
        .OrderBy(x => x)
        .ToList();

    private List<TipoServicioVehiculo> Servicios => Itemlist
        .Select(x => x.TipoServicio)
        .Distinct()
        .Where(s => (int)s != 0)
        .ToList();

    private readonly string[] _paleta = ["#185FA5", "#639922", "#BA7517", "#A32D2D", "#534AB7", "#0F6E56"];

    protected override async Task OnInitializedAsync() => await CargarDatos();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && Itemlist.Any())
            await RenderizarGrafico();
    }

    private async Task CargarDatos()
    {
        try
        {
            Itemlist = await ParametroService.ObtenerCantidadVehiculosPorClaseYServicio();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private async Task RenderizarGrafico()
    {
        var clases    = Clases;
        var servicios = Servicios;

        var datasets = servicios.Select((s, i) => new
        {
            label           = s.GetDisplayName(),  // ← enum → string legible
            data            = clases.Select(c =>
                Itemlist.FirstOrDefault(x => x.TipoVehiculo == c && x.TipoServicio == s)?.CantidadVehiculos ?? 0
            ).ToArray(),
            backgroundColor = _paleta[i % _paleta.Length],
            borderRadius    = 4,
            borderSkipped   = false
        }).ToArray();

        await JS.InvokeVoidAsync("renderChart", "vehiculosChart", new
        {
            type = "bar",
            data = new { labels = clases, datasets },
            options = new
            {
                indexAxis           = "y",
                responsive          = true,
                maintainAspectRatio = false,
                plugins = new
                {
                    legend  = new { position = "top" },
                    tooltip = new { mode = "index", intersect = false }
                },
                scales = new
                {
                    x = new { stacked = true, grid = new { color = "#f0f0f0" }, ticks = new { font = new { size = 11 } } },
                    y = new { stacked = true, grid = new { display = false },   ticks = new { font = new { size = 12 } } }
                }
            }
        });
    }
}