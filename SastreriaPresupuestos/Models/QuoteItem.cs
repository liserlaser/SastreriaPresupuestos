namespace SastreriaPresupuestos.Models
{
    public class QuoteItem
    {
        public int Id { get; set; }

        public string ProductName { get; set; } = "";

        public string TailoringType { get; set; } = "";

        public string Fabric { get; set; } = "";

        public decimal BasePrice { get; set; }

        public decimal FabricPrice { get; set; }

        public decimal ManualPrice { get; set; }

        public int Quantity { get; set; } = 1;

        public decimal Total { get; set; }

        public int QuoteId { get; set; }

        public Quote? Quote { get; set; }
    }
}