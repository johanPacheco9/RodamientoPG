using Domain.Models.Vehiculos;
using Infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.Lineas;

public class LineasService(MainDataContext context)
{
    public async Task<int> Add(Linea lineaObj)
    {
        context.Lineas.Add(lineaObj);
        return await context.SaveChangesAsync();
    }

    public async Task<string?> GetNameByCode(int id)
    {
        return await context.Lineas
            .Where(l => l.Id == id)
            .Select(l => l.Nombre)
            .FirstOrDefaultAsync();
    }

    public async Task<int> Delete(int id)
    {
        return await context.Lineas
            .Where(l => l.Id == id)
            .ExecuteDeleteAsync();
    }

    public async Task<int> Edit(Linea lineaObj)
    {
        context.Lineas.Update(lineaObj);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    /// Reemplaza el INNER JOIN manual usando .Include() de EF Core
    /// </summary>
    public async Task<List<Linea>> GetAll()
    {
        return await context.Lineas
            .Include(l => l.Marca)
            .Take(10)
            .ToListAsync();
    }

    /// <summary>
    /// Trae las líneas filtradas por una marca específica
    /// </summary>
    public async Task<List<Linea>> GetListByMarca(int marcaId)
    {
        return await context.Lineas
            .Where(l => l.IdMarca == marcaId)
            .ToListAsync();
    }

    public async Task<Linea?> GetById(int id)
    {
        return await context.Lineas
            .Include(l => l.Marca)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    /// <summary>
    /// Buscador con LIKE seguro contra inyecciones SQL
    /// </summary>
    public async Task<List<Linea>> GetByPlacaOrNombre(string terminoBusqueda)
    {
        return await context.Lineas
            .Include(l => l.Marca)
            .Where(l => l.Nombre.ToUpper().StartsWith(terminoBusqueda.ToUpper()))
            .Take(20)
            .ToListAsync();
    }
}