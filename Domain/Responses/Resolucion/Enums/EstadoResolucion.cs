using System.ComponentModel.DataAnnotations;
namespace Domain.Responses.Resolucion.Enums;

public enum EstadoResolucion
{
    [Display(Name = "Activa")]
    Activa = 'A',

    [Display(Name = "Anulada")]
    Anulada = 'N',

    [Display(Name = "Revocada")]
    Revocada = 'R'
}