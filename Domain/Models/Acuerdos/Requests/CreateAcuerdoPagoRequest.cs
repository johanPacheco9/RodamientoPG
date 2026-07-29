namespace Domain.Models.Acuerdos.Requests;

public class CreateAcuerdoPagoRequest
{
    public int VehiculoId { get; set; }
    public int? ProcesoId { get; set; }
    
    // Lista de IDs de la entidad Cartera seleccionados para financiar
    public List<int> CarteraIds { get; set; } = [];

    public int NumeroCuotas { get; set; }          // Ejemplo: 6, 12, 24
    public decimal ValorCuotaInicial { get; set; } // Si aplica pago inmediato de entrada
    
    public DateTime FechaSuscripcion { get; set; } = DateTime.UtcNow;
    public string? Observaciones { get; set; }
}

