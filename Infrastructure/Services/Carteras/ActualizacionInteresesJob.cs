    using System.Diagnostics;
    using Infrastructure.AppDbContext;
    using Infrastructure.Services.Liquidaciones;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    namespace Infrastructure.Services.Carteras;

    public class ActualizacionInteresesJob(
        IServiceScopeFactory scopeFactory,
        ILogger<ActualizacionInteresesJob> logger
    ) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var ahora = DateTime.Now;
                var proxima = DateTime.Now.AddSeconds(5); // TEMP: solo para probar local, revertir
                var espera = proxima - ahora;

                logger.LogInformation("Próxima actualización de intereses de cartera: {proxima}", proxima);
                await Task.Delay(espera, stoppingToken);

                await ActualizarIntereses(stoppingToken);
            }
        }

        private async Task ActualizarIntereses(CancellationToken ct)
        {
            var cronometroTotal = Stopwatch.StartNew();
            const int tamanoLote = 2000;

            try
            {
                using var scope = scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<MainDataContext>();
                var liquidacionService = scope.ServiceProvider.GetRequiredService<LiquidacionService>();

                var cronometroCatalogos = Stopwatch.StartNew();
                var parametroSistema = await context.Parametros.AsNoTracking().FirstOrDefaultAsync(ct);
                var tasas = await context.Intereses.AsNoTracking().ToListAsync(ct);
                cronometroCatalogos.Stop();
                logger.LogInformation("Catálogos cargados ({tasas} tasas) en {ms} ms", tasas.Count, cronometroCatalogos.ElapsedMilliseconds);

                // Solo traemos los Ids sin trackear nada todavía — el objeto completo se carga por lote
                var idsPendientes = await context.Cartera
                    .Where(c => !c.IsPagado && !c.IsAnulled)
                    .Select(c => c.Id)
                    .ToListAsync(ct);

                logger.LogInformation("Total pendientes a procesar: {cantidad}", idsPendientes.Count);

                context.ChangeTracker.AutoDetectChangesEnabled = false;

                int procesados = 0;
                foreach (var loteIds in idsPendientes.Chunk(tamanoLote))
                {
                    var cronometroLote = Stopwatch.StartNew();

                    var lote = await context.Cartera
                        .Where(c => loteIds.Contains(c.Id))
                        .ToListAsync(ct);

                    foreach (var cartera in lote)
                    {
                        cartera.ValorInteres = cartera.TieneInteres
                            ? liquidacionService.CalcularInteresMora(cartera.Valor, cartera.Vigencia, parametroSistema, tasas)
                            : 0m;

                        cartera.ValorTotal = cartera.Valor - cartera.Descuento + cartera.ValorInteres;
                    }

                    context.ChangeTracker.DetectChanges();
                    await context.SaveChangesAsync(ct);
                    context.ChangeTracker.Clear(); // libera la memoria del lote anterior

                    procesados += lote.Count;
                    cronometroLote.Stop();
                    logger.LogInformation("Lote guardado: {procesados}/{total} en {ms} ms",
                        procesados, idsPendientes.Count, cronometroLote.ElapsedMilliseconds);
                }

                cronometroTotal.Stop();
                logger.LogInformation(
                    "Intereses recalculados para {cantidad} registros de cartera — total {segundos:F1} s",
                    procesados, cronometroTotal.Elapsed.TotalSeconds);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en ActualizacionInteresesJob (falló tras {segundos:F1} s)", cronometroTotal.Elapsed.TotalSeconds);
            }
        }
    }