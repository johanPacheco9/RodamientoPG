namespace Domain.Responses.Vehiculos;

public record EstadisticaVehiculoDto(
    string TipoVehiculo,
    string TipoServicio,
    int CantidadVehiculos,
    decimal? Valor
);