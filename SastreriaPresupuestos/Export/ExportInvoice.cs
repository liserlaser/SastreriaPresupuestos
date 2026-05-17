using System;
using System.Collections.Generic;
using System.Linq;

namespace SastreriaPresupuestos.Export
{
    public class ExportInvoice
    {
        public string InvoiceNumber { get; set; } = "";

        public string Series { get; set; } = "";

        public DateTime CreatedAt { get; set; }

        public string ClientName { get; set; } = "";

        public List<ExportInvoiceItem> Items { get; set; } = new();

        public decimal Total => Items.Sum(item => item.Total);

        public decimal TaxBase => Total / 1.21m;

        public decimal VatAmount => Total - TaxBase;
    }
}
