using Frontend.Components;
using Infrastructure.AppDbContext;
using Infrastructure.DependencyInjection;
using Infrastructure.Services.Alimentador;
using Infrastructure.Services.Carteras;
using Infrastructure.Services.EmailNotification;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using QuestPDF.Infrastructure;
using System.Text;
using Domain.Models;
using Domain.Responses.Users.Enums;
using Infrastructure.Services.AcuerdosPago;
using Infrastructure.Services.Importados;
using Microsoft.AspNetCore.Identity;

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

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
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
        ExcelTestGenerator.GenerarExcelPrueba(tempPath, 10000);

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
    try
    {
        using var scope = app.Services.CreateScope();
        // En lugar de pedir el contexto directo, le pedimos la factoría instalada
        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MainDataContext>>();

        // Creamos un contexto temporal exclusivo para migrar y poblar la base de datos
        using var context = await contextFactory.CreateDbContextAsync();

        await context.Database.MigrateAsync();

// Admin seeding moved to later block after static files
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
        throw;
    }
    
    
    
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
app.UseStaticFiles(); // Serve static files (e.g., permissions)
}
// Ensure admin user exists (runs in all environments)
// Admin seeding moved to later block after DB initialization





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

// Inicializar y sembrar base de datos en el arranque

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MainDataContext>();
    var carteraService = scope.ServiceProvider.GetRequiredService<CarteraService>();
    DbInitializer.Initialize(context, carteraService);
    using (var adminScope = app.Services.CreateScope())
    {
        var adminContext = adminScope.ServiceProvider.GetRequiredService<MainDataContext>();
        if (!await adminContext.Usuarios.AnyAsync())
        {
            var hasher = new PasswordHasher<object>();
            var admin = new Usuario
            {
                Nombre = "Johan",
                UserName = "admin@dataset-software.com",
                Role = Role.Administrador,
                IsHabilitado = true,
                Auth0Id = "alsdaj",
                CreatedBy = 1,
                FechaCreacion = DateTime.UtcNow,
            };
            admin.Password = hasher.HashPassword(null!, "123456");
            adminContext.Usuarios.Add(admin);
            await adminContext.SaveChangesAsync();
        }
    }
}

using (var scope = app.Services.CreateScope())
{
    var acuerdoPagoService = scope.ServiceProvider.GetRequiredService<AcuerdoPagoService>();
    await acuerdoPagoService.ActualizarVencidosToCoactivo();
}

app.Run();