using Microsoft.EntityFrameworkCore;
using Rodamiento.Shared.Components.Pages.PConsulta;
namespace Infrastructure.Services.Liquidaciones;

public partial class LiquidacionService
{
    public async Task<List<CarteraDetailDto>> DetallesConceptos(string comp, int phasta)
    {
        return await ObtenerDetalleDeuda(comp, phasta);
    }
    
    private async Task<List<CarteraDetailDto>> ObtenerDetalleDeuda(string placa, int hasta)
    {
        var cartera = await context.Cartera
            .AsNoTracking()
            .Where(c => c.Placa == placa && !c.IsPagado && c.Vigencia <= hasta)
            .OrderBy(c => c.Vigencia)
            .ThenBy(c => c.Concepto)
            .ToListAsync();

        var descuentoInteres = await ObtenerPorcentajeDescuentoVigente();

        foreach (var item in cartera)
        {
            item.ValorInteres = EsConceptoRodamiento(item.Concepto)
                ? await CalcularInteresMoraAsync(item.Valor, item.Vigencia)
                : 0;
            item.Descuento = Math.Round((item.ValorInteres * descuentoInteres) / 100m, 0, MidpointRounding.AwayFromZero);
            item.ValorTotal = item.Valor + item.ValorInteres - item.Descuento;
        }

        var valorSistema = await ObtenerValorSistemaAsync();
        var result = cartera.Select(c => new CarteraDetailDto
        {
            Vigencia = c.Vigencia,
            Concepto = c.Concepto,
            Valor = c.Valor,
            ValorInteres = c.ValorInteres,
            Descuento = c.Descuento,
            ValorTotal = c.ValorTotal,
            Seleccionado = true
        }).ToList();

        if (valorSistema > 0 && result.Count > 0)
            result.Add(new CarteraDetailDto
            {
                Vigencia = hasta,
                Concepto = ConceptoSistematizacion,
                Valor = valorSistema,
                ValorTotal = valorSistema,
                Seleccionado = true
            });

        return result;
    }

}