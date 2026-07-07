using Domain.Models;
using Infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.Tarifas;

public partial class TarifaService
{
    /// <summary>
    /// Guarda una nueva configuración de tarifa
    /// </summary>
    public async Task<int> Add(Tarifa tarifaObj)
    {
        context.Tarifas.Add(tarifaObj);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    /// Elimina una tarifa de forma directa usando ExecuteDelete
    /// </summary>
    public async Task<int> Delete(int id)
    {
        return await context.Tarifas
            .Where(t => t.Id == id)
            .ExecuteDeleteAsync();
    }

    /// <summary>
    /// Actualiza los rangos o valores de una tarifa existente
    /// </summary>
    public async Task<int> Edit(Tarifa tarifaObj)
    {
        context.Tarifas.Update(tarifaObj);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    /// Retorna las tarifas ordenadas por clase, servicio y periodo (descendiente)
    /// </summary>
    public async Task<List<Tarifa>> GetAll()
    {
        try
        {
            return await context.Tarifas
                .OrderBy(t => t.TipoServicioVehiculo)
                .ThenBy(t => t.RangoFinal)
                .ThenByDescending(t => t.AnioFiscal)
                .Take(20)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error al obtener el listado de tarifas globales.", ex);
        }
    }

    /// <summary>
    /// Busca una tarifa específica por su ID primario
    /// </summary>
    public async Task<Tarifa?> GetById(int id)
    {
        return await context.Tarifas.FindAsync(id);
    }

    /// <summary>
    /// Filtra tarifas por un periodo (Año) específico. Corregido y protegido contra inyección SQL.
    /// </summary>
    public async Task<List<Tarifa>> GetByPeriodo(int periodo)
    {
        try
        {
            return await context.Tarifas
                .Where(t => t.AnioFiscal == periodo)
                .OrderBy(t => t.TipoServicioVehiculo)
                .ThenBy(t => t.RangoInicial)
                .Take(20)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al buscar tarifas para el periodo {periodo}.", ex);
        }
    }

    /// <summary>
    /// 🔥 LLAMA FUNCIÓN: public.carga_impuesto (Procesa masivamente el impuesto por rangos de periodos)
    /// </summary>
    public async Task<int> CargaImpuestos(int desde, int hasta)
    {
        try
        {
            // Ejecución segura interpolada con el motor nativo de EF Core
            return await context.Database
                .ExecuteSqlAsync($"SELECT public.carga_impuesto({desde}, {hasta})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error crítico en carga_impuesto: {ex.Message}");
            return 0; // Mantiene la compatibilidad de lógica original retornando 0 si falla
        }
    }
}