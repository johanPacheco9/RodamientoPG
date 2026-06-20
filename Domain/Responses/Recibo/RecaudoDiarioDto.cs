using Domain.Responses.Recibo.Enums;
namespace Domain.Responses.Recibo;

public record RecaudoDiarioDto(
    int ReciboId,
    DateTime Fecha,
    EstadoRecibo Estado,
    string Placa,
    string DocumentoPropietario,
    string NombrePropietario,
    int PeriodoDesde,
    int PeriodoHasta,
    decimal ValorImpuesto,
    decimal ValorSancion,
    decimal ValorInteresMora,
    decimal ValorCostas,
    decimal ValorEstampillas,
    decimal ValorSistema,
    decimal ValorCargaDatos,
    decimal Descuento,
    decimal ValorTotal
);