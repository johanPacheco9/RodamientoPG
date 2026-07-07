using System.ComponentModel.DataAnnotations;
namespace Domain.Models.Vehiculos.Enums;

public enum ClaseAgrupacionVehiculo
{
    [Display(Name = "Automóvil")]
    Automovil = 1,

    [Display(Name = "Pasajeros")]
    Pasajeros = 2,

    [Display(Name = "Carga")]
    Carga = 3,

    [Display(Name = "Motocicleta")]
    Motocicleta = 4
}