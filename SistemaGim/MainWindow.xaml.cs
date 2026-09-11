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
    }
}