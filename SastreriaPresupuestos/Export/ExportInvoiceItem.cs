namespace SastreriaPresupuestos.Export
{
    public class ExportInvoiceItem
    {
        public string ProductName { get; set; } = "";

        public string Description { get; set; } = "";

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Total { get; set; }
    }
}
