using Domain.Generics;
using Domain.Models;
using Domain.Models.Carteras;
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
using Infrastructure.Services.Carteras;
using Infrastructure.Services.Security;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Alimentador;

public static class DbInitializer
{
    private const int TargetVehiculosTesting = 10000;

    public static void Initialize(MainDataContext context, CarteraService carteraService)
    {
        context.Database.EnsureCreated();

        SeedParametros(context);
        SeedCatalogos(context);
        SeedReglasLiquidacion(context);
        SeedUsuarios(context);
        SeedVehiculosYCartera(context, carteraService);
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
        EnsureTipoVehiculo(context, 5, 50, "Motocicleta", ClaseAgrupacionVehiculo.Automovil, 1); 
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
        EnsureMarcaLinea(context, "Yamaha", "FZ 25");
        EnsureMarcaLinea(context, "Bajaj", "Pulsar NS 200");
        EnsureMarcaLinea(context, "AKT", "NKD 125");
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
            var moto = context.TipoVehiculos.First(t => t.Id == 5);

            for (var year = 2017; year <= 2026; year++)
            {
                var incremento = (year - 2017) * 7_500;

                context.Tarifas.AddRange(
                    Tarifa(year, 0, 9_999_999, 180_000 + incremento, automovil, TipoServicioVehiculo.Particular, TipoConceptoTarifa.Rodamiento),
                    Tarifa(year, 10_000_000, 99_999_999, 245_000 + incremento, camioneta, TipoServicioVehiculo.Particular, TipoConceptoTarifa.Rodamiento),
                    Tarifa(year, 0, 125, 60_000 + incremento, moto, TipoServicioVehiculo.Particular, TipoConceptoTarifa.Rodamiento),
                    Tarifa(year, 0, 25, 95_000 + incremento, camion, TipoServicioVehiculo.Publico, TipoConceptoTarifa.Carga),
                    Tarifa(year, 0, 45, 75_000 + incremento, automovil, TipoServicioVehiculo.Publico, TipoConceptoTarifa.Pasajeros));
            }
        }

