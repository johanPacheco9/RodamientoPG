using Domain.Models.Carteras.Enums;
using Domain.Models.ProcesoLiquidacion;
using Domain.Models.Recibos;
using Domain.Models.Recibos.Requests;
using Domain.Models.Resoluciones;
using Domain.Models.Resoluciones.Responses;
using Domain.Responses.Liquidacion;
using Domain.Responses.Recibo;
using Domain.Responses.Recibo.Enums;
using Infrastructure.Services.Reportes.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rodamiento.Shared.Components.Pages.PConsulta;

namespace Infrastructure.Services.Liquidaciones;

public partial class LiquidacionService
{
    public async Task<(bool Success, string Message, int ReciboId)> GenerarReciboAsync(CrearReciboRequest request)
    {
        try
        {
            // 1. Validar parámetros de entrada mínimos
            if (request.CarteraIdsSeleccionados == null || !request.CarteraIdsSeleccionados.Any())
            {
                return (false, "Debe seleccionar al menos un registro de cartera para liquidar.", 0);
            }

            // 2. Traer los registros de cartera solicitados que sigan pendientes de pago (Usando context.Cartera en singular)
            var carteraALiquidar = await context.Cartera
                .Where(c => request.CarteraIdsSeleccionados.Contains(c.Id) && !c.IsPagado)
                .ToListAsync();

            if (!carteraALiquidar.Any())
            {
                return (false, "Los registros de cartera seleccionados ya fueron pagados o no son válidos.", 0);
            }

            // 3. Instanciar el encabezado consolidando los valores base (Capital)
            var nuevoRecibo = new Recibo
            {
                VehiculoId = request.VehiculoId,
                Fecha = DateTime.UtcNow,
                Estado = EstadoRecibo.Pendiente,

                ValorCapital = carteraALiquidar.Sum(c => c.Valor),
                InteresMora = carteraALiquidar.Sum(c => c.ValorInteres),
                Descuento = carteraALiquidar.Sum(c => c.Descuento),

                // ACTUALIZADO: Sumas tipadas usando el nuevo Enum TipoConceptoCartera
                Estampillas = carteraALiquidar.Where(c => c.Concepto == TipoConceptoCartera.Estampillas).Sum(c => c.Valor),
                ValorCargaDatos = carteraALiquidar.Where(c => c.Concepto == TipoConceptoCartera.Carga).Sum(c => c.Valor),
                ValorRodamiento = carteraALiquidar.Where(c => c.Concepto == TipoConceptoCartera.Rodamiento).Sum(c => c.Valor),

                // El total del sistema es la suma matemática real de todos los totales de las carteras
                ValorTotalSistema = carteraALiquidar.Sum(c => c.ValorTotal),
                Detalles = new List<ReciboDetalle>() // Aseguramos la inicialización de la lista de navegación
            };

            // 4. Mapear la cartera elegida al histórico de detalles del recibo
            foreach (var cartera in carteraALiquidar)
            {
                nuevoRecibo.Detalles.Add(new ReciboDetalle
                {
                    CarteraId = cartera.Id,
                    Vigencia = cartera.Vigencia,
                    Concepto = cartera.Concepto, // ACTUALIZADO: Asignación directa del Enum
                    Valor = cartera.Valor,
                    ValorInteres = cartera.ValorInteres,
                    Descuento = cartera.Descuento,
                    ValorTotal = cartera.ValorTotal
                });
            }

            // 5. Guardar todo en una única transacción atómica (Verificando tu DbSet context.Recibos)
            context.Recibos.Add(nuevoRecibo);
            await context.SaveChangesAsync();

            // Retornamos la tupla indicando éxito y el ID generado por PostgreSQL
            return (true, "Recibo generado exitosamente.", nuevoRecibo.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error interno al liquidar la cartera para el vehículo {VehiculoId}", request.VehiculoId);

            return (false, $"Error interno al liquidar la cartera: {ex.Message}", 0);
        }
    }

    public async Task<int> Add(string pplaca, DateTime pfecha, string presol, TipoResolucion ptipo, decimal pvalor, int pdesde, int phasta, string pobs, int puser)
    {
        try
        {
            return await context.Database
                .ExecuteSqlAsync($"SELECT * FROM graba_resol({pplaca}, {pfecha}, {presol}, {ptipo}, {pvalor}, {pdesde}, {phasta}, {pobs}, {puser})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en RegistrarResolucionAsync: {ex.Message}");

            return 0;
        }
    }

    public async Task<List<ResolucionResponseDto>> GetResolucionByPlaca(string placa)
    {
        try
        {
            // 1. Validar si el vehículo existe y si tiene resoluciones asociadas
            // Usamos AnyAsync para saber si hay registros antes de hacer la consulta pesada
            var existeResolucion = await context.Resolucion
                .AnyAsync(s => s.Vehiculo.Placa == placa);

            if (!existeResolucion)
            {
                throw new Exception("No se encontraron resoluciones para la placa registrada.");
            }
            var resolucionesDto = await context.Resolucion
                .Where(s => s.Vehiculo.Placa == placa)
                .Select(r => new ResolucionResponseDto(
                    r.Id,
                    r.NumeroResolucion,
                    r.Fecha,
                    r.FechaProceso,
                    r.TipoResolucion,
                    r.Valor,
                    r.Estado,
                    r.Observaciones,
                    r.VehiculoId,
                    r.Vehiculo.Placa,
                    r.UsuarioId,
                    r.Usuario.Nombre,
                    r.ProcesoId,

                    // 🚀 Extraemos únicamente las vigencias numéricas a memoria
                    r.Carteras.Select(c => c.Vigencia).OrderBy(v => v).ToList(),

                    // 🚀 Calculamos el min y max directamente en el motor SQL
                    r.Carteras.Any() ? r.Carteras.Min(c => c.Vigencia) : (int?)null,
                    r.Carteras.Any() ? r.Carteras.Max(c => c.Vigencia) : (int?)null
                ))
                .ToListAsync();

            return resolucionesDto;
        }
        catch (Exception ex) when (ex.Message.Contains("No se encontraron"))
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener las resoluciones para la placa {placa}.", ex);
        }
    }


