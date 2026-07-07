using Domain.Generics;
using Domain.Models.Resoluciones;
using Domain.Responses.Liquidacion;
using Domain.Responses.Users.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Infrastructure.Services.Carteras;

public partial class CarteraService
{
    public async Task<EstadoCuentaVehiculoDto?> GetCarteraByPlaca(
        string placa,
        CancellationToken cancellationToken = default)
    {
        try
        {
            placa = placa.Trim().ToUpper();
            var nowYear = DateTime.UtcNow.Year;

            // 1. Obtener los datos del Vehículo y Propietario (Se mantiene igual)
            var vehiculo = await context.Vehiculos
                .AsNoTracking()
                .Where(v => v.Placa == placa)
                .Select(v => new
                {
                    v.Id,
                    v.Placa,
                    v.Modelo,
                    v.Cilindraje,
                    v.EstadoProcesoId,
                    v.PagoHasta,
                    v.TipoServicioVehiculo,
                    Clase = v.TipoVehiculo != null ? v.TipoVehiculo.Nombre : string.Empty,
                    Marca = v.Marca != null ? v.Marca.Nombre : string.Empty,
                    Linea = v.Linea != null ? v.Linea.Nombre : string.Empty,
                    Color = v.Color != null ? v.Color.Nombre : string.Empty,
                    Estado = v.EstadoProceso.GetDisplayName(),
                    Documento = v.Propietario != null ? v.Propietario.Documento : string.Empty,
                    NombrePropietario = v.Propietario != null ? v.Propietario.Nombre : string.Empty,
                    Direccion = v.Propietario != null ? v.Propietario.Direccion : string.Empty,
                    Telefono = v.Propietario != null ? v.Propietario.Telefono : string.Empty,
                    TipoDocumento = v.Propietario != null ? v.Propietario.TipoDocumento : TipoDocumento.Cc
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (vehiculo == null)
                return null;
            
            // 2. 🚀 TRAEMOS LA CARTERA BASE DE LA BD (Sin el Select directo, para poder iterar)
            var carteraBase = await context.Cartera
                .AsNoTracking()
                .Where(c => c.Placa == placa && 
                            !c.IsPagado && 
                            (c.ResolucionId == null || 
                             (c.Resolucion!.TipoResolucion != TipoResolucion.AnulacionDeuda && 
                              c.Resolucion.TipoResolucion != TipoResolucion.Traslado)))
                .OrderBy(c => c.Vigencia)
                .ToListAsync(cancellationToken);

            var carteraPendiente = new List<ConceptoCarteraDto>();

            // 🚀 RECALCULAMOS EL INTERÉS EN CALIENTE PARA EL DTO
            foreach (var c in carteraBase)
            {
                // Si el concepto está parametrizado para tener interés, lo calculamos al día de HOY
                decimal interesActualizado = c.TieneInteres
                    ? await  liquidacionService.CalcularInteresMora(c.Valor, c.Vigencia)
                    : 0m;

                decimal totalActualizado = c.Valor + interesActualizado - c.Descuento;

                carteraPendiente.Add(new ConceptoCarteraDto(
                    c.Id,
                    c.Vigencia,
                    c.Concepto,
                    c.Tipo,
                    c.Valor,
                    interesActualizado, // 👈 Aquí inyectamos el interés real actualizado al día de hoy
                    c.Descuento,
                    totalActualizado // 👈 El total recalculado con la mora del día
                ));
            }

            // 3. Calculamos los datos globales basados en la lista ya actualizada
            int vigenciaDesde = carteraPendiente.Any() ? carteraPendiente.Min(c => c.Vigencia) : nowYear;
            int vigenciaHasta = carteraPendiente.Any() ? carteraPendiente.Max(c => c.Vigencia) : nowYear;
            decimal totalDeuda = carteraPendiente.Sum(c => c.ValorTotal);

            // 4. Armamos la respuesta final unificada para el frontend
            return new EstadoCuentaVehiculoDto
            {
                VehiculoId = vehiculo.Id,
                Placa = vehiculo.Placa,
                Clase = vehiculo.Clase,
                Modelo = vehiculo.Modelo,
                Marca = vehiculo.Marca,
                Linea = vehiculo.Linea,
                Color = vehiculo.Color,
                TipoServicio = vehiculo.TipoServicioVehiculo.ToString(),
                Cilindraje = vehiculo.Cilindraje,
                EstadoNombre = vehiculo.Estado,
                EstadoId = vehiculo.EstadoProcesoId,
                UltimoPago = vehiculo.PagoHasta,

                Documento = vehiculo.Documento,
                NombrePropietario = vehiculo.NombrePropietario,
                Direccion = vehiculo.Direccion,
                Telefono = vehiculo.Telefono,
                TipoDocumento = vehiculo.TipoDocumento,

                VigenciaDesde = vigenciaDesde,
                VigenciaHasta = vigenciaHasta,
                TotalDeuda = totalDeuda,

                Conceptos = carteraPendiente // 🔥 El frontend (y tus pestañas desglosadas) reciben la verdad del día
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consultando liquidación para placa {Placa}", placa);

            return null;
        }
    }
}