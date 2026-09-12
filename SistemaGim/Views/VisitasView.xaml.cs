using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using SistemaGim.Data;
using SistemaGim.Models;

namespace SistemaGim.Views
{
    public partial class VisitasView : UserControl
    {
        private decimal _montoSesionActual = 0m;
        private string _conceptoSesion = "";

        public VisitasView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            TxtFechaHoy.Text = DateTime.Now.ToString("dd/MMM/yyyy");
            CargarClientes();
            ObtenerPrecioSesionDiaria();
            CargarVisitasDeHoy();
        }

        private void CargarClientes()
        {
            using var context = new SistemaGimDbContext();
            // Cargar solo clientes activos para el buscador
            var clientes = context.Clientes.Where(c => c.Estado == true).OrderBy(c => c.NombreCompleto).ToList();
            CmbClientes.ItemsSource = clientes;
        }

        private void ObtenerPrecioSesionDiaria()
        {
            using var context = new SistemaGimDbContext();
            // Buscar en la tabla de configuración el plan que dura 1 día
            var configDiaria = context.ConfiguracionPrecios.FirstOrDefault(c => c.DiasVigencia == 1);

            if (configDiaria != null)
            {
                _montoSesionActual = configDiaria.Monto;
                _conceptoSesion = configDiaria.Concepto;
                TxtMontoSesion.Text = $"C$ {_montoSesionActual:N2}";
                TxtAlertaPrecio.Visibility = Visibility.Hidden;
                BtnCobrar.IsEnabled = true;
            }
            else
            {
                TxtMontoSesion.Text = "No Configurado";
                TxtAlertaPrecio.Text = "¡Falta registrar el precio de 'Sesión Diaria' (Vigencia 1 día) en Configuración!";
                TxtAlertaPrecio.Visibility = Visibility.Visible;
                BtnCobrar.IsEnabled = false; // Bloquear cobro si no hay precio
            }
        }

        private void CargarVisitasDeHoy()
        {
            using var context = new SistemaGimDbContext();
            DateTime hoy = DateTime.Now.Date;

            // Usamos .Include() para traer los datos del Cliente y el Pago relacionados a la tabla Asistencias
            var visitasHoy = context.Asistencias
                .Include(a => a.Cliente)
                .Include(a => a.Pago)
                .Where(a => a.FechaHora >= hoy)
                .OrderByDescending(a => a.AsistenciaId)
                .ToList();

            DgVisitasHoy.ItemsSource = visitasHoy;
        }

        private void BtnCobrar_Click(object sender, RoutedEventArgs e)
        {
            if (CmbClientes.SelectedValue == null)
            {
                MessageBox.Show("Debe seleccionar un cliente de la lista.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                CmbClientes.Focus();
                return;
            }

            int clienteId = (int)CmbClientes.SelectedValue;

            try
            {
                using var context = new SistemaGimDbContext();

                // 1. Crear el recibo de Pago (Ingreso)
                var nuevoPago = new Pago
                {
                    ClienteId = clienteId,
                    Concepto = _conceptoSesion,
                    Monto = _montoSesionActual,
                    FechaTransaccion = DateTime.Now,
                    FechaInicioVigencia = DateTime.Now.Date,
                    FechaFinVigencia = DateTime.Now.Date // Vence el mismo día
                };
                context.Pagos.Add(nuevoPago);

                // Forzamos guardar el pago primero para obtener el PagoId que SQL Server genera
                context.SaveChanges();

                // 2. Crear el registro de Asistencia vinculado al Pago
                var nuevaAsistencia = new Asistencia
                {
                    ClienteId = clienteId,
                    PagoId = nuevoPago.PagoId, // Conectamos la visita con el recibo de caja
                    FechaHora = DateTime.Now
                };
                context.Asistencias.Add(nuevaAsistencia);

                context.SaveChanges();

                MessageBox.Show("¡Cobro y visita registrados con éxito!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                // Limpiar pantalla y refrescar tabla
                CmbClientes.SelectedItem = null;
                CargarVisitasDeHoy();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al procesar la transacción: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}