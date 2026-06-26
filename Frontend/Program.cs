using Frontend.Components;
using Infrastructure.AppDbContext;
using Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 🚀 SOLUCIÓN: Agrega los servicios necesarios para que app.MapControllers() funcione
builder.Services.AddControllers(); 

builder.Services.AddDbContextFactory<MainDataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")),
    ServiceLifetime.Scoped);

builder.Services.AddDbContext<MainDataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.Zero;
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/forbidden";
    });

// Servicios propios
builder.Services.AddInfrastructureServices();
builder.Services.AddScoped<BusquedaService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var contextFactory = services.GetRequiredService<IDbContextFactory<MainDataContext>>();
            using var context = contextFactory.CreateDbContext();

            Infrastructure.Services.Alimentador.DbInitializer.Initialize(context);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Ocurrió un error local al sembrar los datos iniciales de desarrollo.");
        }
    }
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
});

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Ahora esto se ejecutará sin excepciones
app.MapControllers(); 
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
