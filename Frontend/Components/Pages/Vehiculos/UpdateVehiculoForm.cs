using System.ComponentModel.DataAnnotations;
using Domain.Models.ProcesoLiquidacion;
using Domain.Responses.Users.Enums;
using Domain.Responses.Vehiculos.Enums;

namespace Frontend.Components.Pages.Vehiculos;

public class UpdateVehiculoForm
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La placa es obligatoria.")]
    [StringLength(6, MinimumLength = 5, ErrorMessage = "La placa debe tener entre 5 y 6 caracteres.")]
    [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "La placa solo debe contener letras mayúsculas y números.")]
    public string Placa { get; set; } = string.Empty;

    [Required(ErrorMessage = "El modelo (año) es obligatorio.")]
    [Range(1900, 2027, ErrorMessage = "Incorpore un año de modelo válido.")]
    public int Modelo { get; set; }

    [Range(0, 20000, ErrorMessage = "El cilindraje debe ser un valor positivo.")]
    public int Cilindraje { get; set; }

    [Range(0, 100000, ErrorMessage = "La capacidad de carga debe ser un valor válido.")]
    public int CapacidadCarga { get; set; }

    [Range(0, 150, ErrorMessage = "El cupo de pasajeros debe ser un número coherente.")]
    public int Pasajeros { get; set; }

    [Required(ErrorMessage = "El documento del propietario es obligatorio.")]
    public string DocumentoPropietario { get; set; } = string.Empty;

    // Cambiado para usar el Enum solicitado
    public TipoDocumento TipoDocumentoForm { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una clase de vehículo válida.")]
    public int TipoVehiculoId { get; set; } 

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una marca.")]
    public int MarcaId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una línea homologada.")]
    public int LineaId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe asignar un color registrado.")]
    public int ColorId { get; set; } 

    public TipoServicioVehiculo TipoServicioVehiculo { get; set; }

    public int TipoCarroceriaId { get; set; }

    public EstadoProceso EstadoProcesoId { get; set; } 
    
    public int PropietarioId { get; set; }
}