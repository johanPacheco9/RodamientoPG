namespace Domain.Models.BaseGravable;

public class BaseGravableVigencia
{
    public int Id { get; set; }

    //  Foreign Key hacia la ficha técnica
    public int BaseGravableVehiculoId { get; set; }
    public BaseGravableVehiculo BaseGravableVehiculo { get; set; } = null!;

    //  Año fiscal cobrable (ej: 2026)
    public int Vigencia { get; set; }

    //  Año de fabricación del carro (ej: 2022)
    public int Modelo { get; set; }

    //  Avalúo comercial en pesos ($)
    public decimal Valor { get; set; }
}