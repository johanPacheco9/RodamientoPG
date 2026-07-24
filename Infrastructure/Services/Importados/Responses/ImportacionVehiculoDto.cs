using MiniExcelLibs.Attributes;

namespace Infrastructure.Services.Importados.Responses;

public class ImportacionVehiculoDto
{
    [ExcelColumnName("Placa")]
    public string? Placa { get; set; }

    // 🚀 Al ser int?, si la celda está vacía o tiene formato raro, MiniExcel asigna null sin romperse
    [ExcelColumnName("Modelo")]
    public int? Modelo { get; set; }

    [ExcelColumnName("Cilindraje")]
    public int? Cilindraje { get; set; }

    // --- Propietario ---
    [ExcelColumnName("TipoDocumento")]
    public string? TipoDocumento { get; set; }

    [ExcelColumnName("DocumentoPropietario")]
    public string? DocumentoPropietario { get; set; }

    [ExcelColumnName("NombrePropietario")]
    public string? NombrePropietario { get; set; }

    [ExcelColumnName("TelefonoPropietario")]
    public string? TelefonoPropietario { get; set; }

    [ExcelColumnName("DireccionPropietario")]
    public string? DireccionPropietario { get; set; }

    [ExcelColumnName("CorreoPropietario")]
    public string? CorreoPropietario { get; set; }

    // --- Catálogos ---
    [ExcelColumnName("Marca")]
    public string? Marca { get; set; }

    [ExcelColumnName("Linea")]
    public string? Linea { get; set; }

    [ExcelColumnName("TipoVehiculo")]
    public string? TipoVehiculo { get; set; }

    [ExcelColumnName("Color")]
    public string? Color { get; set; }
}