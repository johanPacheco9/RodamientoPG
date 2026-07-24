using Domain.Models.Vehiculos;
using Domain.Responses.Users.Enums;
using Domain.Responses.Vehiculos.Enums;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using System.Diagnostics;
using System.Text;

namespace Infrastructure.Services.Importados;

public partial class ImportadosService
{
    // Carpeta donde se guardan los reportes .txt de cada importación.
    private static readonly string CarpetaLogs = Path.Combine(Directory.GetCurrentDirectory(), "ImportLogs");

    public async Task<(bool Success, string Message, int Importados, List<string> Errores)> ImportarDesdeExcelAsync(
        Stream archivoStream,
        IProgress<int>? progreso = null)
    {
        var errores = new List<string>();
        int importados = 0;
        int filaActual = 1;
        int totalFilas = 0;
        var cronometro = Stopwatch.StartNew();

        // Rango fijo de vigencias para la cartera de los vehículos importados
        const int desdeCartera = 2012;
        int hastaCartera = DateTime.UtcNow.Year;

        try
        {
            var registros = archivoStream.Query(useHeaderRow: true).ToList();
            totalFilas = registros.Count;

            progreso?.Report(10);

            if (!registros.Any())
            {
                const string msgVacio = "El archivo de Excel está vacío o no tiene la estructura válida.";
                GuardarReporteTxt(false, msgVacio, 0, 0, errores, cronometro.Elapsed);
                return (false, msgVacio, 0, errores);
            }

            // Cargar catálogos iniciales
            var marcasDict = await CargarCatalogoSeguro(
                context.Marcas.AsNoTracking(), m => m.Nombre.ToUpper().Trim(), "Marcas", errores);

            var lineasDict = await CargarCatalogoSeguro(
                context.Lineas.AsNoTracking().Include(l => l.Marca), l => $"{l.Marca.Id}_{l.Nombre.ToUpper().Trim()}", "Lineas", errores);

            var tiposDict = await CargarCatalogoSeguro(
                context.TipoVehiculos.AsNoTracking(), t => t.Nombre.ToUpper().Trim(), "TipoVehiculos", errores);

            var coloresDict = await CargarCatalogoSeguro(
                context.Colores.AsNoTracking(), c => c.Nombre.ToUpper().Trim(), "Colores", errores);

            var propietariosDict = await CargarCatalogoSeguro(
                context.Propietarios.AsNoTracking(), p => p.Documento.Trim(), "Propietarios", errores);

            var placasEnBd = (await context.Vehiculos
                .AsNoTracking()
                .Select(v => v.Placa.ToUpper().Trim())
                .ToListAsync())
                .ToHashSet();

            // Parámetro cacheado para la cartera
            var parametroCacheado = await context.Parametros.AsNoTracking().FirstOrDefaultAsync();

            progreso?.Report(20);

            using var transaction = await context.Database.BeginTransactionAsync();

            context.ChangeTracker.AutoDetectChangesEnabled = false;

            foreach (IDictionary<string, object> fila in registros)
            {
                filaActual++;
                var nombreSavepoint = $"sp_fila_{filaActual}";
                await transaction.CreateSavepointAsync(nombreSavepoint);

                // Control de elementos agregados localmente en ESTA iteración
                string? docPropietarioNuevo = null;
                string? marcaNuevaClave = null;
                string? lineaNuevaClave = null;
                string? tipoNuevoClave = null;
                string? colorNuevoClave = null;

                try
                {
                    var placaLimpia = ObtenerTexto(fila, "Placa").Trim().ToUpper();
                    var docLimpio = ObtenerTexto(fila, "DocumentoPropietario").Trim();

                    if (string.IsNullOrWhiteSpace(placaLimpia) || placaLimpia.Length < 5)
                    {
                        errores.Add($"Fila {filaActual}: La placa '{placaLimpia}' no es válida.");
                        continue;
                    }

                    if (placasEnBd.Contains(placaLimpia))
                    {
                        errores.Add($"Fila {filaActual}: La placa '{placaLimpia}' ya existe en el sistema.");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(docLimpio))
                    {
                        errores.Add($"Fila {filaActual}: El documento del propietario es obligatorio.");
                        continue;
                    }

                    int modeloInt = ObtenerEntero(fila, "Modelo", DateTime.UtcNow.Year);
                    int cilindrajeInt = ObtenerEntero(fila, "Cilindraje", 0);

                    var nombreMarca = ObtenerTextoODefault(fila, "Marca", "GENERICA");
                    var nombreLinea = ObtenerTextoODefault(fila, "Linea", "ESTANDAR");
                    var nombreTipo  = ObtenerTextoODefault(fila, "TipoVehiculo", "AUTOMOVIL");
                    var nombreColor = ObtenerTextoODefault(fila, "Color", "SIN COLOR");

                    // 1. MARCA
                    if (!marcasDict.TryGetValue(nombreMarca, out var marca))
                    {
                        marca = new Marca { Nombre = nombreMarca };
                        context.Marcas.Add(marca);
                        await context.SaveChangesAsync();
                        marcasDict.Add(nombreMarca, marca);
                        marcaNuevaClave = nombreMarca;
                    }

                    // 2. LÍNEA
                    var claveLinea = $"{marca.Id}_{nombreLinea}";
                    if (!lineasDict.TryGetValue(claveLinea, out var linea))
                    {
                        linea = new Linea { Nombre = nombreLinea, IdMarca = marca.Id };
                        context.Lineas.Add(linea);
                        await context.SaveChangesAsync();
                        lineasDict.Add(claveLinea, linea);
                        lineaNuevaClave = claveLinea;
                    }

                    // 3. TIPO VEHÍCULO
                    if (!tiposDict.TryGetValue(nombreTipo, out var tipo))
                    {
                        tipo = new TipoVehiculo { Nombre = nombreTipo };
                        context.TipoVehiculos.Add(tipo);
                        await context.SaveChangesAsync();
                        tiposDict.Add(nombreTipo, tipo);
                        tipoNuevoClave = nombreTipo;
                    }

                    // 4. COLOR
                    if (!coloresDict.TryGetValue(nombreColor, out var color))
                    {
                        color = new Color { Nombre = nombreColor };
                        context.Colores.Add(color);
                        await context.SaveChangesAsync();
                        coloresDict.Add(nombreColor, color);
                        colorNuevoClave = nombreColor;
                    }

                    // 5. PROPIETARIO
                    if (!propietariosDict.TryGetValue(docLimpio, out var propietario))
                    {
                        var tipoDocTexto = ObtenerTexto(fila, "TipoDocumento");
                        var tipoDocParseado = TipoDocumento.Cc;
                        if (!string.IsNullOrWhiteSpace(tipoDocTexto))
                        {
                            Enum.TryParse(tipoDocTexto.Trim(), ignoreCase: true, out tipoDocParseado);
                        }

                        propietario = new Propietario
                        {
                            Documento = docLimpio,
                            TipoDocumento = tipoDocParseado,
                            Nombre = ObtenerTextoODefault(fila, "NombrePropietario", "PROPIETARIO IMPORTADO"),
                            Telefono = ObtenerTexto(fila, "TelefonoPropietario"),
                            Direccion = ObtenerTexto(fila, "DireccionPropietario"),
                            Correo = ObtenerTexto(fila, "CorreoPropietario")
                        };

                        context.Propietarios.Add(propietario);
                        await context.SaveChangesAsync();
                        propietariosDict.Add(docLimpio, propietario);
                        docPropietarioNuevo = docLimpio;
                    }

                    // 6. VEHÍCULO
                    var vehiculo = new Vehiculo
                    {
                        Placa = placaLimpia,
                        Modelo = modeloInt,
                        Cilindraje = cilindrajeInt,
                        PagoHasta = modeloInt - 1,
                        MarcaId = marca.Id,
                        LineaId = linea.Id,
                        TipoVehiculoId = tipo.Id,
                        ColorId = color.Id,
                        PropietarioId = propietario.Id,
                        TipoServicioVehiculo = TipoServicioVehiculo.Particular,
                        TipoCarroceriaId = 1
                    };

                    context.Vehiculos.Add(vehiculo);
                    await context.SaveChangesAsync();

                    // 7. CARTERA
                    await carteraService.GenerarCarteraVehiculo(
                        vehiculo, desdeCartera, hastaCartera, parametroCacheado,
                        guardarCambios: true, // ⚡ CAMBIAR A TRUE para que persista inmediatamente en PostgreSQL
                        vehiculoEsNuevo: true);

                    placasEnBd.Add(placaLimpia);
                    importados++;
                }
                catch (Exception exFila)
                {
                    // 1. Rollback al savepoint de esta fila
                    await transaction.RollbackToSavepointAsync(nombreSavepoint);

                    // 2. Limpiamos SOLO Vehiculos y Carteras agregados en esta iteración fallida
                    //    (Evita desvincular catalogos y parametroCacheado)
                    var entradasLocales = context.ChangeTracker.Entries()
                        .Where(e => e.Entity is Vehiculo || e.Entity.GetType().Name.Contains("Cartera"))
                        .ToList();

                    foreach (var entry in entradasLocales)
                    {
                        entry.State = EntityState.Detached;
                    }

                    // 3. Re-asociamos por seguridad si sigue en memoria
                    if (parametroCacheado != null && context.Entry(parametroCacheado).State == EntityState.Detached)
                    {
                        context.Attach(parametroCacheado);
                    }

                    // 4. Limpiar la caché local del diccionario de lo que falló en esta fila
                    if (docPropietarioNuevo != null) propietariosDict.Remove(docPropietarioNuevo);
                    if (marcaNuevaClave != null) marcasDict.Remove(marcaNuevaClave);
                    if (lineaNuevaClave != null) lineasDict.Remove(lineaNuevaClave);
                    if (tipoNuevoClave != null) tiposDict.Remove(tipoNuevoClave);
                    if (colorNuevoClave != null) coloresDict.Remove(colorNuevoClave);

                    errores.Add($"Fila {filaActual}: Error inesperado - {exFila.Message}");
                }

                if (progreso != null && (filaActual % 25 == 0 || filaActual == registros.Count + 1))
                {
                    var avanceFilas = (double)(filaActual - 1) / registros.Count;
                    var porcentajeEscalado = 20 + (int)(avanceFilas * 70);
                    progreso.Report(Math.Min(porcentajeEscalado, 90));
                }
            }

            progreso?.Report(95);

            if (importados > 0)
            {
                await context.SaveChangesAsync(); // Guarda todas las carteras acumuladas exitosas
                await transaction.CommitAsync();
                cronometro.Stop();

                var msgExito = $"Importación completada con éxito. Vehículos creados: {importados}.";
                GuardarReporteTxt(true, msgExito, importados, totalFilas, errores, cronometro.Elapsed);

                progreso?.Report(100);
                return (true, msgExito, importados, errores);
            }

            await transaction.RollbackAsync();
            cronometro.Stop();

            const string msgSinImportar = "No se importó ningún vehículo debido a errores en la validación.";
            GuardarReporteTxt(false, msgSinImportar, 0, totalFilas, errores, cronometro.Elapsed);

            progreso?.Report(100);
            return (false, msgSinImportar, 0, errores);
        }
        catch (Exception ex)
        {
            cronometro.Stop();
            var msgCritico = $"Error crítico procesando el archivo: {ex.Message}";
            GuardarReporteTxt(false, msgCritico, importados, totalFilas, errores, cronometro.Elapsed);
            return (false, msgCritico, 0, errores);
        }
        finally
        {
            context.ChangeTracker.AutoDetectChangesEnabled = true;
        }
    }

