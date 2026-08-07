namespace Domain.Models.BaseGravable;

/// <summary>
/// Representa el valor del avalúo comercial (Base Gravable) para una combinación específica de modelo y vigencia fiscal.
/// </summary>
/// <remarks>
/// Mapea la celda individual de la tabla oficial de precios del Ministerio de Transporte.
/// Permite determinar el valor sobre el cual se aplicará la tarifa porcentual del impuesto.
/// </remarks>
public class BaseGravableVigencia
{
    /// <summary>
    /// Identificador único del registro de precio de vigencia.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Clave foránea hacia la ficha técnica base del vehículo.
    /// </summary>
    public int BaseGravableVehiculoId { get; set; }

    /// <summary>
    /// Entidad de la ficha técnica a la que pertenece este avalúo.
    /// </summary>
    public BaseGravableVehiculo BaseGravableVehiculo { get; set; } = null!;

    /// <summary>
    /// Año fiscal en el cual se liquida y cobra el impuesto (ej. 2026).
    /// </summary>
    public int Vigencia { get; set; }

    /// <summary>
    /// Año de fabricación o modelo del vehículo al que corresponde este valor (ej. 2012).
    /// </summary>
    public int Modelo { get; set; }

    /// <summary>
    /// Avalúo comercial en pesos colombianos ($ COP) sobre el cual se liquida el impuesto.
    /// </summary>
    public decimal Valor { get; set; }
}