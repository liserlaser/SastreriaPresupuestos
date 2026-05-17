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
        private static readonly HashSet<string> ProductsWithSeparateFabricLine = new(StringComparer.OrdinalIgnoreCase)
        {
            "Traje",
            "Chaqué",
            "Chaqueta",
            "Chaleco",
            "Pantalón",
            "Camisa"
        };

        public static ExportQuote CreateExportQuote(
            string clientName,
            string clientPhone,
            DateTime deliveryDate,
            DateTime? eventDate,
            string title,
            string status,
            decimal deposit,
            string clientNotes,
            ObservableCollection<ProductLine> products,
            int? quoteId = null,
            DateTime? createdAt = null)
        {
            var exportQuote = new ExportQuote
            {
                QuoteId = quoteId,
                CreatedAt = createdAt,
                ClientName = clientName.Trim(),
                ClientPhone = clientPhone.Trim(),
                DeliveryDate = deliveryDate,
                EventDate = eventDate,
                Title = title.Trim(),
                Status = status.Trim(),
                ClientNotes = clientNotes.Trim(),
                Deposit = deposit,                
            };

            foreach (var product in products.Where(p => p != null))
            {
                var productPrice = GetProductPriceWithoutSeparateFabricLine(product);

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
                        Product = $"Tejido {NormalizeText(product.ProductName)}",
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
            return NormalizeText(product.ProductName);
        }

        private static string BuildDescription(ProductLine product)
        {
            var details = new List<string>();

            if (!string.IsNullOrWhiteSpace(product.TailoringType))
                details.Add(NormalizeText(product.TailoringType));

            if (!string.IsNullOrWhiteSpace(product.Fabric) &&
                !(ShouldShowFabricPrice(product) && product.FabricPrice > 0))
            {
                details.Add($"Tejido / referencia: {product.Fabric.Trim()}");
            }

            return string.Join(" · ", details);
        }

        private static decimal GetProductPriceWithoutSeparateFabricLine(ProductLine product)
        {
            var productPrice = ShouldShowFabricPrice(product) && product.FabricPrice > 0
                ? product.Total - product.FabricPrice * product.Quantity
                : product.Total;

            return Math.Max(0, productPrice);
        }

        private static bool ShouldShowFabricPrice(ProductLine product)
        {
            return ProductsWithSeparateFabricLine.Contains(product.ProductName);
        }

        private static string NormalizeText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            return value.Trim()
                .Replace("Confeccion", "Confección");
        }
    }
}