    // ── Guarda un archivo .txt con el resumen de la importación ──────────────
    private static void GuardarReporteTxt(
        bool exito,
        string mensaje,
        int importados,
        int totalFilas,
        List<string> errores,
        TimeSpan duracion)
    {
        try
        {
            Directory.CreateDirectory(CarpetaLogs);

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var nombreArchivo = $"importacion_{timestamp}.txt";
            var rutaCompleta = Path.Combine(CarpetaLogs, nombreArchivo);

            var sb = new StringBuilder();
            sb.AppendLine("==================================================");
            sb.AppendLine(" REPORTE DE IMPORTACIÓN DE VEHÍCULOS");
            sb.AppendLine("==================================================");
            sb.AppendLine($"Fecha/Hora     : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Resultado      : {(exito ? "ÉXITO" : "CON PROBLEMAS")}");
            sb.AppendLine($"Mensaje        : {mensaje}");
            sb.AppendLine($"Duración       : {duracion.TotalSeconds:0.00} segundos");
            sb.AppendLine($"Filas en Excel : {totalFilas}");
            sb.AppendLine($"Importados     : {importados}");
            sb.AppendLine($"Errores/Avisos : {errores.Count}");
            sb.AppendLine("--------------------------------------------------");

            if (errores.Any())
            {
                sb.AppendLine("Detalle de errores/avisos:");
                foreach (var err in errores)
                {
                    sb.AppendLine($"  - {err}");
                }
            }
            else
            {
                sb.AppendLine("Sin errores ni avisos.");
            }

            sb.AppendLine("==================================================");

            File.WriteAllText(rutaCompleta, sb.ToString(), Encoding.UTF8);
        }
        catch
        {
            // Ignorar errores al escribir archivo de log secundario
        }
    }

