using System.Security.Claims;
using Domain.Generics;
using Domain.Models;
using Domain.Models.Acuerdos.Enums;
using Domain.Models.Acuerdos.Responses;
using Domain.Models.ProcesoLiquidacion;
using Domain.Models.Recibos;
using Domain.Models.Recibos.Responses;
using Domain.Models.Resoluciones;
using Domain.Models.Resoluciones.Requests;
using Domain.Models.Resoluciones.Responses;
using Domain.Responses.Liquidacion;
using Domain.Responses.Recibo;
using Domain.Responses.Resolucion.Enums;
using Domain.Responses.Users.Enums;
using Frontend.Reportes;
using Infrastructure.Services.AcuerdosPago;
using Infrastructure.Services.Carteras;
using Infrastructure.Services.Importados;
using Infrastructure.Services.Liquidaciones;
using Infrastructure.Services.Pagos;
using Infrastructure.Services.Parametros;
using Infrastructure.Services.Procesos.Coactivo;
using Infrastructure.Services.Procesos.Persuasivo;
using Infrastructure.Services.Rec2ibos;
using Infrastructure.Services.Resoluciones;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Rodamiento.Shared.Components.Pages.PConsulta;

namespace Frontend.Components.Pages.Consulta;

public partial class Dashboard : ComponentBase, IDisposable
{
    [Inject] private IConfiguration Config { get; set; } = null!;
    [Inject] private BusquedaService BusquedaService { get; set; } = null!;
    [Inject] private LiquidacionService ComparendoService { get; set; } = null!;
    [Inject] private CarteraService CarteraService { get; set; } = null!;
    [Inject] private ReciboService ReciboService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;
    [Inject] private ParametroService ParametroService { get; set; } = null!;
    [Inject] private PagoService PagoService { get; set; } = null!;
    [Inject] private ImportadosService Importadoservice { get; set; } = null!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] private PersuasivoService PersuasivoService { get; set; } = null!;
    [Inject] private CoactivoService CoactivoService { get; set; } = null!;
    [Inject] private ResolucionService ResolucionService { get; set; } = null!;
    [Inject] private AcuerdoPagoService AcuerdoPagoService { get; set; } = null!;

    // ── Estado principal ────────────────────────────────────────────
    private EstadoCuentaVehiculoDto _estadoCuenta = new();
    private List<VigenciaForm> _vigencias = [];
    private List<ConceptoResumenDto> resumenConceptoslist = [];
    private List<ReciboDto> reciboslist = [];
    private List<DetalleReciboDto> _detalleRecibo = [];
    private List<ResolucionResponseDto> resolucioneslist = [];
    private List<Proceso> coactivosList = [];
    private List<AcuerdoPagoDto> acuerdosList = []; // 🚀 Lista de Acuerdos
    private AcuerdoPagoDto? _acuerdoActivo; // 🚀 Acuerdo Vigente actual
    
    private bool isDropdownOpen = false;
    
    private async Task CloseDropdown()
    {
        // Pequeño delay para permitir que el click del ítem se ejecute antes de cerrar
        await Task.Delay(150);
        isDropdownOpen = false;
    }

    private void ToggleDropdown()
    {
        isDropdownOpen = !isDropdownOpen;
    }

    private void SelectTab(int selectedTab)
    {
        isDropdownOpen = false; // Cierra el menú desplegable
        SetTab(selectedTab);    // Cambia la pestaña activa
    }

    private bool _tieneAcuerdoVigente => _acuerdoActivo != null;

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
    private List<int> carterasSeleccionadasIds = [];

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
    private async Task SetTab(int n)
    {
        tab = n;

        if (n == 5 && !string.IsNullOrEmpty(_estadoCuenta?.Placa))
        {
            await CargarCoactivos(_estadoCuenta.Placa);
        }
        else if (n == 6 && !string.IsNullOrEmpty(_estadoCuenta?.Placa))
        {
            await CargarAcuerdosPago(_estadoCuenta.Placa);
        }

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

            var result = await CarteraService.GetCarteraByPlaca(placa);

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
                CargarCoactivos(placa),
                CargarAcuerdosPago(placa)); // 🚀 Cargar acuerdos de pago al consultar la placa

            _vigencias = _estadoCuenta.Conceptos
                .GroupBy(d => d.Vigencia)
                .OrderBy(g => g.Key)
                .Select(g => new VigenciaForm
                {
                    Vigencia = g.Key,
                    Conceptos = g.ToList(),
                    Seleccionado = false
                })
                .ToList();

            RecalcularTotal();
            await SetTab(1);
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
            resolucioneslist = await ComparendoService.GetResolucionByPlaca(_estadoCuenta.Placa);
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
            if (tab == 5)
            {
                coactivosList = await PersuasivoService.List(placa);
                avisosPorProceso = [];

                foreach (var proceso in coactivosList)
                {
                    avisosPorProceso[proceso.Id] = await PersuasivoService.ContarAvisosProceso(proceso.Id);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error coactivos: {ex.Message}");
        }
    }

    // ── Cargar Acuerdos de Pago ─────────────────────────────────────
    private async Task CargarAcuerdosPago(string placa)
    {
        try
        {
            acuerdosList = await AcuerdoPagoService.GetAcuerdosByPlaca(placa);
            _acuerdoActivo = acuerdosList.FirstOrDefault(a => a.Estado == EstadoAcuerdoPago.Vigente);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error acuerdos de pago: {ex.Message}");
        }
    }

    // ── Selección de vigencias ──────────────────────────────────────
    private void RecalcularTotal()
    {
        // 🚀 Omitir del cálculo las vigencias que pertenecen a un Acuerdo de Pago activo
        carterasSeleccionadasIds = _vigencias
            .Where(v => v.Seleccionado && !v.Conceptos.Any(c => c.AcuerdoPagoId != null))
            .SelectMany(v => v.Conceptos)
            .Select(c => c.Id)
            .ToList();

        totalSeleccionado = _vigencias
            .Where(v => v.Seleccionado && !v.Conceptos.Any(c => c.AcuerdoPagoId != null))
            .Sum(v => v.TotalVigencia);

        StateHasChanged();
    }

    // ── Acciones de Acuerdos de Pago ────────────────────────────────
    private void MostrarModalCrearAcuerdo()
    {
        // Abrir modal de creación de acuerdo de pago
    }

    private void VerCuotasAcuerdo(AcuerdoPagoDto acuerdo)
    {
        // Mostrar desglose / plan de cuotas del acuerdo
    }

    private static string BadgeEstadoAcuerdo(EstadoAcuerdoPago estado) => estado switch
    {
        EstadoAcuerdoPago.Vigente => "badge bg-warning text-dark",
        EstadoAcuerdoPago.Pagado => "badge bg-success",
        EstadoAcuerdoPago.Incumplido => "badge bg-danger",
        _ => "badge bg-secondary"
    };

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
    public async Task MostrarModal()
    {
        if (string.IsNullOrEmpty(_estadoCuenta.Placa))
        {
            await MostrarAlerta("warning", "Ingresa una placa.");
            return;
        }

        vigenciaDesde = _estadoCuenta.VigenciaDesde > 0 ? _estadoCuenta.VigenciaDesde : DateTime.UtcNow.Year;
        vigenciaHasta = _estadoCuenta.VigenciaHasta > 0 ? _estadoCuenta.VigenciaHasta : DateTime.UtcNow.Year;
        _mostrarModal = true;
        StateHasChanged();
    }

    private async Task MostrarModalCoactivoDirecto()
    {
        await MostrarAlerta("warning", "Esta función está pendiente de implementación.");
    }

    private async Task CrearProcesoPersuasivo()
    {
        if (vigenciaDesde == 0 || vigenciaHasta == 0)
        {
            await MostrarAlerta("warning", "La vigencia no puede ser cero.");
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
        await SetTab(5);
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

        var usuarioActual = await ObtenerUsuarioActual();

        var resultado = await PersuasivoService.EscalarACoactivo(
            proceso.Id,
            esAutomatico: false,
            usuarioResponsable: usuarioActual,
            motivo: "Escalado manual desde consulta");

        await MostrarAlerta(resultado.Success ? "success" : "warning", resultado.Message);

        if (resultado.Success)
        {
            await ObtenerCartera();
            await SetTab(5);
        }
    }

    private async Task<string?> ObtenerUsuarioActual()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        return authState.User.Identity?.Name;
    }

    // ── Resolución ──────────────────────────────────────────────────
    private async Task MostrarModal_R()
    {
        if (!await VerificarPermiso()) return;

        if (_vigencias != null)
        {
            foreach (var v in _vigencias)
            {
                v.Seleccionado = false;
            }
        }

        resolObj = new Resolucion
        {
            Fecha = DateTime.UtcNow,
            Valor = 0,
            Estado = EstadoResolucion.Activa
        };

        _mostrarModalr = true;
        StateHasChanged();
    }

    private async Task Agregar_resol()
    {
        var anosSeleccionados = _vigencias
            .Where(v => v.Seleccionado)
            .Select(v => v.Vigencia)
            .ToList();

        if (!anosSeleccionados.Any())
        {
            await MostrarAlerta("warning", "Debe seleccionar al menos una vigencia para generar la resolución.");
            return;
        }

        TipoResolucion tipoSeleccionado = tipoResolucion == 2
            ? TipoResolucion.AnulacionDeuda
            : TipoResolucion.Traslado;

        var command = new CreateResolucionRequest(
            Tipo: tipoSeleccionado,
            VehiculoId: _estadoCuenta.VehiculoId,
            Vigencias: anosSeleccionados,
            Observaciones: resolObj.Observaciones ?? string.Empty,
            UsuarioId: 3
        );

        bool resultado = await ResolucionService.Create(command);

        if (resultado)
        {
            await MostrarAlerta("success", "Resolución generada con éxito de forma consecutiva.");
            _mostrarModalr = false;
            await ObtenerCartera();
            await SetTab(1);
        }
        else
        {
            await MostrarAlerta("error", "No se pudo crear la resolución. Verifique las deudas pendientes.");
        }
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

    private static string BadgeEstado(EstadoProceso estadoId) => estadoId switch
    {
        EstadoProceso.SinProceso => "badge bg-success",
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

    private static string BadgeEstadoProceso(EstadoProceso estado) => estado switch
    {
        EstadoProceso.SinProceso => "badge bg-secondary",
        EstadoProceso.Persuasivo => "badge bg-warning text-dark",
        EstadoProceso.MandamientoPago => "badge bg-info text-dark",
        EstadoProceso.Coactivo => "badge bg-danger",
        _ => "badge bg-secondary"
    };

    private static string BadgeEstadoResolucion(EstadoResolucion estado) => estado switch
    {
        EstadoResolucion.Activa => "badge bg-success",
        _ => "badge bg-secondary"
    };

    private int AvisosProceso(Proceso proceso) =>
        avisosPorProceso.TryGetValue(proceso.Id, out var total) ? total : 0;

    // ── Permisos ────────────────────────────────────────────────────
    private async Task<bool> VerificarPermiso()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var rol = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        if (user.Identity?.IsAuthenticated == true && rol != Role.Funcionario.GetDisplayName())
        {
            await MostrarAlerta("warning", "Usuario sin permisos.");
            return false;
        }

        return true;
    }

    private async Task ReversarResolucion(ResolucionResponseDto resolucion)
    {
        if (!await VerificarPermiso()) return;

        bool confirmacion = await MostrarConfirmacion($"¿Está seguro de reversar la resolución N° {resolucion.NumeroResolucion}? Esta acción liberará las deudas vinculadas.");
        if (!confirmacion) return;

        try
        {
            _isLoading = true;
            StateHasChanged();

            bool resultado = await ResolucionService.ReversarResolucion(resolucion.Id, 3);

            if (resultado)
            {
                await MostrarAlerta("success", $"Resolución N° {resolucion.NumeroResolucion} reversada con éxito.");
                await ObtenerCartera();
            }
            else
            {
                await MostrarAlerta("error", "No se pudo reversar la resolución. Verifique el estado de la misma.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al reversar resolución: {ex.Message}");
            await MostrarAlerta("error", $"Error al reversar resolución: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }
    
    // ── JS interop ──────────────────────────────────────────────────
    private async Task MostrarAlerta(string tipo, string mensaje) =>
        await JsRuntime.InvokeVoidAsync("alert", mensaje);

    private async Task<bool> MostrarConfirmacion(string mensaje) =>
        await JsRuntime.InvokeAsync<bool>("confirm", mensaje);

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