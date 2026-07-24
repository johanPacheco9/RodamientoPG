using Infrastructure.AppDbContext;
using Infrastructure.Services.Liquidaciones;
using Infrastructure.Services.Tarifas;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Carteras;

public partial class CarteraService(
    MainDataContext context,
    ILogger<CarteraService> logger,
    LiquidacionService liquidacionService,
    TarifaService tarifaService
)
{
}