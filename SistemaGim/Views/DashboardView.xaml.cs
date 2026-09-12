using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using SistemaGim.Data;

namespace SistemaGim.Views
{
    public partial class DashboardView : UserControl
    {
        // Formateador para el eje Y del gráfico de barras
        public Func<double, string> FormatoMoneda { get; set; } = value => $"C$ {value:N0}";

        public DashboardView()
        {
            InitializeComponent();
            DataContext = this; // Necesario para que funcione el FormatoMoneda en el XAML
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarDatosAsync();
        }

        private async void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            await CargarDatosAsync();
        }

        private async Task CargarDatosAsync()
        {
            BtnActualizar.IsEnabled = false;

            try
            {
                using var context = new SistemaGimDbContext();
                DateTime hoy = DateTime.Now.Date;
                DateTime inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
                DateTime hace7Dias = hoy.AddDays(-6); // Hoy + 6 días atrás = 7 días

                // 1. CARGAR TARJETAS (KPIs)
                int visitasHoy = await context.Asistencias.CountAsync(a => a.FechaHora >= hoy);
                int visitasMes = await context.Asistencias.CountAsync(a => a.FechaHora >= inicioMes);

                decimal pagosHoy = await context.Pagos.Where(p => p.FechaTransaccion >= hoy).SumAsync(p => (decimal?)p.Monto) ?? 0m;
                decimal pagosMes = await context.Pagos.Where(p => p.FechaTransaccion >= inicioMes).SumAsync(p => (decimal?)p.Monto) ?? 0m;

                TxtVisitasHoy.Text = visitasHoy.ToString();
                TxtVisitasMes.Text = visitasMes.ToString();
                TxtPagosHoy.Text = $"C$ {pagosHoy:N2}";
                TxtPagosMes.Text = $"C$ {pagosMes:N2}";

                // 2. CARGAR GRÁFICO DE BARRAS (Últimos 7 días)
                // Obtenemos los pagos de los últimos 7 días y los agrupamos por fecha
                var pagos7Dias = await context.Pagos
                    .Where(p => p.FechaTransaccion >= hace7Dias)
                    .GroupBy(p => p.FechaTransaccion.Date)
                    .Select(g => new { Fecha = g.Key, Total = g.Sum(p => p.Monto) })
                    .ToListAsync();

                // Construir los ejes y las barras
                var valoresBarras = new ChartValues<decimal>();
                var etiquetasFechas = new List<string>();

                for (int i = 0; i <= 6; i++)
                {
                    DateTime fechaActual = hace7Dias.AddDays(i);
                    etiquetasFechas.Add(fechaActual.ToString("dd/MMM"));

                    var pagoDelDia = pagos7Dias.FirstOrDefault(p => p.Fecha == fechaActual);
                    valoresBarras.Add(pagoDelDia != null ? pagoDelDia.Total : 0m);
                }

                GraficoBarras.Series = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Ingresos",
                        Values = valoresBarras,
                        Fill = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#4CAF50")
                    }
                };
                EjeFechas.Labels = etiquetasFechas;

                // 3. CARGAR GRÁFICO DE PASTEL (Ingresos del Mes por Concepto)
                var ingresosPorConcepto = await context.Pagos
                    .Where(p => p.FechaTransaccion >= inicioMes)
                    .GroupBy(p => p.Concepto)
                    .Select(g => new { Concepto = g.Key, Total = g.Sum(x => x.Monto) })
                    .ToListAsync();

                var seriesPastel = new SeriesCollection();
                foreach (var item in ingresosPorConcepto)
                {
                    seriesPastel.Add(new PieSeries
                    {
                        Title = item.Concepto,
                        Values = new ChartValues<decimal> { item.Total },
                        DataLabels = true,
                        LabelPoint = chartPoint => $"{chartPoint.Y:N0} C$ ({chartPoint.Participation:P0})" // Muestra el monto y el %
                    });
                }

                GraficoPastel.Series = seriesPastel;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el dashboard:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnActualizar.IsEnabled = true;
            }
        }
    }
}