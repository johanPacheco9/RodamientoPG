using Domain.Models.Vehiculos;
using Microsoft.AspNetCore.Components;
namespace Frontend.Components.Pages.Marcas;

public partial class Dashboard :  ComponentBase
{
    private bool _mostrarModal = false;
    private bool _isLoading = false;
    private bool _esModoEditar = false;
    private string _buscarnit = string.Empty;
    
    // Alertas de la interfaz
    private string _alertaMensaje = string.Empty;
    private string _alertaTipo = "danger";

    private Marca _marca = new();
    private List<Marca> _marcaList = new();
    private IList<Marca> _selectedEmployees = new List<Marca>();

    protected async override Task OnInitializedAsync()
    {
        await CargarMarcasBase();
    }

    private async Task CargarMarcasBase()
    {
        _isLoading = true;
        LimpiarAlerta();
        try
        {
            const int cantidadInicial = 15;
            var todosLasInfracciones = await AgentesServices.GetAll();

            if (todosLasInfracciones != null && todosLasInfracciones.Any())
            {
                _marcaList = todosLasInfracciones.Take(cantidadInicial).ToList();
            }
        }
        catch (Exception ex)
        {
            MostrarAlerta($"Fallo al leer las marcas del servidor VPS: {ex.Message}", "danger");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void OnRowSelect(Marca item)
    {
        _marca = item;
        _esModoEditar = true;
        _mostrarModal = true;
    }

    private async Task findbyplaca()
    {
        if (string.IsNullOrWhiteSpace(_buscarnit))
        {
            await CargarMarcasBase();
            return;
        }

        _isLoading = true;
        LimpiarAlerta();
        try
        {
            var infraccionesList = await AgentesServices.GetByNombre(_buscarnit.Trim());
            _marcaList = infraccionesList ?? new List<Marca>();
        }
        catch (Exception ex)
        {
            MostrarAlerta($"Error en los criterios de búsqueda: {ex.Message}", "warning");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task Agregar()
    {
        if (string.IsNullOrWhiteSpace(_marca.Nombre)) return;

        try
        {
            _marca.Nombre = _marca.Nombre.ToUpper().Trim();
            var result = await AgentesServices.Add(_marca);
            _mostrarModal = false;
            await CargarMarcasBase();
            MostrarAlerta("Marca guardada con éxito en los catálogos.", "success");
        }
        catch (Exception ex)
        {
            MostrarAlerta($"No se pudo añadir el registro: {ex.Message}", "danger");
        }
    }

    void EditarAgentes(Marca agentes)
    {
        _marca = agentes;
        _esModoEditar = true;
        _mostrarModal = true;
    }

    private async Task EliminaAgentes(int id)
    {
        LimpiarAlerta();
       
        bool confirmationResult = await JsRuntime.InvokeAsync<bool>("confirm", 
            new object[] { "¿Está seguro que desea eliminar de forma lógica esta marca del sistema?" });
        
        if (confirmationResult)
        {
            try
            {
                var count = await AgentesServices.Delete(id);
                await CargarMarcasBase();
                MostrarAlerta("Registro removido correctamente.", "info");
            }
            catch (Exception ex)
            {
                MostrarAlerta($"Fallo al intentar remover la marca: {ex.Message}", "danger");
            }
        }
    }

    private async Task Actualizar(Marca agentes)
    {
        if (string.IsNullOrWhiteSpace(agentes.Nombre)) return;

        try
        {
            agentes.Nombre = agentes.Nombre.ToUpper().Trim();
            var count = await AgentesServices.Edit(agentes);

            if (count > 0)
            {
                _mostrarModal = false;
                await CargarMarcasBase();
                MostrarAlerta("Cambios efectuados de manera exitosa.", "success");
            }
            else
            {
                MostrarAlerta("No se realizaron modificaciones en el campo de texto.", "info");
            }
        }
        catch (Exception ex)
        {
            MostrarAlerta($"Error crítico de actualización: {ex.Message}", "danger");
        }
    }

    public void MostrarModal(bool nkey)
    {
        _esModoEditar = nkey;
        LimpiarAlerta();

        if (_esModoEditar && _marca != null)
        {
            // Mantiene el objeto seleccionado
        }
        else
        {
            _marca = new Marca();
        }
        
        _mostrarModal = true;
    }

    private void CerrarModal() => _mostrarModal = false;
    private void MostrarAlerta(string mensaje, string tipo) { _alertaMensaje = mensaje; _alertaTipo = tipo; }
    private void LimpiarAlerta() => _alertaMensaje = string.Empty;
}