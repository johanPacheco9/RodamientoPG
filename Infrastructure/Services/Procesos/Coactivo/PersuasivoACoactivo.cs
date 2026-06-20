using Domain.Models.ProcesoLiquidacion;
using Domain.Responses.Proceso.Enums;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.Procesos.Coactivo;

public partial  class CoactivoService
{
    public async Task<int> PersuasivoAMandamiento(int opcion, int numeroProceso)
    {
        var procesos = context.Procesos
            .Where(p => p.EstadoProceso == EstadoProceso.Persuasivo);

        if (opcion == 1)
        {
            procesos = procesos.Where(p => p.NumeroProceso == numeroProceso);
        }

        var procesosSeleccionados = await procesos
            .Include(p => p.Vehiculo)
            .ToListAsync();
        
        var valorCostasMandamiento = await context.Parametros
            .Select(s => s.ValorCostasPersuasivo)
            .FirstOrDefaultAsync();

        foreach (var proceso in procesosSeleccionados)
        {
            var desde = proceso.Desde ?? 0;
            var hasta = proceso.Hasta ?? 0;

            await context.Cartera
                .Where(c =>
                    c.Placa == proceso.Vehiculo.Placa &&
                    !c.IsPagado &&
                    c.EstaEnProcesoCoactivo &&
                    c.Vigencia >= desde &&
                    c.Vigencia <= hasta &&
                    c.Concepto == ConceptoCostas)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(c => c.Valor, valorCostasMandamiento)
                    .SetProperty(c => c.ValorTotal, valorCostasMandamiento));

            var valorTotal = await context.Cartera
                .Where(c =>
                    c.Placa == proceso.Vehiculo.Placa &&
                    !c.IsPagado &&
                    c.EstaEnProcesoCoactivo &&
                    c.Vigencia >= desde &&
                    c.Vigencia <= hasta)
                .SumAsync(c => c.ValorTotal == 0 ? c.Valor : c.ValorTotal);

            proceso.EstadoProceso = EstadoProceso.MandamientoPago;
            proceso.FechaMandamiento = DateTime.UtcNow;
            proceso.Valor = valorTotal;
        }

        return await context.SaveChangesAsync();
    }
}