    public async Task<List<Liquidacion>> Deudas_x_Vigencia(int pvig)
    {
        return await context.Database
            .SqlQuery<Liquidacion>($"SELECT * FROM deuda_x_periodos({pvig})")
            .ToListAsync();
    }

    public async Task<List<ReporteDiarioDto>> Lista_Recibos(DateTime pdesde, DateTime phasta)
    {
        var desde = DateTime.SpecifyKind(pdesde.Date, DateTimeKind.Utc);
        var hasta = DateTime.SpecifyKind(phasta.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        // 1. Traemos los recibos pagados con una sola consulta SQL limpia
        var recibos = await context.Recibos
            .AsNoTracking()
            .Include(r => r.Vehiculo)
            .ThenInclude(v => v.Propietario)
            .Include(r => r.Detalles)
            .Where(r =>
                r.Estado == EstadoRecibo.Pagado &&
                r.FechaPago.HasValue &&
                r.FechaPago.Value >= desde &&
                r.FechaPago.Value <= hasta)
            .OrderBy(r => r.FechaPago)
            .ToListAsync();

        // 2. Agrupamos por día de pago para generar un ReporteDiarioDto por cada fecha del rango
        return recibos
            .GroupBy(r => r.FechaPago!.Value.Date)
            .Select(grupoDia =>
            {
                var fechaDia = grupoDia.Key;
                var recibosDia = grupoDia.ToList();

                // Sumatorias conceptuales del día actual
                var totalCapital = recibosDia.Sum(r => r.ValorCapital);
                var totalIntereses = recibosDia.Sum(r => r.InteresMora);
                var totalDescuentos = recibosDia.Sum(r => r.Descuento);
                var totalRecaudado = recibosDia.Sum(r => r.ValorTotalSistema); // Ajustar si es ValorTotalSistema

                var totalRodamiento = recibosDia.Sum(r => r.ValorRodamiento);
                var totalEstampillas = recibosDia.Sum(r => r.Estampillas);
                var totalCargaDatos = recibosDia.Sum(r => r.ValorCargaDatos);

                return new ReporteDiarioDto
                {
                    FechaReporte = fechaDia,
                    CantidadRecibos = recibosDia.Count,
                    TotalRecaudado = totalRecaudado,
                    TotalCapital = totalCapital,
                    TotalIntereses = totalIntereses,
                    TotalDescuentos = totalDescuentos,
                    TotalRodamiento = totalRodamiento,
                    TotalEstampillas = totalEstampillas,
                    TotalCargaDatos = totalCargaDatos,

                    // 🎯 Transacciones detalladas de este día mapeadas al DTO secundario
                    Transacciones = recibosDia.Select(r => new DetalleReciboReporteDto
                    {
                        ReciboId = r.Id,
                        Placa = r.Vehiculo.Placa,
                        PropietarioNombre = r.Vehiculo.Propietario.Nombre,
                        Documento = r.Vehiculo.Propietario.Documento,
                        FechaPago = r.FechaPago,
                        ValorCapital = r.ValorCapital,
                        InteresMora = r.InteresMora,
                        Descuento = r.Descuento,
                        TotalPagado = r.ValorTotalSistema // Ajustar si es ValorTotalSistema en tu base de datos
                    }).ToList()
                };
            })
            .OrderBy(r => r.FechaReporte)
            .ToList();
    }

    public async Task<List<ConceptoResumenDto>> Resumen_Conceptos(string pplaca, int phasta)
    {
        var detalle = await ObtenerDetalleDeuda(pplaca, phasta);

        return detalle
            .GroupBy(c => c.Concepto)
            .Select((g, i) => new ConceptoResumenDto
            {
                Id = i + 1,
                Nombre = g.Key,
                Valor = g.Sum(x => x.Valor),
                ValorIntereses = g.Sum(x => x.ValorInteres),
                Descuento = g.Sum(x => x.Descuento),
                Total = g.Sum(x => x.ValorTotal)
            })
            .ToList();
    }

    public async Task<List<ConceptoLiquidacionDto>> Deuda_x_Conceptos(string comp, int phasta)
    {
        return await LiquidarDeudaPorConceptosAsync(comp, phasta);
    }

    public async Task<List<DetalleReciboDto>> Items_x_Recibo(int precibo)
    {
        // 1. Traemos de la base de datos los detalles de este recibo específico
        var detallesRaw = await context.Recibos
            .Where(r => r.Id == precibo)
            .SelectMany(r => r.Detalles)
            .Select(d => new
            {
                d.Vigencia,
                d.Concepto,
                d.Valor,
                d.ValorInteres,
                d.Descuento
                // Si en ReciboDetalle añadiste Sancion, inclúyela aquí: d.Sancion
            })
            .ToListAsync();

        if (!detallesRaw.Any()) return [];

        // 2. Agrupamos en memoria por Vigencia para armar el DTO consolidado por año
        var resultado = detallesRaw
            .GroupBy(d => d.Vigencia)
            .Select(g => new DetalleReciboDto
            {
                Vigencia = g.Key,

                ValorRodamiento = g.Where(x => x.Concepto == TipoConceptoCartera.Rodamiento).Sum(x => x.Valor),
                ValorCarga = g.Where(x => x.Concepto == TipoConceptoCartera.Carga).Sum(x => x.Valor),
                ValorEstampillas = g.Where(x => x.Concepto == TipoConceptoCartera.Estampillas).Sum(x => x.Valor),

                ValorRecibo = g.Sum(x => x.Valor),

                ValorInteres = g.Sum(x => x.ValorInteres),
                Descuento = g.Sum(x => x.Descuento),
                Sancion = 0
            })
            .OrderBy(r => r.Vigencia)
            .ToList();

        return resultado;
    }

    public async Task<List<ConceptoLiquidacionDto>> Debido_Cobrar()
    {
        return await context.Cartera
            .AsNoTracking()
            .Where(c => !c.IsPagado)
            .GroupBy(c => c.Vigencia)
            .Select(g => new ConceptoLiquidacionDto
            {
                Vigencia = g.Key,
                ValorRodamiento = g.Where(x => x.Concepto == TipoConceptoCartera.Rodamiento).Sum(x => x.Valor),
                ValorCarga = g.Where(x => x.Concepto == TipoConceptoCartera.Carga).Sum(x => x.Valor),
                ValorEstampillas = g.Where(x => x.Concepto == TipoConceptoCartera.Estampillas).Sum(x => x.Valor),
                ValorRecibo = g.Where(x => x.Concepto == TipoConceptoCartera.Costas).Sum(x => x.Valor),
                ValorInteres = g.Sum(x => x.ValorInteres),
                Descuento = g.Sum(x => x.Descuento),
                ValorTotal = g.Sum(x => x.ValorTotal)
            })
            .OrderBy(x => x.Vigencia)
            .ToListAsync();
    }

    public async Task<List<Liquidacion>> Informe_Cartera()
    {
        return await context.Database
            .SqlQuery<Liquidacion>($"SELECT * FROM informe_cartera()")
            .ToListAsync();
    }

    public async Task<ConceptoLiquidacionDto?> Deuda_x_Conc_Total(string comp, int phasta)
    {
        var result = await LiquidarDeudaPorConceptosAsync(comp, phasta);

        if (result.Count == 0) return new ConceptoLiquidacionDto();

        return new ConceptoLiquidacionDto
        {
            Vigencia = result.Min(x => x.Vigencia),
            ValorTotal = result.Sum(x => x.ValorTotal)
        };
    }


    private async Task<decimal> ObtenerValorSistemaAsync()
    {
        return await context.Parametros
            .Select(p => p.ValorSistema)
            .FirstOrDefaultAsync();
    }

    private static bool EsConceptoRodamiento(string concepto) => EsConcepto(concepto, ConceptoRodamiento, "1", "TRANSITO");
    private static bool EsConceptoEstampillas(string concepto) => EsConcepto(concepto, ConceptoEstampillas, "2");
    private static bool EsConceptoCostas(string concepto) => EsConcepto(concepto, ConceptoCostas, "3", "RECIBO");
    private static bool EsConceptoCarga(string concepto) => EsConcepto(concepto, ConceptoCarga, "4");
    private static bool EsConceptoSistema(string concepto) => EsConcepto(concepto, ConceptoSistematizacion);
    private static bool EsClasePorCarga(int tipoVehiculoId) => tipoVehiculoId is 4 or 8 or 9;
    private static bool EsClasePorPasajeros(int tipoVehiculoId) => tipoVehiculoId is 1 or 2 or 3 or 5 or 6 or 7;

    private static bool EsConcepto(string concepto, params string[] opciones)
    {
        return opciones.Any(opcion => string.Equals(concepto?.Trim(), opcion, StringComparison.OrdinalIgnoreCase));
    }
}