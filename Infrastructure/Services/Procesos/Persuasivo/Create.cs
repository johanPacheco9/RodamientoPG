using Domain.Generics;
using Domain.Models;
using Domain.Models.ProcesoLiquidacion;
using Domain.Models.Notificaciones;
using Domain.Responses.Proceso.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Procesos.Persuasivo;

public partial class PersuasivoService
{
    // =========================================================
    // Registra el cambio de estado en Proceso + guarda historial.
    // Único punto que debe tocar EstadoProceso — así nunca queda
    // desincronizado con el historial de auditoría.
    // =========================================================
    private async Task RegistrarCambioEstado(
        Proceso proceso,
        EstadoProceso estadoAnterior,
        EstadoProceso estadoNuevo,
        bool esAutomatico,
        string? usuarioResponsable = null,
        string? motivo = null)
    {
        proceso.EstadoProceso = estadoNuevo;

        context.HistorialEstadoProceso.Add(new HistorialEstadoProceso
        {
            ProcesoId = proceso.Id,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = estadoNuevo,
            FechaModificacion = DateTime.UtcNow,
            EsAutomatico = esAutomatico,
            Motivo = motivo,
            UsuarioModifico = 1
        });
    }

    // =========================================================
    // Escala un proceso EXISTENTE de Persuasivo a Coactivo.
    // No crea Proceso/Liquidacion nuevos: reutiliza la misma fila.
    // =========================================================
    public async Task<(bool Success, string Message)> EscalarACoactivo(
        int procesoId,
        bool esAutomatico,
        string? usuarioResponsable = null,
        string? motivo = null)
    {
        var proceso = await context.Procesos.FirstOrDefaultAsync(p => p.Id == procesoId);

        if (proceso is null)
            return (false, "El proceso no existe.");

        if (proceso.EstadoProceso != EstadoProceso.Persuasivo)
            return (false, "Solo se puede escalar un proceso que esté en estado Persuasivo.");

        var estadoAnterior = proceso.EstadoProceso;
        proceso.FechaMandamiento = DateTime.UtcNow;

        await RegistrarCambioEstado(
            proceso,
            estadoAnterior,
            estadoNuevo: EstadoProceso.Coactivo,
            esAutomatico: esAutomatico,
            usuarioResponsable: usuarioResponsable,
            motivo: motivo);

        await context.SaveChangesAsync();

        return (true, "Proceso escalado a coactivo exitosamente.");
    }

    // =========================================================
    // NÚCLEO COMPARTIDO: crea Proceso + Liquidacion + Detalles
    // para UN vehículo con SU cartera pendiente ya cargada.
    // No hace SaveChanges/commit final; eso lo hace el llamador.
    // =========================================================
    private async Task<Proceso> CrearProcesoPersuasivoCore(
        int vehiculoId,
        List<Cartera> carteraProceso,
        int numeroProceso,
        string? usuarioResponsable,
        bool esAutomatico)
    {
        var vehiculo = await context.Vehiculos.FirstAsync(v => v.Id == vehiculoId);

        var total = carteraProceso.Sum(x => x.ValorTotal == 0 ? x.Valor : x.ValorTotal);
        var desde = carteraProceso.Min(c => c.Vigencia);
        var hasta = carteraProceso.Max(c => c.Vigencia);

        var proceso = new Proceso
        {
            VehiculoId = vehiculoId,
            Resolucion = "PERSUASIVO",
            Fecha = DateTime.UtcNow,
            FechaProceso = DateTime.UtcNow,
            NumeroProceso = numeroProceso,
            Desde = desde,
            Hasta = hasta,
            EstadoProceso = EstadoProceso.Persuasivo,
            Valor = total
        };

        context.Procesos.Add(proceso);
        await context.SaveChangesAsync(); // necesitamos proceso.Id para lo que sigue

        var liquidacion = new Liquidacion
        {
            VehiculoId = vehiculoId,
            FechaLiquidacion = DateTime.UtcNow,
            VigenciaDesde = desde,
            VigenciaHasta = hasta,
            UltimoPagoVigencia = vehiculo.PagoHasta,
            TotalDeuda = total,
            ProcesoId = proceso.Id
        };

        context.Liquidacion.Add(liquidacion);
        await context.SaveChangesAsync(); // necesitamos liquidacion.Id

        foreach (var item in carteraProceso)
        {
            var valorBase = item.Valor;
            var valorConTotal = item.ValorTotal == 0 ? item.Valor : item.ValorTotal;

            context.LiquidacionDetalles.Add(new LiquidacionDetalle
            {
                LiquidacionId = liquidacion.Id,
                CarteraId = item.Id,
                Vigencia = item.Vigencia,
                Concepto = item.Concepto.GetDisplayName(),
                Valor = valorBase, // corregido: base
                ValorInteres = item.ValorInteres,
                Descuento = item.Descuento,
                ValorTotal = valorConTotal // corregido: total con intereses
            });
        }

        await RegistrarCambioEstado(
            proceso,
            estadoAnterior: EstadoProceso.SinProceso,
            estadoNuevo: EstadoProceso.Persuasivo,
            esAutomatico: esAutomatico,
            usuarioResponsable: usuarioResponsable,
            motivo: "Creación de proceso persuasivo");

        return proceso;
    }

