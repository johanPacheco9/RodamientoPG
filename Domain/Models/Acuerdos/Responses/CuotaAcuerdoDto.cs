using Domain.Models.Acuerdos.Enums;
namespace Domain.Models.Acuerdos.Responses;

/// <summary>
/// DTO con el detalle individual de cada cuota del acuerdo.
/// </summary>
public class CuotaAcuerdoDto
{
    public int Id { get; set; }
    public int NumeroCuota { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public DateTime? FechaPago { get; set; }
    public decimal ValorCuota { get; set; }
    public EstadoCuotaAcuerdo EstadoCuota { get; set; }
    public decimal InteresFinanciacion { get; set; }
    public decimal ValorTotal => ValorCuota + InteresFinanciacion;
    public bool IsPagado { get; set; }
}