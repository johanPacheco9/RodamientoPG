using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace Domain.Models.Resoluciones;

public enum TipoResolucion
{
    [Display(Name = "Traslado")]
    Traslado = 10,
    
    [Display(Name = "AnulacionDeuda")] 
    AnulacionDeuda = 20,

    [Display(Name = "Particular")]
    Particular = 30,
}