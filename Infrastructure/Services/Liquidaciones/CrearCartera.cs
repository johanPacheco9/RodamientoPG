using Domain.Models;
namespace Infrastructure.Services.Liquidaciones;

public partial class LiquidacionService
{
    private static Cartera CrearCartera(int vehiculoId, string placa, int vigencia, string concepto, decimal valor, bool tieneInteres)
    {
        return new Cartera
        {
            VehiculoId = vehiculoId,
            Placa = placa,
            Vigencia = vigencia,
            Concepto = concepto,
            Valor = valor,
            IsPagado = false,
            TieneInteres = tieneInteres,
            Descuento = 0,
            ValorInteres = 0,
            ValorTotal = valor
        };
    }
}