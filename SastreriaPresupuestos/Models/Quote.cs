using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using SastreriaPresupuestos.Services;

namespace SastreriaPresupuestos.Models
{
    public class Quote
    {
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public DateTime DeliveryDate { get; set; }

        public DateTime? EventDate { get; set; }

        public string Title { get; set; } = "";

        public string Status { get; set; } = "Pendiente";

        public string Notes { get; set; } = "";

        public string ClientNotes { get; set; } = "";

        public decimal Total { get; set; }

        public int ClientId { get; set; }

        public Client? Client { get; set; }

        public List<QuoteItem> Items { get; set; } = new();

        public List<QuoteDeposit> Deposits { get; set; } = new();

        public decimal Deposit { get; set; }

        public decimal PendingAmount
        {
            get
            {
                var pending = Total - Deposit;
                return pending < 0 ? 0 : pending;
            }
        }

        public string DisplayText
        {
            get
            {
                var statusText = string.IsNullOrWhiteSpace(Status)
                    ? "Pendiente"
                    : Status;

                var eventText = EventDate.HasValue
                    ? EventDate.Value.ToString("dd/MM/yyyy")
                    : "Sin fecha de evento";

                return $"{eventText} · {DisplayTitle} · {statusText} · {Total:N2} €";
            }
        }

        public string EventDateText => EventDate.HasValue
            ? $"Evento · {EventDate.Value:dd/MM/yyyy}"
            : "Evento · —";

        public DateTime SortDate => EventDate ?? DateTime.MaxValue;

        public string DisplayTitle
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Title))
                    return QuoteNumberService.FormatQuoteNumber(Id, CreatedAt);

                return Title;
            }
        }

        public string ProductSummary
        {
            get
            {
                if (Items == null || Items.Count == 0)
                    return "Sin productos";

                return string.Join(", ", Items.Select(item =>
                {
                    if (string.IsNullOrWhiteSpace(item.TailoringType))
                        return item.ProductName;

                    return $"{item.ProductName} {item.TailoringType}";
                }));
            }
        }

        public string StatusLabelText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Status))
                    return "PENDIENTE";

                return Status.Trim().ToUpperInvariant();
            }
        }

        public bool IsAccepted =>
            string.Equals(Status?.Trim(), "Aceptado", StringComparison.OrdinalIgnoreCase);

        public Brush CardBackground
        {
            get
            {
                return Status?.Trim() switch
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
                return Status?.Trim() switch
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
                return Status?.Trim() switch
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
                return Status?.Trim() switch
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
                return Status?.Trim() switch
                {
                    "Aceptado" => Brushes.White,
                    "Entregado" => Brushes.White,
                    "Rechazado" => Brushes.White,
                    _ => new SolidColorBrush(Color.FromRgb(47, 51, 45))
                };
            }
        }


        public bool IsOverdue
        {
            get
            {
                var statusText = Status?.Trim() ?? "";
                var isClosed =
                    string.Equals(statusText, "Entregado", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(statusText, "Rechazado", StringComparison.OrdinalIgnoreCase);

                return !isClosed && EventDate.HasValue && EventDate.Value.Date < DateTime.Today;
            }
        }

        public Visibility OverdueBadgeVisibility => IsOverdue ? Visibility.Visible : Visibility.Collapsed;

        public string DeliveryBadgeText => IsOverdue
            ? "ATRASADO"
            : EventDate.HasValue ? $"{EventDate.Value:dd/MM}" : "SIN EVENTO";

        public Brush DeliveryBadgeBackground
        {
            get
            {
                return IsOverdue
                    ? new SolidColorBrush(Color.FromRgb(142, 63, 51))
                    : new SolidColorBrush(Color.FromRgb(238, 242, 234));
            }
        }

        public Brush DeliveryBadgeForeground
        {
            get
            {
                return IsOverdue
                    ? Brushes.White
                    : new SolidColorBrush(Color.FromRgb(47, 51, 45));
            }
        }

        public bool HasPendingPayment => PendingAmount > 0;

        public Visibility PendingPaymentVisibility => HasPendingPayment ? Visibility.Visible : Visibility.Collapsed;

        public string PendingPaymentText => HasPendingPayment
            ? $"Pendiente · {PendingAmount:N2} €"
            : "Pagado";

        public Brush PendingPaymentForeground
        {
            get
            {
                return HasPendingPayment
                    ? new SolidColorBrush(Color.FromRgb(142, 63, 51))
                    : new SolidColorBrush(Color.FromRgb(93, 135, 88));
            }
        }

        public FontWeight TitleWeight
        {
            get
            {
                return IsAccepted ? FontWeights.Bold : FontWeights.SemiBold;
            }
        }
    }
}
