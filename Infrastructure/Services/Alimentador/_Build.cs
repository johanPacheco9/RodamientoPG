using Domain.Generics;
using Domain.Models;
using Domain.Models.Carteras.Enums;
using Domain.Models.Notificaciones;
using Domain.Models.ProcesoLiquidacion;
using Domain.Models.Recibos;
using Domain.Models.Vehiculos;
using Domain.Models.Vehiculos.Enums;
using Domain.Responses.Liquidacion.Enums;
using Domain.Responses.Recibo.Enums;
using Domain.Responses.Users.Enums;
using Domain.Responses.Vehiculos.Enums;
using Infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Alimentador;

public static class DbInitializer
{
    private const int TargetVehiculosTesting = 10000;

    public static void Initialize(MainDataContext context)
    {
        context.Database.EnsureCreated();

        SeedParametros(context);
        SeedCatalogos(context);
        SeedReglasLiquidacion(context);
        SeedUsuarios(context);
        SeedVehiculosYCartera(context);
        SeedRecibosDePrueba(context);
    }

    private static void SeedParametros(MainDataContext context)
    {
        if (context.Parametros.Any(p => p.Id == 1)) return;

        context.Parametros.Add(new Parametro
        {
            Id = 1,
            Nombre = "Municipio de Albania",
            Nit = "800000000-1",
            Direccion = "Palacio Municipal",
            Telefono = "6050000000",
            Ciudad = "Albania",
            Correo = "rodamiento@test.local",
            CobraAdicional = true,
            MetodoImpuesto = 1,
            FechaLimiteSancion = Utc(2026, 3, 31),
            CuentaTransito = "111111111",
            BancoTransito = "Banco de Pruebas",
            CuentaTercero = "222222222",
            BancoTercero = "Banco de Pruebas",
            NombreSecretario = "Secretario de Transito",
            CargoSecretario = "Secretario",
            ValorRecibo = 15_000,
            ValorSistema = 8_500,
            PorcentajeSancion = 5
        });

        context.SaveChanges();
    }

    private static void SeedCatalogos(MainDataContext context)
    {
        EnsureTipoVehiculo(context, 1, 10, "Automovil", ClaseAgrupacionVehiculo.Automovil, 1);
        EnsureTipoVehiculo(context, 2, 20, "Camioneta", ClaseAgrupacionVehiculo.Automovil, 1);
        EnsureTipoVehiculo(context, 3, 30, "Buseta", ClaseAgrupacionVehiculo.Pasajeros, 2);
        EnsureTipoVehiculo(context, 4, 40, "Camion", ClaseAgrupacionVehiculo.Carga, 2);
        EnsureTipoVehiculo(context, 8, 80, "Volqueta", ClaseAgrupacionVehiculo.Carga, 2);
        EnsureTipoVehiculo(context, 9, 90, "Tractocamion", ClaseAgrupacionVehiculo.Carga, 2);

        EnsureColor(context, 101, "Blanco");
        EnsureColor(context, 102, "Negro");
        EnsureColor(context, 103, "Gris");
        EnsureColor(context, 104, "Rojo");
        EnsureColor(context, 105, "Azul");

        EnsureMarcaLinea(context, "Renault", "Stepway");
        EnsureMarcaLinea(context, "Chevrolet", "Spark GT");
        EnsureMarcaLinea(context, "Toyota", "Hilux");
        EnsureMarcaLinea(context, "Mazda", "CX-30");
        EnsureMarcaLinea(context, "Kenworth", "T800");
        EnsureMarcaLinea(context, "Mercedes Benz", "Sprinter");

        context.SaveChanges();
    }

