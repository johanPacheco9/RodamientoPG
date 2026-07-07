using Domain.Generics;
using Domain.Models;
using Domain.Models.Carteras.Enums;
using Domain.Models.ProcesoLiquidacion;
using Domain.Models.Vehiculos.Enums;
using Domain.Responses.Liquidacion.Enums;
using Domain.Responses.Vehiculos.Enums;
using Infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Tarifas;

public partial class TarifaService(MainDataContext context, ILogger<TarifaService> logger)
{
    private readonly ILogger<TarifaService> _logger = logger;
    
    public async Task<int> CargaImpuestosMasivaAsync(int anioLimiteLiquidacion)
    {
        try
        {
            logger.LogInformation("Iniciando proceso masivo de liquidación hasta el año {AnioLimite}", anioLimiteLiquidacion);

            // 1. Cargar la configuración global única del sistema
            var parametroSistema = await context.Parametros.FirstOrDefaultAsync();
            if (parametroSistema == null)
            {
                logger.LogError("No se encontró la configuración global en la tabla Parametros.");
                return 0;
            }

            bool aplicarCobroCostas = parametroSistema.CobraAdicional;
            decimal tarifaFijaCostas = parametroSistema.ValorCostasPersuasivo;

            // 2. Cargar catálogos en memoria para optimizar rendimiento
            var listadoTarifasGlobales = await context.Tarifas.ToListAsync();
            
            var vehiculosParaProcesar = await context.Vehiculos
                .Where(v => v.EstadoProcesoId == (int)EstadoProceso.SinProceso)
                .ToListAsync();

            // 3. Indexar la cartera existente usando context.Cartera (Singular) y ToString() del Enum
            var registroCarteraExistente = await context.Cartera
                .Select(c => $"{c.Placa}-{c.Vigencia.ToString()}-{c.Concepto.ToString()}")
                .ToHashSetAsync();

            var loteNuevasDeudas = new List<Cartera>();
            const int TAMANIO_LOTE = 1000;

            // 4. Motor de liquidación por vehículo
            foreach (var vehiculo in vehiculosParaProcesar)
            {
                int anioVigenciaActual = vehiculo.PagoHasta + 1;
                if (anioVigenciaActual <= 1997)
                {
                    anioVigenciaActual = vehiculo.Modelo;
                }

                while (anioVigenciaActual <= anioLimiteLiquidacion)
                {
                    decimal valorLiquidadoRodamiento = 0;
                    decimal valorLiquidadoAdicional = 0;

                    // ====================================================================
                    // 🚗 CONCEPTO: RODAMIENTO
                    // ====================================================================
                    string llaveConceptoRodamiento = $"{vehiculo.Placa}-{anioVigenciaActual}-{TipoConceptoCartera.Rodamiento.ToString()}";
                    if (!registroCarteraExistente.Contains(llaveConceptoRodamiento))
                    {
                        var tarifaAplicable = listadoTarifasGlobales
                            .FirstOrDefault(t => t.TipoVehiculoId == vehiculo.TipoVehiculoId 
                                              && t.AnioFiscal == anioVigenciaActual 
                                              && t.ConceptoTarifa == TipoConceptoTarifa.Rodamiento);

                        if (tarifaAplicable != null && tarifaAplicable.Valor > 0)
                        {
                            valorLiquidadoRodamiento = tarifaAplicable.Valor;
                            loteNuevasDeudas.Add(CrearRegistroCartera(vehiculo.Placa, anioVigenciaActual, TipoConceptoCartera.Rodamiento, valorLiquidadoRodamiento));
                            registroCarteraExistente.Add(llaveConceptoRodamiento);
                        }
                    }

                    // ====================================================================
                    // ⚖️ CONCEPTO: COSTAS PERSUASIVAS
                    // ====================================================================
                    if (aplicarCobroCostas)
                    {
                        string llaveConceptoCostas = $"{vehiculo.Placa}-{anioVigenciaActual}-{TipoConceptoCartera.Costas.ToString()}";
                        if (!registroCarteraExistente.Contains(llaveConceptoCostas))
                        {
                            loteNuevasDeudas.Add(CrearRegistroCartera(vehiculo.Placa, anioVigenciaActual, TipoConceptoCartera.Costas, tarifaFijaCostas));
                            registroCarteraExistente.Add(llaveConceptoCostas);
                        }
                    }

                    // ====================================================================
                    // 💼 CONCEPTO: IMPUESTO ADICIONAL (CARGA / PASAJEROS)
                    // ====================================================================
                    if (vehiculo.TipoServicioVehiculo == TipoServicioVehiculo.Publico)
                    {
                        string llaveConceptoAdicional = $"{vehiculo.Placa}-{anioVigenciaActual}-{TipoConceptoCartera.Carga.ToString()}";
                        if (!registroCarteraExistente.Contains(llaveConceptoAdicional))
                        {
                            var detallesClaseVehiculo = vehiculo.TipoVehiculo;

                            if (detallesClaseVehiculo?.Tipo == ClaseAgrupacionVehiculo.Carga)
                            {
                                int capacidadEnToneladas = vehiculo.CapacidadCarga / 1000;

                                var tarifaCarga = listadoTarifasGlobales
                                    .FirstOrDefault(t => t.TipoVehiculoId == vehiculo.TipoVehiculoId 
                                                      && t.AnioFiscal == anioVigenciaActual 
                                                      && t.TipoServicioVehiculo == TipoServicioVehiculo.Publico
                                                      && capacidadEnToneladas >= t.RangoInicial 
                                                      && capacidadEnToneladas <= t.RangoFinal);

                                if (tarifaCarga != null && tarifaCarga.Valor > 0)
                                {
                                    valorLiquidadoAdicional = tarifaCarga.Valor;
                                    loteNuevasDeudas.Add(CrearRegistroCartera(vehiculo.Placa, anioVigenciaActual, TipoConceptoCartera.Carga, valorLiquidadoAdicional));
                                    registroCarteraExistente.Add(llaveConceptoAdicional);
                                }
                            }
                            else if (detallesClaseVehiculo?.Tipo == ClaseAgrupacionVehiculo.Pasajeros)
                            {
                                var tarifaPasajeros = listadoTarifasGlobales
                                    .FirstOrDefault(t => t.TipoVehiculoId == vehiculo.TipoVehiculoId 
                                                      && t.AnioFiscal == anioVigenciaActual 
                                                      && t.TipoServicioVehiculo == TipoServicioVehiculo.Publico
                                                      && vehiculo.Pasajeros >= t.RangoInicial 
                                                      && vehiculo.Pasajeros <= t.RangoFinal);

                                if (tarifaPasajeros != null && tarifaPasajeros.Valor > 0)
                                {
                                    valorLiquidadoAdicional = tarifaPasajeros.Valor;
                                    loteNuevasDeudas.Add(CrearRegistroCartera(vehiculo.Placa, anioVigenciaActual, TipoConceptoCartera.Carga, valorLiquidadoAdicional));
                                    registroCarteraExistente.Add(llaveConceptoAdicional);
                                }
                            }
                        }
                    }

                    // ====================================================================
                    // 🎫 CONCEPTO: ESTAMPILLAS
                    // ====================================================================
                    string llaveConceptoEstampillas = $"{vehiculo.Placa}-{anioVigenciaActual}-{TipoConceptoCartera.Estampillas.ToString()}";
                    if (!registroCarteraExistente.Contains(llaveConceptoEstampillas))
                    {
                        decimal baseCalculoEstampillas = valorLiquidadoRodamiento + valorLiquidadoAdicional;
                        if (baseCalculoEstampillas > 0)
                        {
                            decimal valorLiquidadoEstampillas = Math.Round((baseCalculoEstampillas * 2) / 100);
                            loteNuevasDeudas.Add(CrearRegistroCartera(vehiculo.Placa, anioVigenciaActual, TipoConceptoCartera.Estampillas, valorLiquidadoEstampillas));
                            registroCarteraExistente.Add(llaveConceptoEstampillas);
                        }
                    }

                    anioVigenciaActual++;
                }

                // Guardar en lotes usando context.Cartera (Singular)
                if (loteNuevasDeudas.Count >= TAMANIO_LOTE)
                {
                    context.Cartera.AddRange(loteNuevasDeudas);
                    await context.SaveChangesAsync();
                    loteNuevasDeudas.Clear();
                }
            }

            if (loteNuevasDeudas.Any())
            {
                context.Cartera.AddRange(loteNuevasDeudas);
                await context.SaveChangesAsync();
            }

            logger.LogInformation("Carga masiva de impuestos finalizada con éxito.");
            return 1;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error crítico en CargaImpuestosMasivaAsync.");
            return 0;
        }
    }
    
    // Firma corregida para aceptar TipoConceptoCartera de forma nativa
    private static Cartera CrearRegistroCartera(string placa, int vigencia, TipoConceptoCartera concepto, decimal valor)
    {
        return new Cartera
        {
            Placa = placa,
            Vigencia = vigencia,
            Concepto = concepto, // Se asigna directamente el enum
            Valor = valor,
            Descuento = 0,
            IsPagado = false,
            TieneInteres = true,
            ValorInteres = 0,
            Tipo = concepto.GetDisplayName()
        };
    }
}