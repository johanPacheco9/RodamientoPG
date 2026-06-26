using ClosedXML.Excel;
using Domain.Models;
using Domain.Models.Recibos;
using Domain.Responses.Recibo.Enums;
using Infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Pagos;

public partial class PagoService(MainDataContext context, ILogger<PagoService> logger)
{
    
    private readonly ILogger<PagoService> _logger = logger;
    
    //ahora hacerlo directo al recibo, se tiene el id.
    public async Task<int> ReversarPagoReciboAsync(int reciboId)
    {
        // 1. Buscamos el recibo con sus detalles para conocer los IDs de cartera involucrados
        var recibo = await context.Recibos
            .Include(r => r.Detalles)
            .FirstOrDefaultAsync(r => r.Id == reciboId);

        if (recibo == null) return 0;

        // Si el recibo ya estaba pendiente, evitamos reprocesar innecesariamente
        if (recibo.Estado == EstadoRecibo.Pendiente) return 0;

        // 2. Extraemos los IDs de las carteras afectadas por este recibo específico
        var carteraIds = recibo.Detalles.Select(d => d.CarteraId).ToList();

        if (carteraIds.Any())
        {
            // 3. Actualizamos de golpe las carteras específicas en la base de datos
            await context.Cartera
                .Where(c => carteraIds.Contains(c.Id))
                .ExecuteUpdateAsync(s => s
                        .SetProperty(c => c.IsPagado, false)
                    // Opcional: Si quieres desvincular el recibo de la cartera al reversar:
                    // .SetProperty(c => c.ReciboId, (int?)null) 
                );
        }

        // 4. Devolvemos el estado del recibo a Pendiente y limpiamos fechas de pago
        recibo.Estado = EstadoRecibo.Pendiente;
        recibo.FechaPago = null;
        recibo.FechaAplica = null;
        recibo.FechaProceso = null; // Si aplica limpiar también el procesamiento

        // 5. Guardamos los cambios del encabezado del recibo
        return await context.SaveChangesAsync();
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
