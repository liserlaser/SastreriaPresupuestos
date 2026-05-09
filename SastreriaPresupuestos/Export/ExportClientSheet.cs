using System;

namespace SastreriaPresupuestos.Export
{
    public class ExportClientSheet
    {
        public int QuoteId { get; set; }

        public string ClientName { get; set; } = "";

        public string ClientDni { get; set; } = "";

        public string ClientPhone { get; set; } = "";

        public string OrderTitle { get; set; } = "";

        public DateTime EventDate { get; set; }

        public decimal Deposit { get; set; }

        public string Observations { get; set; } = "";

        public string DisplayQuoteNumber
        {
            get
            {
                if (QuoteId <= 0)
                    return "";

                return $"Presupuesto #{QuoteId:0000}";
            }
        }

        public string DisplayOrderTitle
        {
            get
            {
                if (string.IsNullOrWhiteSpace(OrderTitle))
                    return DisplayQuoteNumber;

                return OrderTitle;
            }
        }
    }
}