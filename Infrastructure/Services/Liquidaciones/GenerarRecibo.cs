using Domain.Responses.Users.Enums;
namespace Infrastructure.Services.Liquidaciones;

public partial class LiquidacionService
{
    public async Task<(bool success, string message, int reciboId)> GenerarRecibo(string xplaca, string xcedula, TipoDocumento tipoDocumento, List<int> pdesde)
    {
        if (pdesde.Count == 0)
            return (false, "Debe seleccionar al menos una vigencia.", 0);

        var result = await GenerarReciboAsync(xplaca, xcedula, tipoDocumento, pdesde.Min(), pdesde.Max());

        return (result.Success, result.Message, result.ReciboId);
    }
}