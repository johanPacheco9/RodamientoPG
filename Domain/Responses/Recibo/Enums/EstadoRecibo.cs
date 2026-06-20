using System.ComponentModel.DataAnnotations;
namespace Domain.Responses.Recibo.Enums;

public enum EstadoRecibo
{
    [Display(Name = "Pendiente")]
    Pendiente = 10,

    [Display(Name = "Aplicado")]
    Aplicado = 20,

    [Display(Name = "Cancelado")]
    Cancelado = 30,

    [Display(Name = "Anulado")]
    Anulado = 50
}
