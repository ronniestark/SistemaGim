using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using SistemaGim.Data;
using SistemaGim.Models;
using SistemaGim.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SistemaGim.Views
{
    public partial class ReportesView : UserControl
    {
        private List<ReporteIngresoDto> _datosIngresos = new List<ReporteIngresoDto>();
        private List<ReporteVisitaDto> _datosVisitas = new List<ReporteVisitaDto>();

        // Ruta del archivo temporal para el IFrame
        private string _rutaPdfTemporal = string.Empty;

        public ReportesView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CmbTipoReporte.SelectedIndex = 0;
            DpInicio.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DpFin.SelectedDate = DateTime.Now.Date;
        }

        private void CmbTipoReporte_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Limpiar visor al cambiar de reporte
            if (PdfViewer.CoreWebView2 != null)
            {
                PdfViewer.CoreWebView2.Navigate("about:blank");
            }
        }

        // 1. IMPORTANTE: Agregar la palabra 'async' aquí
        private async void BtnGenerarVista_Click(object sender, RoutedEventArgs e)
        {
            if (DpInicio.SelectedDate == null || DpFin.SelectedDate == null) return;

            DateTime inicio = DpInicio.SelectedDate.Value.Date;
            DateTime fin = DpFin.SelectedDate.Value.Date.AddDays(1).AddTicks(-1);

            using var context = new SistemaGimDbContext(); // Asegúrate de que este sea el nombre correcto de tu DbContext

            // Definir una ruta temporal única en la carpeta Temp de Windows
            _rutaPdfTemporal = Path.Combine(Path.GetTempPath(), $"PreviewReporte_{Guid.NewGuid()}.pdf");

            try
            {
                if (CmbTipoReporte.SelectedIndex == 0) // GANANCIAS
                {
                    _datosIngresos = context.Pagos
                        .Include(p => p.Cliente)
                        .Where(p => p.FechaTransaccion >= inicio && p.FechaTransaccion <= fin)
                        .OrderBy(p => p.FechaTransaccion)
                        .Select(p => new ReporteIngresoDto
                        {
                            Fecha = p.FechaTransaccion,
                            Cliente = p.Cliente != null ? p.Cliente.NombreCompleto : "Cliente General",
                            Concepto = p.Concepto,
                            Monto = p.Monto
                        }).ToList();

                    if (!_datosIngresos.Any())
                    {
                        MessageBox.Show("No se encontraron ingresos en estas fechas.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    // Generar PDF en la ruta temporal
                    var service = new ReporteFinancieroService();
                    service.GenerarPdf(_rutaPdfTemporal, inicio, fin, _datosIngresos);
                }
                else // VISITAS
                {
                    _datosVisitas = context.Asistencias
                        .Include(a => a.Cliente)
                        .Include(a => a.Pago)
                        .Where(a => a.FechaHora >= inicio && a.FechaHora <= fin)
                        .OrderBy(a => a.FechaHora)
                        .Select(a => new ReporteVisitaDto
                        {
                            FechaHora = a.FechaHora,
                            Cliente = a.Cliente != null ? a.Cliente.NombreCompleto : "Desconocido",
                            Concepto = a.Pago != null ? a.Pago.Concepto : "Entrada libre"
                        }).ToList();

                    if (!_datosVisitas.Any())
                    {
                        MessageBox.Show("No se encontraron visitas en estas fechas.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    // Generar PDF en la ruta temporal
                    var service = new ReporteVisitasService();
                    service.GenerarPdf(_rutaPdfTemporal, inicio, fin, _datosVisitas);
                }

                // 2. IMPORTANTE: Estas líneas cargan el PDF en el visor de la pantalla
                await PdfViewer.EnsureCoreWebView2Async(null);
                PdfViewer.CoreWebView2.Navigate(new Uri(_rutaPdfTemporal).AbsoluteUri);
            }
            catch (Exception ex)
            {
                string detalleError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show($"Error principal: {ex.Message}\n\nError interno: {detalleError}\n\nTraza:\n{ex.StackTrace}",
                                "Error Detallado", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportarPdf_Click(object sender, RoutedEventArgs e)
        {
            if (!HayDatosCargados()) return;

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Archivos PDF (*.pdf)|*.pdf",
                FileName = $"Reporte_{CmbTipoReporte.Text.Replace(" ", "")}_{DateTime.Now:yyyyMMdd}.pdf"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    // Si ya previsualizamos, el archivo ya existe en _rutaPdfTemporal. Solo lo copiamos.
                    if (File.Exists(_rutaPdfTemporal))
                    {
                        File.Copy(_rutaPdfTemporal, sfd.FileName, true);
                    }
                    else
                    {
                        // Fallback: Regenerarlo por si acaso
                        DateTime inicio = DpInicio.SelectedDate.Value;
                        DateTime fin = DpFin.SelectedDate.Value;

                        if (CmbTipoReporte.SelectedIndex == 0)
                            new ReporteFinancieroService().GenerarPdf(sfd.FileName, inicio, fin, _datosIngresos);
                        else
                            new ReporteVisitasService().GenerarPdf(sfd.FileName, inicio, fin, _datosVisitas);
                    }

                    MessageBox.Show("PDF exportado con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al exportar PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            if (!HayDatosCargados()) return;

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Archivos Excel (*.xlsx)|*.xlsx",
                FileName = $"Reporte_{CmbTipoReporte.Text.Replace(" ", "")}_{DateTime.Now:yyyyMMdd}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    if (CmbTipoReporte.SelectedIndex == 0)
                        new ReporteFinancieroService().GenerarExcel(sfd.FileName, _datosIngresos);
                    else
                        new ReporteVisitasService().GenerarExcel(sfd.FileName, _datosVisitas);

                    MessageBox.Show("Excel exportado con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al exportar Excel: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool HayDatosCargados()
        {
            if ((CmbTipoReporte.SelectedIndex == 0 && !_datosIngresos.Any()) ||
                (CmbTipoReporte.SelectedIndex == 1 && !_datosVisitas.Any()))
            {
                MessageBox.Show("Primero dé clic en 'PREVISUALIZAR' para cargar los datos antes de descargar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }


    }
}