    private static void SeedReglasLiquidacion(MainDataContext context)
    {
        if (!context.Descuentos.Any())
        {
            context.Descuentos.AddRange(
                new Descuento { Desde = Utc(2026, 1, 1), Hasta = Utc(2026, 6, 30), Porcentaje = 10 },
                new Descuento { Desde = Utc(2026, 7, 1), Hasta = Utc(2026, 12, 31), Porcentaje = 0 });
        }

        if (!context.Intereses.Any())
        {
            for (var year = 2017; year <= 2026; year++)
            {
                context.Intereses.Add(new Interes
                {
                    Desde = Utc(year, 1, 1),
                    Hasta = Utc(year, 12, 31),
                    Porcentaje = 28m + ((year - 2017) * 0.75m)
                });
            }
        }

        if (!context.Uvts.Any())
        {
            context.Uvts.AddRange(
                new Uvt { FechaDesde = Utc(2024, 1, 1), FechaHasta = Utc(2024, 12, 31), Valor = 47_065 },
                new Uvt { FechaDesde = Utc(2025, 1, 1), FechaHasta = Utc(2025, 12, 31), Valor = 49_799 },
                new Uvt { FechaDesde = Utc(2026, 1, 1), FechaHasta = Utc(2026, 12, 31), Valor = 52_500 });
        }

        if (!context.Tarifas.Any())
        {
            var automovil = context.TipoVehiculos.First(t => t.Id == 1);
            var camioneta = context.TipoVehiculos.First(t => t.Id == 2);
            var camion = context.TipoVehiculos.First(t => t.Id == 4);

            for (var year = 2017; year <= 2026; year++)
            {
                var incremento = (year - 2017) * 7_500;

                context.Tarifas.AddRange(
                    Tarifa(year, 0, 9_999_999, 180_000 + incremento, automovil, TipoServicioVehiculo.Particular, TipoConceptoTarifa.Rodamiento),
                    Tarifa(year, 10_000_000, 99_999_999, 245_000 + incremento, camioneta, TipoServicioVehiculo.Particular, TipoConceptoTarifa.Rodamiento),
                    Tarifa(year, 0, 25, 95_000 + incremento, camion, TipoServicioVehiculo.Publico, TipoConceptoTarifa.Carga),
                    Tarifa(year, 0, 45, 75_000 + incremento, automovil, TipoServicioVehiculo.Publico, TipoConceptoTarifa.Pasajeros));
            }
        }

        context.SaveChanges();
    }

    private static void SeedUsuarios(MainDataContext context)
    {
        if (context.Usuarios is null || context.Usuarios.Any()) return;

        context.Usuarios.Add(new Usuario
        {
            UserName = "admin",
            Nombre = "Administrador Testing",
            Auth0Id = "local-admin",
            Role = Role.Administrador,
            Correo = "admin@test.local",
            Password = "admin",
            IsHabilitado = true
        });

        context.SaveChanges();
    }

