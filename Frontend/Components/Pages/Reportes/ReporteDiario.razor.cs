using Domain.Models;
using Domain.Responses.Reportes;
using Frontend.Reportes;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
namespace Frontend.Components.Pages.Reportes;

public partial class ReporteDiario : ComponentBase
{
    private bool mostrarModal = false;
    public List<ReporteDiarioDto> ReporteDiarioList { get; set; } = [];
    Recibo_pago recibo_Pago = new Recibo_pago();
    private Parametro param_obj = new Parametro();
    public DateTime desde;
    public DateTime hasta;

    bool isLoading = false;

    protected async override Task OnInitializedAsync()
    {
        desde = DateTime.Today;
        hasta = DateTime.Today;
        param_obj = await ParametroService.GetParametroById(1);
    }

    private async Task Imprime_Rep()
    {
        // Reemplazo de DialogService.Confirm de Radzen por confirmación JS Nativa
        bool confirmationResult = await JsRuntime.InvokeAsync<bool>("confirm", "¿Está seguro que desea imprimir el Reporte Diario?");
        
        if (confirmationResult)
        {
            try
            {
                await Imprime_rec();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al generar el recibo: {ex}");
            }
        }
    }

    private async Task Imprime_rec()
    {
        string sdesde = desde.ToString("yyyy-MM-dd");
        string shasta = hasta.ToString("yyyy-MM-dd");
        string rango = $"{sdesde} a: {shasta}";
        
        //await recibo_Pago.DiarioPdf(ReporteDiarioList, rango, param_obj.NombreSecretario, param_obj.Nit, param_obj.Direccion);
        await Task.Delay(2000);
        await Muestra_Pdf("Informe_Diario.pdf");
    }

    public async Task Muestra_Pdf(string xname)
    {
        try
        {
            await DescargarYAbrirArchivo($"{xname.Trim()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    async Task Exporta_Excl()
    {
        isLoading = true;
        StateHasChanged();

        try
        {
            // Ejecutamos la tarea de escritura de forma asíncrona
            await Task.Run(() => Pagoservice.WriteFile(ReporteDiarioList, "C:/DATASET/Informe_diario.xlsx"));
            await Task.Delay(2000);
            await Muestra_Pdf("Informe_diario.xlsx");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al exportar Excel: {ex.Message}");
            await JsRuntime.InvokeVoidAsync("alert", "Ocurrió un error al generar el archivo Excel.");
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private async Task<byte[]> DescargarArchivo(string fileUrl)
    {
        try
        {
            HttpResponseMessage response = await HttpClient.GetAsync(fileUrl);
            response.EnsureSuccessStatusCode();
            byte[] archivoBytes = await response.Content.ReadAsByteArrayAsync();
            return archivoBytes;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al descargar el archivo: {ex.Message}");
            return null;
        }
    }

    private async Task DescargarYAbrirArchivo(string fileName)
    {
        try
        {
            string fileUrl = $"{NavigationManager.BaseUri}api/archivos/{fileName}";
            var fileBytes = await DescargarArchivo(fileUrl);
            if (fileBytes != null)
            {
                var contentStream = new DotNetStreamReference(new MemoryStream(fileBytes));
                await JsRuntime.InvokeVoidAsync("downloadFileFromStream", fileName, contentStream);
            }
            else
            {
                Console.WriteLine($"Error: No se pudo descargar el archivo {fileName}.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al descargar y abrir el archivo: {ex.Message}");
        }
    }

    private async Task Carga_datos()
    {
        try
        {
            isLoading = true;
            StateHasChanged();

            // Corregido: Se asigna el resultado directamente a la lista que renderiza la tabla
            var response = await RepResolServices.Lista_Recibos(desde, hasta);
            ReporteDiarioList = response ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en reporte_diario: {ex.Message}");
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }
}