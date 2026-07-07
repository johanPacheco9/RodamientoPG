using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.Liquidaciones;

public partial class LiquidacionService
{
    public async Task<decimal> CalcularInteresMora(decimal capital, int vigencia, DateTime? fechaCorte = null)
    {
        if (capital <= 0) return 0;

        var corteRaw = fechaCorte?.Date ?? DateTime.Today;
        var corte = DateTime.SpecifyKind(corteRaw, DateTimeKind.Utc);

        var parametroSistema = await context.Parametros.AsNoTracking().FirstOrDefaultAsync();

        int mesVencimiento = parametroSistema?.FechaLimiteSancion.Month ?? 3;
        int diaVencimiento = parametroSistema?.FechaLimiteSancion.Day ?? 31;

        var fechaVencimientoLocal = new DateTime(vigencia, mesVencimiento, diaVencimiento);
        var fechaVencimientoVigencia = DateTime.SpecifyKind(fechaVencimientoLocal, DateTimeKind.Utc);

        if (corte <= fechaVencimientoVigencia) return 0;

        // Volvemos a asegurar que el inicio de la mora mantenga el flag UTC
        var inicioMora = DateTime.SpecifyKind(fechaVencimientoVigencia.AddDays(1), DateTimeKind.Utc);

        var tasasAplicables = await context.Intereses
            .AsNoTracking()
            .Where(i => i.Hasta >= inicioMora && i.Desde <= corte)
            .OrderBy(i => i.Desde)
            .ToListAsync();

        double interesAcumulado = 0;

        // 3. Calcular los días de mora reales por cada registro de tasa
        foreach (var tasa in tasasAplicables)
        {
            var inicioPeriodo = tasa.Desde > inicioMora ? tasa.Desde : inicioMora;
            var finPeriodo = tasa.Hasta < corte ? tasa.Hasta : corte;

            if (inicioPeriodo <= finPeriodo)
            {
                int diasEnTasa = (finPeriodo - inicioPeriodo).Days + 1;
                double diasDelAnio = DateTime.IsLeapYear(inicioPeriodo.Year) ? 366.0 : 365.0;

                double tasaDiaria = ((double)tasa.Porcentaje / 100d) / diasDelAnio;
                double interesDelPeriodo = (double)capital * tasaDiaria * diasEnTasa;

                interesAcumulado += interesDelPeriodo;
            }
        }

        // 4. Aplicación de alivios dinámicos
        decimal porcentajeCobro = parametroSistema?.PorcentajeInteresACobrar ?? 100m;
        if (porcentajeCobro <= 0) porcentajeCobro = 100m;

        decimal interesFinal = ((decimal)interesAcumulado * porcentajeCobro) / 100m;

        return Math.Round(interesFinal, 0, MidpointRounding.AwayFromZero);
    }
}