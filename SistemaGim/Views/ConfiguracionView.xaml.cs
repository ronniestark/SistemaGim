using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SistemaGim.Data;
using SistemaGim.Models;

namespace SistemaGim.Views
{
    public partial class ConfiguracionView : UserControl
    {
        private int _configIdSeleccionado = 0;

        public ConfiguracionView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CargarPrecios();
        }

        private void CargarPrecios()
        {
            using var context = new SistemaGimDbContext();
            DgPrecios.ItemsSource = context.ConfiguracionPrecios.OrderBy(c => c.DiasVigencia).ToList();
        }

        // --- VALIDACIONES DE ENTRADA (SOLO TEXTO / NÚMEROS) ---
        private void ValidarSoloNumeros(object sender, TextCompositionEventArgs e)
        {
            // Solo permite números del 0 al 9
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ValidarSoloDecimales(object sender, TextCompositionEventArgs e)
        {
            // Permite números y un solo punto o coma decimal
            Regex regex = new Regex("[^0-9.,]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtConcepto.Text) || string.IsNullOrWhiteSpace(TxtMonto.Text) || string.IsNullOrWhiteSpace(TxtDias.Text))
            {
                MessageBox.Show("Todos los campos son obligatorios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var context = new SistemaGimDbContext();

                // Limpiar formato de moneda si el usuario digitó comas
                string montoTexto = TxtMonto.Text.Replace(",", ".");

                if (_configIdSeleccionado == 0)
                {
                    context.ConfiguracionPrecios.Add(new ConfiguracionPrecio
                    {
                        Concepto = TxtConcepto.Text.Trim(),
                        Monto = decimal.Parse(montoTexto, System.Globalization.CultureInfo.InvariantCulture),
                        DiasVigencia = int.Parse(TxtDias.Text)
                    });
                }
                else
                {
                    var precioDB = context.ConfiguracionPrecios.Find(_configIdSeleccionado);
                    precioDB.Concepto = TxtConcepto.Text.Trim();
                    precioDB.Monto = decimal.Parse(montoTexto, System.Globalization.CultureInfo.InvariantCulture);
                    precioDB.DiasVigencia = int.Parse(TxtDias.Text);
                    context.ConfiguracionPrecios.Update(precioDB);
                }

                context.SaveChanges();
                LimpiarFormulario();
                CargarPrecios();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Verifique que los montos sean números válidos.\nError: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DgPrecios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgPrecios.SelectedItem is ConfiguracionPrecio precioSeleccionado)
            {
                _configIdSeleccionado = precioSeleccionado.ConfigId;
                TxtConcepto.Text = precioSeleccionado.Concepto;
                TxtMonto.Text = precioSeleccionado.Monto.ToString("0.00");
                TxtDias.Text = precioSeleccionado.DiasVigencia.ToString();

                TxtEstadoFormulario.Text = $"Editando Tarifa ID: {_configIdSeleccionado}";
                BtnEliminar.IsEnabled = true;
            }
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (_configIdSeleccionado == 0) return;

            if (MessageBox.Show("¿Eliminar este precio?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using var context = new SistemaGimDbContext();
                var precioDB = context.ConfiguracionPrecios.Find(_configIdSeleccionado);
                context.ConfiguracionPrecios.Remove(precioDB);
                context.SaveChanges();
                LimpiarFormulario();
                CargarPrecios();
            }
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            _configIdSeleccionado = 0;
            TxtConcepto.Clear();
            TxtMonto.Clear();
            TxtDias.Clear();
            TxtEstadoFormulario.Text = "Creando nuevo precio";
            BtnEliminar.IsEnabled = false;
            DgPrecios.SelectedItem = null;
        }
    }
}