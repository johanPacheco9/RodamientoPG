using Domain.Generics;
using Domain.Models;
using Domain.Models.ProcesoLiquidacion;
using Domain.Responses.Vehiculos;
using Infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.Parametros;

public class ParametroService(MainDataContext context)
{
    // ==========================================
    // ⚙️ MÓDULO: PARÁMETROS GLOBALES
    // ==========================================

    /// <summary>
    /// Actualiza la configuración global de parámetros de la aplicación
    /// </summary>
    public async Task<int> EditParametros(Parametro parametro)
    {
        try
        {
            context.Parametros.Update(parametro);
            return await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error al editar los parámetros globales.", ex);
        }
    }

    /// <summary>
    /// Trae la lista de parámetros (Normalmente hay un solo registro de configuración)
    /// </summary>
    public async Task<List<Parametro>> GetAll()
    {
        try
        {
            return await context.Parametros.ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error al obtener la lista de parámetros.", ex);
        }
    }

    /// <summary>
    /// Obtiene la configuración activa del sistema (Forzado a ID 1 según tu regla legacy)
    /// </summary>
    public async Task<Parametro?> GetParametroActivo()
    {
        try
        {
            return await context.Parametros.FirstOrDefaultAsync(p => p.Id == 1);
        }
        catch (Exception ex)
        {
            throw new Exception("Error al obtener los parámetros de configuración por ID.", ex);
        }
    }

    // ==========================================
    // ⚖️ MÓDULO: ESTADOS PROCESALES (estados_proc)
    // ==========================================

    public async Task<int> EditEstados(Parametro estadoObj)
    {
        context.Parametros.Update(estadoObj);
        return await context.SaveChangesAsync();
    }

    public async Task<List<Parametro>> GetListEstados()
    {
        return await context.Parametros
            .Take(10)
            .ToListAsync();
    }

    // ==========================================
    // 💰 MÓDULO: DESCUENTOS
    // ==========================================

    public async Task<int> AddDescuento(Descuento descuentoObj)
    {
        context.Descuentos.Add(descuentoObj);
        return await context.SaveChangesAsync();
    }

    public async Task<int> EditDctos(Descuento descuentoObj)
    {
        context.Descuentos.Update(descuentoObj);
        return await context.SaveChangesAsync();
    }

    public async Task<List<Descuento>> DescuentosList()
    {
        return await context.Descuentos
            .Take(10)
            .ToListAsync();
    }

    // ==========================================
    // 📊 MÓDULO: ESTADÍSTICAS Y DASHBOARDS
    // ==========================================
    
    /// <summary>
    /// Obtiene el recuento total de vehículos registrados en el sistema, 
    /// agrupados por su tipo (Clase) y su tipo de servicio oficial.
    /// </summary>
    public async Task<List<EstadisticaVehiculoDto>> ObtenerCantidadVehiculosPorClaseYServicio()
    {
        try
        {
            // 1. Agrupamos en la base de datos (Postgres ejecuta un GROUP BY rápido)
            var datosAgrupados = await context.Vehiculos
                .GroupBy(v => new { v.TipoVehiculo.Nombre, v.TipoServicioVehiculo})
                .Select(g => new
                {
                    Clase = g.Key.Nombre,
                    Servicio = g.Key.TipoServicioVehiculo,
                    Total = g.Count()
                })
                .ToListAsync();

            return datosAgrupados
                .Select(d => new EstadisticaVehiculoDto
                (
                    d.Clase.ToUpper(),
                    d.Servicio, 
                    d.Total,
                    0
                ))
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception("Error al generar la estadística consolidada de vehículos.", ex);
        }
    }

    public async Task<Parametro> GetParametroById(int id)
    {
        try
        {
            var parametro = await context.Parametros.FirstOrDefaultAsync(p => p.Id == id);

            if (parametro is null)
            {
                parametro = CrearParametroPorDefecto(id);
                context.Parametros.Add(parametro);
                await context.SaveChangesAsync();
            }

            return parametro;
        }
        catch (Exception e)
        {
            throw new Exception("Error al obtener el parametro con el id", e);
        }   
    }

    private static Parametro CrearParametroPorDefecto(int id)
    {
        return new Parametro
        {
            Id = id,
            Nombre = "Municipio",
            Ciudad = "Albania",
            CobraAdicional = false,
            MetodoImpuesto = 1,
            FechaLimiteSancion = DateTime.UtcNow.Date,
            ValorRecibo = 0,
            ValorSistema = 0,
            PorcentajeSancion = 0
        };
    }
}