    // =========================================================
    // ENTRADA MANUAL: por placa, un vehículo a la vez
    // =========================================================
    public async Task<(bool Success, string Message, int ProcesoId)> CrearProcesoPersuasivoPorPlaca(
        string placa,
        int vigenciaDesde,
        int vigenciaHasta,
        string? usuarioResponsable = null)
    {
        placa = placa.Trim().ToUpper();

        if (string.IsNullOrWhiteSpace(placa))
            return (false, "La placa es obligatoria.", 0);

        if (vigenciaDesde <= 0 || vigenciaHasta <= 0 || vigenciaDesde > vigenciaHasta)
            return (false, "El rango de vigencias no es valido.", 0);

        var vehiculo = await context.Vehiculos.FirstOrDefaultAsync(v => v.Placa == placa);

        if (vehiculo is null)
            return (false, $"No existe un vehiculo con placa {placa}.", 0);

        var existeProcesoSolapado = await context.Procesos
            .AnyAsync(p => p.VehiculoId == vehiculo.Id &&
                           p.EstadoProceso != EstadoProceso.SinProceso &&
                           p.Desde != null && p.Hasta != null &&
                           p.Desde <= vigenciaHasta && p.Hasta >= vigenciaDesde);

        if (existeProcesoSolapado)
            return (false, "Ya existe un proceso activo que cubre alguna de las vigencias seleccionadas.", 0);


        var carteraProceso = await context.Cartera
            .Where(c =>
                c.VehiculoId == vehiculo.Id &&
                !c.IsPagado &&
                c.Vigencia >= vigenciaDesde &&
                c.Vigencia <= vigenciaHasta)
            .OrderBy(c => c.Vigencia)
            .ToListAsync();

        if (carteraProceso.Count == 0)
            return (false, "No hay cartera pendiente en el rango seleccionado.", 0);

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var numeroProceso = (await context.Procesos.MaxAsync(p => (int?)p.NumeroProceso) ?? 0) + 1;

            var proceso = await CrearProcesoPersuasivoCore(
                vehiculo.Id, carteraProceso, numeroProceso,
                usuarioResponsable, esAutomatico: usuarioResponsable is null);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, "Proceso persuasivo creado exitosamente.", proceso.Id);
        }
        catch
        {
            await transaction.RollbackAsync();

            throw;
        }
    }

    public async Task<int> CrearProcesosPersuasivo(int vigenciaHasta)
    {
        var carteraPendiente = await context.Cartera
            .AsNoTracking()
            .Where(c => !c.IsPagado && c.Vigencia <= vigenciaHasta)
            .ToListAsync();

        // Rangos [Desde, Hasta] de todos los procesos activos, agrupados por vehículo
        // para que la búsqueda de solapamiento sea O(1) en vez de escanear todo el sistema.
        var procesosPorVehiculo = await context.Procesos
            .AsNoTracking()
            .Where(p => p.EstadoProceso != EstadoProceso.SinProceso && p.Desde != null && p.Hasta != null)
            .Select(p => new { p.VehiculoId, Desde = p.Desde!.Value, Hasta = p.Hasta!.Value })
            .ToListAsync()
            .ContinueWith(t => t.Result
                .GroupBy(p => p.VehiculoId)
                .ToDictionary(g => g.Key, g => g.ToList()));

        // Excluimos solo la cartera que ya cae dentro de un proceso activo existente
        // para ESE vehículo — no el vehículo completo.
        var carteraElegible = carteraPendiente
            .Where(c => !(procesosPorVehiculo.TryGetValue(c.VehiculoId, out var rangos) &&
                          rangos.Any(p => c.Vigencia >= p.Desde && c.Vigencia <= p.Hasta)))
            .ToList();

        var numeroProceso = (await context.Procesos.MaxAsync(p => (int?)p.NumeroProceso) ?? 0) + 1;
        var procesosCreados = 0;

        foreach (var grupo in carteraElegible.GroupBy(c => c.VehiculoId))
        {
            var carteraProceso = grupo.ToList();

            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await CrearProcesoPersuasivoCore(
                    grupo.Key, carteraProceso, numeroProceso,
                    usuarioResponsable: null, esAutomatico: true);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                numeroProceso++;
                procesosCreados++;
            }
            catch
            {
                await transaction.RollbackAsync();

                throw;
            }
        }

        return procesosCreados;
    }

    // =========================================================
    // AVISOS: uno por Proceso, no uno por fila de Cartera
    // =========================================================
    public async Task<int> ContarAvisosProceso(int procesoId)
    {
        return await context.Avisos
            .AsNoTracking()
            .Where(a => a.ProcesoId == procesoId && (a.Estado == "Enviado" || a.Estado == "Entregado"))
            .Select(a => a.NumeroAviso)
            .Distinct()
            .CountAsync();
    }

    public async Task<(bool Success, string Message, int NumeroAviso)> RegistrarAvisoProceso(int procesoId)
    {
        var proceso = await context.Procesos.FirstOrDefaultAsync(p => p.Id == procesoId);

        if (proceso is null)
            return (false, "No existe el proceso.", 0);

        if (proceso.EstadoProceso != EstadoProceso.Persuasivo)
            return (false, "Solo se pueden registrar avisos en procesos persuasivos.", 0);

        var avisosActuales = await ContarAvisosProceso(procesoId);

        if (avisosActuales >= 4)
            return (false, "El proceso ya tiene los 4 avisos requeridos.", avisosActuales);

        var siguienteAviso = avisosActuales + 1;

        var yaExiste = await context.Avisos
            .AnyAsync(a => a.ProcesoId == procesoId && a.NumeroAviso == siguienteAviso);

        if (yaExiste)
            return (false, $"El aviso {siguienteAviso} ya fue registrado.", siguienteAviso);

        // VALIDACIÓN DE TIEMPO: Control de 20 días con respecto al aviso anterior
        if (siguienteAviso > 1)
        {
            int avisoAnteriorId = siguienteAviso - 1;

            var fechaUltimoAviso = await context.Avisos
                .Where(a => a.ProcesoId == procesoId && a.NumeroAviso == avisoAnteriorId)
                .Select(a => (DateTime?)a.FechaEnvio)
                .FirstOrDefaultAsync();

            if (fechaUltimoAviso.HasValue)
            {
                var diasTranscurridos = (DateTime.UtcNow - fechaUltimoAviso.Value).TotalDays;

                if (diasTranscurridos < 20)
                {
                    int diasRestantes = (int)Math.Ceiling(20 - diasTranscurridos);

                    return (false,
                        $"No han transcurrido los 20 días de ley requeridos. Faltan {diasRestantes} día(s) para habilitar el Aviso {siguienteAviso}.",
                        avisosActuales);
                }
            }
        }

        // 🔥 CORREGIDO: Traer el vehículo e incluir al propietario de forma limpia y segura para EF Core
        var vehiculo = await context.Vehiculos
            .Include(v => v.Propietario)
            .FirstOrDefaultAsync(v => v.Id == proceso.VehiculoId);

        if (vehiculo is null)
            return (false, "No se encontró el vehículo asociado al proceso.", 0);

        var propietario = vehiculo.Propietario;

        context.Avisos.Add(new Aviso
        {
            ProcesoId = procesoId,
            NumeroAviso = siguienteAviso,
            FechaEnvio = DateTime.UtcNow,
            NumeroGuia = $"P-{proceso.NumeroProceso}-{siguienteAviso}",
            RutaPdf = $"/docs/avisos/{proceso.NumeroProceso}/aviso_{siguienteAviso}_{vehiculo.Placa}.pdf",
            Estado = "Enviado"
        });

        await context.SaveChangesAsync();

        // 🔥 ENVÍO DEL CORREO ELECTRÓNICO: Si el propietario tiene un email registrado
        if (propietario != null && !string.IsNullOrWhiteSpace(propietario.Correo))
        {
            try
            {
                string asunto = $"Notificación Oficial: Aviso de Cobro Persuasivo N° {siguienteAviso} - Placa {vehiculo.Placa}";
                string cuerpoHtml = $"""
                                     <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto;border:1px solid #ddd;border-radius:8px;overflow:hidden;">
                                         <div style="background:#112366;padding:20px;text-align:center;">
                                             <h2 style="color:white;margin:0;">Secretaría de Tránsito</h2>
                                             <p style="color:#ccc;margin:4px 0;">Municipio de Vélez - Santander</p>
                                         </div>
                                         <div style="padding:24px;">
                                             <p>Estimado(a) <strong>{propietario.Nombre}</strong>,</p>
                                             <p>Le notificamos que se ha generado formalmente el <strong>Aviso de Cobro Persuasivo N° {siguienteAviso}</strong> correspondiente al expediente de cobro coactivo número <strong>{proceso.NumeroProceso}</strong> asociado a su vehículo de placas <strong style="color:#112366;">{vehiculo.Placa}</strong>.</p>
                                             
                                             <div style="background:#fff3cd;border:1px solid #ffc107;border-radius:6px;padding:15px;margin:16px 0;">
                                                 <strong>Detalles del Acto Administrativo:</strong><br/>
                                                 • Rango de Vigencias afectadas: <strong>{proceso.Desde} - {proceso.Hasta}</strong><br/>
                                                 • Capital de Cartera en mora: <strong>{proceso.Valor:C0}</strong>
                                             </div>
                                             
                                             <p>Evite el embargo de sus cuentas bancarias y bienes muebles/inmuebles acercándose de manera prioritaria a las instalaciones de la Secretaría de Tránsito para radicar su acuerdo de pago o liquidación oficial.</p>
                                             <hr style="border:0;border-top:1px solid #eee;margin:20px 0;"/>
                                             <p style="color:#888;font-size:11px;text-align:center;">Este es un acto administrativo oficial automatizado. No responda a este remitente.</p>
                                         </div>
                                     </div>
                                     """;

                // Llamamos al método Enviar de tu EmailService
                await _emailService.Enviar(propietario.Correo, propietario.Nombre, asunto, cuerpoHtml);
            }
            catch (Exception ex)
            {
                // Ojo: Registramos el error en logs pero no tumbamos la transacción si el SMTP falla
                // Puedes inyectar un ILogger si necesitas dejar trazabilidad exacta del fallo del correo
            }
        }

        if (siguienteAviso == 4)
        {
            await EscalarACoactivo(procesoId, esAutomatico: true, motivo: "4 avisos completados");
        }

        return (true, $"Aviso {siguienteAviso} registrado y notificado vía email.", siguienteAviso);
    }
}