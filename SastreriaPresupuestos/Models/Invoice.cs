using System;
using System.Collections.Generic;
using System.Linq;

namespace SastreriaPresupuestos.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string Series { get; set; } = InvoiceSeries.Store;

        public int Number { get; set; }

        public string InvoiceNumber { get; set; } = "";

        public string Status { get; set; } = "Emitida";

        public int? SourceQuoteId { get; set; }

        public Quote? SourceQuote { get; set; }

        public int ClientId { get; set; }

        public Client? Client { get; set; }

        public int? CorrectsInvoiceId { get; set; }

        public Invoice? CorrectsInvoice { get; set; }

        public List<InvoiceItem> Items { get; set; } = new();

        public decimal Total { get; set; }

        public string Notes { get; set; } = "";

        public string DisplayTitle => string.IsNullOrWhiteSpace(InvoiceNumber)
            ? $"{CreatedAt.Year}/{Series}/{Number:0000}"
            : InvoiceNumber;

        public string ClientName => Client?.Name ?? "";

        public string Summary
        {
            get
            {
                if (Items == null || Items.Count == 0)
                    return "Sin productos";

                return string.Join(", ", Items.Take(3).Select(i => i.ProductName)) +
                    (Items.Count > 3 ? "..." : "");
            }
        }
    }

    public static class InvoiceSeries
    {
        public const string Quote = "P";
        public const string Store = "T";
        public const string Company = "C";
        public const string CorrectiveSimplified = "RT";
        public const string CorrectiveCompany = "RC";

        public static readonly string[] All =
        {
            Quote,
            Store,
            Company,
            CorrectiveSimplified,
            CorrectiveCompany
        };
    }
}
