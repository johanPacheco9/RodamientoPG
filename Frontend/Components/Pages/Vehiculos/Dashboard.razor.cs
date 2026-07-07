using Domain.Models.Vehiculos;
using Domain.Models.Vehiculos.Responses;
using Infrastructure.Services.Carteras;
using Infrastructure.Services.Colores;
using Infrastructure.Services.Lineas;
using Infrastructure.Services.Marcas;
using Infrastructure.Services.Propietarios;
using Infrastructure.Services.TiposVehiculos;
using Infrastructure.Services.Vehiculos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Frontend.Components.Pages.Vehiculos;

public partial class Dashboard
{
    [Inject] private CarteraService CarteraService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private VehiculosService VehiculosService { get; set; } = null!;
    [Inject] private PropietarioService PropietarioService { get; set; } = null!;
    [Inject] private MarcaService GruposService { get; set; } = null!;
    [Inject] private LineasService LineasService { get; set; } = null!;
    [Inject] private TiposService Claseservice { get; set; } = null!;
    [Inject] private ColoresService Clasescolor { get; set; } = null!;
    
    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;

    // Variables de Estado de la UI
    private bool _mostrarModal = false;
    private bool _mostrarModal2 = false;
    private bool _isLoading = false;
    private bool esModoEditar = false;

    private string buscarnit = string.Empty;
    public string rnombre = string.Empty;
    private string alertaMensaje = string.Empty;
    private string alertaTipo = "danger";

    // Modelos de Datos
    private Vehiculo vehiculo = new();
    private Propietario _propietario = new();

    // Grilla basada en DTOs ligeros
    private List<VehiculoDetalleDto> ListVehiculos = new();
    private IList<VehiculoDetalleDto>? selectedEmployees;
    private IList<Linea> lines = new List<Linea>();

    // Colecciones Maestras globales para el Formulario de Registro Nuevo
    private IEnumerable<Marca> customers = new List<Marca>();
    private IEnumerable<TipoVehiculo> Clasesveh = new List<TipoVehiculo>();
    private IEnumerable<Color> ClasesColores = new List<Color>();

    protected async override Task OnInitializedAsync()
    {
        _isLoading = true;
        LimpiarAlerta();
        try
        {
            // Carga inicial de DTOs para la grilla principal
            var todosLosVehiculos = await VehiculosService.GetAll();
            ListVehiculos = todosLosVehiculos ?? new List<VehiculoDetalleDto>();

            if (ListVehiculos.Any())
            {
                selectedEmployees = ListVehiculos.Take(1).ToList();
            }

            // Carga paralela de catálogos maestros de tránsito (Útil para el registro nuevo)
            Task<List<Marca>> tareaMarcas = GruposService.GetAll();
            Task<List<TipoVehiculo>> tareaClases = Claseservice.GetAll();
            Task<List<Color>> tareaColores = Clasescolor.GetAll();

            await Task.WhenAll(tareaMarcas, tareaClases, tareaColores);

            customers = tareaMarcas.Result ?? new List<Marca>();
            Clasesveh = tareaClases.Result ?? new List<TipoVehiculo>();
            ClasesColores = tareaColores.Result ?? new List<Color>();
        }
        catch (Exception ex)
        {
            MostrarAlerta($"Error crítico al cargar catálogos maestros de tránsito: {ex.Message}", "danger");
        }
        finally
        {
            _isLoading = false;
        }
    }

    // 🔥 CORREGIDO: Redirige de forma nativa a la nueva página independiente al dar clic en la fila o editar
    private void GestionarEditar(VehiculoDetalleDto item)
    {
        if (item == null) return;
        
        // Navegamos directamente al componente dedicado pasando el ID por URL
        NavigationManager.NavigateTo($"/Vehiculos/Editar/{item.Id}");
    }

    private void SeleccionarFila(VehiculoDetalleDto item)
    {
        selectedEmployees = new List<VehiculoDetalleDto> { item };
        GestionarEditar(item);
    }

    private async Task CargarLineas()
    {
        if (vehiculo != null && vehiculo.MarcaId > 0)
        {
            lines = await LineasService.GetListByMarca(vehiculo.MarcaId) ?? new List<Linea>();
        }
        else
        {
            lines = new List<Linea>();
        }
    }

