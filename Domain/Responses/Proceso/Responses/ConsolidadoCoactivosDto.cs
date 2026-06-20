namespace Domain.Responses.Proceso.Responses;

public record ConsolidadoCoactivosDto(
    int? NumeroProceso,
    DateTime FechaProceso,
    int TotalVehiculosAfectados
);