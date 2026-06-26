using System.Security.Claims;
using Domain.Models;
using Domain.Models.ProcesoLiquidacion;
using Domain.Models.Recibos;
using Domain.Models.Recibos.Responses;
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
using Infrastructure.Services.Rec2ibos;
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
    [Inject] private ReciboService ReciboService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;
    [Inject] private ParametroService ParametroService { get; set; } = null!;
    
    [Inject] private PagoService PagoService{ get; set; } = null!;
    [Inject] private ImportadosService Importadoservice { get; set; } = null!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] private PersuasivoService PersuasivoService { get; set; } = null!;
    [Inject] private CoactivoService CoactivoService { get; set; } = null!;

    // ── Estado principal ────────────────────────────────────────────
    private EstadoCuentaVehiculoDto _estadoCuenta = new();
    private List<VigenciaGrupo> _vigencias = [];
    private List<ConceptoResumenDto> resumenConceptoslist = [];
    private List<ReciboDto> reciboslist = []; // Inicializado para evitar nulls en renderizado inicial
    private List<DetalleReciboDto> _detalleRecibo = [];
    private List<Resolucion> resolucioneslist = [];
    private List<Proceso> coactivosList = [];
    private Dictionary<int, int> avisosPorProceso = [];
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
    private bool _buscarPlacaInicial = false;

    // ── Resolución ──────────────────────────────────────────────────
    public int vigenciaDesde;
    public int vigenciaHasta;
    public int tipoResolucion = 1;
    public decimal novTotal = 0;

    // ── Selección de vigencias ──────────────────────────────────────
    private decimal totalSeleccionado;
    private List<int> carterasSeleccionadasIds = []; // Cambiado de vigencias a IDs de carteras

    // ───────────────────────────────────────────────────────────────
    [Parameter] public string? PlacaParam { get; set; }

    protected async override Task OnInitializedAsync()
    {
        paramObj = await ParametroService.GetParametroById(1);
        BusquedaService.OnComparendoCapturado += EscucharBusquedaPlaca;

        if (!string.IsNullOrEmpty(PlacaParam))
        {
            _estadoCuenta.Placa = PlacaParam.ToUpper();
            _buscarPlacaInicial = true;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || !_buscarPlacaInicial) return;

        _buscarPlacaInicial = false;
        await ObtenerCartera();
    }

    // ── Tabs ────────────────────────────────────────────────────────
    private void SetTab(int n)
    {
        tab = n;
        StateHasChanged();
    }

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
            
            // 🚀 Clave: Trae vehículo, propietario y todos los conceptos de cartera en una sola consulta
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

            // 🚀 Optimización: Poblamos la tabla agrupando en memoria el resultado directo de la base de datos
            _vigencias = _estadoCuenta.Conceptos
                .GroupBy(d => d.Vigencia)
                .OrderBy(g => g.Key)
                .Select(g => new VigenciaGrupo
                {
                    Vigencia = g.Key,
                    Conceptos = g.ToList(),
                    Seleccionado = false // Por seguridad inicializan desmarcados
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
        try
        {
            reciboslist = await ReciboService.GetRecibosByPlaca(placa);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error recibos: {ex.Message}");
        }
    }

    private async Task CargarResumen(string placa)
    {
        try
        {
            resumenConceptoslist = await ComparendoService.Resumen_Conceptos(placa, _estadoCuenta.VigenciaHasta);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error resumen: {ex.Message}");
        }
    }

    private async Task CargarResoluciones()
    {
        try
        {
            resolucioneslist = await ComparendoService.GetResol(_estadoCuenta.Placa);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error resoluciones: {ex.Message}");
        }
    }

    private async Task CargarCoactivos(string placa)
    {
        try
        {
            coactivosList = await PersuasivoService.List(5, 0, placa);
            avisosPorProceso = [];

            foreach (var proceso in coactivosList)
            {
                avisosPorProceso[proceso.Id] = await PersuasivoService.ContarAvisosProceso(proceso.Id);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error coactivos: {ex.Message}");
        }
    }

    // ── Selección de vigencias ──────────────────────────────────────
    private void RecalcularTotal()
    {
        // 🚀 Aquí está la magia: SelectMany aplana y extrae TODOS los IDs de conceptos de los años marcados
        carterasSeleccionadasIds = _vigencias
            .Where(v => v.Seleccionado)
            .SelectMany(v => v.Conceptos) // Entra a la lista de conceptos de cada año seleccionado
            .Select(c => c.Id)            // Toma el Id de la BD de cada uno (Rodamiento, Estampilla, etc.)
            .ToList();

        // Sumamos el dinero total de los años seleccionados
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

        if (!carterasSeleccionadasIds.Any())
        {
            await MostrarAlerta("warning", "Selecciona al menos una vigencia.");
            return;
        }

        if (!await MostrarConfirmacion("¿Está seguro de generar el Recibo?")) return;

        try
        {
            // 🚀 Enviamos la firma actualizada con los IDs de cartera exactos seleccionados
            var resultado = await ComparendoService.GenerarRecibo(
                _estadoCuenta.VehiculoId,
                _estadoCuenta.Documento,
                _estadoCuenta.TipoDocumento,
                carterasSeleccionadasIds);

            if (!resultado.success)
            {
                await MostrarAlerta("error", $"Error: {resultado.message}");
                return;
            }

            var ultimoRecibo = await Importadoservice.Ultimo_recibo(_estadoCuenta.Placa)
                               ?? throw new Exception("No se ha encontrado el recibo");

            reciboActual = await PagoService.GetRecibo(ultimoRecibo.Num);
            
            // 🚀 Consume el método pivoteado por año que renderiza el reporte
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

        vigenciaDesde = _estadoCuenta.VigenciaDesde > 0 ? _estadoCuenta.VigenciaDesde : DateTime.UtcNow.Year;
        vigenciaHasta = _estadoCuenta.VigenciaHasta > 0 ? _estadoCuenta.VigenciaHasta : DateTime.UtcNow.Year;
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

        if (vigenciaDesde > vigenciaHasta)
        {
            await MostrarAlerta("warning", "La vigencia desde no puede ser mayor a la vigencia hasta.");
            return;
        }

        if (!await MostrarConfirmacion("¿Está seguro de iniciar cobro persuasivo para esta placa?")) return;

        var resultado = await PersuasivoService.CrearProcesoPersuasivoPorPlaca(
            _estadoCuenta.Placa,
            vigenciaDesde,
            vigenciaHasta);

        if (!resultado.Success)
        {
            await MostrarAlerta("warning", resultado.Message);
            return;
        }

        _mostrarModal = false;
        await ObtenerCartera();
        SetTab(5);
        await MostrarAlerta("success", resultado.Message);
    }

    private async Task RegistrarAviso(Proceso proceso)
    {
        if (!await MostrarConfirmacion($"¿Registrar el siguiente aviso del proceso {proceso.NumeroProceso}?")) return;

        var resultado = await PersuasivoService.RegistrarAvisoProceso(proceso.Id);
        await MostrarAlerta(resultado.Success ? "success" : "warning", resultado.Message);
        await CargarCoactivos(_estadoCuenta.Placa);
        StateHasChanged();
    }

    private async Task PasarACoactivo(Proceso proceso)
    {
        if (!await MostrarConfirmacion($"¿Pasar el proceso {proceso.NumeroProceso} a coactivo?")) return;

        try
        {
            await CoactivoService.Procesar(1, proceso.NumeroProceso ?? 0);
            await MostrarAlerta("success", "Proceso trasladado a coactivo.");
            await ObtenerCartera();
            SetTab(5);
        }
        catch (Exception ex)
        {
            await MostrarAlerta("warning", ex.InnerException?.Message ?? ex.Message);
        }
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
        string.IsNullOrEmpty(nombre) ? "?" : string.Concat(nombre.Split(' ').Where(p => p.Length > 0).Take(2).Select(p => p[0]));

    private static string BadgeEstado(int estadoId) => estadoId switch
    {
        1 => "badge bg-success",
        _ => "badge bg-secondary"
    };

    private static string BadgeRecibo(string estado) => estado?.ToLower() switch
    {
        "pagado" => "badge bg-success",
        "pendiente" => "badge bg-warning text-dark",
        _ => "badge bg-secondary"
    };

    private static string BadgeProceso(string tipo) => tipo?.ToLower() switch
    {
        "coactivo" => "badge bg-danger",
        "persuasivo" => "badge bg-warning text-dark",
        _ => "badge bg-secondary"
    };

    private static string BadgeEstadoProceso(object estado) => "badge bg-warning text-dark";

    private int AvisosProceso(Proceso proceso) =>
        avisosPorProceso.TryGetValue(proceso.Id, out var total) ? total : 0;

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

    // ── Clase contenedora UI para la Grilla ─────────────────────────
    public class VigenciaGrupo
    {
        public int Vigencia { get; set; }
    
        // Lista de conceptos detallados (IDs reales de la BD) para este año
        public List<ConceptoCarteraDto> Conceptos { get; set; } = [];
    
        // Suma dinámica de todo lo que compone este año específico
        public decimal TotalVigencia => Conceptos.Sum(c => c.ValorTotal);

        // El checkbox de la interfaz se amarra a esta propiedad
        public bool Seleccionado { get; set; }
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
