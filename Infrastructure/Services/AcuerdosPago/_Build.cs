using Infrastructure.AppDbContext;
using Microsoft.Extensions.Logging;
namespace Infrastructure.Services.AcuerdosPago;

public partial class AcuerdoPagoService(MainDataContext context, ILogger<AcuerdoPagoService> logger)
{
    private readonly MainDataContext _context = context;
    private readonly ILogger<AcuerdoPagoService> _logger = logger;
}