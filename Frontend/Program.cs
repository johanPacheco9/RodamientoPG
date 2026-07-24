using Frontend.Components;
using Infrastructure.AppDbContext;
using Infrastructure.DependencyInjection;
using Infrastructure.Services.Alimentador;
using Infrastructure.Services.Carteras;
using Infrastructure.Services.EmailNotification;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using QuestPDF.Infrastructure;
using System.Text;
using Infrastructure.Services.Importados;

QuestPDF.Settings.License = LicenseType.Community;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddRazorPages(options =>
{
    options.RootDirectory = "/Components/Pages";
});

// 🚀 SOLUCIÓN: Agrega los servicios necesarios para que app.MapControllers() funcione
builder.Services.AddControllers(); 

builder.Services.AddDbContextFactory<MainDataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")),
    ServiceLifetime.Scoped);

builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

var jwtKey = builder.Configuration["Jwt:Key"] ?? "rodamiento-dev-key-change-me-1098825894";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "Rodamiento";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "Rodamiento.Frontend";

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.LoginPath = "/";
        options.AccessDeniedPath = "/forbidden";
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

// Servicios propios
builder.Services.AddInfrastructureServices();
builder.Services.AddScoped<BusquedaService>();

var app = builder.Build();
app.MapPost("/test-email", async (EmailService emailService) =>
{
    await emailService.Enviar(
        "johanpach9@gmail.com",
        "Johan",
        "Prueba SMTP",
        "<h2>Hola</h2><p>Correo de prueba.</p>");

    return Results.Ok("Correo enviado");
});

app.MapGet("/generar-excel-prueba", async (HttpContext context) =>
{
    var tempPath = Path.Combine(Path.GetTempPath(), $"CargaMasiva_Vehiculos_{Guid.NewGuid()}.xlsx");

    try
    {
        // Genera el Excel con los 10,000 registros
        ExcelTestGenerator.GenerarExcelPrueba(tempPath, 1000);

        var bytes = await File.ReadAllBytesAsync(tempPath);

        return Results.File(
            bytes,
            contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileDownloadName: "CargaMasiva_10000_Vehiculos.xlsx"
        );
    }
    finally
    {
        if (File.Exists(tempPath))
        {
            File.Delete(tempPath);
        }
    }
});
// Configure the HTTP request pipeline.

if (!app.Environment.IsDevelopment())
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
app.MapRazorPages();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
