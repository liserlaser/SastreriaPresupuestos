using System;
using System.Globalization;
using System.Windows.Media;

namespace SastreriaPresupuestos.ViewModels
{
    public class WeeklyDeliveryItem
    {
        public int QuoteId { get; set; }

        public int ClientId { get; set; }

        public DateTime DeliveryDate { get; set; }

        public string DayText =>
            CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                DeliveryDate.ToString("dddd dd/MM"));

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

        public Brush StatusBackground
        {
            get
            {
                return Status switch
                {
                    "Aceptado" => new SolidColorBrush(Color.FromRgb(232, 242, 232)),
                    "Entregado" => new SolidColorBrush(Color.FromRgb(235, 235, 235)),
                    "Rechazado" => new SolidColorBrush(Color.FromRgb(243, 229, 226)),
                    _ => new SolidColorBrush(Color.FromRgb(238, 242, 234))
                };
            }
        }

        public Brush StatusForeground
        {
            get
            {
                return Status switch
                {
                    "Aceptado" => new SolidColorBrush(Color.FromRgb(45, 92, 50)),
                    "Entregado" => new SolidColorBrush(Color.FromRgb(80, 80, 80)),
                    "Rechazado" => new SolidColorBrush(Color.FromRgb(122, 46, 46)),
                    _ => new SolidColorBrush(Color.FromRgb(47, 51, 45))
                };
            }
        }
    }
}