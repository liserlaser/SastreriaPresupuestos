using System;

namespace SastreriaPresupuestos.ViewModels
{
    public class WeeklyDeliveryItem
    {
        public int QuoteId { get; set; }

        public int ClientId { get; set; }

        public DateTime DeliveryDate { get; set; }

        public string DayText => DeliveryDate.ToString("dddd dd/MM");

        public string ClientName { get; set; } = "";

        public string ClientPhone { get; set; } = "";

        public string QuoteTitle { get; set; } = "";

        public string Status { get; set; } = "Pendiente";

        public decimal Total { get; set; }

        public string DisplayTitle
        {
            get
            {
                if (string.IsNullOrWhiteSpace(QuoteTitle))
                    return $"Presupuesto #{QuoteId}";

                return QuoteTitle;
            }
        }
    }
}