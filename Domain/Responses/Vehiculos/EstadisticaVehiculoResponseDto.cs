namespace Domain.Responses.Vehiculos;

public record EstadisticaVehiculoResponseDto
(
    string Nombre,
    int? Clase,
    int valor
);