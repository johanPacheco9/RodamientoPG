using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Domain.Models.ProcesoLiquidacion;

public enum EstadoProceso
{
   [Display(Name = "Persuasivo")]
   Persuasivo = 10,
   
   [Display(Name = "Mamdamiento de pago")]
   MandamientoPago = 20,
   
   [Display(Name = "Coactivo")]
   Coactivo = 50
}