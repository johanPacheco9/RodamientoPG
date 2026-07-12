using Domain.Generics;
using Domain.Models.ProcesoLiquidacion;
using Domain.Models.Resoluciones;
using Domain.Responses.Carteras;
using Domain.Responses.Proceso.Enums;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.Carteras;

public partial class CarteraService
{
    public async Task<PaginadoCarteraDto> GetDeudores(FiltroCarteraDto filtro, int pagina, int porPagina)
    {
        // 1. 🚀 FILTRO CLAVE: Excluir las carteras afectadas por Resoluciones de Anulación o Traslado
        var query = context.Cartera
            .Where(c => !c.IsPagado
                        && (c.ResolucionId == null
                            || (c.Resolucion!.TipoResolucion != TipoResolucion.AnulacionDeuda
                                && c.Resolucion.TipoResolucion != TipoResolucion.Traslado)))
            .Include(c => c.Vehiculo)
            .ThenInclude(v => v.Propietario)
            .AsQueryable();

        // ── Filtros ──────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            var b = filtro.Busqueda.Trim().ToUpper();
            query = query.Where(c =>
                c.Placa.Contains(b) ||
                c.Vehiculo.Propietario.Documento.Contains(b));
        }

        if (filtro.VigenciaDesde.HasValue)
            query = query.Where(c => c.Vigencia <= filtro.VigenciaDesde.Value);

        // El estado ya no vive en Vehiculo: se consulta contra Proceso vía VehiculoId
        if (!string.IsNullOrEmpty(filtro.Proceso))
        {
            query = filtro.Proceso.ToLower() switch
            {
                "ninguno" => query.Where(c =>
                    !context.Procesos.Any(p => p.VehiculoId == c.VehiculoId && p.EstadoProceso != EstadoProceso.SinProceso)),

                "persuasivo" => query.Where(c =>
                    context.Procesos.Any(p => p.VehiculoId == c.VehiculoId && p.EstadoProceso == EstadoProceso.Persuasivo)),

                "coactivo" => query.Where(c =>
                    context.Procesos.Any(p => p.VehiculoId == c.VehiculoId && p.EstadoProceso == EstadoProceso.Coactivo)),

                _ => query
            };
        }

        // ── Agrupar por vehículo en memoria ──────────────────────
        var raw = await query
            .Select(c => new
            {
                c.VehiculoId,
                c.Placa,
                c.Vigencia,
                c.ValorTotal,
                TipoDocumento = c.Vehiculo.Propietario.TipoDocumento.ToString(),
                Documento = c.Vehiculo.Propietario.Documento,
                NombrePropietario = c.Vehiculo.Propietario.Nombre,

                // Estado activo del vehículo, resuelto por subconsulta a Proceso
                EstadoProceso = context.Procesos
                    .Where(p => p.VehiculoId == c.VehiculoId && p.EstadoProceso != EstadoProceso.SinProceso)
                    .Select(p => (EstadoProceso?)p.EstadoProceso)
                    .FirstOrDefault() ?? EstadoProceso.SinProceso
            })
            .ToListAsync();

        var agrupado = raw
            .GroupBy(c => c.VehiculoId)
            .Select(g =>
            {
                var primero = g.First();

                return new
                {
                    primero.Placa,
                    primero.TipoDocumento,
                    primero.Documento,
                    primero.NombrePropietario,
                    VigenciasPendientes = g.Select(c => c.Vigencia).Distinct().Count(),
                    TotalDeuda = g.Sum(c => c.ValorTotal),
                    Proceso = primero.EstadoProceso == EstadoProceso.SinProceso
                        ? null
                        : primero.EstadoProceso.GetDisplayName()
                };
            })
            .ToList();

        // ── Filtro deuda mínima (post-agrupación) ─────────────────
        if (filtro.DeudaMinima.HasValue && filtro.DeudaMinima > 0)
            agrupado = agrupado.Where(x => x.TotalDeuda >= filtro.DeudaMinima.Value).ToList();

        // ── Métricas Globales (Reflejan la realidad sin deudas anuladas) ──
        var totalDeudores = agrupado.Count;
        var totalCartera = agrupado.Sum(x => x.TotalDeuda);
        var conProceso = agrupado.Count(x => !string.IsNullOrEmpty(x.Proceso));

        // ── Paginación ────────────────────────────────────────────
        var items = agrupado
            .OrderByDescending(x => x.TotalDeuda)
            .Skip((pagina - 1) * porPagina)
            .Take(porPagina)
            .Select(x => new DeudorDto
            {
                TipoDocumento = x.TipoDocumento,
                Documento = x.Documento,
                NombrePropietario = x.NombrePropietario,
                Placa = x.Placa,
                VigenciasPendientes = x.VigenciasPendientes,
                TotalDeuda = x.TotalDeuda,
                Proceso = x.Proceso
            })
            .ToList();

        return new PaginadoCarteraDto
        {
            Items = items,
            Total = totalDeudores,
            TotalCartera = totalCartera,
            ConProceso = conProceso
        };
    }
}