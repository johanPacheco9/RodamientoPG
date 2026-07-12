using Domain.Models.ProcesoLiquidacion;
using Domain.Responses.Users.Enums;
using Domain.Responses.Vehiculos.Enums;
namespace Domain.Models.Vehiculos.Responses;

public record VehiculoDetailDto(
    int Id,
    string Placa,
    int Modelo,
    int Cilindraje,
    int CapacidadCarga,
    int Pasajeros,
    int TipoVehiculoId,
    int MarcaId,
    int LineaId,
    int ColorId,
    TipoServicioVehiculo TipoServicioVehiculo,
    int TipoCarroceriaId,
    int PropietarioId,
    string DocumentoPropietario,
    TipoDocumento TipoDocumentoPropietario
);