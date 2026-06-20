using Domain.Models.Vehiculos;
using Domain.Responses.Users.Enums;
using Infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.Propietarios;

public class PropietarioService(MainDataContext context)
{
    /// <summary>
    /// Guarda un nuevo propietario en la base de datos
    /// </summary>
    public async Task<int> Add(Propietario propietarioObj)
    {
        context.Propietarios.Add(propietarioObj);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    /// Modifica los datos de un propietario existente
    /// </summary>
    public async Task<int> Edit(Propietario propietarioObj)
    {
        context.Propietarios.Update(propietarioObj);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    /// Elimina un propietario de forma directa por su ID primario
    /// </summary>
    public async Task<int> Delete(int id)
    {
        return await context.Propietarios
            .Where(p => p.Id == id)
            .ExecuteDeleteAsync();
    }

    /// <summary>
    /// Devuelve los primeros 10 propietarios del sistema
    /// </summary>
    public async Task<List<Propietario>> GetList()
    {
        try
        {
            return await context.Propietarios
                .Take(10)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error al obtener el listado de propietarios.", ex);
        }
    }

    /// <summary>
    /// Busca el nombre completo de un propietario usando su documento y tipo de identificación
    /// </summary>
    public async Task<string?> GetNameByDocumento(string documento, TipoDocumento tipoDocumento)
    {
        return await context.Propietarios
            .Where(p => p.Documento == documento && p.TipoDocumento == tipoDocumento)
            .Select(p => p.Nombre)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Obtiene un propietario específico por su ID
    /// </summary>
    public async Task<Propietario?> GetById(int id)
    {
        try
        {
            return await context.Propietarios.FindAsync(id);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener el propietario con ID {id}", ex);
        }
    }

    /// <summary>
    /// Buscador predictivo (LIKE) por documento, 100% protegido contra inyección SQL
    /// </summary>
    public async Task<List<Propietario>> GetByCedula(string cedula)
    {
        try
        {
            return await context.Propietarios
                .Where(p => p.Documento.StartsWith(cedula))
                .Take(20)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error en la búsqueda predictiva de propietarios por documento.", ex);
        }
    }
}