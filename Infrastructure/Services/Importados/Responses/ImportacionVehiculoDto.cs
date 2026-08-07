using System.ComponentModel.DataAnnotations;
using Domain.Models.ProcesoLiquidacion;
using MiniExcelLibs.Attributes;

namespace Infrastructure.Services.Importados.Responses;

public class ImportacionVehiculoDto
{
    [ExcelColumnName("Placa")]
    public string? Placa { get; set; }

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

    // --- Servicio y Capacidades ---
    [ExcelColumnName("TipoServicio")]
    public int? TipoServicio { get; set; } // 👈 ID numérico del Enum TipoServicioVehiculo

    [ExcelColumnName("CapacidadCarga")]
    public int? CapacidadCarga { get; set; }

    [ExcelColumnName("Pasajeros")]
    public int? Pasajeros { get; set; }

    // --- 🚀 Estados y Antecedentes de Negocio Tipados ---
    [ExcelColumnName("UltimaVigenciaPagada")]
    public int? UltimaVigenciaPagada { get; set; }

    [ExcelColumnName("EstadoProceso")]
    public EstadoProceso? EstadoProceso { get; set; }

    [ExcelColumnName("TieneAcuerdoPago")]
    public bool? TieneAcuerdoPago { get; set; }
}