using Domain.Generics;
using Domain.Models.Recibos.Requests;
using Domain.Models.Recibos.Responses;
using Domain.Responses.Users.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Liquidaciones;

public partial class LiquidacionService
{
    public async Task<Result<GenerarReciboResponse>> GenerarRecibo(
        int VehiculoId,
        string xcedula,
        TipoDocumento tipoDocumento,
        List<int> vigencias)
    {
        if (vigencias == null || vigencias.Count == 0)
        {
            return Result<GenerarReciboResponse>.Failure(
                Error.Validation("Recibo.VigenciasVacias", "Debe seleccionar al menos una vigencia."));
        }

        var result = await GenerarReciboAsync(new CrearReciboRequest(VehiculoId, vigencias));
        if (!result.Success)
        {
            return Result<GenerarReciboResponse>.Failure(
                Error.Failure("Recibo.ErrorGeneracion", result.Message));
        }

        var recibo = await context.Recibos
            .Include(r => r.Vehiculo)
            .FirstOrDefaultAsync(r => r.Id == result.ReciboId);

        var placa = recibo?.Vehiculo?.Placa?.Trim() ?? string.Empty;
        var fecha = recibo?.Fecha ?? DateTime.UtcNow;
        var nombreArchivo = !string.IsNullOrEmpty(placa)
            ? $"Recibo_{placa}_{fecha:yyyyMMdd}_{result.ReciboId}.pdf"
            : $"Recibo_{fecha:yyyyMMdd}_{result.ReciboId}.pdf";

        var response = new GenerarReciboResponse(
            result.ReciboId,
            placa,
            fecha,
            nombreArchivo);

        return Result<GenerarReciboResponse>.Success(response);
    }
}