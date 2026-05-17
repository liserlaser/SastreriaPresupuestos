using System;
using System.Collections.Generic;
using System.Linq;
using SastreriaPresupuestos.Services;

namespace SastreriaPresupuestos.Export
{
    public class ExportQuote
    {
        public int? QuoteId { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string ClientName { get; set; } = "";

        public string ClientPhone { get; set; } = "";

        public DateTime DeliveryDate { get; set; }

        public DateTime? EventDate { get; set; }

        public string Title { get; set; } = "";

        public string Status { get; set; } = "";

        public decimal Deposit { get; set; }

        public List<ExportQuoteItem> Items { get; set; } = new();

        public decimal Total => Items.Sum(i => i.Price);

        public decimal PendingAmount
        {
            get
            {
                var pending = Total - Deposit;
                return pending < 0 ? 0 : pending;
            }
        }

        public string DisplayTitle
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Title))
                    return "Presupuesto";

                return Title;
            }
        }

        public string DisplayQuoteNumber
        {
            get
            {
                if (QuoteId == null || QuoteId <= 0)
                    return "";

                return QuoteNumberService.FormatQuoteLabel(
                    QuoteId.Value,
                    CreatedAt ?? DateTime.Now);
            }
        }

        public string ClientNotes { get; set; } = "";
    }
}
