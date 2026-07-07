using Domain.Generics;
using Domain.Models;
using Domain.Models.Recibos;
using Domain.Responses.Recibo;
using Domain.Responses.Recibo.Enums;
using Frontend.Reportes;
using Infrastructure.Services.Pagos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
namespace Frontend.Components.Pages.Pagos;

public partial class Dashboard
{
    [Inject]
    private PagoService PagoService { get; set; } = null!;

    private int _reciboIdBusqueda;
    public Recibo recibos { get; set; } = new();
    public List<DetalleReciboDto> Listxconc = [];
    private Parametro param_obj = new();

    private bool isLoading = false;
    public bool envia_ws = false;
    public string mensaje = "";
    private bool mostrarSpinnerModal = false; // Control de visualización para el Loading del PDF

    protected async override Task OnInitializedAsync()
    {
        param_obj = await ParametroService.GetParametroById(1) ?? new Parametro();
    }

    private async Task BuscaRecibo()
    {
        try
        {
            if (_reciboIdBusqueda == 0)
            {
                await JsRuntime.InvokeVoidAsync("alert", "Por favor, ingresa un valor de Recibo numérico.");

                return;
            }

            isLoading = true;
            var result = await PagoService.GetRecibo(_reciboIdBusqueda);

            if (result != null)
            {
                recibos = result;
                mensaje = $"// Recibo {recibos.Id} cargado exitosamente el {DateTime.UtcNow}. Estado actual: {recibos.Estado.GetDisplayName()}";
            }
            else
            {
                recibos = new Recibo();
                await JsRuntime.InvokeVoidAsync("alert", "El recibo ingresado no existe en el sistema.");
            }
        }
        catch (Exception ex)
        {
            mensaje = $"// Error en la carga del recibo: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task Procesar()
    {
        if (recibos.Id <= 0 || recibos.Estado != EstadoRecibo.Pendiente)
        {
            await JsRuntime.InvokeVoidAsync("alert", "El recibo no se encuentra en estado PENDIENTE para procesar.");

            return;
        }

        bool confirmar = await JsRuntime.InvokeAsync<bool>("confirm", "¿Está seguro de aplicar este pago al sistema vehicular?");
        if (confirmar)
        {
            try
            {
                await PagoService.AplicarPago(_reciboIdBusqueda);
                
                
                await JsRuntime.InvokeVoidAsync("alert", "El pago ha sido asentado y la cartera descargada con éxito.");
            }
            catch (Exception ex)
            {
                await JsRuntime.InvokeVoidAsync("alert", $"Error interno al procesar el pago: {ex.Message}");
            }
        }
    }

    private async Task Reversar()
    {
        if (recibos.Id <= 0 || recibos.Estado != EstadoRecibo.Aplicado)
        {
            await JsRuntime.InvokeVoidAsync("alert", "Únicamente los recibos APLICADOS / PAGADOS se pueden reversar.");

            return;
        }

        bool confirmar = await JsRuntime.InvokeAsync<bool>("confirm", "¿Está seguro de reversar el dinero? Se reactivará la deuda en mora.");
        if (confirmar)
        {
            try
            {
                //Reversar el recibo, el pago supongo, lo haré de 0
               // await PagosServices.Reversa_Rec(recibos.Id);
                await JsRuntime.InvokeVoidAsync("alert", "Transacción reversada. La cartera del vehículo vuelve a estar activa.");
                await BuscaRecibo();
            }
            catch (Exception ex)
            {
                await JsRuntime.InvokeVoidAsync("alert", $"Error al ejecutar el reverso: {ex.Message}");
            }
        }
    }

    private async Task GenerarPdf()
    {
        if (recibos.Id <= 0) return;

        mostrarSpinnerModal = true;
        StateHasChanged();

        try
        {
            var response = await LiquiService.Items_x_Recibo(recibos.Id);
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

            var reporte = new Recibo_pago();
            await reporte.CreatePdf(recibos, Listxconc, param_obj);

            await Task.Delay(1000); // Mantenemos el retraso controlado para asegurar la escritura en el disco local
            string reciboPdf = $"Recibo_{recibos.Id}.pdf";
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
                var contentStream = new DotNetStreamReference(new MemoryStream(fileBytes));
                await JsRuntime.InvokeVoidAsync("downloadFileFromStream", fileName, contentStream);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al despachar el archivo al navegador: {ex.Message}");
        }
    }
    
}