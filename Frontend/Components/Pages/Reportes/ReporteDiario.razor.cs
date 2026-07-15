
using Infrastructure.Services.Reportes;
using Infrastructure.Services.Reportes.Responses;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
namespace Frontend.Components.Pages.Reportes;

public partial class ReporteDiario : ComponentBase
{
    [Inject] private ReportesManager ReporteService { get; set; } = null!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;

    private ReporteDiarioDto? _reporte;
    private DateTime _fechaConsulta = DateTime.Today;
    private bool _isLoading = false;

    protected async override Task OnInitializedAsync()
    {
        await ConsultarReporte();
    }

    private async Task ConsultarReporte()
    {
        _isLoading = true;
        try
        {
            var response = await ReporteService.ObtenerInformeDiario(_fechaConsulta);
            if (response != null)
            {
                _reporte = response;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al calcular arqueo de caja diario: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private double CalcularPorcentaje(decimal montoConcepto)
    {
        if (_reporte == null || _reporte.TotalRecaudado == 0) return 0;
        return (double)Math.Round((montoConcepto / _reporte.TotalRecaudado) * 100, 1);
    }

    private async Task ImprimirReporte()
    {
        // Ejecuta la orden nativa del navegador para imprimir en papel o PDF el balance actual
        await JsRuntime.InvokeVoidAsync("window.print");
    }
}