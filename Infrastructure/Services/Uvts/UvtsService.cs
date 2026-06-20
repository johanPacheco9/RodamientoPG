using Domain.Models;
using Infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.Uvts;

public class UvtService(MainDataContext context)
{
    /// <summary>
    /// Obtiene el valor de la UVT vigente para una fecha específica (Evaluando el rango)
    /// </summary>
    public async Task<Uvt?> GetVigenteByFecha(DateTime fecha)
    {
        try
        {
            // 💡 Corregido: En lugar de buscar una columna 'fecha' inexistente,
            // validamos que la fecha enviada esté dentro del rango histórico.
            return await context.Uvts
                .FirstOrDefaultAsync(u => fecha >= u.FechaDesde && fecha <= u.FechaHasta);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al buscar la UVT vigente para la fecha {fecha.ToShortDateString()}.", ex);
        }
    }

    /// <summary>
    /// Registra un nuevo valor de UVT anual
    /// </summary>
    public async Task<int> Add(Uvt uvtObj)
    {
        context.Uvts.Add(uvtObj);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    /// Actualiza los rangos o el valor de una UVT existente
    /// </summary>
    public async Task<int> Edit(Uvt uvtObj)
    {
        context.Uvts.Update(uvtObj);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    /// Elimina un registro de UVT por su ID primario de manera directa
    /// </summary>
    public async Task<int> Delete(int id)
    {
        return await context.Uvts
            .Where(u => u.Id == id)
            .ExecuteDeleteAsync();
    }

    /// <summary>
    /// Obtiene una UVT específica por su identificador único
    /// </summary>
    public async Task<Uvt?> GetById(int id)
    {
        return await context.Uvts.FindAsync(id);
    }

    /// <summary>
    /// Retorna las primeras 10 UVTs ordenadas cronológicamente
    /// </summary>
    public async Task<List<Uvt>> GetList()
    {
        return await context.Uvts
            .OrderByDescending(u => u.FechaDesde)
            .Take(10)
            .ToListAsync();
    }

    /// <summary>
    /// Retorna el histórico completo de UVTs ordenadas desde la más reciente
    /// </summary>
    public async Task<List<Uvt>> GetAll()
    {
        try
        {
            return await context.Uvts
                .OrderByDescending(u => u.FechaDesde)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error al obtener el histórico completo de UVTs.", ex);
        }
    }
}