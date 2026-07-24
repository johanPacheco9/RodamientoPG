using Infrastructure.Services.Importados.Responses;
using MiniExcelLibs;
namespace Infrastructure.Services.Importados;

public static class ExcelTestGenerator
{
    public static void GenerarExcelPrueba(string rutaSalida, int cantidad = 100)
    {
        var random = new Random(1098825894); // Semilla fija para consistencia

        var nombres = new[] { "Juan", "Carlos", "Diana", "Sandra", "Camila", "Jorge", "Luis", "Pedro", "Maria", "Andres", "Diego", "Paula" };
        var apellidos = new[] { "Gomez", "Rodriguez", "Lopez", "Perez", "Castro", "Silva", "Diaz", "Ortiz", "Mendoza", "Velez" };
        var direcciones = new[] { "Calle 10 # 5-20", "Carrera 7 # 12-45", "Barrio Centro", "Avenida Principal", "Calle 3 # 8-19" };
        var placasBase = new[] { "ALB", "TST", "RDM", "JHN", "CAR", "IMP", "VEH", "TAX", "COL", "MZN", "BGA", "CUC" };
        var prefijosCedulas = new[] { "72", "19", "91", "37", "51", "1098", "1143", "1015" };
        var colores = new[] { "Blanco", "Negro", "Gris", "Rojo", "Azul" };

        // Definimos combinaciones realistas de Marca y Línea
        var catalogosVehiculos = new[]
        {
            new { Marca = "Renault", Linea = "Stepway", Tipo = "Automovil" },
            new { Marca = "Chevrolet", Linea = "Spark GT", Tipo = "Automovil" },
            new { Marca = "Toyota", Linea = "Hilux", Tipo = "Camioneta" },
            new { Marca = "Mazda", Linea = "CX-30", Tipo = "Camioneta" },
            new { Marca = "Yamaha", Linea = "FZ 25", Tipo = "Motocicleta" },
            new { Marca = "Bajaj", Linea = "Pulsar NS 200", Tipo = "Motocicleta" },
            new { Marca = "AKT", Linea = "NKD 125", Tipo = "Motocicleta" },
            new { Marca = "Kenworth", Linea = "T800", Tipo = "Tractocamion" },
            new { Marca = "Mercedes Benz", Linea = "Sprinter", Tipo = "Buseta" }
        };

        var listaDtos = new List<ImportacionVehiculoDto>();
        var placasUsadas = new HashSet<string>();

        for (int i = 1; i <= cantidad; i++)
        {
            // 🎯 Ponderación probabilística realista (55% Motos, 20% Autos, 15% Camionetas, etc.)
            var catalogoElegido = SeleccionarCatalogoRealista(catalogosVehiculos, random);

            // Generación de Placa Única
            string placa;
            do
            {
                placa = $"{Pick(placasBase, random)}{random.Next(10, 99)}{i % 10}";
            } while (!placasUsadas.Add(placa));

            // Documento Propietario
            var prefijo = Pick(prefijosCedulas, random);
            string documento = prefijo.Length == 2 
                ? $"{prefijo}{random.Next(10000, 99999)}{i % 10}" 
                : $"{prefijo}{i:D6}";

            var modelo = random.Next(2015, 2026);

            listaDtos.Add(new ImportacionVehiculoDto
            {
                Placa = placa,
                Modelo = modelo,
                Cilindraje = catalogoElegido.Tipo switch
                {
                    "Motocicleta" => random.Next(100, 250),
                    "Automovil" or "Camioneta" => random.Next(1200, 2500),
                    _ => random.Next(4000, 9000)
                },
                TipoDocumento = "CC",
                DocumentoPropietario = documento,
                NombrePropietario = $"{Pick(nombres, random)} {Pick(apellidos, random)}",
                TelefonoPropietario = $"315{random.Next(1000000, 9999999)}",
                DireccionPropietario = Pick(direcciones, random),
                CorreoPropietario = $"usuario{i}@correo.com",
                Marca = catalogoElegido.Marca,
                Linea = catalogoElegido.Linea,
                TipoVehiculo = catalogoElegido.Tipo,
                Color = Pick(colores, random)
            });
        }
        MiniExcel.SaveAs(rutaSalida, listaDtos, true);
    }

    private static T SeleccionarCatalogoRealista<T>(T[] opciones, Random random)
    {
        int prob = random.Next(1, 101);

        // 55% Motos, 20% Autos, 15% Camionetas, 10% Pesados
        string tipoBuscado = prob switch
        {
            <= 55 => "Motocicleta",
            <= 75 => "Automovil",
            <= 90 => "Camioneta",
            _ => "Tractocamion"
        };

        var deEseTipo = opciones.Where(o => ((dynamic)o).Tipo == tipoBuscado).ToArray();
        return Pick(deEseTipo.Length > 0 ? deEseTipo : opciones, random);
    }

    private static T Pick<T>(IReadOnlyList<T> values, Random random) => values[random.Next(values.Count)];
}