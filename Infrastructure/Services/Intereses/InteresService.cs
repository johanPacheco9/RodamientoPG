using Domain.Models;
using Infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.Intereses;

public class InteresService(MainDataContext context)
{
    /// <summary>
    /// Busca la tasa de interés vigente para una fecha específica (Evaluando el rango)
    /// </summary>
    public async Task<List<Interes>> GetInteresesByFecha(DateTime fecha)
    {
        try
        {
            return await context.Intereses
                .Where(i => fecha >= i.Desde && fecha <= i.Hasta)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error al obtener intereses por fecha", ex);
        }
    }

    /// <summary>
    /// Guarda una nueva tasa de interés
    /// </summary>
    public async Task<int> Add(Interes interes)
    {
        context.Intereses.Add(interes);
        return await context.SaveChangesAsync(); 
    }

    /// <summary>
    /// Borra una tasa de interés por Id usando ExecuteDelete (Ultra rápido)
    /// </summary>
    public async Task<int> DeleteIntereses(int id)
    {
        return await context.Intereses
            .Where(i => i.Id == id)
            .ExecuteDeleteAsync();
    }

    /// <summary>
    /// Obtiene un interés específico por su Id
    /// </summary>
    public async Task<Interes?> GetInteresesById(int id)
    {
        return await context.Intereses.FindAsync(id);
    }

    /// <summary>
    /// Actualiza los rangos y porcentajes de interés
    /// </summary>
    public async Task<int> EditIntereses(Interes interes)
    {
        context.Intereses.Update(interes);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    /// Trae un top 10 de intereses (Equivalente al LIMIT 10 anterior)
    /// </summary>
    public async Task<List<Interes>> GetList()
    {
        try
        {
            return await context.Intereses
                .Take(10)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error al obtener el listado limitado de intereses", ex);
        }
    }

    /// <summary>
    /// Trae todos los intereses ordenados por rango inicial de forma descendente
    /// </summary>
    public async Task<List<Interes>> GetAll()
    {
        try
        {
            return await context.Intereses
                .OrderByDescending(i => i.Desde)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error al obtener todos los intereses", ex);
        }
    }
}