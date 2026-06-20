using Domain.Models;
namespace Infrastructure.Services.Liquidaciones;

public partial class LiquidacionService
{
    public async Task<List<Cartera>> GetConceptos(string comp, int pdesde)
    {
        var detalle = await ObtenerDetalleDeuda(comp, pdesde);

        return detalle
            .GroupBy(c => c.Vigencia)
            .Select(g => new Cartera
            {
                Placa = comp,
                Vigencia = g.Key,
                Valor = g.Sum(x => x.Valor),
                ValorInteres = g.Sum(x => x.ValorInteres),
                Descuento = g.Sum(x => x.Descuento),
                ValorTotal = g.Sum(x => x.ValorTotal)
            })
            .OrderBy(c => c.Vigencia)
            .ToList();
    }
}