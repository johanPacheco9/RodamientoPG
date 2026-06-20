using Domain.Models.Vehiculos;
using Domain.Responses.Liquidacion.Enums;
using Domain.Responses.Vehiculos.Enums;
namespace Infrastructure.Services.Liquidaciones;

public partial class LiquidacionService
{
    private async Task<decimal> ObtenerValorCargaOPasajero(Vehiculo vehiculo, int vigencia)
    {
        if (vehiculo.TipoServicioVehiculo != TipoServicioVehiculo.Publico)
            return 0;

        if (EsClasePorCarga(vehiculo.TipoVehiculoId))
        {
            var toneladas = vehiculo.CapacidadCarga / 1000;

            return await ObtenerTarifa(TipoConceptoTarifa.Carga, vigencia, toneladas);
        }

        if (EsClasePorPasajeros(vehiculo.TipoVehiculoId))
        {
            return await ObtenerTarifa(TipoConceptoTarifa.Pasajeros, vigencia, vehiculo.Pasajeros);
        }

        return 0;
    }
}