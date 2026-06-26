using Domain.Generics;
using Domain.Models;
using Domain.Models.Recibos;
using Domain.Responses.Liquidacion;
using Domain.Responses.Recibo;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Frontend.Reportes;

public class Recibo_pago
{
    public async Task CreatePdf(Recibo recibo, List<DetalleReciboDto> listaConceptos, Parametro param)
    {
        try
        {
            string carpeta = "/var/www/velez/pdf/";
            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            string filename = Path.Combine(carpeta, $"Recibo_{recibo.Id}.pdf");

            decimal vlrCostas = recibo.ValorCapital + recibo.ValorTotalSistema;
            decimal vlrTransito = recibo.ValorTotalSistema - vlrCostas;

            var logo = "wwwroot/logos/logo.jpg";

            Document.Create(container =>
                {
                    // El recibo se imprime dos veces: original y copia
                    foreach (var copia in new[] { "ORIGINAL", "COPIA BANCO / TRÁNSITO" })
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.Letter);
                            page.Margin(30);
                            page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                            page.Content().Column(col =>
                            {
                                // ── Encabezado ──────────────────────────────
                                col.Item().Row(row => { row.RelativeItem().Image(logo).FitWidth(); });

                                col.Item().LineHorizontal(2).LineColor("#2d4a6b");
                                col.Item().PaddingTop(4);

                                col.Item().Row(row =>
                                {
                                    row.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text($"Placa: {recibo.Vehiculo.Placa}").FontSize(11).Bold();
                                        c.Item().Text($"Fecha: {recibo.Fecha.ToShortDateString()}");
                                        c.Item().Text($"No. Documento: {recibo.Vehiculo.Propietario.Documento}");
                                        c.Item().Text($"Nombre: {recibo.Vehiculo.Propietario.Nombre}");
                                    });
                                    row.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text($"Clase: {recibo.Vehiculo.TipoServicioVehiculo.GetDisplayName()}");
                                        c.Item().Text($"Marca: {recibo.Vehiculo.Marca.Nombre}");
                                        c.Item().Text($"Línea: {recibo.Vehiculo.Linea.Nombre}");
                                    });
                                    row.ConstantItem(130).Column(c =>
                                    {
                                        c.Item().Text("COMPROBANTE PAGO").FontSize(13).Bold().FontColor("#2d4a6b");
                                        c.Item().Text($"RC0000{recibo.Id}").FontSize(11).Bold();
                                        c.Item().Text(copia).FontSize(7).FontColor(Colors.Grey.Medium);
                                    });
                                });

                                col.Item().PaddingTop(8);

                                // ── Tabla de conceptos ───────────────────────
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(c =>
                                    {
                                        c.ConstantColumn(45); // Vigencia
                                        c.RelativeColumn();   // Der. tránsito
                                        c.RelativeColumn();   // Imp. carga
                                        c.RelativeColumn();   // Estampillas
                                        c.RelativeColumn();   // Costas
                                        c.RelativeColumn();   // Interés
                                    });

                                    // Encabezados
                                    static IContainer HeaderCell(IContainer c) =>
                                        c.Background("#2d4a6b").Padding(4).AlignCenter();

                                    table.Header(h =>
                                    {
                                        h.Cell().Element(HeaderCell).Text("Vigen.").FontColor(Colors.White).Bold();
                                        h.Cell().Element(HeaderCell).Text("Der. tránsito").FontColor(Colors.White).Bold();
                                        h.Cell().Element(HeaderCell).Text("Imp. carga").FontColor(Colors.White).Bold();
                                        h.Cell().Element(HeaderCell).Text("Estampillas").FontColor(Colors.White).Bold();
                                        h.Cell().Element(HeaderCell).Text("Costas").FontColor(Colors.White).Bold();
                                        h.Cell().Element(HeaderCell).Text("Interés").FontColor(Colors.White).Bold();
                                    });

                                    // Filas
                                    bool par = false;
                                    foreach (var item in listaConceptos)
                                    {
                                        par = !par;
                                        var bg = par ? Colors.White : Colors.Grey.Lighten4;

                                        static IContainer DataCell(IContainer c, string bg) =>
                                            c.Background(bg).Padding(3).AlignRight();

                                        table.Cell().Background(bg).Padding(3).AlignCenter()
                                            .Text(item.Vigencia.ToString()).Bold();
                                        table.Cell().Element(c => DataCell(c, bg))
                                            .Text($"{item.ValorRodamiento:N0}");
                                        table.Cell().Element(c => DataCell(c, bg))
                                            .Text($"{item.ValorCarga:N0}");
                                        table.Cell().Element(c => DataCell(c, bg))
                                            .Text($"{item.ValorEstampillas:N0}");
                                        table.Cell().Element(c => DataCell(c, bg))
                                            .Text($"{item.ValorRecibo:N0}");
                                        table.Cell().Element(c => DataCell(c, bg))
                                            .Text($"{item.ValorInteres:N0}");
                                    }
                                });

                                col.Item().PaddingTop(8);

                                // ── Totales ──────────────────────────────────
                                col.Item().Background("#2d4a6b").Padding(6).Row(row =>
                                {
                                    row.RelativeItem().Text($"Descuento: {recibo.Descuento:C}")
                                        .FontColor(Colors.White).Bold();
                                    row.ConstantItem(200).AlignRight()
                                        .Text($"Valor Tránsito: {vlrTransito:C}")
                                        .FontColor(Colors.White).Bold().FontSize(11);
                                });

                                col.Item().PaddingTop(4).Row(row =>
                                {
                                    row.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text($"Cuenta: {param.CuentaTransito}").Bold();
                                        c.Item().Text(param.BancoTransito);
                                    });
                                });

                                col.Item().PaddingTop(4).Background("#2d4a6b").Padding(6).Row(row =>
                                {
                                    row.RelativeItem();
                                    row.ConstantItem(200).AlignRight()
                                        .Text($"Valor Costas: {vlrCostas:C}")
                                        .FontColor(Colors.White).Bold().FontSize(11);
                                });

                                col.Item().PaddingTop(4).Row(row =>
                                {
                                    row.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text($"Cuenta: {param.CuentaTercero}").Bold();
                                        c.Item().Text(param.BancoTercero);
                                    });
                                });

                                col.Item().PaddingTop(8).Text(
                                        "SEÑOR CONTRIBUYENTE ES OBLIGATORIO TRAER EL RECIBO PAGADO EL MISMO DIA QUE " +
                                        "SE LE ENTREGA SO PENA DE QUE NO SE ACTUALIZE EL SISTEMA QUEDANDO AUN COMO DEUDOR")
                                    .FontSize(7).Italic().FontColor(Colors.Grey.Darken2);
                            });
                        });
                    }
                })
                .GeneratePdf(filename);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generando PDF: {ex.Message}");
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