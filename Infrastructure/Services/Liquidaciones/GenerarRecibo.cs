using Domain.Models.Recibos.Requests;
using Domain.Responses.Users.Enums;
namespace Infrastructure.Services.Liquidaciones;

public partial class LiquidacionService
{
    public async Task<(bool success, string message, int reciboId)> GenerarRecibo(
        int VehiculoId,
        string xcedula,
        TipoDocumento tipoDocumento,
        List<int> vigencias)
    {
        if (vigencias.Count == 0)
            return (false, "Debe seleccionar al menos una vigencia.", 0);

        var result = await GenerarReciboAsync(new CrearReciboRequest(VehiculoId, vigencias));
        return (result.Success, result.Message, result.ReciboId);
    }
}