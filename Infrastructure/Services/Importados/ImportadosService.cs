using Domain.Models.BaseGravable;
using Domain.Models.Vehiculos;
using Domain.Responses.Recibo.Enums;
using Domain.Responses.Users.Enums;
using Domain.Responses.Vehiculos.Enums;
using Infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
namespace Infrastructure.Services.Importados;



public class ImportacionVehiculoDto
{
    public string Placa { get; set; } = null!;
    public int Modelo { get; set; }
    public int Cilindraje { get; set; }
    public string DocumentoPropietario { get; set; } = null!;
    public string NombrePropietario { get; set; } = null!;
    public string TelefonoPropietario { get; set; } = null!;
    public string DireccionPropietario { get; set; } = null!;
    public string CorreoPropietario { get; set; } = null!;
    public int MarcaId { get; set; }
    public int LineaId { get; set; }
    public int TipoVehiculoId { get; set; }
    public int ColorId { get; set; }
}


public class ImportadosService(MainDataContext context)
{
    public class Rvar
    {
        public int Num { get; set; }
    }

    /// <summary>
    /// Trae los primeros 20 registros de la tabla base_gravable
    /// </summary>
    public async Task<List<BaseGravableVehiculo>> GetList()
    {
        try
        {
            return await context.BaseGravableVehiculos // Tu DbSet mapeado
                .Take(20)
                .ToListAsync();
        }
        catch (Exception)
        {
            throw new Exception("No se encontraron Registros de base gravable");
        }
    }

    /// <summary>
    /// Limpia por completo la tabla de bases gravables (Trunco / Delete masivo eficiente)
    /// </summary>
    public async Task<int> DeleteAll()
    {
        // ExecuteDeleteAsync es ultra rápido en EF Core 10, no carga datos en memoria
        return await context.BaseGravableVehiculos.ExecuteDeleteAsync();
    }

