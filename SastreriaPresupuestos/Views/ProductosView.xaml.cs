using System.Windows;
using System.Windows.Controls;

namespace SastreriaPresupuestos.Views
{
    public partial class ProductosView : UserControl
    {
        public ProductosView()
        {
            InitializeComponent();
        }

        private void ProductsDataGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.ProductsDataGrid_PreparingCellForEdit(sender, e);
            }
        }

        private void MoveProductUp_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.MoveProductUp_Click(sender, e);
            }
        }

        private void MoveProductDown_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.MoveProductDown_Click(sender, e);
            }
        }

        private void DuplicateProduct_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.DuplicateProduct_Click(sender, e);
            }
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.DeleteProduct_Click(sender, e);
            }
        }
    }
}
