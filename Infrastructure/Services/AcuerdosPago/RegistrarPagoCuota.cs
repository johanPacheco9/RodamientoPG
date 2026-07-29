using Domain.Models.Acuerdos.Enums;
using Domain.Models.Acuerdos.Responses;
using Domain.Models.Recibos;
using Domain.Responses.Recibo.Enums;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.AcuerdosPago;

public partial class AcuerdoPagoService
{
    public async Task<(bool success, int reciboId, string message)> RegistrarPagoCuota(CuotaAcuerdoDto dto)
    {
        var cuota = await _context.CuotasAcuerdoDePagos
            .Include(c => c.AcuerdoPago)
            .ThenInclude(a => a.Cuotas)
            .FirstOrDefaultAsync(c => c.Id == dto.Id);

        if (cuota is null)
            return (false, 0, "La cuota no existe.");

        if (cuota.Estado == EstadoCuotaAcuerdo.Pagada)
            return (false, 0, "La cuota ya fue pagada.");

        var fechaActualUtc = DateTime.UtcNow;
        var acuerdo = cuota.AcuerdoPago;

        // 1. Crear el recibo utilizando LOS VALORES REALES que vienen del Frontend (DTO)
        var recibo = new Recibo
        {
            VehiculoId = acuerdo.VehiculoId,
            CuotaAcuerdoPagoId = cuota.Id,
            Estado = EstadoRecibo.Pagado,
            Fecha = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc),
            FechaPago = fechaActualUtc,

            // Mapeo explícito desde el DTO enviado por la UI
            ValorCapital = dto.ValorCuota,
            InteresMora = dto.InteresFinanciacion,
            ValorTotalSistema = dto.ValorTotal,

            Descuento = 0,
            Estampillas = 0,
            ValorCargaDatos = 0,
            ValorRodamiento = 0
        };

        _context.Recibos.Add(recibo);

        // 2. Guardar el recibo primero para obtener el Id autogenerado por la BD
        await _context.SaveChangesAsync();

        // 3. Actualizar la cuota con los valores pagados y el número de recibo oficial
        cuota.Estado = EstadoCuotaAcuerdo.Pagada;
        cuota.FechaPago = fechaActualUtc;
        cuota.NumeroRecibo = recibo.Id.ToString();
        cuota.ValorCapital = dto.ValorCuota;
        cuota.ValorInteres = dto.InteresFinanciacion;
        cuota.ValorTotalCuota = dto.ValorTotal;

        // 4. Verificar si era la última cuota para liquidar el acuerdo
        if (acuerdo.Cuotas.All(c => c.Id == dto.Id || c.Estado == EstadoCuotaAcuerdo.Pagada))
        {
            acuerdo.Estado = EstadoAcuerdoPago.Pagado;

            await _context.Cartera
                .Where(c => c.AcuerdoPagoId == acuerdo.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsPagado, true));
        }

        // 5. Impactar la actualización de la cuota y del acuerdo
        var ok = await _context.SaveChangesAsync() > 0 || recibo.Id > 0;
        return (ok, recibo.Id, ok ? "Pago registrado exitosamente." : "Error al registrar el pago.");
    }
}