using Domain.Generics;
using Domain.Models.ProcesoLiquidacion;
using Domain.Models.Notificaciones;
using Domain.Responses.Proceso.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Procesos.Persuasivo;

public partial class PersuasivoService
{
    public async Task<(bool Success, string Message, int ProcesoId)> CrearProcesoPersuasivoPorPlaca(
        string placa,
        int vigenciaDesde,
        int vigenciaHasta)
    {
        placa = placa.Trim().ToUpper();

        if (string.IsNullOrWhiteSpace(placa))
            return (false, "La placa es obligatoria.", 0);

        if (vigenciaDesde <= 0 || vigenciaHasta <= 0 || vigenciaDesde > vigenciaHasta)
            return (false, "El rango de vigencias no es valido.", 0);

        var vehiculo = await context.Vehiculos
            .FirstOrDefaultAsync(v => v.Placa == placa);

        if (vehiculo is null)
            return (false, $"No existe un vehiculo con placa {placa}.", 0);

        var existeProcesoActivo = await context.Procesos
            .AnyAsync(p =>
                p.VehiculoId == vehiculo.Id &&
                p.EstadoProceso != EstadoProceso.SinProceso);

        if (existeProcesoActivo)
            return (false, "El vehiculo ya tiene un proceso activo.", 0);

        var carteraProceso = await context.Cartera
            .Where(c =>
                c.VehiculoId == vehiculo.Id &&
                !c.IsPagado &&
                !c.EstaEnProcesoCoactivo &&
                c.Vigencia >= vigenciaDesde &&
                c.Vigencia <= vigenciaHasta)
            .OrderBy(c => c.Vigencia)
            .ToListAsync();

        if (carteraProceso.Count == 0)
            return (false, "No hay cartera pendiente en el rango seleccionado.", 0);

        var numeroProceso = (await context.Procesos.MaxAsync(p => (int?)p.NumeroProceso) ?? 0) + 1;
        var total = carteraProceso.Sum(x => x.ValorTotal == 0 ? x.Valor : x.ValorTotal);

        var proceso = new Proceso
        {
            VehiculoId = vehiculo.Id,
            Resolucion = "PERSUASIVO",
            Fecha = DateTime.UtcNow,
            FechaProceso = DateTime.UtcNow,
            NumeroProceso = numeroProceso,
            Desde = carteraProceso.Min(c => c.Vigencia),
            Hasta = vigenciaHasta,
            EstadoProceso = EstadoProceso.Persuasivo,
            Valor = total
        };

        context.Procesos.Add(proceso);
        await context.SaveChangesAsync();

        var liquidacion = new Liquidacion
        {
            VehiculoId = vehiculo.Id,
            FechaLiquidacion = DateTime.UtcNow,
            VigenciaDesde = proceso.Desde ?? vigenciaDesde,
            VigenciaHasta = vigenciaHasta,
            UltimoPagoVigencia = vehiculo.PagoHasta,
            TotalDeuda = total,
            ProcesoId = proceso.Id
        };

        context.Liquidacion.Add(liquidacion);
        await context.SaveChangesAsync();

        foreach (var item in carteraProceso)
        {
            context.LiquidacionDetalles.Add(new LiquidacionDetalle
            {
                LiquidacionId = liquidacion.Id,
                CarteraId = item.Id,
                Vigencia = item.Vigencia,
                Concepto = item.Concepto.GetDisplayName(),
                ValorTotal = item.Valor,
                ValorInteres = item.ValorInteres,
                Descuento = item.Descuento,
                Valor = item.ValorTotal == 0 ? item.Valor : item.ValorTotal
            });

            item.EstaEnProcesoCoactivo = true;
        }

        vehiculo.EstadoProceso = EstadoProceso.Persuasivo;

        await context.SaveChangesAsync();

        return (true, "Proceso persuasivo creado exitosamente.", proceso.Id);
    }

    public async Task<int> ContarAvisosProceso(int procesoId)
    {
        var proceso = await context.Procesos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == procesoId);

        if (proceso is null) return 0;

        var desde = proceso.Desde ?? 0;
        var hasta = proceso.Hasta ?? 0;

        return await context.Avisos
            .AsNoTracking()
            .Where(a =>
                a.Cartera.VehiculoId == proceso.VehiculoId &&
                !a.Cartera.IsPagado &&
                a.Cartera.Vigencia >= desde &&
                a.Cartera.Vigencia <= hasta &&
                (a.Estado == "Enviado" || a.Estado == "Entregado"))
            .Select(a => a.NumeroAviso)
            .Distinct()
            .CountAsync();
    }

    public async Task<(bool Success, string Message, int NumeroAviso)> RegistrarAvisoProceso(int procesoId)
    {
        var proceso = await context.Procesos
            .FirstOrDefaultAsync(p => p.Id == procesoId);

        if (proceso is null)
            return (false, "No existe el proceso.", 0);

        if (proceso.EstadoProceso != EstadoProceso.Persuasivo)
            return (false, "Solo se pueden registrar avisos en procesos persuasivos.", 0);

        var avisosActuales = await ContarAvisosProceso(procesoId);
        if (avisosActuales >= 4)
            return (false, "El proceso ya tiene los 4 avisos requeridos.", avisosActuales);

        var siguienteAviso = avisosActuales + 1;
        var desde = proceso.Desde ?? 0;
        var hasta = proceso.Hasta ?? 0;

        var carteraProceso = await context.Cartera
            .Where(c =>
                c.VehiculoId == proceso.VehiculoId &&
                !c.IsPagado &&
                c.Vigencia >= desde &&
                c.Vigencia <= hasta)
            .ToListAsync();

        if (carteraProceso.Count == 0)
            return (false, "No hay cartera pendiente asociada al proceso.", 0);

        foreach (var item in carteraProceso)
        {
            var yaExiste = await context.Avisos.AnyAsync(a =>
                a.CarteraId == item.Id &&
                a.NumeroAviso == siguienteAviso);

            if (yaExiste) continue;

            context.Avisos.Add(new Aviso
            {
                CarteraId = item.Id,
                NumeroAviso = siguienteAviso,
                FechaEnvio = DateTime.UtcNow,
                NumeroGuia = $"P-{proceso.NumeroProceso}-{siguienteAviso}",
                RutaPdf = $"/docs/avisos/{proceso.NumeroProceso}/aviso_{siguienteAviso}_{item.Placa}.pdf",
                Estado = "Enviado"
            });
        }

        await context.SaveChangesAsync();
        return (true, $"Aviso {siguienteAviso} registrado.", siguienteAviso);
    }

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

                        Concepto = item.Concepto.GetDisplayName(),

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
