using Domain.Models.ProcesoLiquidacion;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
namespace Frontend.Components.Pages.Coactivo;

public partial class Dashboard
{
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;
    public List<Liquidacion> Itemlist { get; set; } = new();
    public List<Proceso> Procesoslist { get; set; } = new();
    public List<Proceso> Tranlist { get; set; } = new();
    
    private Proceso? procesoSeleccionadoTemporal;
    
    public DateTime desde;
    public DateTime hasta;
    public int vig;
    public int tran;
    
    private bool mostrarModal = false;
    protected bool Istran { get; set; }
    protected int SelectedItem { get; set; }
    public bool bot = false;
    private bool tab1 = true;
    private bool tab2 = false;
    bool isLoading = false;

    // Indices independientes de paginación manual en memoria (Tamaño fijo a 8 items según tus parámetros de Radzen)
    private int PaginaTab1 { get; set; } = 1;
    private int PaginaTab2 { get; set; } = 1;
    private int PaginaModal { get; set; } = 1;
    private const int TamanoPagina = 8;

    protected override async Task OnInitializedAsync()
    {
        vig = 2025;
        desde = DateTime.UtcNow;
        hasta = DateTime.UtcNow;
    }

    private async Task Muestra_tran()
    {
        await ShowLoading();
        Tranlist = await CoactivoServices.List(tran) ?? new List<Proceso>();
        PaginaModal = 1;
        procesoSeleccionadoTemporal = null;
        MostrarModal();
        StateHasChanged();
    }

    private void SeleccionarFilaModal(Proceso item)
    {
        procesoSeleccionadoTemporal = item;
    }

    private async Task EnviarDatos()
    {
        if (procesoSeleccionadoTemporal != null)
        { 
            tran = (int)procesoSeleccionadoTemporal.NumeroProceso;
            mostrarModal = false;
            StateHasChanged();
        }
        else
        {
            await JsRuntime.InvokeVoidAsync("alert", "Por favor seleccione una transacción de la lista.");
        }
    }

    public void displayTabs(int TabNumber)
    {
        tab1 = (TabNumber == 1);
        tab2 = (TabNumber == 2);
    }

    private void CerrarModal() => mostrarModal = false;
    public void MostrarModal() => mostrarModal = true;

    private async Task Capturar(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out int value))
        {
            SelectedItem = value;
            Istran = (SelectedItem == 1);
        }
    }

    async Task ShowLoading()
    {
        isLoading = true;
        await Task.Yield();
        isLoading = false;
    }

    private async Task Meter_Coactivo()
    {
        bool confirmar = await JsRuntime.InvokeAsync<bool>("confirm", "¿Está seguro de pasar los registros seleccionados a Proceso Coactivo?");
            
        if (confirmar)
        {
            try
            {
                bot = true;
                StateHasChanged();
                await CoactivoServices.Procesar(SelectedItem, tran);
                await JsRuntime.InvokeVoidAsync("alert", "Cartera trasladada a Cobro Coactivo de manera exitosa.");
                await Carga_persuasivo();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error crítico en el paso a Coactivo: {ex.Message}");
            }
            finally
            {
                bot = false;
                StateHasChanged();
            }
        }
    }

    private async Task Carga_datos()
    {
        try
        {
            await ShowLoading();
            Itemlist = await CoactivoServices.SinProcesos(vig) ?? new List<Liquidacion>();
            PaginaTab1 = 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar liquidaciones pendientes vigencia {vig}: {ex.Message}");
        }
    }

    private async Task Carga_persuasivo()
    {
        try
        {
            await ShowLoading();
            Procesoslist = await PersuasivoService.List(SelectedItem, vig, "") ?? new List<Proceso>();
            PaginaTab2 = 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar procesos persuasivos: {ex.Message}");
        }
    }

    private async Task Procesar()
    {
        bool confirmar = await JsRuntime.InvokeAsync<bool>("confirm", "¿Desea aperturar el Proceso Persuasivo masivo para la vigencia seleccionada?");
            
        if (confirmar)
        {
            try
            {
                bot = true;
                StateHasChanged();
                await PersuasivoService.Procesar(vig);
                await JsRuntime.InvokeVoidAsync("alert", "Proceso persuasivo masivo generado de manera exitosa.");
                await Carga_datos();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en apertura masiva persuasiva: {ex.Message}");
            }
            finally
            {
                bot = false;
                StateHasChanged();
            }
        }
    }

    private async Task Obtenercompare()
    {
        try
        {
            string result = BusquedaService.ObtenerUltimoComparendoCapturado();
            Console.WriteLine($"Último registro capturado: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al capturar datos compartidos del busquedaService: {ex.Message}");
        }
    }

    // Métodos lógicos para la gestión de paginación interna en memoria
    private int CalcularTotalPaginas(int cantidadItems)
    {
        return (int)Math.Ceiling((double)cantidadItems / TamanoPagina);
    }

    private IEnumerable<T> ObtenerPaginado<T>(List<T> lista, int paginaActual)
    {
        return lista.Skip((paginaActual - 1) * TamanoPagina).Take(TamanoPagina);
    }
}