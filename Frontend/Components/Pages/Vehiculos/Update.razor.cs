using Domain.Models.Vehiculos;
using Domain.Models.Vehiculos.Requests;
using Domain.Models.Vehiculos.Responses;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Infrastructure.Services.Colores;
using Infrastructure.Services.Lineas;
using Infrastructure.Services.Marcas;
using Infrastructure.Services.Propietarios;
using Infrastructure.Services.TiposVehiculos;
using Infrastructure.Services.Vehiculos;

namespace Frontend.Components.Pages.Vehiculos;

public partial class Update
{
    [Parameter] public int Id { get; set; }

    // Inyecciones de dependencias obligatorias para la clase parcial
    [Inject] public NavigationManager NavigationManager { get; set; } = null!;
    [Inject] public VehiculosService VehiculosService { get; set; } = null!;
    [Inject] public PropietarioService PropietarioService { get; set; } = null!;
    [Inject] public MarcaService MarcaService { get; set; } = null!;
    [Inject] public LineasService LineasService { get; set; } = null!;
    [Inject] public TiposService TiposService { get; set; } = null!;
    [Inject] public ColoresService ColoresService { get; set; } = null!;
    [Inject] public IJSRuntime JsRuntime { get; set; } = null!;

    private UpdateVehiculoForm? _formModel;
    private VehiculoDetailDto? _vehiculoOriginal;
    private bool _isProcessing = true;
    private string _nombrePropietario = string.Empty;

    // Catálogos auxiliares
    private List<Marca> _marcas = new();
    private List<Linea> _lineas = new();
    private List<TipoVehiculo> _clases = new();
    private List<Color> _colores = new();
    
    

    protected async override Task OnInitializedAsync()
    {
        try
        {
            _isProcessing = true;

            // Secuencial: el DbContext scoped no soporta acceso concurrente desde varios hilos
            _marcas = (await MarcaService.GetAll() ?? new()).ToList();
            _lineas = (await LineasService.GetAll() ?? new()).ToList();
            _clases = (await TiposService.GetAll() ?? new()).ToList();
            _colores = (await ColoresService.GetAll() ?? new()).ToList();

            // Obtener vehículo original
            _vehiculoOriginal = await VehiculosService.GetById(Id);

            if (_vehiculoOriginal != null)
            {
                _formModel = new UpdateVehiculoForm
                {
                    Id = _vehiculoOriginal.Id,
                    Placa = _vehiculoOriginal.Placa,
                    Modelo = _vehiculoOriginal.Modelo,
                    Cilindraje = _vehiculoOriginal.Cilindraje,
                    CapacidadCarga = _vehiculoOriginal.CapacidadCarga,
                    Pasajeros = _vehiculoOriginal.Pasajeros,
                    DocumentoPropietario = _vehiculoOriginal.DocumentoPropietario,
                    TipoDocumentoForm = _vehiculoOriginal.TipoDocumentoPropietario,
                    TipoVehiculoId = _vehiculoOriginal.TipoVehiculoId,
                    MarcaId = _vehiculoOriginal.MarcaId,
                    LineaId = _vehiculoOriginal.LineaId,
                    ColorId = _vehiculoOriginal.ColorId,
                    TipoServicioVehiculo = _vehiculoOriginal.TipoServicioVehiculo,
                    TipoCarroceriaId = _vehiculoOriginal.TipoCarroceriaId,
                    PropietarioId = _vehiculoOriginal.PropietarioId
                };

                await BuscarNombrePropietario();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error inicializando vista de edición: {ex.Message}");
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private async Task BuscarNombrePropietario()
    {
        if (_formModel != null && !string.IsNullOrWhiteSpace(_formModel.DocumentoPropietario))
        {
            var response = await PropietarioService.GetNameByDocumento(_formModel.DocumentoPropietario, _formModel.TipoDocumentoForm);
            _nombrePropietario = response != null ? response : "CIUDADANO NO ENCONTRADO EN SISTEMA";
        }
    }

    private async Task ProcesarActualizacion()
    {
        if (_formModel == null) return;

        try
        {
            _isProcessing = true;

            // Construimos el Request mapeando las propiedades del updateform
            var request = new UpdateVehiculoRequest
            {
                Id = _formModel.Id,
                Placa = _formModel.Placa.ToUpper(),
                Modelo = _formModel.Modelo,
                Cilindraje = _formModel.Cilindraje,
                CapacidadCarga = _formModel.CapacidadCarga,
                Pasajeros = _formModel.Pasajeros,
                DocumentoPropietario = _formModel.DocumentoPropietario,
                TipoDocumento = _formModel.TipoDocumentoForm,
                TipoVehiculoId = _formModel.TipoVehiculoId,
                MarcaId = _formModel.MarcaId,
                LineaId = _formModel.LineaId,
                ColorId = _formModel.ColorId,
                TipoServicioVehiculo = _formModel.TipoServicioVehiculo,
                TipoCarroceriaId = _formModel.TipoCarroceriaId,
                PropietarioId = _formModel.PropietarioId
            };

            // Despachamos el request al servicio
            int filasAfectadas = await VehiculosService.Edit(request);

            if (filasAfectadas > 0)
            {
                await JsRuntime.InvokeVoidAsync("alert", "Registro automotor modificado de manera exitosa.");
                Volver();
            }
        }
        catch (Exception ex)
        {
            await JsRuntime.InvokeVoidAsync("alert", $"Error al guardar: {ex.Message}");
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private void Volver()
    {
        NavigationManager.NavigateTo("/Vehiculos");
    }
}