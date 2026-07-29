using Domain.Generics;
using Domain.Models;
using Domain.Models.Recibos;
using Domain.Models.Recibos.Responses;
using Domain.Responses.Liquidacion;
using Domain.Responses.Recibo;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Frontend.Reportes;

public class Recibo_pago
{
    public async Task CreatePdf(ReciboDto recibo, List<DetalleReciboDto> listaConceptos, Parametro param)
    {
        try
        {
            // Resolución dinámica de la carpeta de salida (multiplataforma)
            string carpeta = "/var/www/velez/pdf/";
            try
            {
                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);
            }
            catch
            {
                carpeta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pdf");
                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);
            }

            string filename = Path.Combine(carpeta, $"Recibo_{recibo.Id}.pdf");

            decimal vlrCostas = recibo.ValorCapital + recibo.ValorTotalSistema;
            decimal vlrTransito = recibo.ValorTotalSistema - recibo.ValorCapital;
            if (vlrTransito < 0) vlrTransito = 0;

            // Ruta del logo con verificación de existencia
            string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logos", "logo.jpg");
            if (!File.Exists(logoPath))
                logoPath = "wwwroot/logos/logo.jpg";
            bool tieneLogo = File.Exists(logoPath);

            Document.Create(container =>
                {
                    // El recibo se imprime dos veces: original y copia
                    foreach (var copia in new[] { "ORIGINAL CONTRIBUYENTE", "COPIA BANCO / TRÁNSITO" })
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.Letter);
                            page.Margin(25);
                            page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                            page.Content().Column(col =>
                            {
                                // ── Encabezado ──────────────────────────────
                                col.Item().Row(row =>
                                {
                                    if (tieneLogo)
                                    {
                                        row.ConstantItem(180).Image(logoPath).FitWidth();
                                        row.RelativeItem().PaddingLeft(10).Column(c =>
                                        {
                                            c.Item().Text(param?.Nombre ?? "SECRETARÍA DE TRÁNSITO Y TRANSPORTE").FontSize(10).Bold().FontColor("#1e3a8a");
                                            c.Item().Text($"NIT: {param?.Nit ?? "N/A"} | Municipio de {param?.Ciudad ?? "Vélez"}").FontSize(8).FontColor(Colors.Grey.Darken2);
                                        });
                                    }
                                    else
                                    {
                                        row.RelativeItem().Column(c =>
                                        {
                                            c.Item().Text(param?.Nombre ?? "SECRETARÍA DE TRÁNSITO Y TRANSPORTE").FontSize(11).Bold().FontColor("#1e3a8a");
                                            c.Item().Text($"NIT: {param?.Nit ?? "N/A"} | {param?.Ciudad ?? "Vélez"}").FontSize(8).FontColor(Colors.Grey.Darken2);
                                        });
                                    }

                                    row.ConstantItem(150).Column(c =>
                                    {
                                        c.Item().AlignRight().Text("COMPROBANTE DE PAGO").FontSize(11).Bold().FontColor("#1e3a8a");
                                        c.Item().AlignRight().Text($"RECIBO N° RC-{recibo.Id:D6}").FontSize(10).Bold();
                                        c.Item().AlignRight().Text(copia).FontSize(7).FontColor(Colors.Grey.Medium);
                                    });
                                });

                                col.Item().PaddingVertical(4).LineHorizontal(1.5f).LineColor("#1e3a8a");

                                // ── Datos del Vehículo y Contribuyente ───────
                                col.Item().Background(Colors.Grey.Lighten4).Padding(6).Row(row =>
                                {
                                    row.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text($"Placa: {(string.IsNullOrEmpty(recibo.PlacaVehiculo) ? "N/A" : recibo.PlacaVehiculo)}").FontSize(11).Bold().FontColor("#1e3a8a");
                                        c.Item().Text($"Fecha Expedición: {recibo.Fecha:dd/MM/yyyy}");
                                        c.Item().Text($"Fecha de Pago: {(recibo.FechaPago.HasValue ? recibo.FechaPago.Value.ToString("dd/MM/yyyy HH:mm") : "Pendiente")}");
                                    });
                                    row.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text($"Vigencia Inicial: {recibo.Desde}");
                                        c.Item().Text($"Vigencia Final: {recibo.Hasta}");
                                        c.Item().Text($"Estado: {recibo.Estado.GetDisplayName()}");
                                    });
                                });

                                col.Item().PaddingTop(6);

                                // ── Tabla de conceptos ───────────────────────
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(c =>
                                    {
                                        c.ConstantColumn(50); // Vigencia
                                        c.RelativeColumn();   // Capital / Rodamiento
                                        c.RelativeColumn();   // Carga Datos
                                        c.RelativeColumn();   // Estampillas
                                        c.RelativeColumn();   // Intereses / Mora
                                        c.RelativeColumn();   // Total Concepto
                                    });

                                    static IContainer HeaderCell(IContainer c) =>
                                        c.Background("#1e3a8a").Padding(4).AlignCenter();

                                    table.Header(h =>
                                    {
                                        h.Cell().Element(HeaderCell).Text("Vigencia").FontColor(Colors.White).Bold();
                                        h.Cell().Element(HeaderCell).Text("Capital / Rod.").FontColor(Colors.White).Bold();
                                        h.Cell().Element(HeaderCell).Text("Imp. Carga").FontColor(Colors.White).Bold();
                                        h.Cell().Element(HeaderCell).Text("Estampillas").FontColor(Colors.White).Bold();
                                        h.Cell().Element(HeaderCell).Text("Interés Mora").FontColor(Colors.White).Bold();
                                        h.Cell().Element(HeaderCell).Text("Total Concepto").FontColor(Colors.White).Bold();
                                    });

                                    bool par = false;
                                    if (listaConceptos != null && listaConceptos.Any())
                                    {
                                        foreach (var item in listaConceptos)
                                        {
                                            par = !par;
                                            var bg = par ? Colors.White : Colors.Grey.Lighten5;

                                            static IContainer DataCell(IContainer c, string bg) =>
                                                c.Background(bg).Padding(3).AlignRight();

                                            table.Cell().Background(bg).Padding(3).AlignCenter()
                                                .Text(item.Vigencia.ToString()).Bold();
                                            table.Cell().Element(c => DataCell(c, bg))
                                                .Text($"{item.ValorRodamiento:C0}");
                                            table.Cell().Element(c => DataCell(c, bg))
                                                .Text($"{item.ValorCarga:C0}");
                                            table.Cell().Element(c => DataCell(c, bg))
                                                .Text($"{item.ValorEstampillas:C0}");
                                            table.Cell().Element(c => DataCell(c, bg))
                                                .Text($"{item.ValorInteres:C0}");
                                            table.Cell().Element(c => DataCell(c, bg))
                                                .Text($"{(item.ValorRodamiento + item.ValorCarga + item.ValorEstampillas + item.ValorInteres - item.Descuento):C0}").Bold();
                                        }
                                    }
                                    else
                                    {
                                        var bg = Colors.White;

                                        static IContainer DataCell(IContainer c, string bg) =>
                                            c.Background(bg).Padding(3).AlignRight();

                                        table.Cell().Background(bg).Padding(3).AlignCenter()
                                            .Text(recibo.Fecha.Year.ToString()).Bold();
                                        table.Cell().Element(c => DataCell(c, bg))
                                            .Text($"{recibo.ValorCapital:C0}");
                                        table.Cell().Element(c => DataCell(c, bg))
                                            .Text($"{recibo.ValorCargaDatos:C0}");
                                        table.Cell().Element(c => DataCell(c, bg))
                                            .Text($"{recibo.Estampillas:C0}");
                                        table.Cell().Element(c => DataCell(c, bg))
                                            .Text($"{recibo.InteresMora:C0}");
                                        table.Cell().Element(c => DataCell(c, bg))
                                            .Text($"{recibo.ValorTotalSistema:C0}").Bold();
                                    }
                                });

                                col.Item().PaddingTop(6);

                                // ── Bloque de Totales y Liquidación ─────────
                                col.Item().Background("#1e3a8a").Padding(6).Row(row =>
                                {
                                    row.RelativeItem().Text($"Descuento Aplicado: {recibo.Descuento:C0}")
                                        .FontColor(Colors.White).Bold();
                                    row.ConstantItem(220).AlignRight()
                                        .Text($"TOTAL RECIBIDO: {recibo.ValorTotalSistema:C0}")
                                        .FontColor(Colors.White).Bold().FontSize(11);
                                });

                                if (param != null)
                                {
                                    col.Item().PaddingTop(4).Row(row =>
                                    {
                                        if (!string.IsNullOrEmpty(param.CuentaTransito))
                                        {
                                            row.RelativeItem().Column(c => { c.Item().Text($"Cuenta Tránsito: {param.CuentaTransito} ({param.BancoTransito})").FontSize(7.5f).Bold(); });
                                        }
                                        if (!string.IsNullOrEmpty(param.CuentaTercero))
                                        {
                                            row.RelativeItem().Column(c => { c.Item().Text($"Cuenta Costas/Terceros: {param.CuentaTercero} ({param.BancoTercero})").FontSize(7.5f).Bold(); });
                                        }
                                    });
                                }

                                col.Item().PaddingTop(6).Text(
                                        "NOTA: Conserve este comprobante como constancia oficial de pago. " +
                                        "La actualización de paz y salvo en el RUNT/Sistema se realiza automáticamente una vez asentada la transacción.")
                                    .FontSize(7).Italic().FontColor(Colors.Grey.Darken2);

                                col.Item().PaddingTop(10).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                                col.Item().PaddingTop(6);
                            });
                        });
                    }
                })
                .GeneratePdf(filename);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generando PDF Recibo_{recibo?.Id}: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
        }
    }

    public async Task PazySalvo(EstadoCuentaVehiculoDto item, Parametro param)
    {
        try
        {
            string carpeta = "/var/www/velez/pdf/";
            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            string filename = Path.Combine(carpeta, $"PazySalvo_{item.Placa}.pdf");
            var logo = "wwwroot/logos/logo1_Velez.jpg";
            var hoy = DateTime.UtcNow;

            string texto = $"Que una vez revisado el expediente físico del vehículo automotor de placas {item.Placa} " +
                           $"servicio {item.TipoServicio} de propiedad del señor(a): {item.NombrePropietario} " +
                           $"identificado con No. documento {item.Documento} se informa que se encuentra a paz y salvo " +
                           $"por concepto de derecho anual de placa con corte al 31 de diciembre de {hoy.Year}.";

            Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.Letter);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

                        page.Content().Column(col =>
                        {
                            // ── Encabezado con logos ─────────────────────
                            col.Item().Border(1).Row(row =>
                            {
                                row.ConstantItem(80).Image(logo).FitHeight();
                                row.RelativeItem().AlignCenter().AlignMiddle().Column(c =>
                                {
                                    c.Item().AlignCenter().Text("REPUBLICA DE COLOMBIA").Bold();
                                    c.Item().AlignCenter().Text($"MUNICIPIO DE {param.Ciudad}").Bold();
                                    c.Item().AlignCenter().Text(param.Nombre).Bold();
                                    c.Item().AlignCenter().Text($"NIT {param.Nit}");
                                });
                                row.ConstantItem(80).Image(logo).FitHeight();
                            });

                            col.Item().PaddingTop(30).AlignCenter()
                                .Text("PAZ Y SALVO")
                                .FontSize(24).Bold().FontFamily("Times New Roman");

                            col.Item().PaddingTop(20).AlignCenter()
                                .Text($"EL SUSCRITO SECRETARIO DE TRÁNSITO Y TRANSPORTE DE {param.Ciudad.ToUpper()}")
                                .FontSize(12).Bold().FontFamily("Times New Roman");

                            col.Item().PaddingTop(20).AlignCenter()
                                .Text("INFORMA:")
                                .FontSize(24).Bold().FontFamily("Times New Roman");

                            col.Item().PaddingTop(20)
                                .Text(texto)
                                .FontSize(14).Justify();

                            col.Item().PaddingTop(20)
                                .Text($"Se expide la presente en Vélez, a los {hoy.Day} días del mes de {hoy:MMMM} de {hoy.Year}.");

                            col.Item().PaddingTop(120).Column(c =>
                            {
                                c.Item().Text(param.NombreSecretario).Bold();
                                c.Item().Text(param.CargoSecretario);
                                c.Item().Text(param.Ciudad);
                            });

                            col.Item().PaddingTop(40)
                                .Text($"{param.Direccion} - Tel: {param.Telefono} - Correo: {param.Correo}")
                                .FontSize(9).FontColor(Colors.Grey.Darken1);
                        });
                    });
                })
                .GeneratePdf(filename);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}