using Infrastructure.AppDbContext;
using Microsoft.Extensions.Logging;
namespace Infrastructure.Services.Procesos.Persuasivo;

public partial class PersuasivoService(ILogger<PersuasivoService> logger, MainDataContext context)
{
    private readonly ILogger<PersuasivoService> _logger = logger;
    private readonly MainDataContext _context = context;
}
