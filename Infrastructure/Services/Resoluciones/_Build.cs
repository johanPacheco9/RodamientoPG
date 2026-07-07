using Infrastructure.AppDbContext;
using Microsoft.Extensions.Logging;
namespace Infrastructure.Services.Resoluciones;

public partial class ResolucionService(MainDataContext context, ILogger<ResolucionService> logger)
{
    private readonly MainDataContext _context = context;
    private readonly ILogger<ResolucionService> _logger = logger;
}