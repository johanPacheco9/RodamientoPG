using ClosedXML.Excel;
using Domain.Models;
using Domain.Responses.Recibo.Enums;
using Infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Pagos;

public class PagosService(MainDataContext context)
{
    public async Task<Recibo> GetRecibo(int nrecibo)
    {
        var recibo = await context.Recibos
            .Include(r => r.Vehiculo)
            .FirstOrDefaultAsync(s => s.Id == nrecibo);

        if (recibo == null)
            throw new KeyNotFoundException($"El recibo con ID {nrecibo} no existe en el sistema.");

        return recibo;
    }

    public async Task<int> AplicarPagoRecibo(int reciboId)
    {
        var recibo = await context.Recibos
            .Include(r => r.Vehiculo)
            .FirstOrDefaultAsync(r => r.Id == reciboId);

        if (recibo == null) return 0;

        await context.Cartera
            .Where(c =>
                c.Placa == recibo.Vehiculo.Placa &&
                !c.IsPagado &&
                c.Vigencia >= recibo.Desde &&
                c.Vigencia <= recibo.Hasta)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.IsPagado, true)
                .SetProperty(c => c.ReciboId, recibo.Id));

        recibo.Estado = EstadoRecibo.Cancelado;
        recibo.FechaPago = DateTime.UtcNow;
        recibo.FechaAplica = DateTime.UtcNow;
        recibo.Vehiculo.PagoHasta = recibo.Hasta;

        return await context.SaveChangesAsync();
    }

    public async Task<int> ReversarPagoReciboAsync(int reciboId)
    {
        var recibo = await context.Recibos
            .Include(r => r.Vehiculo)
            .FirstOrDefaultAsync(r => r.Id == reciboId);

        if (recibo == null) return 0;

        await context.Cartera
            .Where(c =>
                c.Placa == recibo.Vehiculo.Placa &&
                c.IsPagado &&
                c.Vigencia >= recibo.Desde &&
                c.Vigencia <= recibo.Hasta)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.IsPagado, false)
                .SetProperty(c => c.ReciboId, (int?)null));

        recibo.Estado = EstadoRecibo.Pendiente;
        recibo.FechaPago = null;
        recibo.FechaAplica = null;

        return await context.SaveChangesAsync();
    }

    public async Task<int> Procesa_Rec(int recibo, string pplaca)
    {
        try
        {
            return await AplicarPagoRecibo(recibo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en AplicarPagoReciboAsync: {ex.Message}");
            return 0;
        }
    }

    public async Task<int> Reversa_Rec(int recibo)
    {
        try
        {
            return await ReversarPagoReciboAsync(recibo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en ReversarPagoReciboAsync: {ex.Message}");
            return 0;
        }
    }

    public async Task WriteFile<T>(List<T> objects, string filePath)
    {
        if (objects == null || objects.Count == 0) return;

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Data");

        var props = typeof(T).GetProperties();

        for (var i = 0; i < props.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = props[i].Name;
        }

        for (var row = 0; row < objects.Count; row++)
        {
            var obj = objects[row];
            for (var col = 0; col < props.Length; col++)
            {
                var value = props[col].GetValue(obj);
                worksheet.Cell(row + 2, col + 1).Value = value?.ToString() ?? string.Empty;
            }
        }

        workbook.SaveAs(filePath);
        await Task.CompletedTask;
    }
}
