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
        var queryCarteras = context.Cartera
            .Where(c => !c.IsPagado
                        && !c.IsAnulled
                        && (c.ResolucionId == null
                            || (c.Resolucion!.TipoResolucion != TipoResolucion.AnulacionDeuda
                                && c.Resolucion.TipoResolucion != TipoResolucion.Traslado)));

        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            var b = filtro.Busqueda.Trim().ToUpper();
            queryCarteras = queryCarteras.Where(c =>
                c.Placa.Contains(b) ||
                c.Vehiculo.Propietario.Documento.Contains(b));
        }

        if (filtro.VigenciaDesde.HasValue)
        {
            queryCarteras = queryCarteras.Where(c => c.Vigencia <= filtro.VigenciaDesde.Value);
        }

        var queryAgrupada = queryCarteras
            .GroupBy(c => new
            {
                c.VehiculoId,
                c.Placa,
                c.Vehiculo.Propietario.Nombre,
                c.Vehiculo.Propietario.Documento,
                TipoDocumento = c.Vehiculo.Propietario.TipoDocumento
            })
            .Select(g => new
            {
                g.Key.VehiculoId,
                g.Key.Placa,
                NombrePropietario = g.Key.Nombre,
                g.Key.Documento,
                TipoDocumento = g.Key.TipoDocumento.ToString(),
                VigenciasPendientes = g.Select(c => c.Vigencia).Distinct().Count(),
                // Ahora (Suma Capital + Intereses)
                TotalDeuda = g.Sum(c => c.ValorTotal + c.ValorInteres),
                EstadoProceso = context.Procesos
                    .Where(p => p.VehiculoId == g.Key.VehiculoId && p.EstadoProceso != EstadoProceso.SinProceso)
                    .Select(p => (EstadoProceso?)p.EstadoProceso)
                    .FirstOrDefault() ?? EstadoProceso.SinProceso
            });

        if (!string.IsNullOrEmpty(filtro.Proceso))
        {
            queryAgrupada = filtro.Proceso.ToLower() switch
            {
                "ninguno" => queryAgrupada.Where(x => x.EstadoProceso == EstadoProceso.SinProceso),
                "persuasivo" => queryAgrupada.Where(x => x.EstadoProceso == EstadoProceso.Persuasivo),
                "coactivo" => queryAgrupada.Where(x => x.EstadoProceso == EstadoProceso.Coactivo),
                _ => queryAgrupada
            };
        }

        if (filtro.DeudaMinima.HasValue && filtro.DeudaMinima > 0)
        {
            queryAgrupada = queryAgrupada.Where(x => x.TotalDeuda >= filtro.DeudaMinima.Value);
        }

        var stats = await queryAgrupada
            .GroupBy(x => 1)
            .Select(g => new
            {
                Total = g.Count(),
                TotalCartera = g.Sum(x => x.TotalDeuda),
                ConProceso = g.Count(x => x.EstadoProceso != EstadoProceso.SinProceso)
            })
            .FirstOrDefaultAsync();

        var itemsDb = await queryAgrupada
            .OrderByDescending(x => x.TotalDeuda)
            .Skip((pagina - 1) * porPagina)
            .Take(porPagina)
            .ToListAsync();

        var items = itemsDb.Select(x => new DeudorDto
        {
            TipoDocumento = x.TipoDocumento,
            Documento = x.Documento,
            NombrePropietario = x.NombrePropietario,
            Placa = x.Placa,
            VigenciasPendientes = x.VigenciasPendientes,
            TotalDeuda = x.TotalDeuda,
            Proceso = x.EstadoProceso == EstadoProceso.SinProceso ? null : x.EstadoProceso.GetDisplayName()
        }).ToList();

        return new PaginadoCarteraDto
        {
            Items = items,
            Total = stats?.Total ?? 0,
            TotalCartera = stats?.TotalCartera ?? 0,
            ConProceso = stats?.ConProceso ?? 0
        };
    }
}