using System.ComponentModel.DataAnnotations;

namespace Domain.Responses.Liquidacion.Enums;

public enum TipoConceptoTarifa
{
    [Display(Name = "Carga")]
    Carga = 1,

    [Display(Name = "Pasajeros")]
    Pasajeros = 2,

    [Display(Name = "Rodamiento")]
    Rodamiento = 3
}