    /// <summary>
    /// PROCEDIMIENTO 1: Ejecuta la función interna de procesamiento de comparendos
    /// </summary>
    public async Task<int> Procesa_Comp(int puser)
    {
        try
        {
            Console.WriteLine("Ejecutando Procesa_Comp");
            return await context.Database
                .ExecuteSqlAsync($"SELECT public.procesa_comp({puser})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en Procesar Importados : {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// PROCEDIMIENTO 2: Llama a la función que valida si existe el código base
    /// </summary>
    public async Task<Rvar?> Existe_base(string pcode)
    {
        try
        {
            // Executa la función de Postgres y mapea el entero resultante a la clase Rvar de forma segura
            return await context.Database
                .SqlQuery<Rvar>($"SELECT * FROM existe_base({pcode})")
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error buscando en existe_base: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Consulta directa a la tabla "recibos" usando LINQ parametrizado seguro
    /// </summary>
    public async Task<Rvar?> Ultimo_recibo(string placa)
    {
        try
        {
            // Pasamos de un string crudo a una consulta LINQ autogenerada
            return await context.Recibos
                .Where(r => r.Vehiculo.Placa == placa && r.Estado == EstadoRecibo.Pendiente)
                .Select(r => new Rvar { Num = r.Id })
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en recibo: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Inserta el mapeo masivo del ministerio. EF Core mapea automáticamente las 30+ columnas
    /// </summary>
    public async Task<int> Add(BaseGravableVehiculo baseGravable)
    {
        try
        {
            context.BaseGravableVehiculos.Add(baseGravable);
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creando Base_Gravable: {ex.Message}");
            throw;
        }
    }
    public async Task<(bool Success, string Message, int Importados, List<string> Errores)> ImportarDesdeExcel(Stream archivoStream)
    {
        var errores = new List<string>();
        int importados = 0;
        int filaActual = 1;

        try
        {
            // MiniExcel lee directamente el Stream mapeándolo al DTO sin cargar todo en memoria RAM
            var registros = archivoStream.Query<ImportacionVehiculoDto>().ToList();

            if (!registros.Any())
                return (false, "El archivo de Excel está vacío.", 0, errores);

            // Cargamos catálogos en memoria rápida para evitar consultas repetitivas a la base de datos
            var marcasExistentes = await context.Marcas.Select(m => m.Id).ToHashSetAsync();
            var lineasExistentes = await context.Lineas.Select(l => l.Id).ToHashSetAsync();
            var tiposExistentes = await context.TipoVehiculos.Select(t => t.Id).ToHashSetAsync();
            var coloresExistentes = await context.Colores.Select(c => c.Id).ToHashSetAsync();
            var placasEnBd = await context.Vehiculos.Select(v => v.Placa.ToUpper().Trim()).ToHashSetAsync();

            // Lista temporal de propietarios en la base de datos para no duplicar ciudadanos
            var propietariosEnBd = await context.Propietarios
                .ToDictionaryAsync(p => p.Documento, p => p.Id);

            using var transaction = await context.Database.BeginTransactionAsync();

            foreach (var fila in registros)
            {
                filaActual++;

                // ── 1. Validaciones de datos mínimos ──────────────────────────
                if (string.IsNullOrWhiteSpace(fila.Placa) || fila.Placa.Length < 5)
                {
                    errores.Add($"Fila {filaActual}: Placa inválida.");
                    continue;
                }

                var placaLimpia = fila.Placa.Trim().ToUpper();
                if (placasEnBd.Contains(placaLimpia))
                {
                    errores.Add($"Fila {filaActual}: La placa '{placaLimpia}' ya está registrada en la base de datos.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(fila.DocumentoPropietario))
                {
                    errores.Add($"Fila {filaActual}: Documento del propietario es obligatorio.");
                    continue;
                }

                // ── 2. Validaciones de Catálogos Existentes ──────────────────
                if (!marcasExistentes.Contains(fila.MarcaId))
                {
                    errores.Add($"Fila {filaActual}: El MarcaId {fila.MarcaId} no existe en el sistema.");
                    continue;
                }
                if (!lineasExistentes.Contains(fila.LineaId))
                {
                    errores.Add($"Fila {filaActual}: El LineaId {fila.LineaId} no existe.");
                    continue;
                }
                if (!tiposExistentes.Contains(fila.TipoVehiculoId))
                {
                    errores.Add($"Fila {filaActual}: El TipoVehiculoId {fila.TipoVehiculoId} no existe.");
                    continue;
                }
                if (!coloresExistentes.Contains(fila.ColorId))
                {
                    errores.Add($"Fila {filaActual}: El ColorId {fila.ColorId} no existe.");
                    continue;
                }

                // ── 3. Gestión Eficiente del Propietario ───────────────────────
                Propietario propietario;
                if (propietariosEnBd.TryGetValue(fila.DocumentoPropietario, out var propietarioId))
                {
                    // Si el propietario ya existe en BD, lo vinculamos directamente sin volver a insertarlo
                    propietario = new Propietario { Id = propietarioId };
                    context.Entry(propietario).State = EntityState.Unchanged; 
                }
                else
                {
                    // Si es nuevo, lo creamos y lo añadimos al diccionario temporal
                    propietario = new Propietario
                    {
                        Documento = fila.DocumentoPropietario,
                        Nombre = fila.NombrePropietario ?? "PROPIETARIO IMPORTADO",
                        Telefono = fila.TelefonoPropietario,
                        Direccion = fila.DireccionPropietario,
                        Correo = fila.CorreoPropietario,
                        TipoDocumento = TipoDocumento.Cc
                    };
                    context.Propietarios.Add(propietario);
                    
                    // Salvamos cambios parciales para obtener la llave autogenerada y evitar duplicaciones si el mismo propietario tiene más vehículos en el Excel
                    await context.SaveChangesAsync(); 
                    propietariosEnBd.Add(propietario.Documento, propietario.Id);
                }

                // ── 4. Creación del Vehículo ───────────────────────────────
                var vehiculo = new Vehiculo
                {
                    Placa = placaLimpia,
                    Modelo = fila.Modelo <= 0 ? DateTime.UtcNow.Year : fila.Modelo,
                    Cilindraje = fila.Cilindraje,
                    PagoHasta = fila.Modelo, // Inicia debiendo la vigencia actual en adelante
                    MarcaId = fila.MarcaId,
                    LineaId = fila.LineaId,
                    TipoVehiculoId = fila.TipoVehiculoId,
                    ColorId = fila.ColorId,
                    PropietarioId = propietario.Id,
                    TipoServicioVehiculo = TipoServicioVehiculo.Particular,
                    TipoCarroceriaId = 1
                };

                context.Vehiculos.Add(vehiculo);
                placasEnBd.Add(placaLimpia);
                importados++;
            }

            if (importados > 0)
            {
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            else
            {
                await transaction.RollbackAsync();
                return (false, "No se logró importar ningún registro debido a inconsistencias en los datos.", 0, errores);
            }

            return (true, $"Importación terminada con éxito. Registros procesados: {importados}.", importados, errores);
        }
        catch (Exception ex)
        {
            return (false, $"Error crítico de infraestructura en fila {filaActual}: {ex.Message}", 0, errores);
        }
    }
}