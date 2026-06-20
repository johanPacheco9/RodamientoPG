using System.Security.Claims;
using Domain.Models;
using Domain.Models.ProcesoLiquidacion;
using Domain.Responses.Liquidacion;
using Domain.Responses.Recibo;
using Domain.Responses.Resolucion.Enums;
using Frontend.Reportes;
using Infrastructure.Services.Importados;
using Infrastructure.Services.Liquidaciones;
using Infrastructure.Services.Pagos;
using Infrastructure.Services.Parametros;
using Infrastructure.Services.Procesos.Coactivo;
using Infrastructure.Services.Procesos.Persuasivo;
using Infrastructure.Services.Vehiculos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Rodamiento.Shared.Components.Pages.PConsulta;

namespace Frontend.Components.Pages.Consulta;

public partial class Dashboard : ComponentBase, IDisposable
{
    [Inject] private IConfiguration Config { get; set; } = null!;
    [Inject] private BusquedaService BusquedaService { get; set; } = null!;
    [Inject] private LiquidacionService ComparendoService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;
    [Inject] private ParametroService ParametroService { get; set; } = null!;
    [Inject] private PagosService PagosServices { get; set; } = null!;
    [Inject] private ImportadosService Importadoservice { get; set; } = null!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] private PersuasivoService PersuasivoService { get; set; } = null!;

    // ── Estado principal ────────────────────────────────────────────
    private EstadoCuentaVehiculoDto _estadoCuenta = new();
    private List<VigenciaGrupo> _vigencias = [];
    private List<ConceptoResumenDto> resumenConceptoslist = [];
    private List<Recibo> reciboslist = [];
    private List<DetalleReciboDto> _detalleRecibo = [];
    private List<Resolucion> resolucioneslist = [];
    private List<Proceso> coactivosList = [];
    private Resolucion resolObj = new();
    private Recibo_pago recibo_Pago = new();
    private Parametro paramObj = new();
    public Recibo reciboActual = new();

    // ── UI ──────────────────────────────────────────────────────────
    private int tab = 1;
    private bool _isLoading = false;
    private bool _mostrarModal = false;
    private bool _mostrarModalr = false;
    private bool MostrarBusquedaCedula = false;
    private bool _islic = true;

    // ── Resolución ──────────────────────────────────────────────────
    public int vigenciaDesde;
    public int vigenciaHasta;
    public int tipoResolucion = 1;
    public decimal novTotal = 0;

    // ── Selección de vigencias ──────────────────────────────────────
    private decimal totalSeleccionado;
    private List<int> vigenciasSeleccionadas = [];

    // ───────────────────────────────────────────────────────────────
    protected override async Task OnInitializedAsync()
    {
        paramObj = await ParametroService.GetParametroById(1);
        BusquedaService.OnComparendoCapturado += EscucharBusquedaPlaca;
    }

    // ── Tabs ────────────────────────────────────────────────────────
    private void SetTab(int n) { tab = n; StateHasChanged(); }

    // ── Búsqueda ────────────────────────────────────────────────────
    private async Task ShowBusqueda()
    {
        MostrarBusquedaCedula = true;
        await InvokeAsync(StateHasChanged);
    }

    private async void EscucharBusquedaPlaca(string placa)
    {
        try
        {
            if (string.IsNullOrEmpty(placa)) return;
            _estadoCuenta.Placa = placa.Trim().ToUpper();
            MostrarBusquedaCedula = false;
            await ObtenerCartera();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al procesar placa del modal: {ex.Message}");
        }
        finally
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    // ── Carga principal ─────────────────────────────────────────────
    private async Task ObtenerCartera()
    {
        if (string.IsNullOrWhiteSpace(_estadoCuenta.Placa))
        {
            await MostrarAlerta("warning", "Ingresa una placa.");
            return;
        }

        try
        {
            _isLoading = true;
            var placa = _estadoCuenta.Placa.Trim().ToUpper();
            var result = await ComparendoService.GetCarteraByPlaca(placa);

            if (result == null || string.IsNullOrEmpty(result.Documento))
            {
                await MostrarAlerta("warning", "Placa no encontrada.");
                return;
            }

            _estadoCuenta = result;

            await Task.WhenAll(
                CargarRecibos(placa),
                CargarResumen(placa),
                CargarResoluciones(),
                CargarCoactivos(placa));

            var detalle = await ComparendoService.DetallesConceptos(placa, _estadoCuenta.VigenciaHasta);
            _vigencias = detalle
                .GroupBy(d => d.Vigencia)
                .OrderBy(g => g.Key)
                .Select(g => new VigenciaGrupo
                {
                    Vigencia = g.Key,
                    Conceptos = g.ToList()
                })
                .ToList();
            RecalcularTotal();
            SetTab(1);
        }
        catch (Exception ex)
        {
            await MostrarAlerta("error", $"Error al buscar: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task CargarRecibos(string placa)
    {
        try { reciboslist = await ComparendoService.GetRecibos(placa); }
        catch (Exception ex) { Console.WriteLine($"Error recibos: {ex.Message}"); }
    }

    private async Task CargarResumen(string placa)
    {
        try { resumenConceptoslist = await ComparendoService.Resumen_Conceptos(placa, _estadoCuenta.VigenciaHasta); }
        catch (Exception ex) { Console.WriteLine($"Error resumen: {ex.Message}"); }
    }

    private async Task CargarResoluciones()
    {
        try { resolucioneslist = await ComparendoService.GetResol(_estadoCuenta.Placa); }
        catch (Exception ex) { Console.WriteLine($"Error resoluciones: {ex.Message}"); }
    }

    private async Task CargarCoactivos(string placa)
    {
        try { coactivosList = await PersuasivoService.List(5, 0, placa); }
        catch (Exception ex) { Console.WriteLine($"Error coactivos: {ex.Message}"); }
    }

    // ── Selección de vigencias ──────────────────────────────────────
    private void RecalcularTotal()
    {
        vigenciasSeleccionadas = _vigencias
            .Where(v => v.Seleccionado)
            .Select(v => v.Vigencia)
            .ToList();

        totalSeleccionado = _vigencias
            .Where(v => v.Seleccionado)
            .Sum(v => v.TotalVigencia);

        StateHasChanged();
    }

    // ── Recibo ──────────────────────────────────────────────────────
    private async Task GenerarRecibo()
    {
        if (string.IsNullOrEmpty(_estadoCuenta.Placa))
        {
            await MostrarAlerta("warning", "Ingresa una placa.");
            return;
        }

        if (!await MostrarConfirmacion("¿Está seguro de generar el Recibo?")) return;

        try
        {
            var resultado = await ComparendoService.GenerarRecibo(
                _estadoCuenta.Placa,
                _estadoCuenta.Documento,
                _estadoCuenta.TipoDocumento,
                vigenciasSeleccionadas);

            if (!resultado.success)
            {
                await MostrarAlerta("error", $"Error: {resultado.message}");
                return;
            }

            var ultimoRecibo = await Importadoservice.Ultimo_recibo(_estadoCuenta.Placa)
                ?? throw new Exception("No se ha encontrado el recibo");

            reciboActual = await PagosServices.GetRecibo(ultimoRecibo.Num);
            _detalleRecibo = await ComparendoService.Items_x_Recibo(ultimoRecibo.Num);

            if (_detalleRecibo.Count == 0)
                throw new Exception("El recibo no tiene ítems");

            await recibo_Pago.CreatePdf(reciboActual, _detalleRecibo, paramObj);
            await Task.Delay(2000);
            await Muestra_Pdf($"Recibo_{ultimoRecibo.Num}.pdf");
            await CargarRecibos(_estadoCuenta.Placa);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error recibo: {ex.Message}");
            await MostrarAlerta("error", $"Error: {ex.Message}");
        }
    }

    // ── Paz y salvo ─────────────────────────────────────────────────
    private async Task GenerarPazYSalvo()
    {
        if (!await VerificarPermiso()) return;

        if (string.IsNullOrEmpty(_estadoCuenta.Placa))
        {
            await MostrarAlerta("warning", "Ingresa una placa.");
            return;
        }

        if (_estadoCuenta.TotalDeuda > 0)
        {
            await MostrarAlerta("warning", "Tiene vigencias pendientes.");
            return;
        }

        if (!await MostrarConfirmacion("¿Está seguro de generar Paz y Salvo?")) return;

        await recibo_Pago.PazySalvo(_estadoCuenta, paramObj);
        await Muestra_Pdf($"PazySalvo_{_estadoCuenta.Placa}.pdf");
    }

    // ── Proceso persuasivo ──────────────────────────────────────────
    public void MostrarModal()
    {
        if (string.IsNullOrEmpty(_estadoCuenta.Placa))
        {
            MostrarAlerta("warning", "Ingresa una placa.");
            return;
        }

        vigenciaDesde = DateTime.UtcNow.Year;
        vigenciaHasta = DateTime.UtcNow.Year;
        _mostrarModal = true;
        StateHasChanged();
    }

    private async Task Agregar()
    {
        if (vigenciaDesde == 0 || vigenciaHasta == 0)
        {
            await MostrarAlerta("warning", "La vigencia no puede ser cero.");
            return;
        }

        if (_estadoCuenta.EstadoId != 1)
        {
            await MostrarAlerta("warning", "Vehículo inactivo.");
            return;
        }

        if (!await MostrarConfirmacion("¿Está seguro de generar la Deuda?")) return;

        await ComparendoService.GeneraDeuda(_estadoCuenta.Placa, vigenciaDesde, vigenciaHasta);
        _mostrarModal = false;
        await ObtenerCartera();
    }

    // ── Resolución ──────────────────────────────────────────────────
    private async Task MostrarModal_R()
    {
        if (!await VerificarPermiso()) return;

        resolObj = new Resolucion
        {
            Fecha = DateTime.UtcNow,
            PeriodoHasta = DateTime.UtcNow.Year
        };
        _mostrarModalr = true;
        StateHasChanged();
    }

    private async Task Agregar_resol()
    {
        if (tipoResolucion == 0) tipoResolucion = 1;

        if (string.IsNullOrEmpty(resolObj.NumeroResolucion))
        {
            await MostrarAlerta("warning", "El número de resolución es obligatorio.");
            return;
        }

        if (tipoResolucion == 2 && _estadoCuenta.TotalDeuda > 0)
        {
            await MostrarAlerta("warning", "Tiene vigencias pendientes.");
            return;
        }

        resolObj.TipoResolucionId = tipoResolucion;
        resolObj.VehiculoId = 0;
        resolObj.Valor = novTotal;
        resolObj.Estado = EstadoResolucion.Activa;
        resolObj.UsuarioId = 3;
        resolObj.Observaciones ??= string.Empty;

        await ComparendoService.Add(
            _estadoCuenta.Placa,
            resolObj.Fecha,
            resolObj.NumeroResolucion,
            resolObj.TipoResolucionId,
            resolObj.Valor,
            _estadoCuenta.VigenciaDesde,
            resolObj.PeriodoHasta,
            resolObj.Observaciones,
            resolObj.UsuarioId);

        _mostrarModalr = false;
        await ObtenerCartera();
        SetTab(1);
    }

    private async Task capturar(ChangeEventArgs e)
    {
        tipoResolucion = int.Parse((string)e.Value!);
        _islic = tipoResolucion != 2;
        StateHasChanged();
    }

    // ── Modales ─────────────────────────────────────────────────────
    private void CerrarModal()
    {
        _mostrarModal = false;
        _mostrarModalr = false;
        MostrarBusquedaCedula = false;
        StateHasChanged();
    }

    // ── PDF ─────────────────────────────────────────────────────────
    public async Task Muestra_Pdf(string archivo)
    {
        try
        {
            var url = $"{NavigationManager.BaseUri}api/archivos/{archivo}";
            var response = await HttpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var bytes = await response.Content.ReadAsByteArrayAsync();
            var stream = new DotNetStreamReference(new MemoryStream(bytes));
            await JsRuntime.InvokeVoidAsync("downloadFileFromStream", archivo, stream);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error PDF: {ex.Message}");
            await MostrarAlerta("error", $"Error al abrir PDF: {ex.Message}");
        }
    }

    // ── Helpers de vista ────────────────────────────────────────────
    private static string Iniciales(string? nombre) =>
        string.IsNullOrEmpty(nombre) ? "?" :
        string.Concat(nombre.Split(' ').Where(p => p.Length > 0).Take(2).Select(p => p[0]));

    private static string BadgeEstado(int estadoId) => estadoId switch
    {
        1 => "badge bg-success",
        _ => "badge bg-secondary"
    };

    private static string BadgeRecibo(string estado) => estado?.ToLower() switch
    {
        "pagado"    => "badge bg-success",
        "pendiente" => "badge bg-warning text-dark",
        _           => "badge bg-secondary"
    };

    private static string BadgeProceso(string tipo) => tipo?.ToLower() switch
    {
        "coactivo"   => "badge bg-danger",
        "persuasivo" => "badge bg-warning text-dark",
        _            => "badge bg-secondary"
    };

    private static string BadgeEstadoProceso(object estado) => "badge bg-warning text-dark";

    // ── Permisos ────────────────────────────────────────────────────
    private async Task<bool> VerificarPermiso()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var rol = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        if (user.Identity?.IsAuthenticated == true && rol != "2")
        {
            await MostrarAlerta("warning", "Usuario sin permisos.");
            return false;
        }

        return true;
    }

    // ── JS interop ──────────────────────────────────────────────────
    private async Task MostrarAlerta(string tipo, string mensaje) =>
        await JsRuntime.InvokeVoidAsync("alert", mensaje);

    private async Task<bool> MostrarConfirmacion(string mensaje) =>
        await JsRuntime.InvokeAsync<bool>("confirm", mensaje);

    public class VigenciaGrupo
    {
        public int Vigencia { get; set; }
        public List<CarteraDetailDto> Conceptos { get; set; } = [];
        public decimal TotalVigencia => Conceptos.Sum(c => c.ValorTotal);

        private bool _seleccionado = true;
        public bool Seleccionado
        {
            get => _seleccionado;
            set
            {
                _seleccionado = value;
                foreach (var c in Conceptos)
                    c.Seleccionado = value;
            }
        }
    }
    // ── Dispose ─────────────────────────────────────────────────────
    public void Dispose()
    {
        try
        {
            BusquedaService.OnComparendoCapturado -= EscucharBusquedaPlaca;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al liberar recursos en Dispose: {ex.Message}");
        }
    }
}