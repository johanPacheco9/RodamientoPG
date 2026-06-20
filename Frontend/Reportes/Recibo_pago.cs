using Domain.Generics;
using Domain.Models;
using Domain.Models.ProcesoLiquidacion;
using Domain.Responses.Liquidacion;
using Domain.Responses.Recibo;
using Domain.Responses.Reportes;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
namespace Frontend.Reportes
{
    public class Recibo_pago ///     VELEZ ******
    {
        //private readonly PagosService _pagosService;

        public async Task CreatePdf(Recibo recibo, List<DetalleReciboDto> listaConceptos, Parametro param)
        {
            try
            {
                if (recibo == null)
                {
                    Console.WriteLine("Error: El objeto recibo es null.");

                    return;
                }

                string carpeta = "/var/www/velez/pdf/";
                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);

                string filename = System.IO.Path.Combine(carpeta, $"Recibo_{recibo.Id}.pdf");
                string texto1 = "";
                PdfDocument doc = new PdfDocument();
                doc.Info.Title = $"Recibo_{recibo.Id}";
                PdfPage page = doc.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);
                XTextFormatter tf = new XTextFormatter(gfx);
                XFont font = new XFont("Arial", 8);

                var fullName = "wwwroot/logos/logo3_Velez.jpg";
                var image = XImage.FromFile(fullName);

                int tl = 1;
                int col = 1;
                int fila = 55;
                XRect rect;

                // Dibuja los dos encabezados (Original y Copia para el banco/tránsito)
                for (int i = 1; i <= 2; i++)
                {
                    fila = (i == 1) ? 55 : 500;

                    gfx.DrawImage(image, 37, fila - 35, 521, 60);
                    gfx.DrawRectangle(XBrushes.DarkBlue, 40, fila + 28, 520, 2);

                    gfx.DrawString($"Placa: {recibo.Vehiculo.Placa ?? "vacio"}", new XFont("Arial", 11, XFontStyleEx.Bold), XBrushes.Black, new XPoint(44, fila + 43));
                    gfx.DrawString($"Clase: {recibo.Vehiculo.TipoServicioVehiculo.GetDisplayName() ?? "vacio"}", font, XBrushes.Black, new XPoint(290, fila + 43));
                    gfx.DrawString($"Fecha: {recibo.Fecha.ToShortDateString()}", font, XBrushes.Black, new XPoint(44, fila + 53));
                    gfx.DrawString("COMPROBANTE PAGO", new XFont("Arial", 13, XFontStyleEx.Bold), XBrushes.DarkBlue, new XPoint(417, fila + 50));
                    gfx.DrawString($"Marca: {recibo.Vehiculo.Marca.Nombre ?? "vacio"}", font, XBrushes.Black, new XPoint(290, fila + 53));
                    gfx.DrawString($"RC0000{recibo.Id}", new XFont("Arial", 11, XFontStyleEx.Bold), XBrushes.Black, new XPoint(495, fila + 63));
                    gfx.DrawString($"No. Documento : {recibo.Vehiculo.Propietario.Nombre} {recibo.Vehiculo.Propietario.Documento ?? "vacio"}", font, XBrushes.Black, new XPoint(44, fila + 63));
                    gfx.DrawString($"Linea: {recibo.Vehiculo.Linea.Nombre ?? "vacio"}", font, XBrushes.Black, new XPoint(290, fila + 63));
                    gfx.DrawString($"Nombre: {recibo.Vehiculo.Propietario.Nombre ?? "vacio"}", font, XBrushes.Black, new XPoint(44, fila + 73));

                    // Tabla de encabezados de columnas
                    gfx.DrawRectangle(XBrushes.DarkBlue, 40, fila + 81, 520, 17);
                    string[] headers = { "Vigen", "Der. transito", "Imp.Carga", "Estampillas", "Costas", "Interes" };
                    int[] xPositions = { 40, 75, 130, 170, 220, 260, 300, 335, 389, 430, 480, 525 };

                    for (int h = 0; h < 6; h++)
                    {
                        gfx.DrawString(headers[h], new XFont("Arial", 7, XFontStyleEx.Bold), XBrushes.White, new XPoint(xPositions[h], fila + 90));
                        gfx.DrawString(headers[h], new XFont("Arial", 7, XFontStyleEx.Bold), XBrushes.White, new XPoint(xPositions[h + 6], fila + 90));
                    }
                }

                int xline = 167;
                int xline1 = 610;
                tf.Alignment = XParagraphAlignment.Right;

