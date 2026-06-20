using Domain.Models.BaseGravable;
using Domain.Responses.Recibo.Enums;
using Infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.Importados;

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
}