    private static void SeedVehiculosYCartera(MainDataContext context)
    {
        var existentes = context.Vehiculos.Count();

        if (existentes >= TargetVehiculosTesting) return;

        var random = new Random(1098825894);

        var nombres = new[] { "Juan", "Carlos", "Diana", "Sandra", "Camila", "Jorge", "Luis", "Pedro", "Maria", "Andres", "Diego", "Paula", "Marta", "Fabian", "Nelson", "Gloria" };
        var apellidos = new[] { "Gomez", "Rodriguez", "Lopez", "Perez", "Castro", "Silva", "Diaz", "Ortiz", "Mendoza", "Velez", "Chinchilla", "Duarte", "Sarmiento", "Rios" };
        var direcciones = new[] { "Calle 10 # 5-20", "Carrera 7 # 12-45", "Barrio Centro", "Avenida Principal", "Calle 3 # 8-19", "Zona Industrial Lt 4", "Avenida Los Patios" };
        var placasBase = new[] { "ALB", "TST", "RDM", "JHN", "CAR", "IMP", "VEH", "TAX", "COL", "MZN", "BGA", "CUC" };

        // Prefijos reales colombianos para simular cédulas variadas (antiguas y nuevas)
        var prefijosCedulas = new[]
        {
            "72",   // Pasto / Nariño
            "19",   // Cauca
            "91",   // Amazonas
            "37",   // Santander
            "51",   // Antioquia
            "1098", // Cédulas nuevas (Santander)
            "1143", // Cédulas nuevas (Bolívar)
            "1015"  // Cédulas nuevas (Bogotá)
        };

        var marcas = context.Marcas.AsNoTracking().OrderBy(m => m.Id).ToList();
        var lineas = context.Lineas.AsNoTracking().OrderBy(l => l.Id).ToList();
        var colores = context.Colores.AsNoTracking().OrderBy(c => c.Id).ToList();
        var tipos = context.TipoVehiculos.AsNoTracking().OrderBy(t => t.Id).ToList();

        // HashSet para controlar duplicados de placas en memoria velozmente
        var placasExistentes = new HashSet<string>(context.Vehiculos.Select(v => v.Placa).ToList());

        // Procesar en lotes de 200 para no reventar la memoria RAM en el SaveChanges
        const int batchSize = 200;
        int loteActual = 0;

        for (var i = existentes; i < TargetVehiculosTesting; i++)
        {
            var nombreCompleto = $"{Pick(nombres, random)} {Pick(apellidos, random)}";

            // 🎯 GENERACIÓN DINÁMICA DE CÉDULAS (Máximo 10 dígitos)
            var prefijo = Pick(prefijosCedulas, random);
            string documentoGenerado;

            if (prefijo.Length == 2)
            {
                // Cédulas tradicionales/antiguas (7 a 8 dígitos):
                // Prefijo (2 dígitos) + aleatorio de 5 dígitos + el residuo 'i % 10' para evitar colisiones idénticas.
                documentoGenerado = $"{prefijo}{random.Next(10000, 99999)}{i % 10}";
            }
            else
            {
                // Cédulas de nueva generación (10 dígitos exactos):
                // Prefijo (4 dígitos) + 'i' formateado secuencialmente con ceros a la izquierda a 6 posiciones.
                documentoGenerado = $"{prefijo}{i:D6}";
            }

            var propietario = new Propietario
            {
                Documento = documentoGenerado,
                Nombre = nombreCompleto,
                Direccion = Pick(direcciones, random),
                Telefono = $"315{random.Next(1000000, 9999999)}",
                TipoDocumento = TipoDocumento.Cc
            };

            var tipo = tipos[i % tipos.Count];
            var marca = marcas[i % marcas.Count];
            var linea = lineas[i % lineas.Count];
            var color = colores[i % colores.Count];

            var modelo = random.Next(2017, 2025);
            var pagoHasta = random.Next(modelo, Math.Min(modelo + 3, 2025));

            var placa = $"{Pick(placasBase, random)}{random.Next(10, 99)}{i % 10}";
            while (placasExistentes.Contains(placa))
            {
                placa = $"{Pick(placasBase, random)}{random.Next(100, 999)}";
            }
            placasExistentes.Add(placa);

            var vehiculo = new Vehiculo
            {
                Placa = placa,
                Modelo = modelo,
                Cilindraje = tipo.Id is 4 or 8 or 9 ? 5200 : random.Next(1200, 2600),
                PagoHasta = pagoHasta,
                CapacidadCarga = tipo.Id is 4 or 8 or 9 ? random.Next(8_000, 22_000) : 0,
                Pasajeros = tipo.Id is 3 ? random.Next(18, 36) : random.Next(4, 7),
                TipoVehiculoId = tipo.Id,
                MarcaId = marca.Id,
                LineaId = linea.Id,
                ColorId = color.Id,
                TipoServicioVehiculo = tipo.Id is 3 or 4 or 8 or 9 ? TipoServicioVehiculo.Publico : TipoServicioVehiculo.Particular,
                TipoCarroceriaId = 1,
                Propietario = propietario // Asignación limpia por objeto de navegación
            };

            context.Vehiculos.Add(vehiculo);

            // Generar la cartera y asociarla dinámicamente
            CrearCarteraDePrueba(context, vehiculo, random);

            loteActual++;
            if (loteActual >= batchSize)
            {
                context.SaveChanges();
                loteActual = 0;
            }
        }

        if (loteActual > 0)
        {
            context.SaveChanges();
        }
    }

    private static void SeedRecibosDePrueba(MainDataContext context)
    {
        if (context.Recibos.Any()) return;

        var vehiculos = context.Vehiculos
            .Include(v => v.Propietario)
            .OrderBy(v => v.Id)
            .Take(8)
            .ToList();

        foreach (var vehiculo in vehiculos)
        {
            var carteraPagada = context.Cartera
                .Where(c => c.VehiculoId == vehiculo.Id && c.Vigencia <= vehiculo.PagoHasta)
                .ToList();

            if (carteraPagada.Count == 0) continue;

            var recibo = new Recibo
            {
                VehiculoId = vehiculo.Id,
                Estado = EstadoRecibo.Pendiente,
                Fecha = Utc(2026, 2, 15),
                FechaPago = Utc(2026, 2, 16),
                ValorCapital = carteraPagada.Sum(c => c.Valor),
                InteresMora = carteraPagada.Sum(c => c.ValorInteres),
                Descuento = carteraPagada.Sum(c => c.Descuento),
                Estampillas = carteraPagada.Where(c => c.Concepto == TipoConceptoCartera.Estampillas).Sum(c => c.Valor),
                ValorCargaDatos = carteraPagada.Where(c => c.Concepto == TipoConceptoCartera.Carga).Sum(c => c.Valor),
                ValorRodamiento = carteraPagada.Where(c => c.Concepto == TipoConceptoCartera.Rodamiento).Sum(c => c.Valor),
                ValorTotalSistema = carteraPagada.Sum(c => c.ValorTotal),
                Detalles = new List<ReciboDetalle>()
            };

            foreach (var item in carteraPagada)
            {
                item.IsPagado = true;

                recibo.Detalles.Add(new ReciboDetalle
                {
                    CarteraId = item.Id,
                    Vigencia = item.Vigencia,
                    Concepto = item.Concepto,
                    Valor = item.Valor,
                    ValorInteres = item.ValorInteres,
                    Descuento = item.Descuento,
                    ValorTotal = item.ValorTotal
                });
            }

            context.Recibos.Add(recibo);
        }
        context.SaveChanges();
    }

