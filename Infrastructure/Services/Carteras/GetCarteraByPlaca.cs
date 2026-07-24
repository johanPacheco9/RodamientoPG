using Domain.Models.ProcesoLiquidacion;
using Domain.Models.Resoluciones;
using Domain.Responses.Liquidacion;
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

            var vehiculo = await context.Vehiculos
                .AsNoTracking()
                .Where(v => v.Placa == placa)
                .Select(v => new
                {
                    v.Id,
                    v.Placa,
                    v.Modelo,
                    v.Cilindraje,
                    v.PagoHasta,
                    v.TipoServicioVehiculo,
                    Clase = v.TipoVehiculo.Nombre,
                    Marca = v.Marca.Nombre,
                    Linea = v.Linea.Nombre,
                    Color = v.Color.Nombre,
                    ProcesoActivo = context.Procesos
                        .Where(p => p.VehiculoId == v.Id && p.EstadoProceso != EstadoProceso.SinProceso)
                        .Select(p => new { p.Id, p.EstadoProceso })
                        .FirstOrDefault(),
                    Documento = v.Propietario.Documento,
                    NombrePropietario = v.Propietario.Nombre,
                    Direccion = v.Propietario.Direccion,
                    Telefono = v.Propietario.Telefono,
                    TipoDocumento = v.Propietario.TipoDocumento
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (vehiculo == null)
                return null;
            
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

            foreach (var c in carteraBase)
            {
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
                    interesActualizado, 
                    c.Descuento,
                    totalActualizado
                ));
            }
            
            int vigenciaDesde = carteraPendiente.Any() ? carteraPendiente.Min(c => c.Vigencia) : nowYear;
            int vigenciaHasta = carteraPendiente.Any() ? carteraPendiente.Max(c => c.Vigencia) : nowYear;
            decimal totalDeuda = carteraPendiente.Sum(c => c.ValorTotal);
            var estadoProceso = vehiculo.ProcesoActivo?.EstadoProceso ?? EstadoProceso.SinProceso;
    
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
                Estado = estadoProceso,
                ProcesoId = vehiculo.ProcesoActivo?.Id,
                UltimoPago = vehiculo.PagoHasta,

                Documento = vehiculo.Documento,
                NombrePropietario = vehiculo.NombrePropietario,
                Direccion = vehiculo.Direccion,
                Telefono = vehiculo.Telefono,
                TipoDocumento = vehiculo.TipoDocumento,

                VigenciaDesde = vigenciaDesde,
                VigenciaHasta = vigenciaHasta,
                TotalDeuda = totalDeuda,

                Conceptos = carteraPendiente
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error consultando liquidación para placa {Placa}", placa);

            return null;
        }
    }
}