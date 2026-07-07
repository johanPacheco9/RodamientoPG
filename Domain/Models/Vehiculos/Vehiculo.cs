using System.ComponentModel.DataAnnotations;
using Domain.Models.ProcesoLiquidacion;
using Domain.Responses.Vehiculos.Enums;

namespace Domain.Models.Vehiculos;

public class Vehiculo : EntityWithTraceability
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "La placa es obligatoria.")]
    [StringLength(6, ErrorMessage = "La placa no puede superar los 6 caracteres.")]
    public string Placa { get; set; } = string.Empty;

    public int Modelo { get; set; }

    public int Cilindraje { get; set; }

    public int PagoHasta { get; set; } 

    public int CapacidadCarga { get; set; }

    public int Pasajeros { get; set; }
    
    // =========================================================
    // 🔗 CLAVES FORÁNEAS (Estándar de la industria: Sufijo Id)
    // =========================================================

    public int TipoVehiculoId { get; set; } 

    public int MarcaId { get; set; }

    public int LineaId { get; set; }

    public int ColorId { get; set; } 

    public TipoServicioVehiculo TipoServicioVehiculo { get; set; }

    public int TipoCarroceriaId { get; set; }

    public EstadoProceso EstadoProcesoId { get; set; } 
    
    public int PropietarioId { get; set; }

    // =========================================================
    // PROPIEDADES DE NAVEGACIÓN (EF Core las mapea automático)
    // =========================================================

    public virtual TipoVehiculo TipoVehiculo { get; set; } = null!;

    public virtual Marca Marca { get; set; } = null!;

    public virtual Linea Linea { get; set; } = null!;

    public virtual Color Color { get; set; } = null!; 

    public EstadoProceso EstadoProceso { get; set; }
    
    public virtual Propietario Propietario { get; set; } = null!;
}


