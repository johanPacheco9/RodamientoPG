using Domain.Models.Vehiculos;
namespace Domain.Models.BaseGravable;

public class BaseGravableVehiculo
{
    public int Id { get; set; }
    
    // Código de homologación del Ministerio / Fasecolda (8 dígitos)
    public string Codigo { get; set; } = string.Empty; 

    // Atributos específicos de la ficha técnica
    public int Cilindraje { get; set; }
    public int Capacidad { get; set; }
    public int Pasajeros { get; set; }
    
    /// <summary>
    /// Relaciones.
    /// </summary>
   
    
    //  1. Relación con Marca
    public int MarcaId { get; set; }
    public Marca Marca { get; set; } = null!;

    //  2. Relación con Linea
    public int LineaId { get; set; }
    public Linea Linea { get; set; } = null!;

    //  3. Relación con TipoVehiculo / ClaseVehiculo (Automóvil, Moto, etc.)
    public int TipoVehiculoId { get; set; }
    public TipoVehiculo TipoVehiculo { get; set; } = null!;

    //  Relación con la matriz de precios (Avalúos por año/modelo)
    public ICollection<BaseGravableVigencia> Vigencias { get; set; } = new List<BaseGravableVigencia>();
}