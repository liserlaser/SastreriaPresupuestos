using System;
using Microsoft.EntityFrameworkCore;
using SastreriaPresupuestos.Data;
using SastreriaPresupuestos.Models;
using SastreriaPresupuestos.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace SastreriaPresupuestos.Views
{
    public partial class PricesWindow : Window
    {
        private ObservableCollection<PriceItem> Prices = new();
        private readonly HashSet<int> DeletedPriceIds = new();

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
                .AsNoTracking()
                .OrderBy(p => p.ProductName)
                .ThenBy(p => p.TailoringType)
                .ToList();

            Prices = new ObservableCollection<PriceItem>(prices);

            PricesDataGrid.ItemsSource = Prices;
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            var newProduct = new PriceItem
            {
                ProductName = "Nuevo producto",
                TailoringType = "",
                Price = 0,
                IsAccessory = false
            };

            Prices.Add(newProduct);
            PricesDataGrid.SelectedItem = newProduct;
            PricesDataGrid.ScrollIntoView(newProduct);
            PricesDataGrid.Focus();
        }

        private void DeleteProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (PricesDataGrid.SelectedItem is not PriceItem selectedProduct)
            {
                MessageBox.Show(
                    "Selecciona un producto para eliminar.",
                    "Editar productos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            var result = MessageBox.Show(
                $"¿Quieres eliminar \"{selectedProduct.ProductName}\"? Los trabajos ya guardados no se modificarán.",
                "Eliminar producto",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            if (selectedProduct.Id > 0)
                DeletedPriceIds.Add(selectedProduct.Id);

            Prices.Remove(selectedProduct);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PricesDataGrid.CommitEdit(System.Windows.Controls.DataGridEditingUnit.Cell, true);
                PricesDataGrid.CommitEdit(System.Windows.Controls.DataGridEditingUnit.Row, true);

                var normalizedProducts = Prices
                    .Select(p => new PriceItem
                    {
                        Id = p.Id,
                        ProductName = (p.ProductName ?? "").Trim(),
                        TailoringType = (p.TailoringType ?? "").Trim(),
                        Price = p.Price < 0 ? 0 : p.Price,
                        IsAccessory = p.IsAccessory
                    })
                    .ToList();

                if (normalizedProducts.Any(p => string.IsNullOrWhiteSpace(p.ProductName)))
                {
                    MessageBox.Show(
                        "Todos los productos deben tener nombre.",
                        "Editar productos",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                var duplicate = normalizedProducts
                    .GroupBy(p => PriceService.BuildKey(p.ProductName, p.TailoringType), StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault(g => g.Count() > 1);

                if (duplicate != null)
                {
                    MessageBox.Show(
                        $"Ya existe un producto con la misma combinación de nombre y tipo: {duplicate.Key}.",
                        "Producto duplicado",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                using var db = new AppDbContext();
                using var transaction = db.Database.BeginTransaction();

                if (DeletedPriceIds.Count > 0)
                {
                    var productsToDelete = db.PriceItems
                        .Where(p => DeletedPriceIds.Contains(p.Id))
                        .ToList();

                    db.PriceItems.RemoveRange(productsToDelete);
                }

                foreach (var editedProduct in normalizedProducts)
                {
                    if (editedProduct.Id <= 0)
                    {
                        db.PriceItems.Add(editedProduct);
                        continue;
                    }

                    var productInDb = db.PriceItems
                        .FirstOrDefault(p => p.Id == editedProduct.Id);

                    if (productInDb == null)
                        continue;

                    productInDb.ProductName = editedProduct.ProductName;
                    productInDb.TailoringType = editedProduct.TailoringType;
                    productInDb.Price = editedProduct.Price;
                    productInDb.IsAccessory = editedProduct.IsAccessory;
                }

                db.SaveChanges();
                transaction.Commit();

                PriceService.LoadPrices();

                MessageBox.Show(
                    "Productos guardados correctamente.",
                    "Editar productos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudieron guardar los productos y tarifas.\n\n{ex.Message}",
                    "Editar productos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;

            Close();
        }
    }
}