    private static void CrearCarteraDePrueba(MainDataContext context, Vehiculo vehiculo, Random random)
    {
        int anioActual = 2026;
        Proceso? procesoDeVehiculo = null;

        for (var vigencia = vehiculo.Modelo; vigencia <= anioActual; vigencia++)
        {
            var baseRodamiento = random.Next(180_000, 360_000);
            var interes = vigencia < anioActual ? Math.Round(baseRodamiento * 0.12m, 0) : 0;
            var descuento = vigencia == anioActual ? Math.Round(interes * 0.10m, 0) : 0;
            var estaPagado = vigencia <= vehiculo.PagoHasta;
            var estaEnCoactivo = !estaPagado && vigencia <= 2021 && random.Next(0, 3) == 0;

            var requiereAviso = !estaPagado && vigencia < anioActual;

            if (requiereAviso)
            {
                if (procesoDeVehiculo == null)
                {
                    procesoDeVehiculo = CrearProcesoDePrueba(vehiculo, vigencia, estaEnCoactivo);
                    context.Procesos.Add(procesoDeVehiculo);
                }
                else if (estaEnCoactivo && procesoDeVehiculo.EstadoProceso != EstadoProceso.Coactivo)
                {
                    procesoDeVehiculo.EstadoProceso = EstadoProceso.Coactivo;
                }
            }

            var carteraRodamiento = GenerarObjetoCartera(vehiculo, vigencia, TipoConceptoCartera.Rodamiento, "IMPUESTO", baseRodamiento, interes, descuento, true, estaPagado,
                estaEnCoactivo);
            if (carteraRodamiento != null)
            {
                if (requiereAviso)
                {
                    GenerarAvisosEscalonados(procesoDeVehiculo!, carteraRodamiento, anioActual, random);
                }
                context.Cartera.Add(carteraRodamiento);
            }

            var estampillas = Math.Round(baseRodamiento * 0.02m, 0);
            var carteraEstampillas = GenerarObjetoCartera(vehiculo, vigencia, TipoConceptoCartera.Estampillas, "ESTAMPILLA", estampillas, 0, 0, false, estaPagado, estaEnCoactivo);
            if (carteraEstampillas != null)
            {
                if (requiereAviso)
                {
                    GenerarAvisosEscalonados(procesoDeVehiculo!, carteraEstampillas, anioActual, random);
                }
                context.Cartera.Add(carteraEstampillas);
            }

            if (vehiculo.TipoServicioVehiculo == TipoServicioVehiculo.Publico)
            {
                var carga = vehiculo.CapacidadCarga > 0 ? random.Next(90_000, 180_000) : random.Next(60_000, 120_000);
                var carteraCarga = GenerarObjetoCartera(vehiculo, vigencia, TipoConceptoCartera.Carga, "ADICIONAL", carga, vigencia < anioActual ? Math.Round(carga * 0.08m, 0) : 0, 0, true,
                    estaPagado, estaEnCoactivo);
                if (carteraCarga != null)
                {
                    if (requiereAviso)
                    {
                        GenerarAvisosEscalonados(procesoDeVehiculo!, carteraCarga, anioActual, random);
                    }
                    context.Cartera.Add(carteraCarga);
                }
            }

            if (!estaPagado && vigencia <= 2024)
            {
                var carteraCostas = GenerarObjetoCartera(vehiculo, vigencia, TipoConceptoCartera.Costas, "PROCESO", random.Next(25_000, 75_000), 0, 0, false, false, estaEnCoactivo);
                if (carteraCostas != null)
                {
                    if (vigencia < anioActual)
                    {
                        if (procesoDeVehiculo == null)
                        {
                            procesoDeVehiculo = CrearProcesoDePrueba(vehiculo, vigencia, estaEnCoactivo);
                            context.Procesos.Add(procesoDeVehiculo);
                        }

                        GenerarAvisosEscalonados(procesoDeVehiculo, carteraCostas, anioActual, random);
                    }
                    context.Cartera.Add(carteraCostas);
                }
            }
        }
    }

