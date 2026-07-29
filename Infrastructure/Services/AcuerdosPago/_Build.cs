using Infrastructure.AppDbContext;
using Infrastructure.Services.Liquidaciones;
using Microsoft.Extensions.Logging;
namespace Infrastructure.Services.AcuerdosPago;

public partial class AcuerdoPagoService(MainDataContext context, ILogger<AcuerdoPagoService> logger,LiquidacionService liquidacionService)
{
    private readonly MainDataContext _context = context;
    private readonly ILogger<AcuerdoPagoService> _logger = logger;
}