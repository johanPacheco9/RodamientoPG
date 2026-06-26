using Infrastructure.AppDbContext;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Infrastructure.Services.EmailNotification;

// ── 1. Servicio de envío de emails ──────────────────────────────────────
public class EmailService(IConfiguration config)
{
    public async Task Enviar(string destinatario, string nombre, string asunto, string cuerpoHtml)
    {
        var mensaje = new MimeMessage();
        mensaje.From.Add(new MailboxAddress(config["Email:NombreRemitente"], config["Email:Remitente"]));
        mensaje.To.Add(new MailboxAddress(nombre, destinatario));
        mensaje.Subject = asunto;
        mensaje.Body = new TextPart("html") { Text = cuerpoHtml };

        using var client = new SmtpClient();
        await client.ConnectAsync(config["Email:Host"], int.Parse(config["Email:Puerto"]!), SecureSocketOptions.SslOnConnect);
        await client.AuthenticateAsync(config["Email:Usuario"], config["Email:Clave"]);
        await client.SendAsync(mensaje);
        await client.DisconnectAsync(true);
    }
}

// ── 2. Tarea programada ─────────────────────────────────────────────────
public class NotificacionCarteraJob(
    IServiceScopeFactory scopeFactory,
    ILogger<NotificacionCarteraJob> logger
) : BackgroundService
{
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var ahora = DateTime.Now;
            var proxima = DateTime.Today.AddDays(ahora.Hour >= 8 ? 1 : 0).AddHours(8);
            var espera = proxima - ahora;

            logger.LogInformation("Próxima notificación de cartera: {proxima}", proxima);
            await Task.Delay(espera, stoppingToken);

            await EnviarNotificaciones(stoppingToken);
        }
    }

    private async Task EnviarNotificaciones(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MainDataContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
            var anioActual = DateTime.Now.Year;

            var deudores = await context.Cartera
                .Where(c => c.Vigencia <= anioActual && !c.IsPagado)
                .Include(c => c.Vehiculo)
                .ThenInclude(v => v.Propietario)
                .Where(c => !string.IsNullOrEmpty(c.Vehiculo.Propietario.Correo))
                .GroupBy(c => c.Vehiculo.Propietario)
                .ToListAsync(ct);

            foreach (var grupo in deudores)
            {
                var propietario = grupo.Key;
                var vigencias = grupo.Select(c => c.Vigencia).Distinct().OrderBy(v => v).ToList();
                var totalDeuda = grupo.Sum(c => c.ValorTotal);

                var cuerpo = GenerarCuerpoEmail(propietario.Nombre, vigencias, totalDeuda);

                await emailService.Enviar(
                    propietario.Correo,
                    propietario.Nombre,
                    "Notificación de deuda vehicular pendiente",
                    cuerpo);

                logger.LogInformation("Notificación enviada a {correo}", propietario.Correo);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error en NotificacionCarteraJob");
        }
    }

    private static string GenerarCuerpoEmail(string nombre, List<int> vigencias, decimal total) => $"""
                                                                                                    <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto;border:1px solid #ddd;border-radius:8px;overflow:hidden;">
                                                                                                        <div style="background:#2d4a6b;padding:20px;text-align:center;">
                                                                                                            <h2 style="color:white;margin:0;">Secretaría de Tránsito</h2>
                                                                                                            <p style="color:#ccc;margin:4px 0;">Municipio de Vélez - Santander</p>
                                                                                                        </div>
                                                                                                        <div style="padding:24px;">
                                                                                                            <p>Estimado(a) <strong>{nombre}</strong>,</p>
                                                                                                            <p>Le informamos que su vehículo presenta deuda pendiente por concepto de 
                                                                                                               impuesto vehicular en las siguientes vigencias:</p>
                                                                                                            <ul>
                                                                                                                {string.Join("", vigencias.Select(v => $"<li>Vigencia <strong>{v}</strong></li>"))}
                                                                                                            </ul>
                                                                                                            <div style="background:#fff3cd;border:1px solid #ffc107;border-radius:6px;padding:12px;margin:16px 0;">
                                                                                                                <strong>Total adeudado: {total:C}</strong>
                                                                                                            </div>
                                                                                                            <p>Para realizar el pago o consultar su estado de cuenta, acérquese a las 
                                                                                                               oficinas de la Secretaría de Tránsito.</p>
                                                                                                            <p style="color:#888;font-size:12px;">Este es un mensaje automático, 
                                                                                                               por favor no responda este correo.</p>
                                                                                                        </div>
                                                                                                    </div>
                                                                                                    """;
}