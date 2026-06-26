using Domain.Generics;
using Domain.Models;
using Domain.Models.ProcesoLiquidacion;
using Domain.Models.Recibos;
using Domain.Models.Recibos.Requests;
using Domain.Models.Vehiculos;
using Domain.Responses.Liquidacion;
using Domain.Responses.Liquidacion.Enums;
using Domain.Responses.Recibo;
using Domain.Responses.Recibo.Enums;
using Domain.Responses.Reportes;
using Domain.Responses.Users.Enums;
using Domain.Responses.Vehiculos.Enums;
using Infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rodamiento.Shared.Components.Pages.PConsulta;

namespace Infrastructure.Services.Liquidaciones;

public partial class LiquidacionService(MainDataContext context)
{
    private const string ConceptoRodamiento = "RODAMIENTO";
    private const string ConceptoEstampillas = "ESTAMPILLAS";
    private const string ConceptoCostas = "COSTAS";
    private const string ConceptoCarga = "CARGA";
    private const string ConceptoSistematizacion = "SISTEMATIZACION";

    public async Task<decimal> CalcularInteresMoraAsync(decimal capital, int vigencia, DateTime? fechaCorte = null)
    {
        if (capital <= 0) return 0;

        var corte = fechaCorte?.Date ?? DateTime.Today;
        var tasas = await context.Intereses
            .AsNoTracking()
            .ToListAsync();

        decimal interes = 0;

        for (var anio = vigencia; anio <= corte.Year; anio++)
        {
            var ultimoMes = anio == corte.Year ? corte.Month : 12;

            for (var mes = 1; mes <= ultimoMes; mes++)
            {
                var tasa = tasas.FirstOrDefault(i =>
                    i.Desde.Year == anio &&
                    mes >= i.Desde.Month &&
                    mes <= i.Hasta.Month);

                if (tasa == null) continue;

                var dias = anio == corte.Year && mes == corte.Month ? corte.Day : 30;
                var proporcionDias = dias / 365d;
                var factor = Math.Pow(1 + ((double)tasa.Porcentaje / 100d), proporcionDias) - 1;
                interes += capital * (decimal)factor;
            }
        }

        return Math.Round((interes * 25m) / 100m, 0, MidpointRounding.AwayFromZero);
    }

    public async Task<List<ConceptoLiquidacionDto>> LiquidarDeudaPorConceptosAsync(string placa, int hasta)
    {
        var detalle = await ObtenerDetalleDeuda(placa, hasta);

        return detalle
            .GroupBy(c => c.Vigencia)
            .Select(g => new ConceptoLiquidacionDto
            {
                Vigencia = g.Key,
                ValorRodamiento = g.Where(x => EsConceptoRodamiento(x.Concepto)).Sum(x => x.Valor),
                ValorCarga = g.Where(x => EsConceptoCarga(x.Concepto)).Sum(x => x.Valor),
                ValorEstampillas = g.Where(x => EsConceptoEstampillas(x.Concepto)).Sum(x => x.Valor),
                ValorRecibo = g.Where(x => EsConceptoCostas(x.Concepto)).Sum(x => x.Valor),
                ValorInteres = g.Sum(x => x.ValorInteres),
                Descuento = g.Sum(x => x.Descuento),
                ValorSistema = g.Where(x => EsConceptoSistema(x.Concepto)).Sum(x => x.Valor),
                ValorTotal = g.Sum(x => x.ValorTotal)
            })
            .OrderBy(x => x.Vigencia)
            .ToList();
    }

