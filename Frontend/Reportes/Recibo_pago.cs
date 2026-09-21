using Domain.Generics;
using Domain.Models;
using Domain.Models.Recibos;
using Domain.Models.Recibos.Responses;
using Domain.Responses.Liquidacion;
using Domain.Responses.Recibo;
using Domain.Responses.Recibo.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Frontend.Reportes;

public class Recibo_pago
{
    public async Task<string> CreatePdf(
        ReciboDto recibo,
        List<DetalleReciboDto> listaConceptos,
        Parametro param,
        string carpetaPdf = "",
        string nombreArchivo = "")
    {
        // Resolución dinámica de la carpeta de salida (multiplataforma)
        string carpeta = !string.IsNullOrEmpty(carpetaPdf) ? carpetaPdf : "/var/www/velez/pdf/";
        
        if (!Directory.Exists(carpeta))
            Directory.CreateDirectory(carpeta);

        string archivoFinal = !string.IsNullOrEmpty(nombreArchivo)
            ? nombreArchivo
            : (!string.IsNullOrEmpty(recibo.PlacaVehiculo)
                ? $"Recibo_{recibo.PlacaVehiculo.Trim()}_{recibo.Fecha:yyyyMMdd}_{recibo.Id}.pdf"
                : $"Recibo_{recibo.Fecha:yyyyMMdd}_{recibo.Id}.pdf");

        string filename = Path.Combine(carpeta, archivoFinal);

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
                        page.MarginHorizontal(32);
                        page.MarginVertical(28);
                        page.DefaultTextStyle(x => x.FontSize(8.5f).FontFamily("Arial").FontColor("#1f2937"));

                        page.Content().Column(col =>
                        {
                            // ── ENCABEZADO PRINCIPAL ──────────────────────
                            col.Item().BorderBottom(2).BorderColor("#1e3a8a").PaddingBottom(8).Row(row =>
                            {
                                if (tieneLogo)
                                {
                                    row.ConstantItem(120).Height(55).Image(logoPath).FitArea();
                                    row.RelativeItem().PaddingLeft(12).Column(c =>
                                    {
                                        c.Item().Text(param?.Nombre ?? "ALCALDÍA MUNICIPAL DE VÉLEZ").FontSize(11).Bold().FontColor("#1e3a8a");
                                        c.Item().Text($"SECRETARÍA DE TRÁNSITO Y TRANSPORTE").FontSize(8.5f).Bold().FontColor("#475569");
                                        c.Item().Text($"NIT: {param?.Nit ?? "890.205.345-1"} • {param?.Ciudad ?? "Vélez, Santander"}").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                                        c.Item().Text($"{param?.Direccion ?? "Palacio Municipal"} • Tel: {param?.Telefono ?? "N/A"}").FontSize(7.5f).FontColor(Colors.Grey.Medium);
                                    });
                                }
                                else
                                {
                                    row.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text(param?.Nombre ?? "ALCALDÍA MUNICIPAL DE VÉLEZ").FontSize(12).Bold().FontColor("#1e3a8a");
                                        c.Item().Text($"SECRETARÍA DE TRÁNSITO Y TRANSPORTE").FontSize(9).Bold().FontColor("#475569");
                                        c.Item().Text($"NIT: {param?.Nit ?? "890.205.345-1"} • {param?.Ciudad ?? "Vélez, Santander"}").FontSize(8).FontColor(Colors.Grey.Darken1);
                                        c.Item().Text($"{param?.Direccion ?? "Palacio Municipal"} • Tel: {param?.Telefono ?? "N/A"}").FontSize(7.5f).FontColor(Colors.Grey.Medium);
                                    });
                                }

                                // Tarjeta No. de Recibo
                                row.ConstantItem(170).Border(1).BorderColor("#1e3a8a").Background("#f8fafc").Padding(6).Column(c =>
                                {
                                    c.Item().AlignCenter().Text("RECIBO OFICIAL DE LIQUIDACIÓN").FontSize(7).Bold().FontColor("#1e3a8a");
                                    c.Item().AlignCenter().Text($"RC-{recibo.Id:D6}").FontSize(14).Bold().FontColor("#dc2626");
                                    c.Item().PaddingTop(2).AlignCenter().Background("#e2e8f0").PaddingVertical(1).PaddingHorizontal(4).Text(copia).FontSize(6.5f).Bold().FontColor("#334155");
                                });
                            });

                            col.Item().PaddingTop(10);

                            // ── TARJETAS DE INFORMACIÓN DEL VEHÍCULO Y RECIBO ──
                            col.Item().Row(row =>
                            {
                                // Tarjeta 1: Información del Vehículo
                                row.RelativeItem().Border(1).BorderColor("#cbd5e1").Background("#f8fafc").Padding(8).Column(c =>
                                {
                                    c.Item().Row(r =>
                                    {
                                        r.RelativeItem().Text("DATOS DEL VEHÍCULO").FontSize(8).Bold().FontColor("#1e3a8a");
                                    });
                                    c.Item().PaddingVertical(3).LineHorizontal(0.5f).LineColor("#cbd5e1");

                                    c.Item().Row(r =>
                                    {
                                        r.ConstantItem(60).Text("PLACA:").FontSize(8).FontColor(Colors.Grey.Darken2);
                                        r.RelativeItem().Text(string.IsNullOrEmpty(recibo.PlacaVehiculo) ? "N/A" : recibo.PlacaVehiculo.ToUpper()).FontSize(11).Bold().FontColor("#0f172a");
                                    });

                                    c.Item().PaddingTop(2).Row(r =>
                                    {
                                        r.ConstantItem(60).Text("Vigencias:").FontSize(7.5f).FontColor(Colors.Grey.Darken2);
                                        r.RelativeItem().Text($"Desde {recibo.Desde} hasta {recibo.Hasta}").FontSize(8).Bold().FontColor("#334155");
                                    });

                                    c.Item().PaddingTop(2).Row(r =>
                                    {
                                        r.ConstantItem(60).Text("Concepto:").FontSize(7.5f).FontColor(Colors.Grey.Darken2);
                                        r.RelativeItem().Text("Impuesto de Rodamiento y Derechos de Tránsito").FontSize(7.5f).FontColor("#334155");
                                    });
                                });

                                row.ConstantItem(10); // Separador

                                // Tarjeta 2: Fechas y Estado
                                row.RelativeItem().Border(1).BorderColor("#cbd5e1").Background("#f8fafc").Padding(8).Column(c =>
                                {
                                    c.Item().Text("DATOS DE EXPEDICIÓN").FontSize(8).Bold().FontColor("#1e3a8a");
                                    c.Item().PaddingVertical(3).LineHorizontal(0.5f).LineColor("#cbd5e1");

                                    c.Item().Row(r =>
                                    {
                                        r.ConstantItem(90).Text("Fecha Expedición:").FontSize(7.5f).FontColor(Colors.Grey.Darken2);
                                        r.RelativeItem().Text($"{recibo.Fecha:dd/MM/yyyy HH:mm}").FontSize(8).Bold().FontColor("#334155");
                                    });

                                    c.Item().PaddingTop(2).Row(r =>
                                    {
                                        r.ConstantItem(90).Text("Fecha Límite Pago:").FontSize(7.5f).FontColor(Colors.Grey.Darken2);
                                        r.RelativeItem().Text($"{recibo.Fecha.AddDays(15):dd/MM/yyyy}").FontSize(8).Bold().FontColor("#dc2626");
                                    });

                                    c.Item().PaddingTop(2).Row(r =>
                                    {
                                        r.ConstantItem(90).Text("Estado Liquidación:").FontSize(7.5f).FontColor(Colors.Grey.Darken2);
                                        var estadoTxt = recibo.Estado.GetDisplayName();
                                        r.RelativeItem().Text(estadoTxt.ToUpper()).FontSize(8).Bold()
                                            .FontColor(recibo.Estado == EstadoRecibo.Pagado ? "#15803d" : "#b45309");
                                    });
                                });
                            });

                            col.Item().PaddingTop(10);

                            // ── TABLA DE CONCEPTOS LIQUIDADOS ───────────
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.ConstantColumn(45); // Año
                                    c.RelativeColumn(1.3f); // Rodamiento
                                    c.RelativeColumn(1.1f); // Carga Datos
                                    c.RelativeColumn(1.1f); // Estampillas
                                    c.RelativeColumn(1.1f); // Interés Mora
                                    c.RelativeColumn(1.1f); // Descuento
                                    c.RelativeColumn(1.4f); // Total
                                });

                                static IContainer HeaderCell(IContainer c) =>
                                    c.Background("#1e3a8a").PaddingVertical(4).PaddingHorizontal(3).AlignCenter().AlignMiddle();

                                table.Header(h =>
                                {
                                    h.Cell().Element(HeaderCell).Text("AÑO").FontSize(7.5f).FontColor(Colors.White).Bold();
                                    h.Cell().Element(HeaderCell).Text("RODAMIENTO").FontSize(7.5f).FontColor(Colors.White).Bold();
                                    h.Cell().Element(HeaderCell).Text("CARGA DATOS").FontSize(7.5f).FontColor(Colors.White).Bold();
                                    h.Cell().Element(HeaderCell).Text("ESTAMPILLAS").FontSize(7.5f).FontColor(Colors.White).Bold();
                                    h.Cell().Element(HeaderCell).Text("INT. MORA").FontSize(7.5f).FontColor(Colors.White).Bold();
                                    h.Cell().Element(HeaderCell).Text("DCTO (-)").FontSize(7.5f).FontColor(Colors.White).Bold();
                                    h.Cell().Element(HeaderCell).Text("SUBTOTAL").FontSize(7.5f).FontColor(Colors.White).Bold();
                                });

                                bool par = false;
                                if (listaConceptos != null && listaConceptos.Any())
                                {
                                    foreach (var item in listaConceptos)
                                    {
                                        par = !par;
                                        var bg = par ? "#ffffff" : "#f8fafc";
                                        var subtotalItem = item.ValorRodamiento + item.ValorCarga + item.ValorEstampillas + item.ValorInteres - (item.Descuento ?? 0);

                                        static IContainer DataCell(IContainer c, string bg) =>
                                            c.Background(bg).BorderBottom(0.5f).BorderColor("#e2e8f0").PaddingVertical(3).PaddingHorizontal(4).AlignRight().AlignMiddle();

                                        table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#e2e8f0").PaddingVertical(3).AlignCenter().AlignMiddle()
                                            .Text(item.Vigencia.ToString()).FontSize(8).Bold().FontColor("#1e3a8a");

                                        table.Cell().Element(c => DataCell(c, bg)).Text($"{item.ValorRodamiento:N0}").FontSize(8);
                                        table.Cell().Element(c => DataCell(c, bg)).Text($"{item.ValorCarga:N0}").FontSize(8);
                                        table.Cell().Element(c => DataCell(c, bg)).Text($"{item.ValorEstampillas:N0}").FontSize(8);
                                        table.Cell().Element(c => DataCell(c, bg)).Text($"{item.ValorInteres:N0}").FontSize(8).FontColor(item.ValorInteres > 0 ? "#b91c1c" : "#1f2937");
                                        table.Cell().Element(c => DataCell(c, bg)).Text($"{(item.Descuento.HasValue && item.Descuento > 0 ? $"-{item.Descuento.Value:N0}" : "$0")}").FontSize(8).FontColor("#15803d");
                                        table.Cell().Element(c => DataCell(c, bg)).Text($"{subtotalItem:N0}").FontSize(8.5f).Bold().FontColor("#0f172a");
                                    }
                                }
                                else
                                {
                                    var bg = "#ffffff";
                                    static IContainer DataCell(IContainer c, string bg) =>
                                        c.Background(bg).BorderBottom(0.5f).BorderColor("#e2e8f0").PaddingVertical(3).PaddingHorizontal(4).AlignRight().AlignMiddle();

                                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#e2e8f0").PaddingVertical(3).AlignCenter().AlignMiddle()
                                        .Text(recibo.Fecha.Year.ToString()).FontSize(8).Bold().FontColor("#1e3a8a");

                                    table.Cell().Element(c => DataCell(c, bg)).Text($"{recibo.ValorCapital:N0}").FontSize(8);
                                    table.Cell().Element(c => DataCell(c, bg)).Text($"{recibo.ValorCargaDatos:N0}").FontSize(8);
                                    table.Cell().Element(c => DataCell(c, bg)).Text($"{recibo.Estampillas:N0}").FontSize(8);
                                    table.Cell().Element(c => DataCell(c, bg)).Text($"{recibo.InteresMora:N0}").FontSize(8).FontColor(recibo.InteresMora > 0 ? "#b91c1c" : "#1f2937");
                                    table.Cell().Element(c => DataCell(c, bg)).Text($"{(recibo.Descuento > 0 ? $"-{recibo.Descuento:N0}" : "$0")}").FontSize(8).FontColor("#15803d");
                                    table.Cell().Element(c => DataCell(c, bg)).Text($"{recibo.ValorTotalSistema:N0}").FontSize(8.5f).Bold().FontColor("#0f172a");
                                }
                            });

                            col.Item().PaddingTop(8);

                            // ── CUADRO DE RESUMEN FINANCIERO Y TOTALES ─────
                            col.Item().Row(row =>
                            {
                                // Información Bancaria para Pago
                                row.RelativeItem(1.2f).Border(1).BorderColor("#cbd5e1").Background("#f8fafc").Padding(6).Column(c =>
                                {
                                    c.Item().Text("INFORMACIÓN PARA CONSIGNACIÓN / RECAUDO").FontSize(7.5f).Bold().FontColor("#1e3a8a");
                                    c.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor("#cbd5e1");

                                    if (param != null)
                                    {
                                        if (!string.IsNullOrEmpty(param.CuentaTransito))
                                        {
                                            c.Item().PaddingTop(2).Text(t =>
                                            {
                                                t.Span("Banco Tránsito: ").FontSize(7).Bold();
                                                t.Span($"{param.BancoTransito} • Cuenta: {param.CuentaTransito}").FontSize(7);
                                            });
                                        }
                                        if (!string.IsNullOrEmpty(param.CuentaTercero))
                                        {
                                            c.Item().PaddingTop(1).Text(t =>
                                            {
                                                t.Span("Banco Convenio/Costas: ").FontSize(7).Bold();
                                                t.Span($"{param.BancoTercero} • Cuenta: {param.CuentaTercero}").FontSize(7);
                                            });
                                        }
                                    }
                                    c.Item().PaddingTop(3).Text("Pague oportunamente en las entidades autorizadas o en las ventanillas de la Secretaría de Tránsito presentando este recibo con su código de barras.").FontSize(6.5f).FontColor(Colors.Grey.Darken2);
                                });

                                row.ConstantItem(12); // Separador

                                // Desglose de Totales
                                row.RelativeItem(0.9f).Border(1).BorderColor("#1e3a8a").Background("#ffffff").Column(c =>
                                {
                                    static void FilaResumen(ColumnDescriptor col, string label, string val, bool neg = false, bool bold = false)
                                    {
                                        col.Item().PaddingVertical(2).PaddingHorizontal(6).Row(r =>
                                        {
                                            r.RelativeItem().Text(label).FontSize(7.5f).FontColor(Colors.Grey.Darken3);
                                            var text = r.ConstantItem(75).AlignRight().Text(val).FontSize(7.5f)
                                                .FontColor(neg ? "#15803d" : "#0f172a");
                                            if (bold)
                                            {
                                                text.Bold();
                                            }
                                        });
                                    }

                                    FilaResumen(c, "Total Capital:", $"${recibo.ValorCapital:N0}");
                                    FilaResumen(c, "Intereses Mora:", $"${recibo.InteresMora:N0}");
                                    FilaResumen(c, "Estampillas / Gastos:", $"${recibo.Estampillas + recibo.ValorCargaDatos:N0}");
                                    if (recibo.Descuento > 0)
                                    {
                                        FilaResumen(c, "Descuento Aplicado:", $"-${recibo.Descuento:N0}", neg: true, bold: true);
                                    }

                                    // TOTAL GENERAL DESTACADO
                                    c.Item().Background("#1e3a8a").PaddingVertical(5).PaddingHorizontal(6).Row(r =>
                                    {
                                        r.RelativeItem().Text("TOTAL A PAGAR:").FontSize(9).Bold().FontColor(Colors.White);
                                        r.ConstantItem(85).AlignRight().Text($"${recibo.ValorTotalSistema:N0}").FontSize(10.5f).Bold().FontColor(Colors.White);
                                    });
                                });
                            });

                            col.Item().PaddingTop(10);

                            // ── SECCIÓN DE FIRMAS Y CÓDIGO ────────────────
                            col.Item().Row(row =>
                            {
                                // Sello / Espacio firma funcionario
                                row.RelativeItem().Border(0.5f).BorderColor("#cbd5e1").Padding(6).Column(c =>
                                {
                                    c.Item().Text("LIQUIDADO Y EXPEDIDO POR:").FontSize(6.5f).Bold().FontColor(Colors.Grey.Darken2);
                                    c.Item().PaddingTop(22).LineHorizontal(0.5f).LineColor("#94a3b8");
                                    c.Item().PaddingTop(2).AlignCenter().Text(param?.NombreSecretario ?? "Firma Autorizada y Sello de Ventanilla").FontSize(7).Bold().FontColor("#334155");
                                    c.Item().AlignCenter().Text(param?.CargoSecretario ?? "Secretario(a) de Tránsito y Transporte").FontSize(6.5f).FontColor(Colors.Grey.Darken1);
                                });

                                row.ConstantItem(12);

                                // Sello de Banco y Timbre de Pago
                                row.RelativeItem().Border(0.5f).BorderColor("#cbd5e1").Padding(6).Column(c =>
                                {
                                    c.Item().Text("TIMBRE O SELLO DE CAJA / BANCO RECAUDADOR:").FontSize(6.5f).Bold().FontColor(Colors.Grey.Darken2);
                                    c.Item().PaddingTop(22).LineHorizontal(0.5f).LineColor("#94a3b8");
                                    c.Item().PaddingTop(2).AlignCenter().Text("Fecha, Cajero y Firma de Recibido").FontSize(7).FontColor(Colors.Grey.Darken1);
                                });
                            });

                            col.Item().PaddingTop(6);

                            // ── CÓDIGO DE REFERENCIA / BARRAS SIMULADO ───
                            col.Item().Background("#f1f5f9").Border(0.5f).BorderColor("#cbd5e1").PaddingVertical(4).PaddingHorizontal(8).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text($"Ref. Recaudo: (415)7709998000000(8020)00{recibo.Id:D6}(3900){(long)recibo.ValorTotalSistema:D10}(96){recibo.Fecha.AddDays(15):yyyyMMdd}")
                                        .FontSize(6.5f).FontFamily("Courier New").FontColor("#334155");
                                    c.Item().Text("Comprobante válido únicamente con el timbre de la entidad bancaria recaudadora.")
                                        .FontSize(6f).Italic().FontColor(Colors.Grey.Darken1);
                                });
                            });

                            col.Item().PaddingTop(8).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                        });
                    });
                }
            })
            .GeneratePdf(filename);
        return filename;
    }

    public async Task<string> PazySalvo(EstadoCuentaVehiculoDto item, Parametro param, string carpetaPdf = "")
    {
        string carpeta = !string.IsNullOrEmpty(carpetaPdf) ? carpetaPdf : "/var/www/velez/pdf/";
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

        Console.WriteLine($"✅ PDF generado: {filename}");
        return filename;
    }
}