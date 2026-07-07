using System.ComponentModel.DataAnnotations;
namespace Domain.Responses.Vehiculos.Enums;

public enum TipoServicioVehiculo
{
    [Display(Name = "Oficial")]
    Oficial = 1,

    [Display(Name = "Público")]
    Publico = 2,

    [Display(Name = "Particular")]
    Particular = 3,

    [Display(Name = "Otro")]
    Otro = 4,

    [Display(Name = "Diplomático")]
    Diplomatico = 5, 
    
    [Display(Name = "Carga o pasajero")]
    CargaoPasajero = 10
    
}
