using Domain.Models.Vehiculos;
using Infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.Colores;

public class ColoresService(MainDataContext context)
{
    public async Task<int> Add(Color colorObj)
    {
        context.Colores.Add(colorObj);
        return await context.SaveChangesAsync();
    }

    public async Task<string?> GetNameColoresById(int id)
    {
        // SELECT nombre FROM colores WHERE id = @id
        return await context.Colores
            .Where(c => c.Id == id)
            .Select(c => c.Nombre)
            .FirstOrDefaultAsync();
    }

    public async Task<int> Delete(int id)
    {
        // Buscamos y borramos en un solo paso eficiente en EF Core
        var filasAfectadas = await context.Colores
            .Where(c => c.Id == id)
            .ExecuteDeleteAsync();
            
        return filasAfectadas;
    }

    public async Task<int> Edit(Color colorObj)
    {
        context.Colores.Update(colorObj);
        return await context.SaveChangesAsync();
    }

    public async Task<List<Color>> GetAll()
    {
        return await context.Colores.ToListAsync();
    }

    public async Task<Color?> GetById(int id)
    {
        return await context.Colores.FindAsync(id);
    }

    public async Task<List<Color>> GetByNombre(string nombre)
    {
        // Traduce automáticamente a: WHERE nombre LIKE 'TEXTO%' LIMIT 20
        return await context.Colores
            .Where(c => c.Nombre.ToUpper().StartsWith(nombre.ToUpper()))
            .Take(20)
            .ToListAsync();
    }
}