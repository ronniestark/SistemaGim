using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using SistemaGim.Data;
using SistemaGim.Models;

namespace SistemaGim.Views
{
    public partial class ClientesView : UserControl
    {
        // Variable para controlar si estamos editando (ID > 0) o creando (ID == 0)
        private int _clienteIdSeleccionado = 0;

        public ClientesView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CargarClientes();
        }

        // --- MÉTODO PARA LEER (READ) ---
        private void CargarClientes(string busqueda = "")
        {
            try
            {
                using var context = new SistemaGimDbContext();

                var query = context.Clientes.AsQueryable();

                if (!string.IsNullOrWhiteSpace(busqueda))
                {
                    query = query.Where(c => c.NombreCompleto.Contains(busqueda));
                }

                // Cargamos la tabla ordenando por los más recientes
                DgClientes.ItemsSource = query.OrderByDescending(c => c.ClienteId).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar clientes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- MÉTODO PARA GUARDAR (CREATE / UPDATE) ---
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validación básica
            if (string.IsNullOrWhiteSpace(TxtNombreCompleto.Text))
            {
                MessageBox.Show("El Nombre Completo es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtNombreCompleto.Focus();
                return;
            }

            try
            {
                using var context = new SistemaGimDbContext();

                if (_clienteIdSeleccionado == 0)
                {
                    // CREATE: Insertar nuevo cliente
                    var nuevoCliente = new Cliente
                    {
                        NombreCompleto = TxtNombreCompleto.Text.Trim(),
                        Telefono = TxtTelefono.Text.Trim(),
                        Estado = ChkEstado.IsChecked ?? true,
                        FechaRegistro = DateTime.Now
                    };
                    context.Clientes.Add(nuevoCliente);
                }
                else
                {
                    // UPDATE: Actualizar cliente existente
                    var clienteDB = context.Clientes.Find(_clienteIdSeleccionado);
                    if (clienteDB != null)
                    {
                        clienteDB.NombreCompleto = TxtNombreCompleto.Text.Trim();
                        clienteDB.Telefono = TxtTelefono.Text.Trim();
                        clienteDB.Estado = ChkEstado.IsChecked ?? true;
                        context.Clientes.Update(clienteDB);
                    }
                }

                // Confirmar cambios en SQL
                context.SaveChanges();

                MessageBox.Show("Datos guardados correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                LimpiarFormulario();
                CargarClientes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- MÉTODO PARA ELIMINAR (DELETE) ---
        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (_clienteIdSeleccionado == 0) return;

            var result = MessageBox.Show("¿Estás seguro de eliminar este cliente? Los pagos asociados quedarán huérfanos.",
                                         "Confirmar Eliminación",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var context = new SistemaGimDbContext();
                    var clienteDB = context.Clientes.Find(_clienteIdSeleccionado);

                    if (clienteDB != null)
                    {
                        context.Clientes.Remove(clienteDB);
                        context.SaveChanges();

                        LimpiarFormulario();
                        CargarClientes();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // --- EVENTO: AL SELECCIONAR UNA FILA EN EL DATAGRID ---
        private void DgClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgClientes.SelectedItem is Cliente clienteSeleccionado)
            {
                _clienteIdSeleccionado = clienteSeleccionado.ClienteId;

                TxtNombreCompleto.Text = clienteSeleccionado.NombreCompleto;
                TxtTelefono.Text = clienteSeleccionado.Telefono;
                ChkEstado.IsChecked = clienteSeleccionado.Estado;

                TxtEstadoFormulario.Text = $"Editando Cliente ID: {_clienteIdSeleccionado}";
                BtnEliminar.IsEnabled = true; // Habilitamos el botón de borrar
                BtnDarBaja.IsEnabled = true;
            }
        }

        private void BtnDarBaja_Click(object sender, RoutedEventArgs e)
        {
            if (_clienteIdSeleccionado == 0) return;

            var result = MessageBox.Show("¿Deseas marcar este cliente como inactivo?",
                                         "Dar de Baja",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using var context = new SistemaGimDbContext();
                    var clienteDB = context.Clientes.Find(_clienteIdSeleccionado);

                    if (clienteDB != null)
                    {
                        clienteDB.Estado = false; // Lo pasamos a inactivo
                        context.Clientes.Update(clienteDB);
                        context.SaveChanges();

                        MessageBox.Show("Cliente dado de baja exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                        LimpiarFormulario();
                        CargarClientes();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al dar de baja: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // --- EVENTO: AL ESCRIBIR EN EL BUSCADOR ---
        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            CargarClientes(TxtBuscar.Text);
        }

        // --- EVENTO Y MÉTODO: LIMPIAR ---
        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            _clienteIdSeleccionado = 0;
            TxtNombreCompleto.Clear();
            TxtTelefono.Clear();
            ChkEstado.IsChecked = true;

            TxtEstadoFormulario.Text = "Creando nuevo registro";
            BtnEliminar.IsEnabled = false; // Deshabilitamos borrar si no hay nadie seleccionado
            DgClientes.SelectedItem = null;
        }
    }
}