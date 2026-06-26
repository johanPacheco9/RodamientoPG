using System.ComponentModel.DataAnnotations;
namespace Domain.Responses.Recibo.Enums;

public enum EstadoRecibo
{
    [Display(Name = "Pendiente")]
    Pendiente = 10,

    //Hecho pero sin confirmación del pago aún. el tránsito no ha sido autorizado para recibir pagos.
    [Display(Name = "Aplicado")]
    Aplicado = 20,

    [Display(Name = "Pagado")]
    Pagado = 30,

    [Display(Name = "Anulado")]
    Anulado = 50
}
