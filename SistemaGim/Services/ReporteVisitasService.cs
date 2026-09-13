using System;
using System.Collections.Generic;
using ClosedXML.Excel;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using SistemaGim.Models;

namespace SistemaGim.Services
{
    public class ReporteVisitasService
    {
        public void GenerarPdf(string rutaArchivo, DateTime inicio, DateTime fin, List<ReporteVisitaDto> datos)
        {
            using (PdfWriter writer = new PdfWriter(rutaArchivo))
            using (PdfDocument pdf = new PdfDocument(writer))
            using (Document document = new Document(pdf))
            {
                document.Add(new Paragraph("REPORTE ESTADÍSTICO DE VISITAS")
                    .SetTextAlignment(TextAlignment.CENTER).SetFontSize(18).SetBold());
                document.Add(new Paragraph($"Período: {inicio:dd/MM/yyyy} al {fin:dd/MM/yyyy}")
                    .SetTextAlignment(TextAlignment.CENTER).SetFontSize(12).SetMarginBottom(20));

                Table table = new Table(new float[] { 2, 2, 4, 4 }).UseAllAvailableWidth();

                table.AddHeaderCell(new Cell().Add(new Paragraph("Fecha").SetBold()).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Hora").SetBold()).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Cliente").SetBold()).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Pase / Membresía").SetBold()).SetBackgroundColor(ColorConstants.LIGHT_GRAY));

                foreach (var item in datos)
                {
                    table.AddCell(new Cell().Add(new Paragraph(item.FechaHora.ToString("dd/MM/yyyy"))));
                    table.AddCell(new Cell().Add(new Paragraph(item.FechaHora.ToString("hh:mm tt"))));
                    table.AddCell(new Cell().Add(new Paragraph(item.Cliente)));
                    table.AddCell(new Cell().Add(new Paragraph(item.Concepto)));
                }

                document.Add(table);
                document.Add(new Paragraph($"Total de Asistencias en el período: {datos.Count}")
                    .SetBold().SetMarginTop(15));
            }
        }

        public void GenerarExcel(string rutaArchivo, List<ReporteVisitaDto> datos)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Visitas");
                worksheet.Cell(1, 1).Value = "Fecha";
                worksheet.Cell(1, 2).Value = "Hora";
                worksheet.Cell(1, 3).Value = "Cliente";
                worksheet.Cell(1, 4).Value = "Concepto";

                var rangoEncabezado = worksheet.Range("A1:D1");
                rangoEncabezado.Style.Font.Bold = true;
                rangoEncabezado.Style.Fill.BackgroundColor = XLColor.LightGray;

                int fila = 2;
                foreach (var item in datos)
                {
                    worksheet.Cell(fila, 1).Value = item.FechaHora.ToString("dd/MM/yyyy");
                    worksheet.Cell(fila, 2).Value = item.FechaHora.ToString("hh:mm tt");
                    worksheet.Cell(fila, 3).Value = item.Cliente;
                    worksheet.Cell(fila, 4).Value = item.Concepto;
                    fila++;
                }

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(rutaArchivo);
            }
        }
    }


}