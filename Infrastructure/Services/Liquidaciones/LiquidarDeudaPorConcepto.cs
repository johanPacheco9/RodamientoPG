using Domain.Responses.Liquidacion;
namespace Infrastructure.Services.Liquidaciones;

public partial class LiquidacionService
{
    public async Task<List<ConceptoLiquidacionDto>> LiquidarDeudaPorConceptosAsync(string placa, int hasta)
    {
        var detalle = await ObtenerDetalleDeuda(placa, hasta);

        return detalle
            .GroupBy(c => c.Vigencia)
            .Select(g => new ConceptoLiquidacionDto
            {
                Vigencia = g.Key,
                ValorRodamiento = g.Where(x => EsConceptoRodamiento(x.Concepto)).Sum(x => x.Valor),
                ValorCarga = g.Where(x => EsConceptoCarga(x.Concepto)).Sum(x => x.Valor),
                ValorEstampillas = g.Where(x => EsConceptoEstampillas(x.Concepto)).Sum(x => x.Valor),
                ValorRecibo = g.Where(x => EsConceptoCostas(x.Concepto)).Sum(x => x.Valor),
                ValorInteres = g.Sum(x => x.ValorInteres),
                Descuento = g.Sum(x => x.Descuento),
                ValorSistema = g.Where(x => EsConceptoSistema(x.Concepto)).Sum(x => x.Valor),
                ValorTotal = g.Sum(x => x.ValorTotal)
            })
            .OrderBy(x => x.Vigencia)
            .ToList();
    }
}