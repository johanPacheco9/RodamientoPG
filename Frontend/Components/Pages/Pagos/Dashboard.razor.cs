using Domain.Generics;
using Domain.Models;
using Domain.Models.Recibos.Responses;
using Domain.Responses.Recibo;
using Domain.Responses.Recibo.Enums;
using Frontend.Reportes;
using Infrastructure.Services.Liquidaciones;
using Infrastructure.Services.Pagos;
using Infrastructure.Services.Parametros;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Frontend.Components.Pages.Pagos;

public partial class Dashboard
{
    [Inject]
    private PagoService PagoService { get; set; } = null!;
    [Inject]
    private LiquidacionService LiquiService { get; set; } = null!;
    [Inject]
    private ParametroService ParametroService { get; set; } = null!;

    private int _reciboIdBusqueda;
    public ReciboDto? recibo { get; set; }

    // Propiedades requeridas para la generación del PDF original
    private Parametro param_obj = new();
    private List<DetalleReciboDto> Listxconc = [];

    private bool isLoading = false;
    public bool envia_ws = false;
    public string mensaje = "";
    private bool mostrarSpinnerModal = false;

    protected override async Task OnInitializedAsync()
    {
        param_obj = await ParametroService.GetParametroById(1) ?? new Parametro();
    }

    private async Task BuscaRecibo()
    {
        if (_reciboIdBusqueda <= 0)
        {
            await JsRuntime.InvokeVoidAsync("alert", "Por favor, ingresa un número de recibo válido.");
            return;
        }

        try
        {
            isLoading = true;
            var result = await PagoService.GetRecibo(_reciboIdBusqueda);

            if (result != null)
            {
                recibo = result;
                mensaje = $"// Recibo {recibo.Id} cargado exitosamente. Estado actual: {recibo.Estado.GetDisplayName()}";
            }
        }
        catch (KeyNotFoundException ex)
        {
            recibo = null;
            mensaje = $"// {ex.Message}";
            await JsRuntime.InvokeVoidAsync("alert", ex.Message);
        }
        catch (Exception ex)
        {
            mensaje = $"// Error al cargar el recibo: {ex.Message}";
            await JsRuntime.InvokeVoidAsync("alert", $"Error: {ex.Message}");
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task Procesar()
    {
        if (recibo == null || recibo.Id <= 0 || recibo.Estado != EstadoRecibo.Pendiente)
        {
            await JsRuntime.InvokeVoidAsync("alert", "El recibo no se encuentra en estado PENDIENTE para procesar.");
            return;
        }

        bool confirmar = await JsRuntime.InvokeAsync<bool>("confirm", "¿Está seguro de aplicar este pago al sistema vehicular?");

        if (!confirmar) return;

        try
        {
            await PagoService.AplicarPago(_reciboIdBusqueda);
            await JsRuntime.InvokeVoidAsync("alert", "El pago ha sido asentado y la cartera descargada con éxito.");
            await BuscaRecibo();
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("alert", $"Error interno al procesar el pago: {ex.Message}");
        }
    }

    private async Task Reversar()
    {
        if (recibo == null || recibo.Id <= 0 || recibo.Estado != EstadoRecibo.Pagado)
        {
            await JsRuntime.InvokeVoidAsync("alert", "Únicamente los recibos PAGADOS se pueden reversar.");
            return;
        }

        bool confirmar = await JsRuntime.InvokeAsync<bool>("confirm", "¿Está seguro de reversar el dinero? Se reactivará la deuda en mora.");

        if (!confirmar) return;

        try
        {
            var filas = await PagoService.ReversarPago(recibo.Id);

            if (filas > 0)
            {
                mensaje += $"\n// Recibo {recibo.Id} reversado. La cartera vuelve a estar activa.";
                await JsRuntime.InvokeVoidAsync("alert", "Transacción reversada con éxito.");
                await BuscaRecibo();
            }
            else
            {
                await JsRuntime.InvokeVoidAsync("alert", "No se pudo reversar el recibo. Verifica que esté en estado Pagado.");
            }
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("alert", $"Error al ejecutar el reverso: {ex.Message}");
        }
    }

    private async Task GenerarPdf()
    {
        if (recibo == null || recibo.Id <= 0) return;

        mostrarSpinnerModal = true;
        StateHasChanged();

        try
        {
            var response = await LiquiService.Items_x_Recibo(recibo.Id);
            if (response != null && response.Any())
            {
                Listxconc = response.Select(x => new DetalleReciboDto
                {
                    Vigencia = x.Vigencia,
                    ValorRodamiento = x.ValorRodamiento,
                    ValorCarga = x.ValorCarga,
                    ValorEstampillas = x.ValorEstampillas,
                    ValorRecibo = x.ValorRecibo,
                    ValorInteres = x.ValorInteres
                }).ToList();
            }
            else
            {
                Listxconc = [];
            }

            // Uso directo del DTO recibo
            var reporte = new Recibo_pago();
            await reporte.CreatePdf(recibo, Listxconc, param_obj);

            await Task.Delay(1000); // Mantenemos el retraso controlado para asegurar la escritura en el disco local
            string reciboPdf = $"Recibo_{recibo.Id}.pdf";
            await DescargarYAbrirArchivo(reciboPdf.Trim());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error crítico generando documento oficial: {ex.Message}");
        }
        finally
        {
            mostrarSpinnerModal = false;
            StateHasChanged();
        }
    }

    private async Task DescargarYAbrirArchivo(string fileName)
    {
        try
        {
            string fileUrl = $"{NavigationManager.BaseUri}api/archivos/{fileName}";
            HttpResponseMessage response = await HttpClient.GetAsync(fileUrl);
            if (response.IsSuccessStatusCode)
            {
                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                using var contentStream = new DotNetStreamReference(new MemoryStream(fileBytes));
                await JsRuntime.InvokeVoidAsync("downloadFileFromStream", fileName, contentStream);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al despachar el archivo al navegador: {ex.Message}");
        }
    }
}