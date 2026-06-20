namespace Infrastructure.Services.Vehiculos.Responses;

public record PlacasByCedulaResponse(
    int Id, 
    string? Placa,
    string? ClaseVehiculo, 
    string? Marca,         
    string? Linea,      
    int? Modelo
);