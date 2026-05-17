namespace SastreriaPresupuestos.Models
{
    public class InvoiceItem
    {
        public int Id { get; set; }

        public string ProductName { get; set; } = "";

        public string Description { get; set; } = "";

        public int Quantity { get; set; } = 1;

        public decimal UnitPrice { get; set; }

        public decimal Total { get; set; }

        public int InvoiceId { get; set; }

        public Invoice? Invoice { get; set; }
    }
}
