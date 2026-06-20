using System.ComponentModel.DataAnnotations;

namespace Domain.Responses.Liquidacion.Enums;

public enum ConceptoSistema
{
    [Display(Name = "Rodamiento")]
    Rodamiento = 1,

    [Display(Name = "Estampillas")]
    Estampillas = 2,

    [Display(Name = "Costas")]
    Costas = 3,

    [Display(Name = "Carga")]
    Carga = 4,

    [Display(Name = "Sancion")]
    Sancion = 6,

    [Display(Name = "Sistematizacion")]
    Sistematizacion = 99
}
