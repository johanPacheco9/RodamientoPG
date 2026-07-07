using Infrastructure.AppDbContext;
using Infrastructure.Services.Carteras;
using Microsoft.Extensions.Logging;
namespace Infrastructure.Services.Liquidaciones;

public partial class LiquidacionService(
    MainDataContext context,
    ILogger<LiquidacionService> logger)
{
    
    private const string ConceptoRodamiento = "RODAMIENTO";
    private const string ConceptoEstampillas = "ESTAMPILLAS";
    private const string ConceptoCostas = "COSTAS";
    private const string ConceptoCarga = "CARGA";
    private const string ConceptoSistematizacion = "SISTEMATIZACION";
}