using System.Windows.Controls;
using SistemaGim.Views;

namespace SistemaGim.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();

            this.DataContext = new DashboardView();
        }
    }
}