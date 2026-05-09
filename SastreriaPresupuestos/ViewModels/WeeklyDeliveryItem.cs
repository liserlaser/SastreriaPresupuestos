using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace SastreriaPresupuestos.ViewModels
{
    public class WeeklyDeliveryItem
    {
        public int QuoteId { get; set; }

        public int ClientId { get; set; }

        public DateTime DeliveryDate { get; set; }

        public string DayText => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
            DeliveryDate.ToString("dddd dd/MM"));

        public string ClientName { get; set; } = "";

        public string ClientPhone { get; set; } = "";

        public string QuoteTitle { get; set; } = "";

        public string Status { get; set; } = "Pendiente";

        public decimal Deposit { get; set; }

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

        public bool IsAccepted =>
            string.Equals(NormalizedStatus, "Aceptado", StringComparison.OrdinalIgnoreCase);

        public string StatusLabelText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(NormalizedStatus))
                    return "PENDIENTE";

                return NormalizedStatus.ToUpperInvariant();
            }
        }

        public Brush CardBackground
        {
            get
            {
                return NormalizedStatus switch
                {
                    "Aceptado" => new SolidColorBrush(Color.FromRgb(236, 245, 234)),
                    "Entregado" => new SolidColorBrush(Color.FromRgb(238, 240, 236)),
                    "Rechazado" => new SolidColorBrush(Color.FromRgb(247, 235, 232)),
                    _ => Brushes.White
                };
            }
        }

        public Brush CardBorderBrush
        {
            get
            {
                return NormalizedStatus switch
                {
                    "Aceptado" => new SolidColorBrush(Color.FromRgb(93, 135, 88)),
                    "Entregado" => new SolidColorBrush(Color.FromRgb(120, 130, 112)),
                    "Rechazado" => new SolidColorBrush(Color.FromRgb(170, 100, 90)),
                    _ => new SolidColorBrush(Color.FromRgb(215, 221, 210))
                };
            }
        }

        public Thickness CardBorderThickness
        {
            get
            {
                return NormalizedStatus switch
                {
                    "Aceptado" => new Thickness(4, 1, 1, 1),
                    "Entregado" => new Thickness(3, 1, 1, 1),
                    "Rechazado" => new Thickness(3, 1, 1, 1),
                    _ => new Thickness(1)
                };
            }
        }

        public Brush StatusBackground
        {
            get
            {
                return NormalizedStatus switch
                {
                    "Aceptado" => new SolidColorBrush(Color.FromRgb(93, 135, 88)),
                    "Entregado" => new SolidColorBrush(Color.FromRgb(120, 130, 112)),
                    "Rechazado" => new SolidColorBrush(Color.FromRgb(170, 100, 90)),
                    _ => new SolidColorBrush(Color.FromRgb(238, 242, 234))
                };
            }
        }

        public Brush StatusForeground
        {
            get
            {
                return NormalizedStatus switch
                {
                    "Aceptado" => Brushes.White,
                    "Entregado" => Brushes.White,
                    "Rechazado" => Brushes.White,
                    _ => new SolidColorBrush(Color.FromRgb(47, 51, 45))
                };
            }
        }

        public FontWeight ClientNameWeight
        {
            get
            {
                return IsAccepted ? FontWeights.Bold : FontWeights.SemiBold;
            }
        }

        private string NormalizedStatus
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Status))
                    return "Pendiente";

                return Status.Trim();
            }
        }
    }
}