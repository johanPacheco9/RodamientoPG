using Domain.Responses.Liquidacion.Enums;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.Tarifas;

public partial class TarifaService
{
    public async Task<decimal> ObtenerTarifa(TipoConceptoTarifa conceptoTarifa, int vigencia, int? valorRango = null)
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
