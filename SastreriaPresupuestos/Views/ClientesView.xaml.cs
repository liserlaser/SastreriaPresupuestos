using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SastreriaPresupuestos.Views
{
    public partial class ClientesView : UserControl
    {
        public ClientesView()
        {
            InitializeComponent();
        }

        private void ClientSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.ClientSearchTextBox_TextChanged(sender, e);
            }
        }

        private void StatusFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.StatusFilterButton_Click(sender, e);
            }
        }

        private void DeliveryFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.DeliveryFilterButton_Click(sender, e);
            }
        }

        private void ClientsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.ClientsListBox_MouseDoubleClick(sender, e);
            }
        }

        private void DeleteQuote_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.DeleteQuote_Click(sender, e);
            }
        }
    }
}
