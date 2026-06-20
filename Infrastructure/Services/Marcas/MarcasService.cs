using Domain.Models.Vehiculos;
using Infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.Marcas;

public class MarcaService(MainDataContext context)
{
    /// <summary>
    /// Guarda una nueva marca de vehículo
    /// </summary>
    public async Task<int> Add(Marca marcaObj)
    {
        context.Marcas.Add(marcaObj);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    /// Obtiene el nombre de una marca por su código ID
    /// </summary>
    public async Task<string?> GetNameByCode(int id)
    {
        return await context.Marcas
            .Where(m => m.Id == id)
            .Select(m => m.Nombre)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Elimina de forma ultra rápida una marca por ID usando ExecuteDelete
    /// </summary>
    public async Task<int> Delete(int id)
    {
        return await context.Marcas
            .Where(m => m.Id == id)
            .ExecuteDeleteAsync();
    }

    /// <summary>
    /// Actualiza los datos de una marca existente
    /// </summary>
    public async Task<int> Edit(Marca marcaObj)
    {
        context.Marcas.Update(marcaObj);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    /// Retorna el listado completo de marcas
    /// </summary>
    public async Task<List<Marca>> GetAll()
    {
        try
        {
            return await context.Marcas.ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error al obtener el listado de marcas", ex);
        }
    }

    /// <summary>
    /// Busca una marca específica por su ID usando la memoria intermedia de EF
    /// </summary>
    public async Task<Marca?> GetById(int id)
    {
        return await context.Marcas.FindAsync(id);
    }

    /// <summary>
    /// Buscador predictivo (LIKE) completamente protegido contra inyección SQL
    /// </summary>
    public async Task<List<Marca>> GetByNombre(string termino)
    {
        try
        {
            return await context.Marcas
                .Where(m => m.Nombre.ToUpper().StartsWith(termino.ToUpper()))
                .Take(20)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error en la búsqueda predictiva de marcas", ex);
        }
    }
}