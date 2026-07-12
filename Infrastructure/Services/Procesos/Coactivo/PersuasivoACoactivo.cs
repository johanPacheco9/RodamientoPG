using Domain.Models.Carteras.Enums;
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
            if (await TieneCuatroAvisosCumplidos(proceso) <=4 )
            {
                throw new InvalidOperationException(
                    $"El proceso {proceso.NumeroProceso} de la placa {proceso.Vehiculo.Placa} aun no tiene los 4 avisos requeridos para pasar a coactivo.");
            }

            var desde = proceso.Desde ?? 0;
            var hasta = proceso.Hasta ?? 0;

            await context.Cartera
                .Where(c =>
                    c.Placa == proceso.Vehiculo.Placa &&
                    !c.IsPagado &&
                    c.Vigencia >= desde &&
                    c.Vigencia <= hasta &&
                    c.Concepto == TipoConceptoCartera.Costas)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(c => c.Valor, valorCostasMandamiento)
                    .SetProperty(c => c.ValorTotal, valorCostasMandamiento));

            var valorTotal = await context.Cartera
                .Where(c =>
                    c.Placa == proceso.Vehiculo.Placa &&
                    !c.IsPagado &&
                    c.Vigencia >= desde &&
                    c.Vigencia <= hasta)
                .SumAsync(c => c.ValorTotal == 0 ? c.Valor : c.ValorTotal);

            proceso.EstadoProceso = EstadoProceso.MandamientoPago;
            proceso.FechaMandamiento = DateTime.UtcNow;
            proceso.Valor = valorTotal;
        }

        return await context.SaveChangesAsync();
    }

    private async Task<int> TieneCuatroAvisosCumplidos(Proceso proceso)
    {
        return await context.Avisos
                     .AsNoTracking()
                     .Where(a => a.ProcesoId == proceso.Id && (a.Estado == "Enviado" || a.Estado == "Entregado"))
                     .Select(a => a.NumeroAviso)
                     .Distinct()
                     .CountAsync();
    }
}
