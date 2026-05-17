using System;

namespace SastreriaPresupuestos.ViewModels
{
    public class InvoiceListItem
    {
        public int InvoiceId { get; set; }

        public string InvoiceNumber { get; set; } = "";

        public DateTime CreatedAt { get; set; }

        public string ClientName { get; set; } = "";

        public string Summary { get; set; } = "";

        public decimal Total { get; set; }

        public string Series { get; set; } = "";
    }
}
