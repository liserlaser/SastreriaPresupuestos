using System.Windows;
using System.Windows.Controls;

namespace SastreriaPresupuestos.Views
{
    public partial class DocumentosView : UserControl
    {
        public DocumentosView()
        {
            InitializeComponent();
        }

        private void ExportClientSheetButton_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.ExportClientSheetButton_Click(sender, e);
            }
        }
    }
}
