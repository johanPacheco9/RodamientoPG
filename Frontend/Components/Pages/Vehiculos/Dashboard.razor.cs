using Domain.Models;
using Domain.Models.Vehiculos;
using Microsoft.JSInterop;

namespace Frontend.Components.Pages.Vehiculos;

public partial class Dashboard
{
    // Variables de Estado de la UI y Alertas Nativas de Bootstrap
    private bool _mostrarModal = false;
    private bool _mostrarModal2 = false;
    private bool _isLoading = false;
    private bool esModoEditar = false;
    
    private string buscarnit = string.Empty;
    public string rnombre = string.Empty;
    private string alertaMensaje = string.Empty;
    private string alertaTipo = "danger"; // danger, success, warning, info

    // Modelos de Datos del Dominio de Tránsito
    private Vehiculo vehiculo = new();
    private Propietario _propietario = new();
    private List<Vehiculo> ListVehiculos = new();
    private IList<Vehiculo>? selectedEmployees;

    // Colecciones Maestras para los Selects de Bootstrap
    private IEnumerable<TipoVehiculo> Clasesveh = new List<TipoVehiculo>();
    private IEnumerable<Marca> customers = new List<Marca>();
    private IEnumerable<Linea> lines = new List<Linea>();
    private IEnumerable<Color> ClasesColores = new List<Color>();

