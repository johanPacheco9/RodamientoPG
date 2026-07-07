using Domain.Models;
using Domain.Models.Carteras.Enums;
namespace Infrastructure.Services.Carteras;

public partial class CarteraService
{
    private static Cartera CrearCartera(int vehiculoId, string placa, int vigencia, TipoConceptoCartera concepto, decimal valor, bool tieneInteres)
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