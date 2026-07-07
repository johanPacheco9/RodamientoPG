using Domain.Responses.Vehiculos.Enums;
namespace Domain.Responses.Vehiculos;

public record EstadisticaVehiculoDto(
    string TipoVehiculo,
    TipoServicioVehiculo TipoServicio,
    int CantidadVehiculos,
    decimal? Valor
);