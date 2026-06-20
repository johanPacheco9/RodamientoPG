using Domain.Models.ProcesoLiquidacion;
using Domain.Responses.Proceso.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Procesos.Persuasivo;

public partial class PersuasivoService
{
    public async Task<int> CrearProcesosPersuasivo(int vigenciaHasta)
    {
        var numeroProceso =
            (await context.Procesos.MaxAsync(p => (int?)p.NumeroProceso) ?? 0) + 1;

        var grupos = await context.Cartera
            .Include(c => c.Vehiculo)
            .Where(c =>
                !c.IsPagado &&
                !c.EstaEnProcesoCoactivo &&
                c.Vigencia <= vigenciaHasta)
            .GroupBy(c => new
            {
                c.VehiculoId,
                c.Vehiculo.Placa
            })
            .Select(g => new
            {
                VehiculoId = g.Key.VehiculoId,
                Placa = g.Key.Placa,
                Desde = g.Min(x => x.Vigencia),
                Hasta = g.Max(x => x.Vigencia)
            })
            .ToListAsync();

        foreach (var grupo in grupos)
        {
            var carteraProceso = await context.Cartera
                .Where(c =>
                    c.VehiculoId == grupo.VehiculoId &&
                    !c.IsPagado &&
                    !c.EstaEnProcesoCoactivo &&
                    c.Vigencia >= grupo.Desde &&
                    c.Vigencia <= vigenciaHasta)
                .ToListAsync();

            if (!carteraProceso.Any())
                continue;

            var proceso = new Proceso
            {
                VehiculoId = grupo.VehiculoId,
                Resolucion = "0",
                Fecha = DateTime.UtcNow,
                FechaProceso = DateTime.UtcNow,

                NumeroProceso = numeroProceso,

                Desde = grupo.Desde,
                Hasta = vigenciaHasta,

                EstadoProceso = EstadoProceso.Persuasivo,

                Valor = carteraProceso.Sum(x => x.ValorTotal)
            };

            context.Procesos.Add(proceso);

            await context.SaveChangesAsync();

            var vehiculo = await context.Vehiculos
                .FirstAsync(v => v.Id == grupo.VehiculoId);

            var liquidacion = new Liquidacion
            {
                VehiculoId = grupo.VehiculoId,

                FechaLiquidacion = DateTime.UtcNow,

                VigenciaDesde = grupo.Desde,
                VigenciaHasta = vigenciaHasta,

                UltimoPagoVigencia = vehiculo.PagoHasta,

                TotalDeuda = carteraProceso.Sum(x => x.ValorTotal),

                ProcesoId = proceso.Id
            };

            context.Liquidacion.Add(liquidacion);

            await context.SaveChangesAsync();

            foreach (var item in carteraProceso)
            {
                context.LiquidacionDetalles.Add(
                    new LiquidacionDetalle
                    {
                        LiquidacionId = liquidacion.Id,

                        CarteraId = item.Id,

                        Vigencia = item.Vigencia,

                        Concepto = item.Concepto,

                        ValorTotal = item.Valor,

                        ValorInteres = item.ValorInteres,

                        Descuento = item.Descuento,

                        Valor = item.ValorTotal
                    });
            }

            await context.Cartera
                .Where(c =>
                    c.VehiculoId == grupo.VehiculoId &&
                    !c.IsPagado &&
                    !c.EstaEnProcesoCoactivo &&
                    c.Vigencia >= grupo.Desde &&
                    c.Vigencia <= vigenciaHasta)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(c => c.EstaEnProcesoCoactivo, true));

            numeroProceso++;
        }

        return await context.SaveChangesAsync();
    }
}