    protected async override Task OnInitializedAsync()
    { 
        _isLoading = true;
        LimpiarAlerta();
        try
        {
            var todosLosVehiculos = await VehiculosService.GetAll();
            ListVehiculos = todosLosVehiculos ?? new List<Vehiculo>();
            
            if (ListVehiculos.Any())
            {
                selectedEmployees = ListVehiculos.Take(1).ToList();
            }

            // Cargas en paralelo optimizadas para acelerar el renderizado del ERP
            var cargarMarcas = GruposService.GetAll();
            var cargarClases = Claseservice.GetAll();
            var cargarColores = Clasescolor.GetAll();

            await Task.WhenAll(cargarMarcas, cargarClases, cargarColores);

            customers = cargarMarcas.Result ?? new List<Marca>();
            Clasesveh = cargarClases.Result ?? new List<TipoVehiculo>();
            ClasesColores = cargarColores.Result ?? new List<Color>();
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

    private async Task OnRowSelect(Vehiculo item)
    {
        if (item == null) return;

        _isLoading = true;
        LimpiarAlerta();
        vehiculo = item;

        try
        {
            if (!string.IsNullOrWhiteSpace(vehiculo.Placa))
            {
                var result = await ComparendoService.GetCarteraByPlaca(vehiculo.Placa.Trim());
                if (result != null)
                    rnombre = result.NombrePropietario ?? string.Empty;
            }

            await CargarLineas();
            esModoEditar = true;
            _mostrarModal = true;
        }
        catch (Exception ex)
        {
            MostrarAlerta($"Error al procesar la fila del vehículo seleccionado: {ex.Message}", "warning");
        }
        finally
        {
            _isLoading = false;
        }
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

    private async Task capturar(object value)
    {
        // Disparado de forma nativa al cambiar el select de la Marca
        await CargarLineas();
    }

    private async Task BuscarNombreInfractor()
    {
        if (string.IsNullOrWhiteSpace(vehiculo.DocumentoPropietario))
            return;

        LimpiarAlerta();
        try
        {
            var nombre = await PropietarioService.GetNameByDocumento(
                vehiculo.DocumentoPropietario,
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

    private async Task Actualizar(Vehiculo v)
    {
        if (!IsValid(v))
        {
            MostrarAlerta("No es posible actualizar. Hay campos obligatorios pendientes.", "warning");
            return;
        }

        try
        {
            if (!string.IsNullOrEmpty(v.Placa))
                v.Placa = v.Placa.ToUpper().Trim();

            var count = await VehiculosService.Edit(v);
            if (count > 0)
            {
                await CerrarModal();
                var todos = await VehiculosService.GetAll();
                ListVehiculos = todos?.ToList() ?? new();
                MostrarAlerta("Datos del automotor actualizados correctamente.", "success");
            }
            else
            {
                MostrarAlerta("No se detectaron modificaciones en los campos del vehículo.", "info");
            }
        }
        catch (Exception ex)
        {
            MostrarAlerta($"Error técnico al editar registro: {ex.Message}", "danger");
        }
    }

    private bool IsValid(Vehiculo v)
    {
        if (v == null) return false;
        if (string.IsNullOrWhiteSpace(v.Placa) || v.Placa.Length < 5) return false;
        if (string.IsNullOrWhiteSpace(v.DocumentoPropietario)) return false;
        if (v.MarcaId <= 0) return false;
        if (v.TipoVehiculoId <= 0) return false;
        if (v.TipoIdentificacionId <= 0) return false;
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
                ListVehiculos = todos?.Take(18).ToList() ?? new();
            }
            else
            {
                // Búsqueda directa por el documento del propietario o placa según lo definido
                var resultado = await VehiculosService.GetVehiculosByDocumentoPropietario(buscarnit.Trim());
                ListVehiculos = resultado ?? new();
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
        
        _propietario.Documento = vehiculo.DocumentoPropietario;

        try
        {
            int result = await VehiculosService.GenerarPropietarioAsociado(vehiculo.Placa, _propietario);

            if (result > 0)
            {
                _mostrarModal2 = false;
                await BuscarNombreInfractor(); // Refresca la información del titular en el modal principal
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

    private async Task Recalcular()
    {
        if (!esModoEditar)
        {
            MostrarAlerta("Operación rechazada. Debe guardar formalmente el vehículo antes de procesar una liquidación.", "warning");
            return;
        }

        // Llamada nativa JS confirm para simular cuadros de diálogo limpios sin Radzen
        bool confirm = await JsRuntime.InvokeAsync<bool>("confirm", "¿Desea proceder con el recálculo masivo de impuestos para esta placa coactiva?");

        if (confirm)
        {
            try
            {
                var comp = await ComparendoService.GetCarteraByPlaca(vehiculo.Placa);
                if (comp == null || comp.VigenciaDesde == 0)
                {
                    MostrarAlerta("Información faltante. Genere la pre-deuda o estado de cartera preliminar primero.", "info");
                    return;
                }
                
                await ComparendoService.GeneraDeuda(vehiculo.Placa, comp.VigenciaDesde, comp.VigenciaHasta);
                await CerrarModal();
                MostrarAlerta("Recálculo financiero de cartera finalizado con éxito.", "success");
            }
            catch (Exception ex)
            {
                MostrarAlerta($"Fallo al ejecutar recálculo del proceso de liquidación: {ex.Message}", "danger");
            }
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
                TipoIdentificacionId = 1,
                TipoVehiculoId = 1,
                MarcaId = 0,
                LineaId = 0,
                PagoHasta = DateTime.UtcNow.Year
            };
            rnombre = string.Empty;
            lines = new List<Linea>();
        }

        _mostrarModal = true;
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
            Documento = vehiculo.DocumentoPropietario,
            TipoDocumento = _propietario.TipoDocumento
        };
        _mostrarModal2 = true;
    }

    // Métodos auxiliares internos de control de notificaciones Bootstrap
    private void MostrarAlerta(string mensaje, string tipo)
    {
        alertaMensaje = mensaje;
        alertaTipo = tipo;
    }

    private void LimpiarAlerta()
    {
        alertaMensaje = string.Empty;
    }
    private void GestionarEditar(Vehiculo item)
    {
        // Asignamos el vehículo actual a la variable del modal
        vehiculo = item;
    
        // Cargamos el nombre del propietario si ya existe
        rnombre = item.Propietario?.Nombre ?? string.Empty;
    
        // Cargamos las líneas asociadas a la marca de este vehículo
        _ = CargarLineas(); 
    
        // Abrimos el modal en modo edición
        MostrarModal(true);
    }

    private void SeleccionarFila(Vehiculo item)
    {
        selectedEmployees = new List<Vehiculo> { item };
        // Ejecutamos tu lógica original de selección de fila (buscar multas, etc.)
        _ = OnRowSelect(item);
    }
}