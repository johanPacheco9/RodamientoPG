using Domain.Models.Vehiculos;

namespace Domain.Models.BaseGravable;

/// <summary>
/// Representa la ficha técnica unificada o referencia de homologación de un vehículo (Fasecolda / MinTransporte).
/// </summary>
/// <remarks>
/// Agrupa las especificaciones físicas inmutables del vehículo (Marca, Línea, Cilindraje, Clase).
/// Funciona como cabecera para la matriz de avalúos comerciales por modelo y vigencia fiscal.
/// </remarks>
public class BaseGravableVehiculo
{
    /// <summary>
    /// Identificador único del registro de base gravable.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Código de homologación oficial de 8 dígitos asignado por el Ministerio de Transporte o Fasecolda.
    /// </summary>
    public string Codigo { get; set; } = string.Empty; 

    /// <summary>
    /// Cilindraje del motor en centímetros cúbicos (cc).
    /// </summary>
    public int Cilindraje { get; set; }

    /// <summary>
    /// Capacidad de carga en kilogramos o toneladas (aplicable a vehículos de carga o mixtos).
    /// </summary>
    public int Capacidad { get; set; }

    /// <summary>
    /// Capacidad máxima de pasajeros según homologación de tránsito.
    /// </summary>
    public int Pasajeros { get; set; }

    #region Relaciones
    
    /// <summary>
    /// Identificador de la marca asociada.
    /// </summary>
    public int MarcaId { get; set; }

    /// <summary>
    /// Entidad de la marca comercial del vehículo.
    /// </summary>
    public Marca Marca { get; set; } = null!;

    /// <summary>
    /// Identificador de la línea específica dentro de la marca.
    /// </summary>
    public int LineaId { get; set; }

    /// <summary>
    /// Entidad de la línea comercial del vehículo (ej. CX-30, Stepway, Spark).
    /// </summary>
    public Linea Linea { get; set; } = null!;

    /// <summary>
    /// Identificador de la clase o tipo de vehículo.
    /// </summary>
    public int TipoVehiculoId { get; set; }

    /// <summary>
    /// Clase de agrupación del vehículo (Automóvil, Camioneta, Motocicleta, Carga, etc.).
    /// </summary>
    public TipoVehiculo TipoVehiculo { get; set; } = null!;

    /// <summary>
    /// Colección de avalúos comerciales asociados a esta ficha técnica para diferentes modelos y años fiscales.
    /// </summary>
    public ICollection<BaseGravableVigencia> Vigencias { get; set; } = new List<BaseGravableVigencia>();

    #endregion
}