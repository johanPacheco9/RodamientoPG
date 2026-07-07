using DocumentFormat.OpenXml.Spreadsheet;
using Domain.Models.Vehiculos;
using Domain.Models;
using Domain.Models.Vehiculos.Requests;
using Domain.Models.Vehiculos.Responses;
using Infrastructure.AppDbContext;
using Infrastructure.Services.Vehiculos.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace Infrastructure.Services.Vehiculos;

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
    public async Task<int> Edit(UpdateVehiculoRequest request)
    {
        try
        {
            using var ctx = await contextFactory.CreateDbContextAsync();
        
            // 1. Buscar el vehículo existente en la base de datos
            var vehiculoDb = await ctx.Vehiculos
                .FirstOrDefaultAsync(v => v.Id == request.Id);

            if (vehiculoDb == null)
            {
                logger.LogWarning("Se intentó editar un vehículo que no existe. ID: {Id}", request.Id);
                return 0; 
            }

            // 2. Actualizar las propiedades con los datos del Request
            vehiculoDb.Placa = request.Placa;
            vehiculoDb.Modelo = request.Modelo;
            vehiculoDb.Cilindraje = request.Cilindraje;
            vehiculoDb.CapacidadCarga = request.CapacidadCarga;
            vehiculoDb.Pasajeros = request.Pasajeros;
            vehiculoDb.TipoVehiculoId = request.TipoVehiculoId;
            vehiculoDb.MarcaId = request.MarcaId;
            vehiculoDb.LineaId = request.LineaId;
            vehiculoDb.ColorId = request.ColorId;
            vehiculoDb.TipoCarroceriaId = request.TipoCarroceriaId;
            vehiculoDb.PropietarioId = request.PropietarioId;
            vehiculoDb.FechaModificacion = DateTime.UtcNow;
            vehiculoDb.UsuarioModifico = 1;
            
            vehiculoDb.TipoServicioVehiculo = (Domain.Responses.Vehiculos.Enums.TipoServicioVehiculo)request.TipoServicioVehiculo;

            // 3. Guardar cambios (EF ya trackea las modificaciones automáticamente)
            return await ctx.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error crítico al editar el vehículo con ID {Id}", request.Id);
            throw;
        }
    }

    public async Task<VehiculoDetailDto?> GetById(int id)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();

        return await ctx.Vehiculos
            .AsNoTracking()
            .Where(v => v.Id == id)
            .Select(v => new VehiculoDetailDto(
                v.Id,
                v.Placa,
                v.Modelo,
                v.Cilindraje,
                v.CapacidadCarga,
                v.Pasajeros,
                v.TipoVehiculoId,
                v.MarcaId,
                v.LineaId,
                v.ColorId,
                v.TipoServicioVehiculo,
                v.TipoCarroceriaId,
                v.EstadoProcesoId,
                v.PropietarioId,
                v.Propietario.Documento,
                v.Propietario.TipoDocumento))
            .FirstOrDefaultAsync();
    }
    

    public async Task<List<VehiculoDetalleDto>> GetAll()
    {
        try
        {
            using var ctx = await contextFactory.CreateDbContextAsync();

            return await ctx.Vehiculos
                .AsNoTracking()
                .Take(30)
                .Select(v => new VehiculoDetalleDto(
                    v.Id,
                    v.Placa,
                    v.Modelo,
                    v.Cilindraje,
                    v.PagoHasta,
                    v.Propietario.Documento,
                    v.TipoVehiculo.Nombre,
                    v.Marca.Nombre,
                    v.Linea.Nombre,
                    v.EstadoProceso.ToString()
                ))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ocurrió un fallo al consultar el listado general de vehículos.");
            throw new Exception("Error al obtener el listado general de vehículos optimizado.", ex);
        }
    }

// 🚀 NUEVO MÉTODO AUXILIAR: Para traer la entidad completa SOLO cuando se va a editar
    public async Task<Vehiculo?> GetByIdCompleto(int id)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();
        return await ctx.Vehiculos
            .Include(v => v.Propietario)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<List<Vehiculo>> GetByDocumentoPropietario(string documento)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();

        return await ctx.Vehiculos
            .Where(v => v.Propietario.Documento == documento)
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
            .Where(v => v.Propietario.Documento == documento)
            .ToListAsync();
    }


    public async Task<List<PlacasByCedulaResponse>> GetInfoVehiculo(GetInfoVehiculoRequest request)
    {
        try
        {
            // 1. Validación inicial corregida
            if (string.IsNullOrWhiteSpace(request.Cedula) && string.IsNullOrWhiteSpace(request.Placa))
                throw new Exception("Placa o cédula son requeridos para realizar la búsqueda.");

            using var context = await contextFactory.CreateDbContextAsync();

            // 2. Creamos la consulta base (IQueryable) sin ejecutarla aún
            var query = context.Vehiculos.AsNoTracking();

            // 3. Filtro dinámico por Cédula/Documento del propietario si viene en el request
            if (!string.IsNullOrWhiteSpace(request.Cedula))
            {
                string cedulaBusqueda = request.Cedula.Trim();
                query = query.Where(v => EF.Functions.Like(v.Propietario.Documento, $"%{cedulaBusqueda}%"));
            }

            // 4. Filtro dinámico por Placa si viene en el request
            if (!string.IsNullOrWhiteSpace(request.Placa))
            {
                string placaBusqueda = request.Placa.Trim().ToUpper();
                query = query.Where(v => EF.Functions.Like(v.Placa, $"%{placaBusqueda}%"));
            }

            // 5. Proyectamos y limitamos a las primeras 20 ocurrencias
            return await query
                .Take(20)
                .Select(v => new PlacasByCedulaResponse(
                    v.Id,
                    v.Placa,
                    v.TipoVehiculo != null ? v.TipoVehiculo.Nombre : string.Empty,
                    v.Marca != null ? v.Marca.Nombre : string.Empty,
                    v.Linea != null ? v.Linea.Nombre : string.Empty,
                    v.Modelo
                ))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // Usamos de forma segura las propiedades del objeto 'request' en los logs
            logger.LogError(ex,
                "Error al ejecutar la búsqueda de vehículos. Filtros -> Cédula: {Cedula}, Placa: {Placa}",
                request.Cedula, request.Placa);

            throw new Exception("Ocurrió un error al ejecutar la consulta de vehículos para el ciudadano.", ex);
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