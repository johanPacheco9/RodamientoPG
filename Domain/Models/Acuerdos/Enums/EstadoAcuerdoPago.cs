using System.ComponentModel.DataAnnotations;
namespace Domain.Models.Acuerdos.Enums;

public enum EstadoAcuerdoPago
{
    Vigente = 1,    // El usuario está al día o pagando normalmente
    Pagado = 2,     // Cumplió con el 100% de las cuotas
    Incumplido = 3, // Dejó de pagar (se rompe la facilidad de pago)
    Anulado = 4,  
    
    [Display(Name = "Finalizado")]
    Finalizado = 5
}