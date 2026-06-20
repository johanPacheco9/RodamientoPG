using Infrastructure.AppDbContext;
using Microsoft.Extensions.Logging;
namespace Infrastructure.Services.Carteras;

public partial class CarteraService(MainDataContext context, ILogger<CarteraService> logger)
{
    private readonly MainDataContext _context = context;
    private readonly ILogger<CarteraService> _logger = logger;
}