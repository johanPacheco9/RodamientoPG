using System.ComponentModel.DataAnnotations;
namespace Domain.Models.Carteras.Enums;

public enum TipoConceptoCartera
{
    [Display(Name = "Derechos de Tránsito / Rodamiento")]
    Rodamiento = 1,

    [Display(Name = "Estampillas")]
    Estampillas = 2,

    [Display(Name = "Costas Persuasivas / Gastos Administrativos")]
    Costas = 3,

    [Display(Name = "Impuesto Adicional (Carga/Pasajeros)")]
    Carga = 4,

    // =========================================================
    // 🚀 CONCEPTOS EXTRA PARA BLINDAR EL MOTOR
    // =========================================================

    [Display(Name = "Sanción por Extemporaneidad")]
    Sancion = 5,

    [Display(Name = "Intereses de Mora")]
    InteresMora = 6,

    [Display(Name = "Derechos de Sistematización / Valor Recibo")]
    Sistematizacion = 7
}