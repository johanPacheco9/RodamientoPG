using Domain.Models.Vehiculos;
using Infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.TiposVehiculos;

public class TiposService(MainDataContext context)
{
    /// <summary>
    /// Registra una nueva clase de vehículo directamente en la tabla clases_veh
    /// </summary>
    public async Task<int> Add(TipoVehiculo tipoVehiculoObj)
    {
        // 💡 Corregido: Ahora sí guarda en la tabla correspondiente de EF Core
        context.TipoVehiculos.Add(tipoVehiculoObj);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    /// Elimina una clase de vehículo de forma ultra rápida por su ID primario
    /// </summary>
    public async Task<int> Delete(int id)
    {
        return await context.TipoVehiculos
            .Where(t => t.Id == id)
            .ExecuteDeleteAsync();
    }

    /// <summary>
    /// Actualiza el nombre o datos de una clase de vehículo existente
    /// </summary>
    public async Task<int> Update(TipoVehiculo tipoVehiculoObj)
    {
        context.TipoVehiculos.Update(tipoVehiculoObj);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    /// Obtiene el nombre plano de la clase de vehículo por su ID
    /// </summary>
    public async Task<string?> GetNameByCode(int code)
    {
        return await context.TipoVehiculos
            .Where(t => t.Id == code)
            .Select(t => t.Nombre)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Retorna el catálogo completo de clases de vehículos
    /// </summary>
    public async Task<List<TipoVehiculo>> GetAll()
    {
        try
        {
            return await context.TipoVehiculos.ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error al obtener el listado de clases de vehículos.", ex);
        }
    }

    /// <summary>
    /// Busca una clase de vehículo específica por su ID primario
    /// </summary>
    public async Task<TipoVehiculo?> GetById(int id)
    {
        return await context.TipoVehiculos.FindAsync(id);
    }

    /// <summary>
    /// Buscador predictivo (LIKE) por el nombre de la clase, 100% parametrizado y seguro
    /// </summary>
    public async Task<List<TipoVehiculo>> GetByNombre(string nombre)
    {
        try
        {
            return await context.TipoVehiculos
                .Where(t => t.Nombre.ToUpper().StartsWith(nombre.ToUpper()))
                .Take(20)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error en la búsqueda predictiva de clases por nombre: {nombre}", ex);
        }
    }
}