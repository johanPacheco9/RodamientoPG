using Infrastructure.AppDbContext;
using Infrastructure.Services.EmailNotification;
using Microsoft.Extensions.Logging;
namespace Infrastructure.Services.Procesos.Persuasivo;

public partial class PersuasivoService(ILogger<PersuasivoService> logger, MainDataContext context, EmailService emailService)
{
    private readonly ILogger<PersuasivoService> _logger = logger;
    private readonly MainDataContext _context = context;
    private readonly EmailService _emailService = emailService;
}
