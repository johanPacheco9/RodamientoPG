using Domain.Models.Carteras.Enums;
using Domain.Models.ProcesoLiquidacion;
using Domain.Responses.Users.Enums;
namespace Domain.Responses.Liquidacion;

public class EstadoCuentaVehiculoDto
{
    // ... Todos tus campos de Vehículo y Propietario se quedan igual ...
    public int VehiculoId { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Clase { get; set; } = string.Empty;
    public int Modelo { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Linea { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string TipoServicio { get; set; } = string.Empty;
    public int Cilindraje { get; set; }
    public string Carroceria { get; set; } = string.Empty;
    public string Carga { get; set; } = string.Empty;
    public string EstadoNombre { get; set; } = string.Empty;
    public EstadoProceso EstadoId { get; set; }

    // Propietario
    public TipoDocumento TipoDocumento { get; set; }
    public string Documento { get; set; } = string.Empty;
    public string NombrePropietario { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }

    // Liquidación Global
    public int VigenciaDesde { get; set; }
    public int VigenciaHasta { get; set; }
    public decimal TotalDeuda { get; set; }
    public decimal Avaluo { get; set; }
    public decimal? UltimoPago { get; set; }

    public List<ConceptoCarteraDto> Conceptos { get; set; } = [];
}

public record ConceptoCarteraDto(
    int Id,
    int Vigencia,
    TipoConceptoCartera Concepto,
    string Tipo,
    decimal Valor,
    decimal ValorInteres,
    decimal Descuento,
    decimal ValorTotal
);