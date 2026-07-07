using Infrastructure.AppDbContext;
using Infrastructure.Services.Liquidaciones;
using Infrastructure.Services.Tarifas;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Carteras;

public partial class CarteraService(
    MainDataContext context, 
    ILogger<CarteraService> logger,
    LiquidacionService liquidacionService, 
    TarifaService tarifaService) // <-- Recibes todos por constructor primario
{
    // C# 12 expone 'context', 'logger', 'liquidacionService' y 'tarifaService' 
    // automáticamente a toda la clase. ¡Ya no necesitas declarar variables privadas abajo!

    private const string ConceptoRodamiento = "RODAMIENTO";
    private const string ConceptoEstampillas = "ESTAMPILLAS";
    private const string ConceptoCostas = "COSTAS";
    private const string ConceptoCarga = "CARGA";
    private const string ConceptoSistematizacion = "SISTEMATIZACION";

    private static bool EsConceptoRodamiento(string concepto) => EsConcepto(concepto, ConceptoRodamiento, "1", "TRANSITO");
    private static bool EsConceptoEstampillas(string concepto) => EsConcepto(concepto, ConceptoEstampillas, "2");
    private static bool EsConceptoCostas(string concepto) => EsConcepto(concepto, ConceptoCostas, "3", "RECIBO");
    private static bool EsConceptoCarga(string concepto) => EsConcepto(concepto, ConceptoCarga, "4");
    private static bool EsConceptoSistema(string concepto) => EsConcepto(concepto, ConceptoSistematizacion);
    private static bool EsClasePorCarga(int tipoVehiculoId) => tipoVehiculoId is 4 or 8 or 9;
    private static bool EsClasePorPasajeros(int tipoVehiculoId) => tipoVehiculoId is 1 or 2 or 3 or 5 or 6 or 7;
    
    private static bool EsConcepto(string concepto, params string[] opciones)
    {
        return opciones.Any(opcion => string.Equals(concepto?.Trim(), opcion, StringComparison.OrdinalIgnoreCase));
    }
}