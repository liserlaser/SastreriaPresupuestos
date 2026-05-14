using System.Collections.Generic;
using System.Linq;
using SastreriaPresupuestos.Data;
using SastreriaPresupuestos.Models;

namespace SastreriaPresupuestos.Services
{
    public static class PriceService
    {
        public static Dictionary<string, decimal> Prices { get; private set; } = new();

        public static void LoadPrices()
        {
            using var db = new AppDbContext();

            EnsureDefaultPrices(db);

            Prices = db.PriceItems
                .AsEnumerable()
                .GroupBy(p => BuildKey(p.ProductName, p.TailoringType))
                .ToDictionary(
                    g => g.Key,
                    g => g.Last().Price);
        }

        private static void EnsureDefaultPrices(AppDbContext db)
        {
            if (db.PriceItems.Any())
                return;

            var defaultPrices = new List<PriceItem>
            {
                new() { ProductName = "Traje", TailoringType = "Confeccion", Price = 400 },
                new() { ProductName = "Chaqué", TailoringType = "Confeccion", Price = 1000 },
                new() { ProductName = "Chaleco", TailoringType = "Confeccion", Price = 250 },
                new() { ProductName = "Chaqueta", TailoringType = "Confeccion", Price = 300 },
                new() { ProductName = "Pantalón", TailoringType = "Confeccion", Price = 150 },
                new() { ProductName = "Camisa", TailoringType = "Confeccion", Price = 80 },

                new() { ProductName = "Traje", TailoringType = "Medida", Price = 950 },
                new() { ProductName = "Chaqué", TailoringType = "Medida", Price = 1200 },
                new() { ProductName = "Chaleco", TailoringType = "Medida", Price = 250 },
                new() { ProductName = "Chaqueta", TailoringType = "Medida", Price = 400 },
                new() { ProductName = "Pantalón", TailoringType = "Medida", Price = 250 },
                new() { ProductName = "Camisa", TailoringType = "Medida", Price = 180 },

                new() { ProductName = "Traje", TailoringType = "Artesanal", Price = 1500 },
                new() { ProductName = "Chaqué", TailoringType = "Artesanal", Price = 2000 },
                new() { ProductName = "Chaleco", TailoringType = "Artesanal", Price = 250 },
                new() { ProductName = "Chaqueta", TailoringType = "Artesanal", Price = 600 },
                new() { ProductName = "Pantalón", TailoringType = "Artesanal", Price = 350 },

                new() { ProductName = "Corbata", TailoringType = "", Price = 60, IsAccessory = true },
                new() { ProductName = "Pañuelo", TailoringType = "", Price = 30, IsAccessory = true },
                new() { ProductName = "Gemelos", TailoringType = "", Price = 60, IsAccessory = true },
                new() { ProductName = "Tirantes", TailoringType = "", Price = 50, IsAccessory = true },
                new() { ProductName = "Zapatos", TailoringType = "", Price = 150, IsAccessory = true }
            };

            db.PriceItems.AddRange(defaultPrices);
            db.SaveChanges();
        }

        public static string BuildKey(string? productName, string? tailoringType)
        {
            var normalizedProductName = productName?.Trim() ?? string.Empty;
            var normalizedTailoringType = tailoringType?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(normalizedTailoringType))
                return normalizedProductName;

            return $"{normalizedProductName}-{normalizedTailoringType}";
        }

        public static decimal GetPrice(string? productName, string? tailoringType)
        {
            if (string.IsNullOrWhiteSpace(productName))
                return 0;

            var key = BuildKey(productName, tailoringType);
            return Prices.TryGetValue(key, out var price) ? price : 0;
        }

        public static List<string> GetProductNames()
        {
            using var db = new AppDbContext();

            EnsureDefaultPrices(db);

            var products = db.PriceItems
                .Select(p => p.ProductName)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct()
                .OrderBy(p => p)
                .ToList();

            if (!products.Contains("Concepto Libre"))
                products.Add("Concepto Libre");

            return products;
        }

        public static List<string> GetTailoringTypes(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName) || productName == "Concepto Libre")
                return new List<string>();

            using var db = new AppDbContext();

            EnsureDefaultPrices(db);

            return db.PriceItems
                .Where(p => p.ProductName == productName && !string.IsNullOrWhiteSpace(p.TailoringType))
                .Select(p => p.TailoringType)
                .Distinct()
                .OrderBy(t => t)
                .ToList();
        }

        public static bool HasTailoringTypes(string productName)
        {
            return GetTailoringTypes(productName).Any();
        }
    }
}