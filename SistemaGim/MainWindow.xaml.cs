using System.Windows;
using SistemaGim.Views; // Importamos la carpeta de nuestras vistas

namespace SistemaGim
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // 1. Cargar el Dashboard por defecto apenas inicie el sistema
            MainContent.Content = new DashboardView();
        }

        // 2. Este es el evento del botón que agregamos en el XAML
        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            // Cuando hagan clic, inyectamos la vista del Dashboard en el contenedor principal
            MainContent.Content = new DashboardView();
        }

        private void BtnClientes_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ClientesView();
            MenuLateral.IsLeftDrawerOpen = false; // Oculta el menú automáticamente al hacer clic
        }

        private void BtnVisitas_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new VisitasView();
            MenuLateral.IsLeftDrawerOpen = false; // Oculta el menú automáticamente al hacer clic
        }

        private void BtnConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ConfiguracionView();
            MenuLateral.IsLeftDrawerOpen = false; // Oculta el menú automáticamente al hacer clic
        }

        private void BtnMembresias_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new MembresiasView();
            MenuLateral.IsLeftDrawerOpen = false; // Oculta el menú automáticamente al hacer clic
        }

        private void BtnInventario_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new SistemaGim.Views.InventarioView();
            MenuLateral.IsLeftDrawerOpen = false;
        }
    }
}