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

        public string ClientTaxId { get; set; } = "";

        public string ClientAddress { get; set; } = "";

        public decimal InvoiceTotal { get; set; }

        public List<ExportInvoiceItem> Items { get; set; } = new();

        public bool IsCompanyInvoice =>
            Series == Models.InvoiceSeries.Company ||
            Series == Models.InvoiceSeries.CorrectiveCompany;

        public decimal ItemsTotal => Items.Sum(item => item.Total);

        public decimal Total => IsCompanyInvoice
            ? TaxBase + VatAmount
            : InvoiceTotal != 0 ? InvoiceTotal : ItemsTotal;

        public decimal TaxBase => IsCompanyInvoice ? ItemsTotal : Total / 1.21m;

        public decimal VatAmount => IsCompanyInvoice ? TaxBase * 0.21m : Total - TaxBase;
    }
}
