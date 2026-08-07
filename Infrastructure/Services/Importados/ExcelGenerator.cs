using Domain.Models.ProcesoLiquidacion;
using Domain.Responses.Vehiculos.Enums;
using Infrastructure.Services.Importados.Responses;
using MiniExcelLibs;

namespace Infrastructure.Services.Importados;

public static class ExcelTestGenerator
{
    public static void GenerarExcelPrueba(string rutaSalida, int cantidad = 100)
    {
        var random = new Random(1098825894);

        var nombres = new[] { "Juan", "Carlos", "Diana", "Sandra", "Camila", "Jorge", "Luis", "Pedro", "Maria", "Andres", "Diego", "Paula" };
        var apellidos = new[] { "Gomez", "Rodriguez", "Lopez", "Perez", "Castro", "Silva", "Diaz", "Ortiz", "Mendoza", "Velez" };
        var direcciones = new[] { "Calle 10 # 5-20", "Carrera 7 # 12-45", "Barrio Centro", "Avenida Principal", "Calle 3 # 8-19" };
        var placasBase = new[] { "ALB", "TST", "RDM", "JHN", "CAR", "IMP", "VEH", "TAX", "COL", "MZN", "BGA", "CUC" };
        var prefijosCedulas = new[] { "72", "19", "91", "37", "51", "1098", "1143", "1015" };
        var colores = new[] { "Blanco", "Negro", "Gris", "Rojo", "Azul" };

        var catalogosVehiculos = new[]
        {
            // MOTOCICLETAS (78% de probabilidad)
            new { Marca = "YAMAHA", Linea = "FZ 25", Tipo = "MOTOCICLETA", CilindrajeMin = 249, CilindrajeMax = 249, Servicio = TipoServicioVehiculo.Particular, CargaMin = 0, CargaMax = 0, Pasajeros = 2 },
            new { Marca = "BAJAJ", Linea = "PULSAR NS 200", Tipo = "MOTOCICLETA", CilindrajeMin = 199, CilindrajeMax = 199, Servicio = TipoServicioVehiculo.Particular, CargaMin = 0, CargaMax = 0, Pasajeros = 2 },
            new { Marca = "AKT", Linea = "NKD 125", Tipo = "MOTOCICLETA", CilindrajeMin = 124, CilindrajeMax = 124, Servicio = TipoServicioVehiculo.Particular, CargaMin = 0, CargaMax = 0, Pasajeros = 2 },
            
            // AUTOMOVILES (16% de probabilidad)
            new { Marca = "RENAULT", Linea = "STEPWAY", Tipo = "AUTOMOVIL", CilindrajeMin = 1600, CilindrajeMax = 1600, Servicio = TipoServicioVehiculo.Particular, CargaMin = 0, CargaMax = 0, Pasajeros = 5 },
            new { Marca = "CHEVROLET", Linea = "SPARK GT", Tipo = "AUTOMOVIL", CilindrajeMin = 1200, CilindrajeMax = 1200, Servicio = TipoServicioVehiculo.Particular, CargaMin = 0, CargaMax = 0, Pasajeros = 5 },
            
            // CAMIONETAS (4% de probabilidad)
            new { Marca = "TOYOTA", Linea = "HILUX", Tipo = "CAMIONETA", CilindrajeMin = 2400, CilindrajeMax = 2800, Servicio = TipoServicioVehiculo.Particular, CargaMin = 0, CargaMax = 0, Pasajeros = 5 },
            new { Marca = "MAZDA", Linea = "CX-30", Tipo = "CAMIONETA", CilindrajeMin = 2000, CilindrajeMax = 2500, Servicio = TipoServicioVehiculo.Particular, CargaMin = 0, CargaMax = 0, Pasajeros = 5 },
            
            // CAMIONES / CARGA MEDIA (1.5% de probabilidad) - Servicio Carga (ID = 10)
            new { Marca = "CHEVROLET", Linea = "NKR", Tipo = "CAMION", CilindrajeMin = 2800, CilindrajeMax = 3000, Servicio = TipoServicioVehiculo.Carga, CargaMin = 3500, CargaMax = 8000, Pasajeros = 3 },
            
            // TRACTOCAMIONES (0.5% de probabilidad) - Servicio Carga (ID = 10)
            new { Marca = "KENWORTH", Linea = "T800", Tipo = "TRACTOCAMION", CilindrajeMin = 10800, CilindrajeMax = 12000, Servicio = TipoServicioVehiculo.Carga, CargaMin = 15000, CargaMax = 32000, Pasajeros = 2 }
        };

        var propietariosFrecuentes = new List<(string Documento, string Nombre)>();
        for (int p = 1; p <= 15; p++)
        {
            var prefijo = Pick(prefijosCedulas, random);
            string doc = prefijo.Length == 2 ? $"{prefijo}{random.Next(10000, 99999)}0" : $"{prefijo}{p:D6}";
            string nombreCompleto = $"{Pick(nombres, random)} {Pick(apellidos, random)}";
            propietariosFrecuentes.Add((doc, nombreCompleto));
        }

        var listaDtos = new List<ImportacionVehiculoDto>();
        var placasUsadas = new HashSet<string>();
        var letrasMoto = "ABCDEFGHJKLMNPQRSTUVWXYZ";

        int anioActual = DateTime.UtcNow.Year;
        int limitePrescripcion = anioActual - 5; // Límite de 5 años atrás (ej: 2021)

        for (int i = 1; i <= cantidad; i++)
        {
            var catalogoElegido = SeleccionarCatalogoRealista(catalogosVehiculos, random);

            string placa;
            do
            {
                string prefijoPlaca = Pick(placasBase, random);
                if (catalogoElegido.Tipo == "MOTOCICLETA")
                {
                    char letraFinal = letrasMoto[random.Next(letrasMoto.Length)];
                    placa = $"{prefijoPlaca}{random.Next(10, 99)}{letraFinal}";
                }
                else
                {
                    placa = $"{prefijoPlaca}{random.Next(100, 1000)}";
                }
            } while (!placasUsadas.Add(placa));

            bool esMultiVehiculo = random.Next(1, 101) <= 12;
            string documento;
            string nombrePropietario;

            if (esMultiVehiculo)
            {
                var duenoExistente = Pick(propietariosFrecuentes, random);
                documento = duenoExistente.Documento;
                nombrePropietario = duenoExistente.Nombre;
            }
            else
            {
                var prefijo = Pick(prefijosCedulas, random);
                documento = prefijo.Length == 2 ? $"{prefijo}{random.Next(10000, 99999)}{i % 10}" : $"{prefijo}{i:D6}";
                nombrePropietario = $"{Pick(nombres, random)} {Pick(apellidos, random)}";
            }

            var modelo = random.Next(2012, anioActual + 1);

            int probEstado = random.Next(1, 101);
            EstadoProceso estadoProcesoSimulado = probEstado switch
            {
                <= 70 => EstadoProceso.SinProceso,
                <= 85 => EstadoProceso.Persuasivo,
                <= 95 => EstadoProceso.MandamientoPago,
                _ => EstadoProceso.Coactivo
            };

            int cilindraje = random.Next(catalogoElegido.CilindrajeMin, catalogoElegido.CilindrajeMax + 1);
            int capacidadCarga = catalogoElegido.CargaMax > 0 ? random.Next(catalogoElegido.CargaMin, catalogoElegido.CargaMax) : 0;

            // 🔴 AJUSTE DE MORA REALISTA (Máximo 5 años de antigüedad por prescripción)
            int probPagos = random.Next(1, 101);
            int ultimaVigenciaPagada = probPagos switch
            {
                <= 40 => anioActual - 1,                      // 1 año de mora (40%)
                <= 65 => anioActual - 2,                      // 2 años de mora (25%)
                <= 85 => anioActual - 3,                      // 3 años de mora (20%)
                <= 95 => anioActual - 4,                      // 4 años de mora (10%)
                _ => Math.Max(limitePrescripcion, modelo - 1) // 5 años max. por prescripción (5%)
            };

            // Asegurar que no sea menor al año previo de fabricación del vehículo
            if (ultimaVigenciaPagada < modelo - 1)
            {
                ultimaVigenciaPagada = modelo - 1;
            }

            listaDtos.Add(new ImportacionVehiculoDto
            {
                Placa = placa,
                Modelo = modelo,
                Cilindraje = cilindraje,
                TipoDocumento = "CC",
                DocumentoPropietario = documento,
                NombrePropietario = nombrePropietario,
                TelefonoPropietario = $"315{random.Next(1000000, 9999999)}",
                DireccionPropietario = Pick(direcciones, random),
                CorreoPropietario = $"usuario_{documento}@correo.com",
                Marca = catalogoElegido.Marca,
                Linea = catalogoElegido.Linea,
                TipoVehiculo = catalogoElegido.Tipo,
                Color = Pick(colores, random),
                
                TipoServicio = (int)catalogoElegido.Servicio,
                CapacidadCarga = capacidadCarga,
                Pasajeros = catalogoElegido.Pasajeros,

                UltimaVigenciaPagada = ultimaVigenciaPagada,
                EstadoProceso = estadoProcesoSimulado,
                TieneAcuerdoPago = random.Next(1, 101) <= 5
            });
        }

        MiniExcel.SaveAs(rutaSalida, listaDtos, true);
    }

    private static T SeleccionarCatalogoRealista<T>(T[] opciones, Random random)
    {
        int prob = random.Next(1, 1001);

        string tipoBuscado = prob switch
        {
            <= 780 => "MOTOCICLETA",
            <= 940 => "AUTOMOVIL",
            <= 980 => "CAMIONETA",
            <= 995 => "CAMION",
            _ => "TRACTOCAMION"
        };

        var deEseTipo = opciones.Where(o => ((dynamic)o).Tipo == tipoBuscado).ToArray();
        return Pick(deEseTipo.Length > 0 ? deEseTipo : opciones, random);
    }

    private static T Pick<T>(IReadOnlyList<T> values, Random random) => values[random.Next(values.Count)];
}