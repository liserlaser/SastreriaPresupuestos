namespace SastreriaPresupuestos.Export
{
    public class ExportQuoteItem
    {
        public string Product { get; set; } = "";

        public string Description { get; set; } = "";

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public bool IsFabricLine { get; set; } = false;
    }
}