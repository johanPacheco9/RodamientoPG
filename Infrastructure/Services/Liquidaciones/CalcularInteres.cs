using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Liquidaciones;

public partial class LiquidacionService
{
    public async Task<decimal> CalcularInteresMora(decimal capital, int vigencia, DateTime? fechaCorte = null)
    {
        var parametroSistema = await context.Parametros.AsNoTracking().FirstOrDefaultAsync();
        var tasas = await context.Intereses.AsNoTracking().ToListAsync();

        return CalcularInteresMora(capital, vigencia, parametroSistema, tasas, fechaCorte);
    }

    public decimal CalcularInteresMora(decimal capital, int vigencia, Parametro? parametroSistema, List<Interes> tasas, DateTime? fechaCorte = null)
    {
        if (capital <= 0) return 0m;

        // Trabajar puramente con la parte de FECHA (sin horas ni zonas horarias) para evitar desfases de UTC-5
        DateTime corte = fechaCorte?.Date ?? DateTime.Today;

        int mesVencimiento = parametroSistema?.FechaLimiteSancion.Month ?? 3;
        int diaVencimiento = parametroSistema?.FechaLimiteSancion.Day ?? 31;

        var fechaVencimientoVigencia = new DateTime(vigencia, mesVencimiento, diaVencimiento);

        if (corte <= fechaVencimientoVigencia) return 0m;

        DateTime inicioMora = fechaVencimientoVigencia.AddDays(1);

        // Filtrar y ordenar tasas aplicables dentro del rango
        var tasasAplicables = tasas
            .Where(i => i.Hasta.Date >= inicioMora && i.Desde.Date <= corte)
            .OrderBy(i => i.Desde)
            .ToList();

        double interesAcumulado = 0;

        foreach (var tasa in tasasAplicables)
        {
            DateTime inicioTasa = tasa.Desde.Date;
            DateTime finTasa = tasa.Hasta.Date;

            var inicioPeriodo = inicioTasa > inicioMora ? inicioTasa : inicioMora;
            var finPeriodo = finTasa < corte ? finTasa : corte;

            if (inicioPeriodo <= finPeriodo)
            {
                int diasEnTasa = (finPeriodo - inicioPeriodo).Days + 1;
                double diasDelAnio = DateTime.IsLeapYear(inicioPeriodo.Year) ? 366.0 : 365.0;

                // 💡 CÁLCULO TRIBUTARIO COLOMBIANO (Estatuto Tributario Art. 635)
                // Se asume que tasa.Porcentaje guarda el porcentaje Anual (ej: 28.5 para 28.5%)
                double tasaAnualDecimal = (double)tasa.Porcentaje / 100.0;
                
                // Opción A: Interés Simple Diario de Ley Tributaria
                double tasaDiaria = tasaAnualDecimal / diasDelAnio;

                interesAcumulado += (double)capital * tasaDiaria * diasEnTasa;
            }
        }

        decimal porcentajeCobro = parametroSistema?.PorcentajeInteresACobrar ?? 100m;
        if (porcentajeCobro <= 0) porcentajeCobro = 100m;

        decimal interesFinal = ((decimal)interesAcumulado * porcentajeCobro) / 100m;

        // Redondeo tributario oficial al mil más cercano ($1.000) o al peso ($1) según tu parámetro de interfaz
        return Math.Round(interesFinal, 0, MidpointRounding.AwayFromZero);
    }
}