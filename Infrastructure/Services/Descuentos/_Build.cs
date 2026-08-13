using Infrastructure.AppDbContext;
using Microsoft.Extensions.Logging;
namespace Infrastructure.Services.Descuentos;

public partial class DescuentosManager(ILogger<DescuentosManager> logger, MainDataContext context)
{
    private readonly ILogger<DescuentosManager> _logger = logger;
    private readonly MainDataContext _context = context;
}