    private async Task BuscarNombreInfractor()
    {
        if (string.IsNullOrWhiteSpace(vehiculo.Propietario.Documento))
            return;

        LimpiarAlerta();
        try
        {
            var nombre = await PropietarioService.GetNameByDocumento(
                vehiculo.Propietario.Documento,
                _propietario.TipoDocumento);

            if (!string.IsNullOrEmpty(nombre))
            {
                rnombre = nombre;
            }
            else
            {
                rnombre = "No encontrado en base de datos";
                MostrarModal2();
            }

            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            MostrarAlerta($"Fallo al validar documento del propietario: {ex.Message}", "danger");
        }
    }

    private async Task Addveh()
    {
        if (!IsValid(vehiculo))
        {
            MostrarAlerta("Por favor complete todos los campos obligatorios antes de continuar.", "warning");
            return;
        }

        try
        {
            if (!string.IsNullOrEmpty(vehiculo.Placa))
                vehiculo.Placa = vehiculo.Placa.ToUpper().Trim();

            var resultado = await VehiculosService.Add(vehiculo);
            if (resultado == 0)
            {
                MostrarAlerta($"La placa {vehiculo.Placa} ya se encuentra registrada en el sistema.", "warning");
            }
            else
            {
                await CerrarModal();
                var todos = await VehiculosService.GetAll();
                ListVehiculos = todos?.ToList() ?? new();
                MostrarAlerta("Vehículo guardado y registrado correctamente de forma oficial.", "success");
            }
        }
        catch (Exception ex)
        {
            MostrarAlerta($"Error técnico en la inserción de datos: {ex.Message}", "danger");
        }
    }
    
    private bool IsValid(Vehiculo v)
    {
        if (v == null) return false;
        if (string.IsNullOrWhiteSpace(v.Placa) || v.Placa.Length < 5) return false;
        if (string.IsNullOrWhiteSpace(v.Propietario.Documento)) return false;
        if (v.MarcaId <= 0) return false;
        if (v.TipoVehiculoId <= 0) return false;
        if (v.Propietario.TipoDocumento <= 0) return false;

        return true;
    }

    private async Task findbycedula()
    {
        _isLoading = true;
        LimpiarAlerta();
        try
        {
            if (string.IsNullOrWhiteSpace(buscarnit))
            {
                var todos = await VehiculosService.GetAll();
                ListVehiculos = todos ?? new();
            }
            else
            {
                var todos = await VehiculosService.GetAll();
                ListVehiculos = todos?.Where(x => x.Placa.Contains(buscarnit.ToUpper())).ToList() ?? new();
            }
        }
        catch (Exception ex)
        {
            MostrarAlerta($"Error al filtrar la consulta en la grilla: {ex.Message}", "danger");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task Agregar_Prop()
    {
        if (string.IsNullOrWhiteSpace(_propietario.Direccion))
        {
            MostrarAlerta("Debe especificar la dirección urbana de notificación para el propietario.", "warning");
            return;
        }
        if (string.IsNullOrWhiteSpace(_propietario.Telefono))
        {
            MostrarAlerta("El número de teléfono/celular es obligatorio para RUNT.", "warning");
            return;
        }

        _propietario.Documento = vehiculo.Propietario.Documento;

        try
        {
            int result = await VehiculosService.GenerarPropietarioAsociado(vehiculo.Placa, _propietario);

            if (result > 0)
            {
                _mostrarModal2 = false;
                await BuscarNombreInfractor();
            }
            else
            {
                MostrarAlerta("La transacción fue rechazada por la base de datos de propietarios.", "danger");
            }
        }
        catch (Exception ex)
        {
            MostrarAlerta($"Error al registrar ciudadano: {ex.Message}", "danger");
        }
    }

    public void MostrarModal(bool modoEditar)
    {
        esModoEditar = modoEditar;
        LimpiarAlerta();

        if (!esModoEditar)
        {
            vehiculo = new Vehiculo
            {
                TipoVehiculoId = 1,
                MarcaId = 0,
                LineaId = 0,
                PagoHasta = DateTime.UtcNow.Year
            };
            rnombre = string.Empty;
            lines = new List<Linea>();
            _mostrarModal = true;
        }
    }

    private async Task CerrarModal()
    {
        _mostrarModal = false;
        selectedEmployees = null;
        await InvokeAsync(StateHasChanged);
    }

    private void CerrarModal2() => _mostrarModal2 = false;

    public void MostrarModal2()
    {
        _propietario = new Propietario
        {
            Documento = vehiculo.Propietario.Documento,
            TipoDocumento = _propietario.TipoDocumento
        };
        _mostrarModal2 = true;
    }

    private void MostrarAlerta(string mensaje, string tipo)
    {
        alertaMensaje = mensaje;
        alertaTipo = tipo;
    }

    private void LimpiarAlerta()
    {
        alertaMensaje = string.Empty;
    }
}