    public async Task<(bool Success, string Message, int ReciboId)> GenerarReciboAsync(CrearReciboRequest request)
    {
        try
        {
            // 1. Validar parámetros de entrada mínimos
            if (request.CarteraIdsSeleccionados == null || !request.CarteraIdsSeleccionados.Any())
            {
                return (false, "Debe seleccionar al menos un registro de cartera para liquidar.", 0);
            }

            // 2. Traer los registros de cartera solicitados que sigan pendientes de pago
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

                // 💡 CORRECCIÓN: Sumamos el .Valor base (Capital) de cada concepto, no el .ValorTotal
                Estampillas = carteraALiquidar.Where(c => c.Tipo == "ESTAMPILLA").Sum(c => c.Valor),
                ValorCargaDatos = carteraALiquidar.Where(c => c.Concepto == "CARGA").Sum(c => c.Valor),
                ValorRodamiento = carteraALiquidar.Where(c => c.Concepto == "RODAMIENTO").Sum(c => c.Valor),

                // El total del sistema es la suma matemática real de todos los totales de las carteras
                ValorTotalSistema = carteraALiquidar.Sum(c => c.ValorTotal)
            };

            // 4. Mapear la cartera elegida al histórico de detalles del recibo
            foreach (var cartera in carteraALiquidar)
            {
                nuevoRecibo.Detalles.Add(new ReciboDetalle
                {
                    CarteraId = cartera.Id,
                    Vigencia = cartera.Vigencia,
                    Concepto = cartera.Concepto,
                    Valor = cartera.Valor,
                    ValorInteres = cartera.ValorInteres,
                    Descuento = cartera.Descuento,
                    ValorTotal = cartera.ValorTotal
                });
            }

            // 5. Guardar todo en una única transacción atómica
            context.Recibos.Add(nuevoRecibo);
            await context.SaveChangesAsync();

            // Retornamos la tupla indicando éxito y el ID generado por PostgreSQL
            return (true, "Recibo generado exitosamente.", nuevoRecibo.Id);
        }
        catch (Exception ex)
        {
            // Loggear el error con tu servicio de logger si está disponible (ej. _logger.LogError...)
            return (false, $"Error interno al liquidar la cartera: {ex.Message}", 0);
        }
    }


    public async Task<int> GenerarCarteraVehiculoAsync(string placa, int desde, int hasta)
    {
        var vehiculo = await context.Vehiculos.FirstOrDefaultAsync(v => v.Placa == placa);

        if (vehiculo == null) return 0;

        var parametro = await context.Parametros.AsNoTracking().FirstOrDefaultAsync();

        await context.Cartera
            .Where(c => c.Placa == placa && !c.IsPagado && c.Vigencia >= desde && c.Vigencia <= hasta)
            .ExecuteDeleteAsync();

        vehiculo.PagoHasta = desde;

        var nuevasDeudas = new List<Cartera>();

        for (var vigencia = desde; vigencia <= hasta; vigencia++)
        {
            var valorRodamiento = await ObtenerTarifa(TipoConceptoTarifa.Rodamiento, vigencia);
            if (valorRodamiento > 0)
            {
                nuevasDeudas.Add(CrearCartera(vehiculo.Id, placa, vigencia, ConceptoRodamiento, valorRodamiento, tieneInteres: true));
            }

            if (parametro?.CobraAdicional == true && parametro.ValorCostasPersuasivo > 0 && vigencia != 2026)
            {
                nuevasDeudas.Add(CrearCartera(vehiculo.Id, placa, vigencia, ConceptoCostas, parametro.ValorCostasPersuasivo, tieneInteres: true));
            }

            var valorCargaPasajeros = await ObtenerValorCargaOPasajero(vehiculo, vigencia);
            if (valorCargaPasajeros > 0)
            {
                nuevasDeudas.Add(CrearCartera(vehiculo.Id, placa, vigencia, ConceptoCarga, valorCargaPasajeros, tieneInteres: true));
            }

            var valorEstampillas = Math.Round(((valorRodamiento + valorCargaPasajeros) * 2m) / 100m, 0, MidpointRounding.AwayFromZero);
            if (valorEstampillas > 0)
            {
                nuevasDeudas.Add(CrearCartera(vehiculo.Id, placa, vigencia, ConceptoEstampillas, valorEstampillas, tieneInteres: true));
            }
        }

        context.Cartera.AddRange(nuevasDeudas);

        return await context.SaveChangesAsync();
    }

    public async Task<int> Add(string pplaca, DateTime pfecha, string presol, int ptipo, decimal pvalor, int pdesde, int phasta, string pobs, int puser)
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

    public async Task<List<Resolucion>> GetResol(string comparendo)
    {
        try
        {
            return await context.Database
                .SqlQuery<Resolucion>($@"SELECT n.*, t.nombre as ntipo
                                           FROM resoluciones n
                                           LEFT JOIN tipos_nov t ON (t.id = n.tipo)
                                           WHERE n.placa::varchar = {comparendo}")
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error al obtener las resoluciones", ex);
        }
    }

    public async Task<EstadoCuentaVehiculoDto?> GetCarteraByPlaca(
        string placa,
        CancellationToken cancellationToken = default)
    {
        try
        {
            placa = placa.Trim().ToUpper();
            var nowYear = DateTime.UtcNow.Year;

            // 1. Obtener los datos del Vehículo y Propietario (Se mantiene igual)
            var vehiculo = await context.Vehiculos
                .AsNoTracking()
                .Where(v => v.Placa == placa)
                .Select(v => new
                {
                    v.Id,
                    v.Placa,
                    v.Modelo,
                    v.Cilindraje,
                    v.EstadoProcesoId,
                    v.PagoHasta,
                    v.TipoServicioVehiculo,
                    Clase = v.TipoVehiculo != null ? v.TipoVehiculo.Nombre : string.Empty,
                    Marca = v.Marca != null ? v.Marca.Nombre : string.Empty,
                    Linea = v.Linea != null ? v.Linea.Nombre : string.Empty,
                    Color = v.Color != null ? v.Color.Nombre : string.Empty,
                    Estado = v.EstadoProceso.GetDisplayName(),
                    Documento = v.Propietario != null ? v.Propietario.Documento : string.Empty,
                    NombrePropietario = v.Propietario != null ? v.Propietario.Nombre : string.Empty,
                    Direccion = v.Propietario != null ? v.Propietario.Direccion : string.Empty,
                    Telefono = v.Propietario != null ? v.Propietario.Telefono : string.Empty,
                    TipoDocumento = v.Propietario != null ? v.Propietario.TipoDocumento : TipoDocumento.Cc
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (vehiculo == null)
                return null;

            // 2. 🚀 CORRECCIÓN: Traemos las filas de cartera pendientes de pago desglosadas
            var carteraPendiente = await context.Cartera
                .AsNoTracking()
                .Where(c => c.Placa == placa && !c.IsPagado)
                .OrderBy(c => c.Vigencia)
                .Select(c => new ConceptoCarteraDto(
                    c.Id,
                    c.Vigencia,
                    c.Concepto,
                    c.Tipo,
                    c.Valor,
                    c.ValorInteres,
                    c.Descuento,
                    c.ValorTotal
                ))
                .ToListAsync(cancellationToken);

            // 3. Calculamos los datos globales basados en la lista obtenida
            int vigenciaDesde = carteraPendiente.Any() ? carteraPendiente.Min(c => c.Vigencia) : nowYear;
            int vigenciaHasta = carteraPendiente.Any() ? carteraPendiente.Max(c => c.Vigencia) : nowYear;
            decimal totalDeuda = carteraPendiente.Sum(c => c.ValorTotal);

            // 4. Armamos la respuesta final unificada
            return new EstadoCuentaVehiculoDto
            {
                // Vehículo
                VehiculoId = vehiculo.Id,
                Placa = vehiculo.Placa,
                Clase = vehiculo.Clase,
                Modelo = vehiculo.Modelo,
                Marca = vehiculo.Marca,
                Linea = vehiculo.Linea,
                Color = vehiculo.Color,
                TipoServicio = vehiculo.TipoServicioVehiculo.ToString(),
                Cilindraje = vehiculo.Cilindraje,
                EstadoNombre = vehiculo.Estado,
                EstadoId = vehiculo.EstadoProcesoId,
                UltimoPago = vehiculo.PagoHasta,

                // Propietario
                Documento = vehiculo.Documento,
                NombrePropietario = vehiculo.NombrePropietario,
                Direccion = vehiculo.Direccion,
                Telefono = vehiculo.Telefono,
                TipoDocumento = vehiculo.TipoDocumento,

                // Liquidación / Deuda
                VigenciaDesde = vigenciaDesde,
                VigenciaHasta = vigenciaHasta,
                TotalDeuda = totalDeuda,

                // 🔥 El cambio clave: El frontend ahora recibe la lista de deudas desglosada
                Conceptos = carteraPendiente
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consultando liquidación para placa {Placa}", placa);

            return null;
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

        return recibos.Select(r => new ReporteDiarioDto
            {
                NumeroRecibo = r.Id.ToString(),
                Placa = r.Vehiculo.Placa,
                FechaCreacion = r.Fecha,
                FechaPago = r.FechaPago ?? r.Fecha,
                AnioDesde = r.Detalles.Count == 0 ? 0 : r.Detalles.Min(d => d.Vigencia),
                AnioHasta = r.Detalles.Count == 0 ? 0 : r.Detalles.Max(d => d.Vigencia),
                ValorTotal = r.Detalles.Sum(d => d.ValorTotal),
                ValorImpuesto = r.Detalles.Where(d => d.Concepto == ConceptoRodamiento).Sum(d => d.Valor),
                ValorCarga = r.Detalles.Where(d => d.Concepto == ConceptoCarga).Sum(d => d.Valor),
                ValorCostas = r.Detalles.Where(d => d.Concepto == ConceptoCostas).Sum(d => d.Valor),
                ValorSistematizacion = r.ValorTotalSistema,
                ValorInteres = r.Detalles.Sum(d => d.ValorInteres),
                ValorEstampillas = r.Detalles.Where(d => d.Concepto == ConceptoEstampillas).Sum(d => d.Valor),
                ValorSancion = 0,
                Descuento = r.Detalles.Sum(d => d.Descuento),
                NombrePropietario = r.Vehiculo.Propietario.Nombre
            })
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

                ValorRodamiento = g.Where(x => x.Concepto == "RODAMIENTO").Sum(x => x.Valor),
                ValorCarga = g.Where(x => x.Concepto == "CARGA").Sum(x => x.Valor),
                ValorEstampillas = g.Where(x => x.Concepto == "ESTAMPILLAS").Sum(x => x.Valor),

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
                ValorRodamiento = g.Where(x => x.Concepto == ConceptoRodamiento).Sum(x => x.Valor),
                ValorCarga = g.Where(x => x.Concepto == ConceptoCarga).Sum(x => x.Valor),
                ValorEstampillas = g.Where(x => x.Concepto == ConceptoEstampillas).Sum(x => x.Valor),
                ValorRecibo = g.Where(x => x.Concepto == ConceptoCostas).Sum(x => x.Valor),
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