    // ── Helper: carga un catálogo a Dictionary tolerando claves duplicadas ────
    private static async Task<Dictionary<string, T>> CargarCatalogoSeguro<T>(
        IQueryable<T> query,
        Func<T, string> selectorClave,
        string nombreCatalogo,
        List<string> errores)
        where T : class
    {
        var todos = await query.ToListAsync();

        var grupos = todos
            .GroupBy(selectorClave)
            .ToList();

        var duplicados = grupos.Where(g => g.Count() > 1).ToList();
        if (duplicados.Any())
        {
            var detalle = string.Join(", ", duplicados.Select(g => $"'{g.Key}' ({g.Count()} veces)"));
            errores.Add($"Aviso: se encontraron registros duplicados preexistentes en {nombreCatalogo}: {detalle}. " +
                        "Se usó el primer registro de cada uno; se recomienda depurar la tabla.");
        }

        return grupos.ToDictionary(g => g.Key, g => g.First());
    }

    // ── Helpers de conversión segura ─────────────────────────────────────────

    private static string ObtenerTexto(IDictionary<string, object> fila, string columna)
    {
        if (!fila.TryGetValue(columna, out var valor) || valor is null) return string.Empty;
        return valor.ToString()?.Trim() ?? string.Empty;
    }

    private static string ObtenerTextoODefault(IDictionary<string, object> fila, string columna, string valorDefault)
    {
        var texto = ObtenerTexto(fila, columna);
        return string.IsNullOrWhiteSpace(texto) ? valorDefault : texto.ToUpper();
    }

    private static int ObtenerEntero(IDictionary<string, object> fila, string columna, int valorDefault)
    {
        if (!fila.TryGetValue(columna, out var valor) || valor is null)
            return valorDefault;

        switch (valor)
        {
            case int i: return i;
            case double d: return (int)d;
            case decimal dec: return (int)dec;
            case float f: return (int)f;
            case long l: return (int)l;
        }

        var texto = valor.ToString()?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(texto))
            return valorDefault;

        texto = texto.Replace(".", "").Replace(",", "");

        if (int.TryParse(texto, out var parsed))
            return parsed;

        if (double.TryParse(valor.ToString(), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var parsedDouble))
            return (int)parsedDouble;

        return valorDefault;
    }
}