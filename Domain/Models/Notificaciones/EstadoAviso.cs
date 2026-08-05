using System.ComponentModel.DataAnnotations;
namespace Domain.Models.Notificaciones;

public enum EstadoAviso
{
    [Display(Name = "Enviado")]
    Enviado = 1,

    [Display(Name = "Pasajeros")]
    Pasajeros = 2,

    [Display(Name = "Rodamiento")]
    Rodamiento = 3
}