        context.SaveChanges();
    }

    private static void SeedUsuarios(MainDataContext context)
    {
        if (context.Usuarios is null) return;

        var admin = context.Usuarios.FirstOrDefault(u => u.UserName == "admin");
        if (admin is not null)
        {
            if (!PasswordHasher.Verify("admin", admin.Password))
            {
                admin.Password = PasswordHasher.Hash("admin");
                admin.Role = Role.Administrador;
                admin.IsHabilitado = true;
                context.SaveChanges();
            }

            return;
        }

        context.Usuarios.Add(new Usuario
        {
            UserName = "admin",
            Nombre = "Administrador Testing",
            Auth0Id = "local-admin",
            Role = Role.Administrador,
            Correo = "admin@test.local",
            Password = PasswordHasher.Hash("admin"),
            IsHabilitado = true
        });

        context.SaveChanges();
    }

    private static void SeedVehiculosYCartera(MainDataContext context, CarteraService carteraService)
    {
        var existentes = context.Vehiculos.Count();
        if (existentes >= TargetVehiculosTesting) return;

        var random = new Random(1098825894);

        var nombres = new[] { "Juan", "Carlos", "Diana", "Sandra", "Camila", "Jorge", "Luis", "Pedro", "Maria", "Andres", "Diego", "Paula", "Marta", "Fabian", "Nelson", "Gloria" };
        var apellidos = new[] { "Gomez", "Rodriguez", "Lopez", "Perez", "Castro", "Silva", "Diaz", "Ortiz", "Mendoza", "Velez", "Chinchilla", "Duarte", "Sarmiento", "Rios" };
        var direcciones = new[] { "Calle 10 # 5-20", "Carrera 7 # 12-45", "Barrio Centro", "Avenida Principal", "Calle 3 # 8-19", "Zona Industrial Lt 4", "Avenida Los Patios" };
        var placasBase = new[] { "ALB", "TST", "RDM", "JHN", "CAR", "IMP", "VEH", "TAX", "COL", "MZN", "BGA", "CUC" };
        var prefijosCedulas = new[] { "72", "19", "91", "37", "51", "1098", "1143", "1015" };

        var marcas = context.Marcas.AsNoTracking().ToList();
        var lineas = context.Lineas.AsNoTracking().ToList();
        var colores = context.Colores.AsNoTracking().ToList();
        var tipos = context.TipoVehiculos.AsNoTracking().ToList();

        var placasExistentes = new HashSet<string>(context.Vehiculos.Select(v => v.Placa).ToList());

        const int batchSize = 1000;
        var vehiculosNuevos = new List<Vehiculo>();

        for (var i = existentes; i < TargetVehiculosTesting; i++)
        {
            var nombreCompleto = $"{Pick(nombres, random)} {Pick(apellidos, random)}";
            var prefijo = Pick(prefijosCedulas, random);
            string documentoGenerado = prefijo.Length == 2 
                ? $"{prefijo}{random.Next(10000, 99999)}{i % 10}" 
                : $"{prefijo}{i:D6}";

            var propietario = new Propietario
            {
                Documento = documentoGenerado,
                Nombre = nombreCompleto,
                Direccion = Pick(direcciones, random),
                Telefono = $"315{random.Next(1000000, 9999999)}",
                TipoDocumento = TipoDocumento.Cc
            };

            var tipo = SeleccionarTipoRealista(tipos, random);
            var marca = SeleccionarMarcaCoherente(marcas, tipo.Id, random);
            var lineasMarca = lineas.Where(l => l.IdMarca == marca.Id).ToList();
            var linea = lineasMarca.Any() ? Pick(lineasMarca, random) : Pick(lineas, random);
            var color = Pick(colores, random);

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
                Cilindraje = tipo.Id switch
                {
                    5 => random.Next(100, 250),
                    1 or 2 => random.Next(1200, 2500),
                    _ => random.Next(4000, 9000)
                },
                PagoHasta = pagoHasta,
                CapacidadCarga = tipo.Id is 4 or 8 or 9 ? random.Next(8_000, 22_000) : 0,
                Pasajeros = tipo.Id is 3 ? random.Next(18, 36) : (tipo.Id == 5 ? 2 : 5),
                TipoVehiculoId = tipo.Id,
                MarcaId = marca.Id,
                LineaId = linea.Id,
                ColorId = color.Id,
                TipoServicioVehiculo = tipo.Id is 3 or 4 or 8 or 9 ? TipoServicioVehiculo.Publico : TipoServicioVehiculo.Particular,
                TipoCarroceriaId = 1,
                Propietario = propietario
            };

            vehiculosNuevos.Add(vehiculo);

            // Guardado por lotes masivos
            if (vehiculosNuevos.Count >= batchSize || i == TargetVehiculosTesting - 1)
            {
                // 🚀 Guardar el bloque de vehículos de una sola vez
                context.Vehiculos.AddRange(vehiculosNuevos);
                context.SaveChanges();

                // 🚀 Generar Cartera y Procesos en memoria para el lote actual
                GenerarCarteraYProcesosEnLote(context, carteraService, vehiculosNuevos, random);
                
                vehiculosNuevos.Clear();
            }
        }
    }

    private static void GenerarCarteraYProcesosEnLote(
        MainDataContext context, 
        CarteraService carteraService, 
        List<Vehiculo> vehiculos, 
        Random random)
    {
        var procesosNuevos = new List<Proceso>();

        foreach (var vehiculo in vehiculos)
        {
            int anioDesde = vehiculo.PagoHasta + 1;
            int anioHasta = 2026;

            if (anioDesde <= anioHasta)
            {
                // Genera las carteras para el vehículo
                carteraService.GenerarCarteraVehiculo(vehiculo.Placa, anioDesde, anioHasta).GetAwaiter().GetResult();

                // Simular Proceso Legal sin consultar a la BD en cada iteración
                if (anioDesde < 2023)
                {
                    bool esCoactivo = anioDesde <= 2020 && random.Next(0, 3) == 0;
                    var fechaMandamiento = Utc(anioDesde + 1, 3, 15);

                    var proceso = new Proceso
                    {
                        VehiculoId = vehiculo.Id,
                        Fecha = fechaMandamiento,
                        FechaMandamiento = fechaMandamiento,
                        FechaProceso = fechaMandamiento,
                        Valor = 0,
                        EstadoProceso = esCoactivo ? EstadoProceso.Coactivo : EstadoProceso.Persuasivo,
                        Desde = anioDesde,
                        Avisos = new List<Aviso>()
                    };

                    // Asociar avisos según las vigencias
                    for (int vigencia = anioDesde; vigencia < anioHasta; vigencia++)
                    {
                        int anosDeMora = 2026 - vigencia;
                        int totalAvisos = anosDeMora switch
                        {
                            1 => 2,
                            >= 2 => 4,
                            _ => 0
                        };

                        for (int k = 1; k <= totalAvisos; k++)
                        {
                            int mesEnvio = 2 + (k * 2);
                            var fechaAviso = Utc(vigencia + 1, mesEnvio > 12 ? 12 : mesEnvio, random.Next(1, 28));

                            proceso.Avisos.Add(new Aviso
                            {
                                Proceso = proceso,
                                NumeroAviso = k,
                                FechaEnvio = fechaAviso,
                                NumeroGuia = $"GR-{vigencia}{vehiculo.Id}{k}",
                                RutaPdf = $"/docs/avisos/{vigencia}/aviso_{k}_{vehiculo.Placa}.pdf",
                                Estado = random.Next(0, 10) == 0 ? "Devuelto" : "Entregado"
                            });
                        }
                    }

                    procesosNuevos.Add(proceso);
                }
            }
        }

        if (procesosNuevos.Any())
        {
            context.Procesos.AddRange(procesosNuevos);
            context.SaveChanges();
        }
    }

    private static TipoVehiculo SeleccionarTipoRealista(List<TipoVehiculo> tipos, Random random)
    {
        int prob = random.Next(1, 101);

        int tipoId = prob switch
        {
            <= 55 => 5, // 55% Motocicletas
            <= 75 => 1, // 20% Automóviles
            <= 90 => 2, // 15% Camionetas
            <= 95 => 4, // 5% Camiones
            <= 98 => 8, // 3% Volquetas
            _ => 3      // 2% Busetas / Tractocamiones
        };

        return tipos.FirstOrDefault(t => t.Id == tipoId) ?? tipos.First();
    }

    private static Marca SeleccionarMarcaCoherente(List<Marca> marcas, int tipoVehiculoId, Random random)
    {
        if (tipoVehiculoId == 5)
        {
            var marcasMotos = marcas.Where(m => new[] { "Yamaha", "Bajaj", "AKT" }.Contains(m.Nombre)).ToList();
            if (marcasMotos.Any()) return Pick(marcasMotos, random);
        }
        else if (tipoVehiculoId is 1 or 2)
        {
            var marcasAutos = marcas.Where(m => new[] { "Renault", "Chevrolet", "Toyota", "Mazda" }.Contains(m.Nombre)).ToList();
            if (marcasAutos.Any()) return Pick(marcasAutos, random);
        }

        return Pick(marcas, random);
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