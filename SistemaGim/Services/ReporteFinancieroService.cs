using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using SistemaGim.Models;

namespace SistemaGim.Services
{
    public class ReporteFinancieroService
    {
        public void GenerarPdf(string rutaArchivo, DateTime inicio, DateTime fin, List<ReporteIngresoDto> datos)
        {
            using (PdfWriter writer = new PdfWriter(rutaArchivo))
            using (PdfDocument pdf = new PdfDocument(writer))
            using (Document document = new Document(pdf))
            {
                // Título
                document.Add(new Paragraph("REPORTE FINANCIERO Y DE GANANCIAS")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(18).SetBold());

                document.Add(new Paragraph($"Período: {inicio:dd/MM/yyyy} al {fin:dd/MM/yyyy}")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(12).SetMarginBottom(20));

                // Crear Tabla de 4 columnas
                Table table = new Table(new float[] { 2, 4, 3, 2 }).UseAllAvailableWidth();

                // Encabezados con estilo
                Color colorEncabezado = ColorConstants.LIGHT_GRAY;
                table.AddHeaderCell(new Cell().Add(new Paragraph("Fecha").SetBold()).SetBackgroundColor(colorEncabezado));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Cliente").SetBold()).SetBackgroundColor(colorEncabezado));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Concepto").SetBold()).SetBackgroundColor(colorEncabezado));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Monto (C$)").SetBold()).SetBackgroundColor(colorEncabezado));

                decimal total = 0;

                // Llenar datos
                foreach (var item in datos)
                {
                    table.AddCell(new Cell().Add(new Paragraph(item.Fecha.ToString("dd/MM/yyyy"))));
                    table.AddCell(new Cell().Add(new Paragraph(item.Cliente)));
                    table.AddCell(new Cell().Add(new Paragraph(item.Concepto)));
                    table.AddCell(new Cell().Add(new Paragraph($"C$ {item.Monto:N2}")).SetTextAlignment(TextAlignment.RIGHT));
                    total += item.Monto;
                }

                // Fila de Total
                table.AddCell(new Cell(1, 3).Add(new Paragraph("TOTAL DE GANANCIAS:").SetBold()).SetTextAlignment(TextAlignment.RIGHT));
                table.AddCell(new Cell().Add(new Paragraph($"C$ {total:N2}").SetBold()).SetTextAlignment(TextAlignment.RIGHT).SetBackgroundColor(ColorConstants.YELLOW));

                document.Add(table);
            }
        }

        public void GenerarExcel(string rutaArchivo, List<ReporteIngresoDto> datos)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Estado de Ganancias");
                worksheet.Cell(1, 1).Value = "Fecha";
                worksheet.Cell(1, 2).Value = "Cliente";
                worksheet.Cell(1, 3).Value = "Concepto";
                worksheet.Cell(1, 4).Value = "Monto (C$)";

                var rangoEncabezado = worksheet.Range("A1:D1");
                rangoEncabezado.Style.Font.Bold = true;
                rangoEncabezado.Style.Fill.BackgroundColor = XLColor.LightGray;

                int fila = 2;
                foreach (var item in datos)
                {
                    worksheet.Cell(fila, 1).Value = item.Fecha.ToString("dd/MM/yyyy");
                    worksheet.Cell(fila, 2).Value = item.Cliente;
                    worksheet.Cell(fila, 3).Value = item.Concepto;
                    worksheet.Cell(fila, 4).Value = item.Monto;
                    fila++;
                }

                worksheet.Cell(fila, 3).Value = "TOTAL:";
                worksheet.Cell(fila, 3).Style.Font.Bold = true;
                worksheet.Cell(fila, 4).FormulaA1 = $"=SUM(D2:D{fila - 1})";
                worksheet.Cell(fila, 4).Style.Font.Bold = true;

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(rutaArchivo);
            }
        }
    }

   
}