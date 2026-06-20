using Domain.Models.Vehiculos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Frontend.Components.Pages.Propietarios;

public partial class Dashboard : ComponentBase
{
    private bool _mostrarModal = false;
    private string _buscarnit = string.Empty;
    private Propietario infractoresobj = new Propietario();
    private List<Propietario> infractores { get; set; } = new List<Propietario>();

    private bool esModoEditar;
    private bool isLoading = false;

    // Configuración de paginación manual
    private int PaginaActual { get; set; } = 1;
    private int TamanoPagina { get; set; } = 12;
    private int TotalPaginas => (int)Math.Ceiling((double)infractores.Count / TamanoPagina);

    protected override async Task OnInitializedAsync()
    {
        await CargarListaPropietarios();
    }

    private async Task CargarListaPropietarios()
    {
        isLoading = true;
        try
        {
            var todosLosInfractores = await InfractoresService.GetList();
            if (todosLosInfractores != null)
            {
                infractores = todosLosInfractores;
            }
            PaginaActual = 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar propietarios: {ex.Message}");
        }
        finally
        {
            isLoading = false;
        }
    }

    private Task OnRowSelect(Propietario item)
    {
        if (item == null) return Task.CompletedTask;

        infractoresobj = item;
        esModoEditar = true;
        _mostrarModal = true;
        return Task.CompletedTask;
    }

    private async Task Findbycedula()
    {
        if (string.IsNullOrWhiteSpace(_buscarnit))
        {
            await CargarListaPropietarios();
            return;
        }

        isLoading = true;
        try
        {
            var infractoresList = await InfractoresService.GetByCedula(_buscarnit.Trim());
            infractores = infractoresList ?? new List<Propietario>();
            PaginaActual = 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al buscar por cédula: {ex.Message}");
            infractores = new List<Propietario>();
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task Agregar()
    {
        if (string.IsNullOrWhiteSpace(infractoresobj.Documento) || string.IsNullOrWhiteSpace(infractoresobj.Nombre))
        {
            await js.InvokeVoidAsync("alert", "Los campos Documento y Nombre son obligatorios.");
            return;
        }

        try
        {
            var result = await InfractoresService.Add(infractoresobj);
            _mostrarModal = false;
            await js.InvokeVoidAsync("alert", "Propietario agregado con éxito.");
            await CargarListaPropietarios();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al guardar propietario: {ex.Message}");
        }
    }

    private async Task Actualizar(Propietario infractor)
    {
        try
        {
            var count = await InfractoresService.Edit(infractor);
            if (count > 0)
            {
                _mostrarModal = false;
                await js.InvokeVoidAsync("alert", "Propietario actualizado correctamente.");
                await CargarListaPropietarios();
            }
            else
            {
                Console.WriteLine("Error al actualizar el infractor.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar: {ex.Message}");
        }
    }

    public void MostrarModal(bool nkey)
    {
        esModoEditar = nkey;
        if (!esModoEditar)
        {
            infractoresobj = new Propietario
            {
                TipoDocumento = Domain.Responses.Users.Enums.TipoDocumento.Cc,
                Documento = string.Empty,
                Nombre = string.Empty,
                Telefono = string.Empty,
                Direccion = string.Empty
            };
        }
        _mostrarModal = true;
    }
    
    private void CerrarModal()
    {
        _mostrarModal = false;
    }

    // Helpers Lógicos de Paginación en Memoria
    private IEnumerable<Propietario> ObtenerItemsPaginados()
    {
        return infractores.Skip((PaginaActual - 1) * TamanoPagina).Take(TamanoPagina);
    }

    private void CambiarPagina(int nuevaPagina)
    {
        if (nuevaPagina >= 1 && nuevaPagina <= TotalPaginas)
        {
            PaginaActual = nuevaPagina;
        }
    }
}