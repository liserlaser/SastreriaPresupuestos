using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SastreriaPresupuestos.Export;
using SastreriaPresupuestos.Models;

namespace SastreriaPresupuestos.Services
{
    public static class ExportDataService
    {
        public static ExportQuote CreateExportQuote(
            string clientName,
            string clientPhone,
            DateTime deliveryDate,
            string title,
            string status,
            decimal deposit,
            string clientNotes,
            ObservableCollection<ProductLine> products)
        {
            var exportQuote = new ExportQuote
            {
                ClientName = clientName.Trim(),
                ClientPhone = clientPhone.Trim(),
                DeliveryDate = deliveryDate,
                Title = title.Trim(),
                Status = status.Trim(),
                ClientNotes = clientNotes.Trim(),
                Deposit = deposit,                
            };

            foreach (var product in products)
            {
                var productPrice = product.Total;

                if (ShouldShowFabricPrice(product) && product.FabricPrice > 0)
                {
                    productPrice -= product.FabricPrice * product.Quantity;
                }

                exportQuote.Items.Add(new ExportQuoteItem
                {
                    Product = BuildProductName(product),
                    Description = BuildDescription(product),
                    Quantity = product.Quantity,
                    Price = productPrice
                });

                if (ShouldShowFabricPrice(product) && product.FabricPrice > 0)
                {
                    exportQuote.Items.Add(new ExportQuoteItem
                    {
                        Product = $"Tejido {product.ProductName}",
                        Description = string.IsNullOrWhiteSpace(product.Fabric)
                            ? ""
                            : product.Fabric,
                        Quantity = product.Quantity,
                        Price = product.FabricPrice * product.Quantity,
                        IsFabricLine = true
                    });
                }
            }

            return exportQuote;
        }

        private static string BuildProductName(ProductLine product)
        {
            if (string.IsNullOrWhiteSpace(product.TailoringType))
                return product.ProductName;

            return $"{product.ProductName} {product.TailoringType}";
        }

        private static string BuildDescription(ProductLine product)
        {
            if (string.IsNullOrWhiteSpace(product.Fabric))
                return "";

            if (ShouldShowFabricPrice(product) && product.FabricPrice > 0)
                return "";

            return $"Tejido / referencia: {product.Fabric}";
        }

        private static bool ShouldShowFabricPrice(ProductLine product)
        {
            return product.ProductName == "Traje" ||
                   product.ProductName == "Chaqué" ||
                   product.ProductName == "Chaqueta" ||
                   product.ProductName == "Chaleco" ||
                   product.ProductName == "Pantalón" ||
                   product.ProductName == "Camisa";
        }
    }
}