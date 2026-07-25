using Domain.Models.Acuerdos.Enums;
namespace Domain.Models.Acuerdos.Responses;

public class AcuerdoPagoDto
{
    public int Id { get; set; }
    public string NumeroAcuerdo { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public DateTime FechaSuscripcion { get; set; }
    
    public int NumeroCuotas { get; set; }

    public decimal ValorCapitalFinanciado { get; set; }
    public decimal ValorInteresFinanciado { get; set; }
    public decimal ValorCuotaInicial { get; set; }
    public decimal ValorTotalFinanciado { get; set; }

    public decimal SaldoPendiente { get; set; }

    public EstadoAcuerdoPago Estado { get; set; } = EstadoAcuerdoPago.Vigente;
    public string? Observaciones { get; set; }

    public int VehiculoId { get; set; }
    public int? ProcesoId { get; set; }

    // --- NUEVO: Información del Proceso y Vigencias ---
    public int? NumeroProceso { get; set; }
    public int? VigenciaDesde { get; set; }
    public int? VigenciaHasta { get; set; }

    // Colección de cuotas
    public List<CuotaAcuerdoDto> Cuotas { get; set; } = [];
}