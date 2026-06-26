using Domain.Responses.Recibo.Enums;
namespace Domain.Models.Recibos.Responses;

public record ReciboDto(
    int Id,
    EstadoRecibo Estado,
    DateTime Fecha,
    DateTime? FechaPago,
    decimal ValorCapital,
    decimal InteresMora,
    decimal Descuento,
    decimal Estampillas,
    decimal ValorTotalSistema,
    decimal ValorCargaDatos,
    decimal ValorRodamiento,
    decimal ValorTotalCalculado,
    int Desde,
    int Hasta,
    bool EstaCompleto,
    string PlacaVehiculo,
    List<ReciboDetalleDto> Detalles // Cambiado de Vigencias a Detalles para hacer match con el modelo
);