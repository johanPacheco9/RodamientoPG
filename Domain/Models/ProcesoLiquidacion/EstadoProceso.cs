using System.ComponentModel.DataAnnotations;
namespace Domain.Models.ProcesoLiquidacion;

public enum EstadoProceso
{
   [Display(Name = "Persuasivo")]
   Persuasivo = 10,
   
   [Display(Name = "Mandamiento de pago")]
   MandamientoPago = 20,

   [Display(Name = "Sin proceso")]
   SinProceso = 0,
   
   [Display(Name = "Coactivo")]
   Coactivo = 50
}
