using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using SistemaGim.Data;
using SistemaGim.Models;

namespace SistemaGim.Views
{
    public partial class InventarioView : UserControl
    {
        private int _maquinaIdSeleccionada = 0;

        public InventarioView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CargarInventario();
            CmbTipo.SelectedIndex = 0;
            CmbEstado.SelectedIndex = 0;
        }

        private void CargarInventario(string busqueda = "")
        {
            using var context = new SistemaGimDbContext();
            var query = context.InventarioMaquinas.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(m => m.Nombre.Contains(busqueda) || m.Tipo.Contains(busqueda));
            }

            DgInventario.ItemsSource = query.OrderByDescending(m => m.MaquinaId).ToList();
        }

        private void ValidarSoloNumeros(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ValidarSoloDecimales(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.,]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNombre.Text) || string.IsNullOrWhiteSpace(TxtCosto.Text) || string.IsNullOrWhiteSpace(TxtCantidad.Text))
            {
                MessageBox.Show("Nombre, Costo y Cantidad son campos obligatorios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var context = new SistemaGimDbContext();
                string costoTexto = TxtCosto.Text.Replace(",", ".");
                string tipoSeleccionado = ((ComboBoxItem)CmbTipo.SelectedItem)?.Content.ToString() ?? "Máquina";
                string estadoSeleccionado = ((ComboBoxItem)CmbEstado.SelectedItem)?.Content.ToString() ?? "Activo";

                if (_maquinaIdSeleccionada == 0)
                {
                    var nuevaMaquina = new InventarioMaquina
                    {
                        Nombre = TxtNombre.Text.Trim(),
                        Tipo = tipoSeleccionado,
                        PesoEspecifico = string.IsNullOrWhiteSpace(TxtPeso.Text) ? null : TxtPeso.Text.Trim(),
                        Costo = decimal.Parse(costoTexto, System.Globalization.CultureInfo.InvariantCulture),
                        Cantidad = int.Parse(TxtCantidad.Text),
                        Estado = estadoSeleccionado,
                        FechaAdquisicion = DateTime.Now
                    };
                    context.InventarioMaquinas.Add(nuevaMaquina);
                }
                else
                {
                    var maquinaDB = context.InventarioMaquinas.Find(_maquinaIdSeleccionada);
                    if (maquinaDB != null)
                    {
                        maquinaDB.Nombre = TxtNombre.Text.Trim();
                        maquinaDB.Tipo = tipoSeleccionado;
                        maquinaDB.PesoEspecifico = string.IsNullOrWhiteSpace(TxtPeso.Text) ? null : TxtPeso.Text.Trim();
                        maquinaDB.Costo = decimal.Parse(costoTexto, System.Globalization.CultureInfo.InvariantCulture);
                        maquinaDB.Cantidad = int.Parse(TxtCantidad.Text);
                        maquinaDB.Estado = estadoSeleccionado;
                        context.InventarioMaquinas.Update(maquinaDB);
                    }
                }

                context.SaveChanges();
                MessageBox.Show("Equipo guardado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                LimpiarFormulario();
                CargarInventario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DgInventario_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgInventario.SelectedItem is InventarioMaquina item)
            {
                _maquinaIdSeleccionada = item.MaquinaId;
                TxtNombre.Text = item.Nombre;
                TxtPeso.Text = item.PesoEspecifico;
                TxtCosto.Text = item.Costo.ToString("0.00");
                TxtCantidad.Text = item.Cantidad.ToString();

                // Seleccionar en ComboBoxes
                foreach (ComboBoxItem cbItem in CmbTipo.Items)
                {
                    if (cbItem.Content.ToString() == item.Tipo) CmbTipo.SelectedItem = cbItem;
                }
                foreach (ComboBoxItem cbItem in CmbEstado.Items)
                {
                    if (cbItem.Content.ToString() == item.Estado) CmbEstado.SelectedItem = cbItem;
                }

                TxtEstadoFormulario.Text = $"Editando ID: {_maquinaIdSeleccionada}";
                BtnEliminar.IsEnabled = true;
            }
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (_maquinaIdSeleccionada == 0) return;

            if (MessageBox.Show("¿Está seguro de eliminar este registro del inventario?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                using var context = new SistemaGimDbContext();
                var item = context.InventarioMaquinas.Find(_maquinaIdSeleccionada);
                if (item != null)
                {
                    context.InventarioMaquinas.Remove(item);
                    context.SaveChanges();
                    LimpiarFormulario();
                    CargarInventario();
                }
            }
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            CargarInventario(TxtBuscar.Text);
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            _maquinaIdSeleccionada = 0;
            TxtNombre.Clear();
            TxtPeso.Clear();
            TxtCosto.Clear();
            TxtCantidad.Clear();
            CmbTipo.SelectedIndex = 0;
            CmbEstado.SelectedIndex = 0;
            TxtEstadoFormulario.Text = "Nuevo registro";
            BtnEliminar.IsEnabled = false;
            DgInventario.SelectedItem = null;
        }
    }
}