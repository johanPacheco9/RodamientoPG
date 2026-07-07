using Domain.Models;
using Microsoft.JSInterop;
namespace Frontend.Components.Pages.Uvts;

public partial class Dashboard
{
    private bool _mostrarModal = false;
    private string _textoBusqueda = string.Empty;
    private Uvt _uvtFormulario = new();
    private List<Uvt> _listadoUvts = new();
    private bool _esModoEditar;
    private bool _isLoading;

    // Variables de control de la paginación manual
    private int PaginaActual { get; set; } = 1;
    private int TamanoPagina { get; set; } = 8;
    private int TotalPaginas => (int)Math.Ceiling((double)_listadoUvts.Count / TamanoPagina);

    protected async override Task OnInitializedAsync()
    {
        await CargarHistoricoUvts();
    }

    private async Task CargarHistoricoUvts()
    {
        _isLoading = true;
        try
        {
            var respuesta = await UvtService.GetAll();
            _listadoUvts = respuesta ?? new List<Uvt>();
            PaginaActual = 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar el histórico de UVTs: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void OnRowSelect(Uvt uvtSeleccionado)
    {
        if (uvtSeleccionado == null) return;

        // Instanciamos un nuevo objeto con los mismos datos para romper la referencia bidireccional en tiempo real en la grilla
        _uvtFormulario = new Uvt
        {
            Id = uvtSeleccionado.Id,
            FechaDesde = uvtSeleccionado.FechaDesde,
            FechaHasta = uvtSeleccionado.FechaHasta,
            Valor = uvtSeleccionado.Valor
        };

        _esModoEditar = true;
        _mostrarModal = true;
    }

    private async Task BuscarUvtPorFecha()
    {
        _isLoading = true;
        try
        {
            var respuesta = await UvtService.GetAll();
            var historicoCompleto = respuesta ?? new List<Uvt>();

            if (string.IsNullOrWhiteSpace(_textoBusqueda))
            {
                _listadoUvts = historicoCompleto;
            }
            else
            {
                string filtro = _textoBusqueda.Trim();
                _listadoUvts = historicoCompleto
                    .Where(u => u.FechaDesde.Year.ToString().Contains(filtro) ||
                                u.FechaHasta.Year.ToString().Contains(filtro) ||
                                u.FechaDesde.ToString("dd/MM/yyyy").Contains(filtro))
                    .ToList();
            }
            PaginaActual = 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error ejecutando el filtro de UVT: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task LimpiarBusqueda()
    {
        _textoBusqueda = string.Empty;
        await CargarHistoricoUvts();
    }

    private async Task ProcesarGuardado()
    {
        if (_uvtFormulario.Valor <= 0)
        {
            await JSRuntime.InvokeVoidAsync("alert", "El valor de la UVT debe ser mayor a cero.");

            return;
        }

        if (_esModoEditar)
            await EjecutarActualizacion();
        else
            await EjecutarInsercion();
    }

    private async Task EjecutarInsercion()
    {
        try
        {
            var result = await UvtService.Add(_uvtFormulario);
            _mostrarModal = false;
            await JSRuntime.InvokeVoidAsync("alert", "Vigencia UVT guardada con éxito.");
            await CargarHistoricoUvts();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inserción UVT: {ex.Message}");
        }
    }

    private async Task EjecutarActualizacion()
    {
        try
        {
            var filasAfectadas = await UvtService.Edit(_uvtFormulario);
            if (filasAfectadas > 0)
            {
                _mostrarModal = false;
                await JSRuntime.InvokeVoidAsync("alert", "Configuración UVT modificada con éxito.");
                await CargarHistoricoUvts();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error actualización UVT: {ex.Message}");
        }
    }

    public void AbrirModalCrear(bool modoEditar)
    {
        _esModoEditar = modoEditar;

        if (!_esModoEditar)
        {
            _uvtFormulario = new Uvt
            {
                FechaDesde = new DateTime(DateTime.Today.Year, 1, 1),
                FechaHasta = new DateTime(DateTime.Today.Year, 12, 31),
                Valor = 0
            };
        }

        _mostrarModal = true;
    }

    private void CerrarModal()
    {
        _mostrarModal = false;
    }

    private IEnumerable<Uvt> ObtenerItemsPaginados()
    {
        return _listadoUvts.Skip((PaginaActual - 1) * TamanoPagina).Take(TamanoPagina);
    }

    private void CambiarPagina(int nuevaPagina)
    {
        if (nuevaPagina >= 1 && nuevaPagina <= TotalPaginas)
        {
            PaginaActual = nuevaPagina;
        }
    }
}