    private static Proceso CrearProcesoDePrueba(Vehiculo vehiculo, int vigenciaInicial, bool esCoactivo)
    {
        var fechaMandamiento = Utc(vigenciaInicial + 1, 3, 15);

        return new Proceso
        {
            Vehiculo = vehiculo, // Asignamos la relación por objeto de navegación de forma segura
            Fecha = fechaMandamiento,
            FechaMandamiento = fechaMandamiento,
            FechaProceso = fechaMandamiento,
            Valor = 0,
            EstadoProceso = esCoactivo ? EstadoProceso.Coactivo : EstadoProceso.Persuasivo,
            Desde = vigenciaInicial,
            Avisos = new List<Aviso>() // Previene excepciones de referencia nula al llamar .Add() más adelante
        };
    }

    private static Cartera GenerarObjetoCartera(
        Vehiculo vehiculo, int vigencia, TipoConceptoCartera concepto, string tipo,
        decimal valor, decimal interes, decimal descuento, bool tieneInteres, bool isPagado, bool estaEnCoactivo)
    {
        return new Cartera
        {
            Vehiculo = vehiculo, // Vinculación mediante objeto limpia
            Placa = vehiculo.Placa,
            Vigencia = vigencia,
            Concepto = concepto,
            Tipo = tipo,
            Valor = valor,
            ValorInteres = interes,
            Descuento = descuento,
            ValorTotal = valor + interes - descuento,
            TieneInteres = tieneInteres,
            IsPagado = isPagado,
        };
    }

    private static void GenerarAvisosEscalonados(Proceso proceso, Cartera cartera, int anioActual, Random random)
    {
        int anosDeMora = anioActual - cartera.Vigencia;

        int totalAvisosAGenerar = anosDeMora switch
        {
            1 => 2,
            >= 2 => 4,
            _ => 0
        };

        for (int i = 1; i <= totalAvisosAGenerar; i++)
        {
            int mesEnvio = 2 + (i * 2);
            var fechaAviso = Utc(cartera.Vigencia + 1, mesEnvio > 12 ? 12 : mesEnvio, random.Next(1, 28));

            proceso.Avisos.Add(new Aviso
            {
                Proceso = proceso,
                NumeroAviso = i,
                FechaEnvio = fechaAviso,
                NumeroGuia = $"GR-{cartera.Vigencia}{cartera.Id}{i}",
                RutaPdf = $"/docs/avisos/{cartera.Vigencia}/aviso_{i}_{cartera.Placa}.pdf",
                Estado = random.Next(0, 10) == 0 ? "Devuelto" : "Entregado"
            });
        }
    }

    private static void EnsureTipoVehiculo(MainDataContext context, int id, int codigo, string nombre, ClaseAgrupacionVehiculo tipo, int modalidad)
    {
        if (context.TipoVehiculos.Any(t => t.Id == id)) return;

        context.TipoVehiculos.Add(new TipoVehiculo
        {
            Id = id,
            Codigo = codigo,
            Nombre = nombre,
            Tipo = tipo,
            ModalidadServicio = modalidad,
            Uvt = 0
        });
    }

    private static void EnsureColor(MainDataContext context, int codigo, string nombre)
    {
        if (context.Colores.Any(c => c.Codigo == codigo)) return;

        context.Colores.Add(new Color
        {
            Codigo = codigo,
            Nombre = nombre
        });
    }

    private static void EnsureMarcaLinea(MainDataContext context, string marcaNombre, string lineaNombre)
    {
        var marca = context.Marcas.FirstOrDefault(m => m.Nombre == marcaNombre);
        if (marca is null)
        {
            marca = new Marca { Nombre = marcaNombre };
            context.Marcas.Add(marca);
            context.SaveChanges();
        }

        if (context.Lineas.Any(l => l.Nombre == lineaNombre && l.IdMarca == marca.Id)) return;

        context.Lineas.Add(new Linea
        {
            Nombre = lineaNombre,
            IdMarca = marca.Id,
            Marca = marca
        });
    }

    private static Tarifa Tarifa(
        int anio, int rangoInicial, int rangoFinal, decimal valor,
        TipoVehiculo tipoVehiculo, TipoServicioVehiculo servicio, TipoConceptoTarifa concepto)
    {
        return new Tarifa
        {
            AnioFiscal = anio,
            RangoInicial = rangoInicial,
            RangoFinal = rangoFinal,
            Valor = valor,
            TipoVehiculoId = tipoVehiculo.Id,
            TipoVehiculo = tipoVehiculo,
            TipoServicioVehiculo = servicio,
            ConceptoTarifa = concepto
        };
    }

    private static T Pick<T>(IReadOnlyList<T> values, Random random) => values[random.Next(values.Count)];

    private static DateTime Utc(int year, int month, int day) => new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
}