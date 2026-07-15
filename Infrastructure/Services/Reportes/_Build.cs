using Infrastructure.AppDbContext;
using Microsoft.Extensions.Logging;
namespace Infrastructure.Services.Reportes;

public partial class ReportesManager(MainDataContext context, ILogger<ReportesManager> logger)
{
    private readonly MainDataContext _context = context;
    private readonly ILogger<ReportesManager> _logger = logger;
}