                // 🧠 REEMPLAZO DE RCON: Renderizado de conceptos financieros usando el DTO limpio
                foreach (var item in listaConceptos)
                {
                    if (tl == 1)
                    {
                        col = 40;
                        tl = 2;
                    }
                    else
                    {
                        col = 300;
                        tl = 1;
                    }

                    gfx.DrawString(item.Vigencia.ToString(), font, XBrushes.Black, new XPoint(col, xline));
                    gfx.DrawString(item.Vigencia.ToString(), font, XBrushes.Black, new XPoint(col, xline1));

                    // Derecho de Rodamiento / Tránsito
                    rect = new XRect(col + 20, xline - 8, 55, 10);
                    tf.DrawString($"{item.ValorRodamiento:###,###,###}", font, XBrushes.Black, rect, XStringFormats.TopLeft);
                    rect = new XRect(col + 20, xline1 - 8, 55, 10);
                    tf.DrawString($"{item.ValorRodamiento:###,###,###}", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                    // Impuesto de Carga
                    rect = new XRect(col + 65, xline - 8, 55, 10);
                    tf.DrawString($"{item.ValorCarga:###,###,###}", font, XBrushes.Black, rect, XStringFormats.TopLeft);
                    rect = new XRect(col + 65, xline1 - 8, 55, 10);
                    tf.DrawString($"{item.ValorCarga:###,###,###}", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                    // Estampillas municipales
                    rect = new XRect(col + 110, xline - 8, 55, 10);
                    tf.DrawString($"{item.ValorEstampillas:###,###,###}", font, XBrushes.Black, rect, XStringFormats.TopLeft);
                    rect = new XRect(col + 110, xline1 - 8, 55, 10);
                    tf.DrawString($"{item.ValorEstampillas:###,###,###}", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                    // Costas de Recibo
                    rect = new XRect(col + 150, xline - 8, 55, 10);
                    tf.DrawString($"{item.ValorRecibo:###,###,###}", font, XBrushes.Black, rect, XStringFormats.TopLeft);
                    rect = new XRect(col + 150, xline1 - 8, 55, 10);
                    tf.DrawString($"{item.ValorRecibo:###,###,###}", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                    // Intereses de mora
                    rect = new XRect(col + 195, xline - 8, 55, 10);
                    tf.DrawString($"{item.ValorInteres:###,###,###}", font, XBrushes.Black, rect, XStringFormats.TopLeft);
                    rect = new XRect(col + 195, xline1 - 8, 55, 10);
                    tf.DrawString($"{item.ValorInteres:###,###,###}", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                    if (tl == 1)
                    {
                        xline += 13;
                        xline1 += 13;
                    }
                }

                if (tl == 2)
                {
                    xline += 13;
                    xline1 += 13;
                }

                // =========================================================
                // 💰 OPERACIONES MATEMÁTICAS PRECISAS (Usando decimal)
                // =========================================================
                decimal vlrCostas = recibo.ValorCapital + recibo.ValorTotalSistema;
                decimal vlrTransito = recibo.ValorTotalSistema - vlrCostas;

                // SECCIÓN SUPERIOR: Cuenta del Organismo de Tránsito
                gfx.DrawRectangle(XBrushes.DarkBlue, 40, xline - 5, 520, 17);
                gfx.DrawString($"Descuento: {recibo.Descuento:C}", new XFont("Arial", 9, XFontStyleEx.Bold), XBrushes.White, new XPoint(40, xline + 5));

                texto1 = $"Valor Transito: {vlrTransito:C}";
                rect = new XRect(375, xline - 2, 178, 27);
                tf.Alignment = XParagraphAlignment.Center;
                tf.DrawString(texto1, new XFont("Arial", 11, XFontStyleEx.Bold), XBrushes.White, rect, XStringFormats.TopLeft);

                texto1 = $"Cuenta {param.CuentaTransito} \r\n {param.BancoTransito}";
                rect = new XRect(42, xline + 22, 178, 27);
                tf.Alignment = XParagraphAlignment.Center;
                tf.DrawString(texto1, new XFont("Arial", 11, XFontStyleEx.Bold), XBrushes.Black, rect, XStringFormats.TopLeft);

                string advertencia =
                    "SEÑOR CONTRIBUYENTE ES OBLIGATORIO TRAER EL RECIBO PAGADO EL MISMO DIA QUE SE LE ENTREGA SO PENA DE QUE NO SE ACTUALIZE EL SISTEMA QUEDANDO AUN COMO DEUDOR";
                rect = new XRect(42, xline + 65, 495, 25);
                tf.Alignment = XParagraphAlignment.Justify;
                tf.DrawString(advertencia, new XFont("Arial", 8, XFontStyleEx.Regular), XBrushes.Black, rect, XStringFormats.TopLeft);

                // SECCIÓN INFERIOR: Cuenta del Tercero (Costas / Operador de Sistema)
                gfx.DrawRectangle(XBrushes.DarkBlue, 40, xline1 - 5, 520, 17);
                texto1 = $"Valor Costas: {vlrCostas:C}";
                rect = new XRect(370, xline1 - 2, 178, 25);
                tf.Alignment = XParagraphAlignment.Center;
                tf.DrawString(texto1, new XFont("Arial", 11, XFontStyleEx.Bold), XBrushes.White, rect, XStringFormats.TopLeft);

                gfx.DrawLine(new XPen(XColor.FromArgb(32, 32, 30)), new XPoint(43, xline1 + 31), new XPoint(220, xline1 + 31));
                texto1 = $"Cuenta {param.CuentaTercero} \r\n {param.BancoTercero}";
                rect = new XRect(42, xline1 + 32, 178, 27);
                tf.Alignment = XParagraphAlignment.Center;
                tf.DrawString(texto1, new XFont("Arial", 11, XFontStyleEx.Bold), XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new XRect(42, xline1 + 65, 495, 25);
                tf.Alignment = XParagraphAlignment.Justify;
                tf.DrawString(advertencia, new XFont("Arial", 8, XFontStyleEx.Regular), XBrushes.Black, rect, XStringFormats.TopLeft);

                doc.Save(filename);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generando el recibo PDF: {ex.Message}");
            }
        }

        public async Task DiarioPdf(List<ReporteDiarioDto> recibos, string rango, string pname, string pnit, string pdir)
        {
            try
            {
                string carpeta = "/var/www/velez/pdf/";
                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);
                string filename = System.IO.Path.Combine(carpeta, $"Informe_Diario.pdf");
                PdfDocument doc = new PdfDocument();
                doc.Info.Title = "Informe_Diario";
                PdfPage page = doc.AddPage();
                page.Orientation = PdfSharp.PageOrientation.Landscape;
                XGraphics gfx = XGraphics.FromPdfPage(page);
                XTextFormatter tf = new XTextFormatter(gfx);
                XRect rect = new XRect(40, 20, 250, 50);
                XFont font = new XFont("Arial", 8);
                string texto1 = pname + " \r\n " + "NIT " + pnit + " \r\n " + "RECIBO DE INGRESOS" + " \r\n " + pdir;
                //                gfx.DrawImage(image, 40, 20, 40, 50);
                int xline = 105;
                decimal t1 = 0;
                decimal t2 = 0;
                decimal t3 = 0;
                decimal t4 = 0;
                int cl = 50;
                decimal t5 = 0;
                decimal t6 = 0;
                decimal t7 = 0;
                decimal t8 = 0;
                decimal t9 = 0;
                int pagina = 1;
                foreach (var item in recibos)
                {
                    if (cl >= 31)
                    {
                        if (t1 > 0)
                        {
                            page = doc.AddPage();
                            pagina = pagina + 1;
                            page.Orientation = PdfSharp.PageOrientation.Landscape;
                            gfx = XGraphics.FromPdfPage(page);
                            tf = new XTextFormatter(gfx);
                        }
                        rect = new XRect(40, 20, 762, 60);
                        cl = 1;
                        gfx.DrawRectangle(XBrushes.White, rect);
                        tf.Alignment = XParagraphAlignment.Center;
                        tf.DrawString(texto1, new XFont("Arial", 11, XFontStyleEx.Bold), XBrushes.Black, rect, XStringFormats.TopLeft);
                        gfx.DrawString("Fecha: " + rango, new XFont("Arial", 8, XFontStyleEx.Bold), XBrushes.Black, new XPoint(40, 90));
                        gfx.DrawString("Pagina: " + pagina, new XFont("Arial", 7, XFontStyleEx.Regular), XBrushes.Black, new XPoint(740, 90));
                        gfx.DrawRectangle(XBrushes.Gray, 40, 93, 748, 18);
                        gfx.DrawString("Recibo Placa        Fecha             Vigencia    Propietario", new XFont("Arial", 8, XFontStyleEx.Bold), XBrushes.White, new XPoint(41, 106));
                        gfx.DrawString("Impuesto          Carga           Costas         Estamp.        Sistem.          Sancion       Interes     Descuento                  Total",
                            new XFont("Arial", 8, XFontStyleEx.Bold), XBrushes.White, new XPoint(347, 106));
                        xline = 105;
                    }
                    xline = xline + 15;
                    gfx.DrawString(item.NumeroRecibo.ToString(), new XFont("Arial", 8, XFontStyleEx.Regular), XBrushes.Black, new XPoint(40, xline));
                    gfx.DrawString(item.Placa, new XFont("Arial", 8, XFontStyleEx.Regular), XBrushes.Black, new XPoint(68, xline));
                    gfx.DrawString(item.FechaCreacion.ToString("dd/mm/yyyy"), new XFont("Arial", 8, XFontStyleEx.Regular), XBrushes.Black, new XPoint(108, xline));
                    gfx.DrawString(item.AnioDesde.ToString() + "-" + item.AnioHasta.ToString(), new XFont("Arial", 8, XFontStyleEx.Regular), XBrushes.Black, new XPoint(155, xline));
                    gfx.DrawString(item.NombrePropietario ?? "Vacio", new XFont("Arial", 8, XFontStyleEx.Regular), XBrushes.Black, new XPoint(200, xline));
                    rect = new XRect(330, xline - 7, 55, 10);
                    gfx.DrawRectangle(XBrushes.White, rect);
                    tf.Alignment = XParagraphAlignment.Right;
                    tf.DrawString(item.ValorImpuesto.ToString("###,###,###"), font, XBrushes.Black, rect, XStringFormats.TopLeft);
                    rect = new XRect(390, xline - 7, 40, 10);
                    gfx.DrawRectangle(XBrushes.White, rect);
                    tf.Alignment = XParagraphAlignment.Right;
                    tf.DrawString(item.ValorCarga.ToString("###,###,###"), font, XBrushes.Black, rect, XStringFormats.TopLeft);
                    rect = new XRect(435, xline - 7, 45, 10);
                    gfx.DrawRectangle(XBrushes.White, rect);
                    tf.Alignment = XParagraphAlignment.Right;
                    tf.DrawString(item.ValorCostas.ToString("###,###,###"), font, XBrushes.Black, rect, XStringFormats.TopLeft);
                    rect = new XRect(486, xline - 7, 45, 10);
                    gfx.DrawRectangle(XBrushes.White, rect);
                    tf.Alignment = XParagraphAlignment.Right;
                    tf.DrawString(item.ValorEstampillas.ToString("###,###,###"), font, XBrushes.Black, rect, XStringFormats.TopLeft);
                    rect = new XRect(535, xline - 7, 45, 10);
                    gfx.DrawRectangle(XBrushes.White, rect);
                    tf.Alignment = XParagraphAlignment.Right;
                    tf.DrawString(item.ValorSistematizacion.ToString("###,###,###"), font, XBrushes.Black, rect, XStringFormats.TopLeft);
                    rect = new XRect(585, xline - 7, 45, 10);
                    gfx.DrawRectangle(XBrushes.White, rect);
                    tf.Alignment = XParagraphAlignment.Right;
                    tf.DrawString(item.ValorSancion.ToString("##,###,###"), font, XBrushes.Black, rect, XStringFormats.TopLeft);
                    rect = new XRect(634, xline - 7, 40, 10);
                    gfx.DrawRectangle(XBrushes.White, rect);
                    tf.Alignment = XParagraphAlignment.Right;
                    tf.DrawString(item.ValorInteres.ToString("##,###,###"), font, XBrushes.Black, rect, XStringFormats.TopLeft);
                    rect = new XRect(680, xline - 7, 5, 10);
                    gfx.DrawRectangle(XBrushes.White, rect);
                    tf.Alignment = XParagraphAlignment.Right;
                    tf.DrawString(item.Descuento.ToString("##,###,###"), font, XBrushes.Black, rect, XStringFormats.TopLeft);
                    rect = new XRect(738, xline - 7, 45, 10);
                    gfx.DrawRectangle(XBrushes.White, rect);
                    tf.Alignment = XParagraphAlignment.Right;
                    tf.DrawString(item.ValorTotal.ToString("###,###,###"), font, XBrushes.Black, rect, XStringFormats.TopLeft);
                    t1 = (decimal)(t1 + item.ValorImpuesto);
                    t2 = (decimal)(t2 + item.ValorCarga);
                    t5 = (decimal)(t5 + item.ValorSistematizacion);
                    t3 = (decimal)(t3 + item.ValorCostas);
                    t4 = (decimal)(t4 + item.ValorEstampillas);
                    t6 = (decimal)(t6 + item.ValorSancion);
                    t7 = (decimal)(t7 + item.ValorInteres);
                    t8 = (decimal)(t8 + item.Descuento);
                    t9 = (decimal)(t9 + item.ValorTotal);
                    cl = cl + 1;
                }
                gfx.DrawLine(new XPen(XColor.FromArgb(32, 32, 36)), new XPoint(40, xline + 5), new XPoint(784, xline + 5));
                gfx.DrawString("Total Pagos: " + recibos.Count.ToString(), new XFont("Arial", 8, XFontStyleEx.Bold), XBrushes.Black, new XPoint(40, xline + 14));
                rect = new XRect(320, xline + 8, 65, 10);
                gfx.DrawRectangle(XBrushes.White, rect);
                tf.Alignment = XParagraphAlignment.Right;
                tf.DrawString(t1.ToString("###,###,###"), font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new XRect(387, xline + 8, 45, 10);
                gfx.DrawRectangle(XBrushes.White, rect);
                tf.Alignment = XParagraphAlignment.Right;
                tf.DrawString(t2.ToString("###,###,###"), font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new XRect(435, xline + 8, 45, 10);
                gfx.DrawRectangle(XBrushes.White, rect);
                tf.Alignment = XParagraphAlignment.Right;
                tf.DrawString(t3.ToString("##,###,###"), font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new XRect(486, xline + 8, 45, 10);
                gfx.DrawRectangle(XBrushes.White, rect);
                tf.Alignment = XParagraphAlignment.Right;
                tf.DrawString(t4.ToString("##,###,###"), font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new XRect(536, xline + 8, 45, 10);
                gfx.DrawRectangle(XBrushes.White, rect);
                tf.Alignment = XParagraphAlignment.Right;
                tf.DrawString(t5.ToString("##,###,###"), font, XBrushes.Black, rect, XStringFormats.TopLeft);
                rect = new XRect(585, xline + 8, 45, 10);
                gfx.DrawRectangle(XBrushes.White, rect);
                tf.Alignment = XParagraphAlignment.Right;
                tf.DrawString(t6.ToString("##,###,###"), font, XBrushes.Black, rect, XStringFormats.TopLeft);
                rect = new XRect(634, xline + 8, 45, 10);
                gfx.DrawRectangle(XBrushes.White, rect);
                tf.Alignment = XParagraphAlignment.Right;
                tf.DrawString(t7.ToString("##,###,###"), font, XBrushes.Black, rect, XStringFormats.TopLeft);
                rect = new XRect(681, xline + 8, 45, 10);
                gfx.DrawRectangle(XBrushes.White, rect);
                tf.Alignment = XParagraphAlignment.Right;
                tf.DrawString(t8.ToString("##,###,###"), font, XBrushes.Black, rect, XStringFormats.TopLeft);
                rect = new XRect(730, xline + 8, 55, 10);
                gfx.DrawRectangle(XBrushes.White, rect);
                tf.Alignment = XParagraphAlignment.Right;
                tf.DrawString(t9.ToString("###,###,###"), font, XBrushes.Black, rect, XStringFormats.TopLeft);
                doc.Save(filename);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task MdtoPdf(List<Proceso> mdtos)
        {
            try
            {
                string carpeta = "/var/www/velez/pdf/";
                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);
                string filename = System.IO.Path.Combine(carpeta, $"Mandamiento.pdf");
                PdfDocument doc = new PdfDocument();
                doc.Info.Title = "Mandamiento_pago";
                string texto1;
                int vlt = 0;
                var fullName = "wwwroot/logos/logo1_Velez.jpg";
                var image = XImage.FromFile(fullName);
                fullName = "wwwroot/logos/logo2_Velez.jpg";
                var image2 = XImage.FromFile(fullName);
                fullName = "wwwroot/logos/firma.jpg";
                var firma = XImage.FromFile(fullName);
                string pie = "Dirección: Calle 9 N° 2-37 - Peticiones, Quejas y Reclamos - Correo electrónico:alcaldia@vélez-santander.gov.co - Página Web: www.velez-santander.gov.co";
                foreach (var item in mdtos)
                {
                    PdfPage page = doc.AddPage();
                    XGraphics gfx = XGraphics.FromPdfPage(page);
                    XTextFormatter tf = new XTextFormatter(gfx);
                    XRect rect = new XRect(40, 90, 250, 550);
                    XFont font = new XFont("Verdana", 8);
                    gfx.DrawImage(image, 43, 19, 72, 60);
                    gfx.DrawImage(image2, 480, 19, 72, 60);
                    gfx.DrawLine(new XPen(XColor.FromArgb(32, 32, 36)), new XPoint(40, 80), new XPoint(562, 80));
                    gfx.DrawString("ALCALDÍA MUNICIPAL DE VÉLEZ", new XFont("Arial", 9, XFontStyleEx.Bold), XBrushes.Black, new XRect(0, 0, page.Width, 55), XStringFormat.Center);
                    gfx.DrawString("SECRETARIA DE TRANSITO", new XFont("Arial", 9, XFontStyleEx.Bold), XBrushes.Black, new XRect(0, 0, page.Width, 80), XStringFormat.Center);
                    gfx.DrawString("Velez, Santander " + item.FechaMandamiento.ToShortDateString(), new XFont("Arial", 8, XFontStyleEx.Regular), XBrushes.Black, new XPoint(40, 90));
                    gfx.DrawString("Señor(a)", new XFont("Arial", 9, XFontStyleEx.Regular), XBrushes.Black, new XPoint(40, 104));
                    gfx.DrawString(item.Vehiculo.Propietario.Nombre, new XFont("Arial", 9, XFontStyleEx.Bold), XBrushes.Black, new XPoint(40, 114));
                    gfx.DrawString(item.Vehiculo.Propietario.Direccion, new XFont("Arial", 9, XFontStyleEx.Regular), XBrushes.Black, new XPoint(40, 124));
                    gfx.DrawString(item.Vehiculo.Propietario.Documento, new XFont("Arial", 9, XFontStyleEx.Regular), XBrushes.Black, new XPoint(40, 134));
                    var pen = new XPen(XColors.Black, 0.5);
                    gfx.DrawRectangle(pen, 436, 86, 125, 29);
                    texto1 = "Si ya cancelo hacer caso omiso\r\nal presente acto administrativo";
                    rect = new XRect(442, 90, 113, 22);
                    gfx.DrawRectangle(XBrushes.White, rect);
                    tf.Alignment = XParagraphAlignment.Justify;
                    tf.DrawString(texto1, new XFont("Verdana", 7), XBrushes.Black, rect, XStringFormats.TopLeft);
                    vlt = (int)item.Valor;
                    gfx.DrawString("MANDAMIENTO DE PAGO No. " + item.Resolucion, new XFont("Arial", 9, XFontStyleEx.Bold), XBrushes.Black, new XRect(0, 0, page.Width, 290), XStringFormat.Center);
                    texto1 =
                        "El suscrito Secretario de Hacienda de Velez - Santander, en uso de sus Facultades Legales, en especial las consagradas en la Ley 1066 del 29 de Julio de 2006, el Decreto 4473 del 15 de Diciembre de 2006, por el cual se reglamenta la ley 1066 de 2006 y";
                    rect = new XRect(40, 155, 520, 55);
                    gfx.DrawRectangle(XBrushes.White, rect); // UZN175 VACIO LA CC
                    tf.Alignment = XParagraphAlignment.Justify;
                    tf.DrawString(texto1, font, XBrushes.Black, rect, XStringFormats.TopLeft);
                    gfx.DrawString("CONSIDERANDO", new XFont("Arial", 9, XFontStyleEx.Bold), XBrushes.Black, new XRect(0, 0, page.Width, 388), XStringFormat.Center);
                    texto1 =
                        "Al tenor de lo dispuesto en el Artículo 52 de la Ley 336 de 1996 en concordancia con la ley 105 de 1993, los entes territoriales tienen jurisdicción de cobro coactivo situación que lo contempla la ley 1066 del 2006 en su artículo 5. \r\n \r\nQue de Conformidad " +
                        "con el Artículo 828 numeral 2 del Estatuto Tributario Nacional en concordancia con el Artículo 99 numeral 1 del Código de Procedimiento Administrativo y de lo contencioso Administrativo, todo acto administrativo ejecutoriado como lo es una liquidación oficial" +
                        "constituye título ejecutivo que presta mérito para su ejecución a través del Cobro Coactivo, por constar en él una obligación clara, expresa y actualmente exigible a favor de la Secretaria de Transito y Transporte de Velez - Santander. \r\n \r\nQue mediante acto " +
                        " administrativo No. LQ-" + item.Id + " del " + item.FechaMandamiento.ToShortDateString() + ", se liquida oficialmente los derechos de transito sobre el automotor de placas " +
                        item.Vehiculo.Placa + " de propiedad de " + item.Vehiculo.Propietario.Nombre + " identificado con la cc " + item.Vehiculo.Propietario.Documento + " de las vigencias " + item.Desde + " a " + item.Hasta +
                        " , en la cual consta una obligación " +
                        "clara, expresa y actualmente Exigible a favor de la Secretaria de Transito y Transporte de Velez - Santander, por valor de " + item.Valor.ToString("C") +
                        ", más los intereses causados y costas procesales que se causen hasta el momento en que se realice el pago total de la Obligación. " +
                        "Si bien es cierto, la obligacion esta a favor de la Secretaria de Transito de Velez, dicho Despacho ha remitido el titulo ejecutivo para su respectivo cobro coactivo a esta Secretaria de Hacienda, que tiene facultades de cobro coactivo. \r\n \r\nQue por lo anteriormente" +
                        " expuesto y con fundamento e n los Artículos 5 de la ley 1066 y Artículos 825 -1, 826 y S.S. del Título V III del Estatuto Tributario y demás Normas concordantes, el suscrito Secretario de Hacienda de Velez - Santander en uso de las funciones contenidas. \r\n \r\nPor lo antes expuesto";
                    rect = new XRect(40, 210, 520, 217);
                    gfx.DrawRectangle(XBrushes.White, rect);
                    tf.Alignment = XParagraphAlignment.Justify;
                    tf.DrawString(texto1, font, XBrushes.Black, rect, XStringFormats.TopLeft);
                    gfx.DrawString("RESUELVE", new XFont("Arial", 9, XFontStyleEx.Bold), XBrushes.Black, new XRect(0, 0, page.Width, 850), XStringFormat.Center);
                    texto1 =
                        "ARTICULO PRIMERO: Librar Mandamiento de Pago por la Vía Administrativa Coactiva a favor de la secretaria de hacienda de velez - Santander, y a cargo del Señor(a ) " +
                        item.Vehiculo.Propietario.Nombre + ", por la cuantía de " + item.Valor.ToString("C") + ", de conformidad con lo expuesto en la parte motiva, más los " +
                        "intereses y actualizaciones que se causen desde cuando se hizo exigible la obligación y hasta cuando s e cancele, más las costas ocasionadas en el presente proceso.\r\n \r\nARTICULO SEGUNDO: Notificar este Mandamiento de Pago personalmente al ejecutado, su apoderado o representante " +
                        "legal, de conformidad con el art. 8 de la ley 2213 del 2022, es decir no se requiere citación previa para la notificación del mandamiento de pago.\r\n \r\nARTICULO TERCERO: Advertir que dispone de los quince (15) días siguientes a la notificación de esta providencia para cancelar el monto de " +
                        " la deuda con sus respectivos intereses o proponer excepciones de conformidad con lo establecido en los artículos 830 y 8 31 del Estatuto Tributario Nacional.\r\n \r\nARTICULO CUARTO: ORDENESE las medidas cautelares a que haya lugar con el objeto de garantizar el pago de la obligación. Ofíciese " +
                        "la iinvestigación de bienes y los embargos pertinentes.\r\n \r\nDada en Velez - Santander. " + item.FechaMandamiento.ToShortDateString();
                    rect = new XRect(40, 450, 520, 350);
                    gfx.DrawRectangle(XBrushes.White, rect);
                    tf.Alignment = XParagraphAlignment.Justify;
                    tf.DrawString(texto1, font, XBrushes.Black, rect, XStringFormats.TopLeft);

                    gfx.DrawImage(firma, 83, 670, 300, 110);
                    rect = new XRect(55, 787, 490, 33);
                    gfx.DrawRectangle(XBrushes.White, rect);
                    tf.Alignment = XParagraphAlignment.Center;
                    tf.DrawString(pie, new XFont("Verdana", 8), XBrushes.Black, rect, XStringFormats.TopLeft);
                }
                doc.Save(filename);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task PazySalvo(EstadoCuentaVehiculoDto item, Parametro param)
        {
            try
            {
                string carpeta = "/var/www/velez/pdf/";
                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);
                string filename = System.IO.Path.Combine(carpeta, $"PazySalvo_{item.Placa}.pdf");
                PdfDocument doc = new PdfDocument();
                doc.Info.Title = "Paz y Salvo";
                string texto2 = "";
                DateTime Fec = DateTime.UtcNow;
                PdfPage page = doc.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);
                XTextFormatter tf = new XTextFormatter(gfx);
                XRect rect = new XRect(40, 90, 250, 550);
                XFont font = new XFont("Arial", 12);
                var fullName = "wwwroot/logos/logo1_Velez.jpg";
                var image = XImage.FromFile(fullName);
                texto2 = "Que una vez revisado el expediente físico del vehículo automotor de placas " + item.Placa + " servicio " + item.TipoServicio + " de propiedad del señor(a): " +
                         item.NombrePropietario + " identificado con No. documento " + item.Documento + " se informa que se encuentra a paz y salvo " +
                         "por concepto de derecho anual de placa con corte al 31 de diciembre de " + Fec.Year.ToString() + ".";
                string texto1 = "REPUBLICA DE COLOMBIA \r\n MUNICIPIO DE " + param.Ciudad + "\r\n" + param.Nombre + " \r\n NIT " + param.Nit;
                var pen = new XPen(XColors.Black, 0.5);
                gfx.DrawRectangle(pen, 40, 40, 507, 74);
                gfx.DrawLine(new XPen(XColor.FromArgb(32, 32, 36)), new XPoint(118, 40), new XPoint(118, 114));
                gfx.DrawLine(new XPen(XColor.FromArgb(32, 32, 36)), new XPoint(467, 40), new XPoint(467, 114));
                gfx.DrawImage(image, 43, 43, 72, 60);
                gfx.DrawImage(image, 469, 43, 72, 60);
                rect = new XRect(120, 47, 345, 60);
                gfx.DrawRectangle(XBrushes.White, rect);
                tf.Alignment = XParagraphAlignment.Center;
                tf.DrawString(texto1, new XFont("Arial", 12, XFontStyleEx.Bold), XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new XRect(40, 140, 507, 47);
                gfx.DrawRectangle(XBrushes.White, rect);
                tf.Alignment = XParagraphAlignment.Center;
                tf.DrawString("PAZ Y SALVO", new XFont("Times New Roman", 24, XFontStyleEx.Bold), XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new XRect(40, 200, 507, 47);
                gfx.DrawRectangle(XBrushes.White, rect);
                tf.Alignment = XParagraphAlignment.Center;
                tf.DrawString("EL SUSCRITO SECRETARIO DE TRÁNSITO Y TRANSPORTE DE " + param.Ciudad.ToUpper(), new XFont("Times New Roman", 12, XFontStyleEx.Bold), XBrushes.Black, rect,
                    XStringFormats.TopLeft);

                rect = new XRect(40, 260, 510, 47);
                gfx.DrawRectangle(XBrushes.White, rect);
                tf.Alignment = XParagraphAlignment.Center;
                tf.DrawString("INFORMA:", new XFont("Times New Roman", 24, XFontStyleEx.Bold), XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new XRect(40, 320, 507, 87);
                gfx.DrawRectangle(XBrushes.White, rect);
                tf.Alignment = XParagraphAlignment.Justify;
                tf.DrawString(texto2, new XFont("Arial", 14, XFontStyleEx.Regular), XBrushes.Black, rect, XStringFormats.TopLeft);
                gfx.DrawString("Se expide la presente en Vélez, a los " + DateTime.UtcNow.Day + ".", new XFont("Arial", 12, XFontStyleEx.Regular), XBrushes.Black,
                    new XPoint(40, 480));
                gfx.DrawString(param.NombreSecretario, new XFont("Arial", 12, XFontStyleEx.Bold), XBrushes.Black, new XPoint(40, 640));
                gfx.DrawString(param.CargoSecretario, new XFont("Arial", 12, XFontStyleEx.Regular), XBrushes.Black, new XPoint(40, 652));
                gfx.DrawString(param.Ciudad.ToLower(), new XFont("Arial", 12, XFontStyleEx.Regular), XBrushes.Black, new XPoint(40, 664));
                gfx.DrawString(param.Direccion + " - Tel: " + param.Telefono + "  -  Correo electrónico: " + param.Correo, new XFont("Arial", 9, XFontStyleEx.Regular), XBrushes.Black,
                    new XPoint(40, 810));
                doc.Save(filename);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generando PDF: {ex.Message}");
            }
        }
    }
}