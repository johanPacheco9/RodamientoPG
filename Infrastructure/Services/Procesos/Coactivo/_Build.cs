using Infrastructure.AppDbContext;
using Microsoft.Extensions.Logging;
namespace Infrastructure.Services.Procesos.Coactivo;

public partial class CoactivoService(MainDataContext context, ILogger<CoactivoService> logger)
{
    private const string ConceptoCostas = "COSTAS";
    private readonly ILogger<CoactivoService> _logger = logger;
    private readonly MainDataContext _context = context;
}