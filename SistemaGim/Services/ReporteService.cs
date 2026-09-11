using System.IO;
using ClosedXML.Excel;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.Collections.Generic;
using System;

namespace SistemaGim.Services
{
    public class ReportService
    {
        // === GENERADOR DE EXCEL (ClosedXML) ===
        public void GenerarReporteIngresosExcel(string rutaArchivo, List<PagoDto> pagos)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Ingresos");

                // Cabeceras
                worksheet.Cell(1, 1).Value = "ID Pago";
                worksheet.Cell(1, 2).Value = "Fecha";
                worksheet.Cell(1, 3).Value = "Concepto";
                worksheet.Cell(1, 4).Value = "Monto (C$)";

                // Datos
                int fila = 2;
                foreach (var pago in pagos)
                {
                    worksheet.Cell(fila, 1).Value = pago.PagoId;
                    worksheet.Cell(fila, 2).Value = pago.FechaTransaccion.ToString("dd/MM/yyyy");
                    worksheet.Cell(fila, 3).Value = pago.Concepto;
                    worksheet.Cell(fila, 4).Value = pago.Monto;
                    fila++;
                }

                // Estilo a la tabla
                var rango = worksheet.Range(1, 1, fila - 1, 4);
                rango.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                worksheet.Columns().AdjustToContents();

                workbook.SaveAs(rutaArchivo);
            }
        }

        // === GENERADOR DE PDF (iText 7) ===
        public void GenerarTicketPdf(string rutaArchivo, string cliente, string concepto, decimal monto)
        {
            using (PdfWriter writer = new PdfWriter(rutaArchivo))
            using (PdfDocument pdf = new PdfDocument(writer))
            using (Document document = new Document(pdf))
            {
                document.Add(new Paragraph("TICKET DE PAGO - GIMNASIO")
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetFontSize(16));

                document.Add(new Paragraph($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}"));
                document.Add(new Paragraph($"Cliente/Pase: {cliente}"));
                document.Add(new Paragraph($"Concepto: {concepto}"));
                document.Add(new Paragraph($"Total Pagado: C$ {monto}"));

                document.Add(new Paragraph("¡Gracias por su visita!")
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetMarginTop(20));
            }
        }
    }

    public class PagoDto
    {
        public int PagoId { get; set; }
        public DateTime FechaTransaccion { get; set; }
        public string Concepto { get; set; }
        public decimal Monto { get; set; }
    }
}
