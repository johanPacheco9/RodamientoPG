using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.Vehiculos;
using Domain.Responses.Liquidacion.Enums;
using Domain.Responses.Vehiculos.Enums;

namespace Domain.Models;

public class Tarifa : EntityWithTraceability
{
    [Key]
    public int Id { get; set; }

    public int AnioFiscal { get; set; } // Tu columna 'periodo' (Ej: 2024, 2025)

    public int RangoInicial { get; set; } // Tu columna 'desde' (Ej: 0 cc, o $0 de avalúo)

    public int RangoFinal { get; set; } // Tu columna 'hasta' (Ej: 1500 cc, o $50'000.000 de avalúo)

    // ====================================================================
    // 💰 BLOQUE FINANCIERO ESTRICTO
    // ====================================================================
    [Column(TypeName = "decimal(18,2)")]
    public decimal Valor { get; set; } // Tarifa plana o porcentaje a aplicar según el caso

    // ====================================================================
    // 🔗 CLAVES FORÁNEAS (Cruzadas con tus tablas maestras de Vehículos)
    // ====================================================================

    public int TipoVehiculoId { get; set; } // Reemplaza a 'clase' o 'ClaseId'

    public  TipoServicioVehiculo TipoServicioVehiculo { get; set; }

    public TipoConceptoTarifa ConceptoTarifa { get; set; } = TipoConceptoTarifa.Rodamiento;

    // ====================================================================
    // 🚀 PROPIEDADES DE NAVEGACIÓN (Entity Framework Core)
    // ====================================================================

    public virtual TipoVehiculo TipoVehiculo { get; set; } = null!;

    public virtual TipoServicioVehiculo? TipoServicio { get; set; } = null!;
}
