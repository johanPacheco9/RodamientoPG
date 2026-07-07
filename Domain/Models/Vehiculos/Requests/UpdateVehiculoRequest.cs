using Domain.Responses.Users.Enums;
using Domain.Responses.Vehiculos.Enums;
namespace Domain.Models.Vehiculos.Requests;

public class UpdateVehiculoRequest
{
    public int Id { get; set; }
    public string Placa { get; set; } = string.Empty;
    public int Modelo { get; set; }
    public int Cilindraje { get; set; }
    public int CapacidadCarga { get; set; }
    public int Pasajeros { get; set; }
    public string DocumentoPropietario { get; set; } = string.Empty;
    public TipoDocumento TipoDocumento {get; set; }
    public int TipoVehiculoId { get; set; } 
    public int MarcaId { get; set; }
    public int LineaId { get; set; }
    public int ColorId { get; set; } 
    public TipoServicioVehiculo TipoServicioVehiculo { get; set; } // O el Enum si lo prefieres directo
    public int TipoCarroceriaId { get; set; }
    public int PropietarioId { get; set; }
}