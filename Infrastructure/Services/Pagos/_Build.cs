using ClosedXML.Excel;
using Infrastructure.AppDbContext;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Pagos;

public partial class PagoService(MainDataContext context, ILogger<PagoService> logger)
{
    
    private readonly ILogger<PagoService> _logger = logger;
    
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
