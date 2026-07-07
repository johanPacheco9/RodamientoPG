namespace Domain.Models.Vehiculos.Responses;


public record VehiculoDetalleDto(
    int Id,
    string Placa,
    int Modelo,
    int Cilindraje,
    int PagoHasta,
    string DocumentoPropietario,
    string TipoVehiculo,
    string Marca,
    string Linea,
    string EstadoProceso
);

