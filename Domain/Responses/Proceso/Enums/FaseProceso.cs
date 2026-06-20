using System.ComponentModel.DataAnnotations;
namespace Domain.Responses.Proceso.Enums;

public enum FaseProceso
{
    [Display(Name = "Persuasivo")]
    Persuasivo = 1,

    [Display(Name = "Mandamiento de Pago")]
    MandamientoPago = 2,

    [Display(Name = "Medidas Cautelares")]
    MedidasCautelares = 3,

    [Display(Name = "Coactivo")]
    Coactivo = 4
}