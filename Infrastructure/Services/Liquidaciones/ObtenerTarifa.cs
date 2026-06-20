using Domain.Responses.Liquidacion.Enums;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.Liquidaciones;

public partial class LiquidacionService
{
    private async Task<decimal> ObtenerTarifa(TipoConceptoTarifa conceptoTarifa, int vigencia, int? valorRango = null)
    {
        var query = context.Tarifas
            .AsNoTracking()
            .Where(t => t.ConceptoTarifa == conceptoTarifa && t.AnioFiscal == vigencia);

        if (valorRango.HasValue)
        {
            query = query.Where(t => valorRango.Value >= t.RangoInicial && valorRango.Value <= t.RangoFinal);
        }

        return await query
            .Select(t => t.Valor)
            .FirstOrDefaultAsync();
    }
}