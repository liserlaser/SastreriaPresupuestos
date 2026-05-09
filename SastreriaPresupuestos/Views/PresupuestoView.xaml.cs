using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SastreriaPresupuestos.Views
{
    public partial class PresupuestoView : UserControl
    {
        public PresupuestoView()
        {
            InitializeComponent();
        }

        private void BudgetQuotesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.BudgetQuotesListBox_SelectionChanged(sender, e);
            }
        }

        private void BudgetQuotesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.BudgetQuotesListBox_MouseDoubleClick(sender, e);
            }
        }
    }
}
