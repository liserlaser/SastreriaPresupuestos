using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SastreriaPresupuestos.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        private void UpcomingDeliveriesMonthButton_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.UpcomingDeliveriesMonthButton_Click(sender, e);
            }
        }

        private void UpcomingDeliveryCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.UpcomingDeliveryCard_MouseLeftButtonDown(sender, e);
            }
        }

        private void CalendarDelivery_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.CalendarDelivery_MouseDoubleClick(sender, e);
            }
        }
    }
}
