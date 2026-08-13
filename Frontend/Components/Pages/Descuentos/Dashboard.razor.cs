using Domain.Models;
using Infrastructure.Services.Descuentos;
using Microsoft.AspNetCore.Components;
namespace Frontend.Components.Pages.Descuentos; // Ajusta el namespace según la ubicación real de tus páginas

public partial class Dashboard: ComponentBase
{
    [Inject] DescuentosManager DescuentosManager { get; set; } = null!;
    private List<Descuento>? _descuentos;
    private bool _cargando = true;
    private bool _guardando;
    private string? _mensajeError;

    // Formulario Crear/Editar
    private bool _mostrarModalForm;
    private int? _editandoId;
    private DescuentoFormModel _descuentoForm = new();

    // Eliminar
    private bool _mostrarModalEliminar;
    private Descuento? _descuentoAEliminar;

    protected override async Task OnInitializedAsync()
    {
        await CargarDescuentos();
    }

    private async Task CargarDescuentos()
    {
        _cargando = true;
        try
        {
            _descuentos = await DescuentosManager.GetAll();
        }
        finally
        {
            _cargando = false;
        }
    }

    private void AbrirModalCrear()
    {
        _editandoId = null;
        _descuentoForm = new DescuentoFormModel
        {
            Desde = DateTime.Now,
            Hasta = DateTime.Now.AddDays(30)
        };
        _mensajeError = null;
        _mostrarModalForm = true;
    }

    private void AbrirModalEditar(Descuento descuento)
    {
        _editandoId = descuento.Id;
        _descuentoForm = new DescuentoFormModel
        {
            Desde = descuento.Desde,
            Hasta = descuento.Hasta,
            Porcentaje = descuento.Porcentaje
        };
        _mensajeError = null;
        _mostrarModalForm = true;
    }

    private void CerrarModalForm()
    {
        _mostrarModalForm = false;
        _mensajeError = null;
    }

    private async Task GuardarDescuento()
    {
        _guardando = true;
        _mensajeError = null;

        try
        {
            (bool Exito, string Mensaje, Descuento? Descuento) resultado;

            if (_editandoId is null)
            {
                resultado = await DescuentosManager.Create(_descuentoForm.Desde, _descuentoForm.Hasta, _descuentoForm.Porcentaje);
            }
            else
            {
                resultado = await DescuentosManager.Update(_editandoId.Value, _descuentoForm.Desde, _descuentoForm.Hasta, _descuentoForm.Porcentaje);
            }

            if (!resultado.Exito)
            {
                _mensajeError = resultado.Mensaje;
                return;
            }

            _mostrarModalForm = false;
            await CargarDescuentos();
        }
        finally
        {
            _guardando = false;
        }
    }

    private void AbrirModalEliminar(Descuento descuento)
    {
        _descuentoAEliminar = descuento;
        _mensajeError = null;
        _mostrarModalEliminar = true;
    }

    private void CerrarModalEliminar()
    {
        _mostrarModalEliminar = false;
        _descuentoAEliminar = null;
        _mensajeError = null;
    }

    private async Task ConfirmarEliminar()
    {
        if (_descuentoAEliminar is null) return;

        _guardando = true;
        _mensajeError = null;

        try
        {
            var resultado = await DescuentosManager.Delete(_descuentoAEliminar.Id);

            if (!resultado.Exito)
            {
                _mensajeError = resultado.Mensaje;
                return;
            }

            _mostrarModalEliminar = false;
            _descuentoAEliminar = null;
            await CargarDescuentos();
        }
        finally
        {
            _guardando = false;
        }
    }

    private class DescuentoFormModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "La fecha inicial es obligatoria.")]
        public DateTime Desde { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "La fecha final es obligatoria.")]
        public DateTime Hasta { get; set; }

        [System.ComponentModel.DataAnnotations.Range(0.01, 100, ErrorMessage = "El porcentaje debe estar entre 0 y 100.")]
        public decimal Porcentaje { get; set; }
    }
}