using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using SistemaGim.Data;
using SistemaGim.Models;

namespace SistemaGim.Views
{
    public partial class MembresiasView : UserControl
    {
        private int _pagoActivoId = 0; // Guardaremos el ID de la membresía activa para asociar la visita

        public MembresiasView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CargarListas();
        }

        private void CargarListas()
        {
            using var context = new SistemaGimDbContext();

            // Cargar clientes activos para ambos combos
            var clientes = context.Clientes.Where(c => c.Estado == true).OrderBy(c => c.NombreCompleto).ToList();
            CmbClientePago.ItemsSource = clientes;
            CmbClienteAcceso.ItemsSource = clientes;

            // Cargar planes de membresía (Solo los que duren más de 1 día, excluyendo la sesión diaria)
            var planes = context.ConfiguracionPrecios.Where(p => p.DiasVigencia > 1).OrderBy(p => p.DiasVigencia).ToList();
            CmbPlan.ItemsSource = planes;
        }

        // --- LÓGICA DE VENTA DE MEMBRESÍA ---

        private void CmbPlan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbPlan.SelectedItem is ConfiguracionPrecio plan)
            {
                DateTime inicio = DateTime.Now.Date;
                DateTime fin = inicio.AddDays(plan.DiasVigencia);

                TxtFechaInicio.Text = inicio.ToString("dd/MMM/yyyy");
                TxtFechaFin.Text = fin.ToString("dd/MMM/yyyy");
                TxtTotalPagar.Text = $"C$ {plan.Monto:N2}";
            }
        }

        private void BtnCobrarMembresia_Click(object sender, RoutedEventArgs e)
        {
            if (CmbClientePago.SelectedValue == null || CmbPlan.SelectedValue == null)
            {
                MessageBox.Show("Seleccione un cliente y un plan de membresía.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var context = new SistemaGimDbContext();
                var plan = (ConfiguracionPrecio)CmbPlan.SelectedItem;
                int clienteId = (int)CmbClientePago.SelectedValue;

                var nuevoPago = new Pago
                {
                    ClienteId = clienteId,
                    Concepto = plan.Concepto,
                    Monto = plan.Monto,
                    FechaTransaccion = DateTime.Now,
                    FechaInicioVigencia = DateTime.Now.Date,
                    FechaFinVigencia = DateTime.Now.Date.AddDays(plan.DiasVigencia)
                };

                context.Pagos.Add(nuevoPago);
                context.SaveChanges();

                MessageBox.Show("Membresía asignada y cobrada con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                // Limpiar form
                CmbClientePago.SelectedItem = null;
                CmbPlan.SelectedItem = null;
                TxtFechaInicio.Clear();
                TxtFechaFin.Clear();
                TxtTotalPagar.Text = "";

                // Si este mismo cliente estaba seleccionado en Acceso, refrescarlo
                if (CmbClienteAcceso.SelectedValue != null && (int)CmbClienteAcceso.SelectedValue == clienteId)
                {
                    CmbClienteAcceso_SelectionChanged(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar membresía: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // --- LÓGICA DE CONTROL DE ACCESO Y VISITAS ---

        private void CmbClienteAcceso_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbClienteAcceso.SelectedValue == null) return;

            int clienteId = (int)CmbClienteAcceso.SelectedValue;

            using var context = new SistemaGimDbContext();

            // Buscar la membresía más reciente de este cliente
            var ultimaMembresia = context.Pagos
                .Where(p => p.ClienteId == clienteId && p.Concepto != "Sesión Diaria")
                .OrderByDescending(p => p.FechaFinVigencia)
                .FirstOrDefault();

            // Cargar el historial de asistencias (últimas 30)
            DgHistorialVisitas.ItemsSource = context.Asistencias
                .Where(a => a.ClienteId == clienteId)
                .OrderByDescending(a => a.FechaHora)
                .Take(30)
                .ToList();

            EvaluarEstadoMembresia(ultimaMembresia);
        }

        private void EvaluarEstadoMembresia(Pago ultimaMembresia)
        {
            if (ultimaMembresia == null)
            {
                ConfigurarTarjetaEstado("SIN MEMBRESÍA", "-", "-", "#F44336", "Cancel", false);
                return;
            }

            int diasRestantes = (ultimaMembresia.FechaFinVigencia.Date - DateTime.Now.Date).Days;
            _pagoActivoId = ultimaMembresia.PagoId;

            string infoVence = $"Vence: {ultimaMembresia.FechaFinVigencia:dd/MMM/yyyy}";

            if (diasRestantes < 0)
            {
                // Vencida
                ConfigurarTarjetaEstado("MEMBRESÍA VENCIDA", "0", infoVence, "#F44336", "AlertCircle", false);
            }
            else if (diasRestantes <= 5)
            {
                // Por vencer (Amarillo/Naranja)
                ConfigurarTarjetaEstado("POR VENCER", diasRestantes.ToString(), infoVence, "#FF9800", "Alert", true);
            }
            else
            {
                // Activa (Verde)
                ConfigurarTarjetaEstado("MEMBRESÍA ACTIVA", diasRestantes.ToString(), infoVence, "#4CAF50", "ShieldCheck", true);
            }
        }

        private void ConfigurarTarjetaEstado(string estado, string dias, string info, string colorHex, string icono, bool permitirEntrada)
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex));

            TxtStatusMembresia.Text = estado;
            TxtStatusMembresia.Foreground = brush;

            TxtDiasRestantes.Text = dias;
            TxtDiasRestantes.Foreground = brush;

            TxtVencimientoInfo.Text = info;

            IconStatus.Kind = (MaterialDesignThemes.Wpf.PackIconKind)Enum.Parse(typeof(MaterialDesignThemes.Wpf.PackIconKind), icono);
            IconStatus.Foreground = brush;

            BtnRegistrarEntrada.IsEnabled = permitirEntrada;
        }

        private void BtnRegistrarEntrada_Click(object sender, RoutedEventArgs e)
        {
            if (CmbClienteAcceso.SelectedValue == null) return;

            try
            {
                int clienteId = (int)CmbClienteAcceso.SelectedValue;

                using var context = new SistemaGimDbContext();

                var asistencia = new Asistencia
                {
                    ClienteId = clienteId,
                    PagoId = _pagoActivoId, // Asociamos la entrada a esta membresía pagada
                    FechaHora = DateTime.Now
                };

                context.Asistencias.Add(asistencia);
                context.SaveChanges();

                MessageBox.Show("¡Entrada registrada con éxito!", "Bienvenido", MessageBoxButton.OK, MessageBoxImage.Information);

                // Refrescar historial
                CmbClienteAcceso_SelectionChanged(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar entrada: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}