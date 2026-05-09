using System;
using System.Collections.Generic;

namespace SastreriaPresupuestos.Models
{
    public class Quote
    {
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime DeliveryDate { get; set; }

        public string Title { get; set; } = "";

        public string Status { get; set; } = "Pendiente";

        public string Notes { get; set; } = "";

        public string ClientNotes { get; set; } = "";

        public decimal Total { get; set; }

        public int ClientId { get; set; }

        public Client? Client { get; set; }

        public List<QuoteItem> Items { get; set; } = new();

        public decimal Deposit { get; set; }

        public decimal PendingAmount
        {
            get
            {
                var pending = Total - Deposit;
                return pending < 0 ? 0 : pending;
            }
        }

        public string DisplayText
        {
            get
            {
                var titleText = string.IsNullOrWhiteSpace(Title)
                    ? $"Presupuesto #{Id}"
                    : Title;

                var statusText = string.IsNullOrWhiteSpace(Status)
                    ? "Pendiente"
                    : Status;

                return $"{DeliveryDate:dd/MM/yyyy} · {titleText} · {statusText} · {Total:N2} €";
            }
        }

        public string DisplayTitle
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Title))
                    return $"Presupuesto #{Id}";

                return Title;
            }
        }
    }
}