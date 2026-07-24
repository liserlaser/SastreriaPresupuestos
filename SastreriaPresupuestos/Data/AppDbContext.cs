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

        public DbSet<QuoteDeposit> QuoteDeposits => Set<QuoteDeposit>();

        public DbSet<Invoice> Invoices => Set<Invoice>();

        public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();

        public DbSet<PriceItem> PriceItems => Set<PriceItem>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = GetDatabasePath();

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Invoice>()
                .HasOne(invoice => invoice.Client)
                .WithMany()
                .HasForeignKey(invoice => invoice.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public static string GetDatabasePath()
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SastreriaPresupuestos");

            Directory.CreateDirectory(folder);

            return Path.Combine(folder, "sastreria.db");
        }
    }
}
