using DocumentFormat.OpenXml.Spreadsheet;
using Domain.Models.Vehiculos;
using Domain.Models; // Asegúrate de que esté el namespace correcto de Propietario
using Infrastructure.AppDbContext;
using Infrastructure.Services.Vehiculos.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Infrastructure.Services.Vehiculos;

// 🎯 Cambié el nombre del parámetro del constructor de 'context' a 'contextFactory' para evitar confusiones
public class VehiculosService(IDbContextFactory<MainDataContext> contextFactory, ILogger<VehiculosService> logger)
{
    public async Task<int> Add(Vehiculo vehiculo)
    {
        try
        {
            using var ctx = await contextFactory.CreateDbContextAsync();
            ctx.Vehiculos.Add(vehiculo);
            return await ctx.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error crítico al guardar el vehículo con placa {Placa}", vehiculo.Placa);
            return 0;
        }
    }

    public async Task<int> Edit(Vehiculo vehiculo)
    {
        try
        {
            using var ctx = await contextFactory.CreateDbContextAsync();
            ctx.Vehiculos.Update(vehiculo);
            return await ctx.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error crítico al editar el vehículo con ID {Id}", vehiculo.Id);
            throw;
        }
    }

    public async Task<Vehiculo?> GetById(int id)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();
        return await ctx.Vehiculos
            .Include(v => v.TipoVehiculo) 
            .Include(v => v.Marca)
            .Include(v => v.Linea)
            .Include(v => v.EstadoProceso)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Vehiculo?> GetByPlaca(string placa)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();
        return await ctx.Vehiculos
            .Include(v => v.TipoVehiculo)
            .Include(v => v.Marca)
            .Include(v => v.Linea)
            .Include(v => v.EstadoProceso)
            .FirstOrDefaultAsync(v => v.Placa == placa.ToUpper());
    }

    public async Task<List<Vehiculo>> GetList()
    {
        using var ctx = await contextFactory.CreateDbContextAsync();
        return await ctx.Vehiculos
            .Take(10)
            .ToListAsync();
    }

    public async Task<List<Vehiculo>> GetAll()
    {
        try
        {
            using var ctx = await contextFactory.CreateDbContextAsync();
            return await ctx.Vehiculos
                .Include(v => v.TipoVehiculo)
                .Include(v => v.Marca)
                .Include(v => v.Linea)
                .Include(v => v.EstadoProceso)
                .Take(30)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ocurrió un fallo al consultar el listado general de vehículos.");
            throw new Exception("Error al obtener el listado general de vehículos con sus catálogos.", ex);
        }
    }

    public async Task<List<Vehiculo>> GetByDocumentoPropietario(string documento)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();
        return await ctx.Vehiculos
            .Where(v => v.DocumentoPropietario == documento)
            .Include(v => v.TipoVehiculo)
            .Include(v => v.Marca)
            .ToListAsync();
    }
    
    public async Task<List<Vehiculo>> GetVehiculosByDocumentoPropietario(string documento)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();
        return await ctx.Vehiculos
            .Include(v => v.Marca)
            .Include(v => v.Linea)
            .Include(v => v.TipoVehiculo)
            .Include(v => v.EstadoProceso)
            .Where(v => v.DocumentoPropietario == documento)
            .ToListAsync();
    }
    

    public async Task<List<PlacasByCedulaResponse>> GetVehiculosAsociadosAComparendo(string cedula)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cedula))
                return new List<PlacasByCedulaResponse>();

            using var context = await contextFactory.CreateDbContextAsync();
        
            string terminoBusqueda = cedula.Trim();

            return await context.Vehiculos
                .Where(v => EF.Functions.Like(v.Propietario.Documento, $"%{terminoBusqueda}%"))
                .Take(20)
                .Select(v => new PlacasByCedulaResponse(
                    v.Id,
                    v.Placa,
                    v.TipoVehiculo.Nombre,
                    v.Marca.Nombre, 
                    v.Linea.Nombre,
                    v.Modelo
                ))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al ejecutar la búsqueda de placas para la cédula: {Cedula}", cedula);
            throw new Exception($"Error al ejecutar la consulta de placas para el ciudadano: {cedula}", ex);
        }
    }

    public async Task<int> GenerarPropietarioAsociado(string placa, Propietario propietario)
    {
        if (propietario == null) return 0;

        try
        {
            using var ctx = await contextFactory.CreateDbContextAsync();
            
            // Revisa si coincide con la propiedad real de Propietario (TipoDocumento o TipoIdentificacionId)
            bool existePropietario = await ctx.Propietarios
                .AnyAsync(p => p.Documento == propietario.Documento && p.TipoDocumento == propietario.TipoDocumento);
            
            if (!existePropietario)
            {
                ctx.Propietarios.Add(propietario);
                await ctx.SaveChangesAsync();
            }
            return 1; 
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error crítico en el servicio al generar propietario con documento {Documento}", propietario.Documento);
            return 0;
        }
    }
}