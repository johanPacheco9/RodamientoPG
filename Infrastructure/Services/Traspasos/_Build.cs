using Infrastructure.AppDbContext;
using Microsoft.Extensions.Logging;
namespace Infrastructure.Services.Traspasos;

public partial class TraspasoManager(MainDataContext context, ILogger<TraspasoManager> logger)
{
    private readonly MainDataContext _context = context;
    private readonly ILogger<TraspasoManager> _logger = logger;
}