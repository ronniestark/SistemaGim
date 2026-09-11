using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using SistemaGim.Data; // Importar tu DbContext

namespace SistemaGim.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        // Evento que se ejecuta automáticamente cuando se abre la pantalla
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarDatosAsync();
        }

        // Evento del botón "Actualizar Datos"
        private async void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            await CargarDatosAsync();
        }

        // Método principal para consultar SQL y llenar los TextBlocks
        private async Task CargarDatosAsync()
        {
            // Deshabilitar botón temporalmente para que no den muchos clics seguidos
            BtnActualizar.IsEnabled = false;

            try
            {
                using var context = new SistemaGimDbContext();

                // 1. Definir los rangos de tiempo
                DateTime hoy = DateTime.Now.Date;

                int diff = (7 + (hoy.DayOfWeek - DayOfWeek.Monday)) % 7;
                DateTime inicioSemana = hoy.AddDays(-1 * diff).Date;

                DateTime inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

                // 2. Cálculos de Asistencias (Visitas)
                int visitasHoy = await context.Asistencias.CountAsync(a => a.FechaHora >= hoy);
                int visitasSemana = await context.Asistencias.CountAsync(a => a.FechaHora >= inicioSemana);
                int visitasMes = await context.Asistencias.CountAsync(a => a.FechaHora >= inicioMes);

                // 3. Cálculos de Ingresos (Pagos)
                decimal pagosHoy = await context.Pagos
                    .Where(p => p.FechaTransaccion >= hoy)
                    .SumAsync(p => (decimal?)p.Monto) ?? 0m;

                decimal pagosSemana = await context.Pagos
                    .Where(p => p.FechaTransaccion >= inicioSemana)
                    .SumAsync(p => (decimal?)p.Monto) ?? 0m;

                decimal pagosMes = await context.Pagos
                    .Where(p => p.FechaTransaccion >= inicioMes)
                    .SumAsync(p => (decimal?)p.Monto) ?? 0m;

                decimal pagosTotales = await context.Pagos
                    .SumAsync(p => (decimal?)p.Monto) ?? 0m;

                // 4. Asignar resultados a la UI (formateando como texto)
                TxtVisitasHoy.Text = visitasHoy.ToString();
                TxtVisitasSemana.Text = visitasSemana.ToString();
                TxtVisitasMes.Text = visitasMes.ToString();

                TxtPagosHoy.Text = $"C$ {pagosHoy:N2}";
                TxtPagosSemana.Text = $"C$ {pagosSemana:N2}";
                TxtPagosMes.Text = $"C$ {pagosMes:N2}";
                TxtPagosTotales.Text = $"C$ {pagosTotales:N2}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la base de datos:\n{ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Volver a habilitar el botón
                BtnActualizar.IsEnabled = true;
            }
        }
    }
}