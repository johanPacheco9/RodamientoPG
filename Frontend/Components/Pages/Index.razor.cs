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

    protected override async Task OnInitializedAsync() => await CargarDatos();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Ejecutamos cada vez que la lista contenga datos reales cargados de la base de datos
        if (Itemlist.Any())
            await RenderizarGraficoDona();
    }

    private async Task CargarDatos()
    {
        try
        {
            Itemlist = await ParametroService.ObtenerCantidadVehiculosPorClaseYServicio();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar estadísticas del index: {ex.Message}");
        }
    }

    private async Task RenderizarGraficoDona()
    {
        // 1. Agrupamos los datos por clase de vehículo para el gráfico circular
        var datosAgrupados = Itemlist
            .GroupBy(x => x.TipoVehiculo)
            .Select(g => new
            {
                Clase = g.Key,
                Total = g.Sum(x => x.CantidadVehiculos)
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        var labels = datosAgrupados.Select(x => x.Clase).ToArray();
        var data = datosAgrupados.Select(x => x.Total).ToArray();

        // Paleta de colores profesionales a juego con la UI
        var coloresFondo = new[] { "#185FA5", "#639922", "#BA7517", "#A32D2D", "#534AB7", "#0F6E56" };

        // Mandamos la configuración limpia de tipo doughnut a Chart.js
        await JS.InvokeVoidAsync("renderChart", "vehiculosChart", new
        {
            type = "doughnut",
            data = new 
            { 
                labels = labels, 
                datasets = new[] 
                {
                    new 
                    {
                        data = data,
                        backgroundColor = coloresFondo.Take(labels.Length).ToArray(),
                        borderWidth = 2,
                        borderColor = "#ffffff"
                    }
                } 
            },
            options = new
            {
                responsive = true,
                maintainAspectRatio = false,
                cutout = "65%", // El tamaño del agujero interno de la dona para que se vea moderno
                plugins = new
                {
                    legend = new 
                    { 
                        position = "right", // Leyendas a la derecha para ganar espacio vertical
                        labels = new { boxWidth = 12, font = new { size = 11 } }
                    },
                    tooltip = new { enabled = true }
                }
            }
        });
    }
}