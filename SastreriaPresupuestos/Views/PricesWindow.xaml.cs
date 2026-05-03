using Microsoft.EntityFrameworkCore;
using SastreriaPresupuestos.Data;
using SastreriaPresupuestos.Models;
using SastreriaPresupuestos.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace SastreriaPresupuestos.Views
{
    public partial class PricesWindow : Window
    {
        private ObservableCollection<PriceItem> Prices = new();

        public PricesWindow()
        {
            InitializeComponent();

            LoadPrices();
        }

        private void LoadPrices()
        {
            using var db = new AppDbContext();

            PriceService.LoadPrices();

            var prices = db.PriceItems
                .OrderBy(p => p.ProductName)
                .ThenBy(p => p.TailoringType)
                .ToList();

            Prices = new ObservableCollection<PriceItem>(prices);

            PricesDataGrid.ItemsSource = Prices;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            using var db = new AppDbContext();

            foreach (var editedPrice in Prices)
            {
                var priceInDb = db.PriceItems
                    .FirstOrDefault(p => p.Id == editedPrice.Id);

                if (priceInDb == null)
                    continue;

                if (editedPrice.Price < 0)
                    editedPrice.Price = 0;

                priceInDb.Price = editedPrice.Price;
            }

            db.SaveChanges();

            PriceService.LoadPrices();

            MessageBox.Show(
                "Precios guardados correctamente.",
                "Configuración de precios",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            DialogResult = true;

            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;

            Close();
        }
    }
}