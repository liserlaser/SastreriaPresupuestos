using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using SastreriaPresupuestos.Models;

namespace SastreriaPresupuestos.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Client> Clients => Set<Client>();

        public DbSet<Quote> Quotes => Set<Quote>();

        public DbSet<QuoteItem> QuoteItems => Set<QuoteItem>();

        public DbSet<PriceItem> PriceItems => Set<PriceItem>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = GetDatabasePath();

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        public static string GetDatabasePath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "sastreria.db");
        }
    }
}