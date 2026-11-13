using System;
using System.Collections.Generic;
using System.Globalization;

namespace SastreriaPresupuestos.ViewModels
{
    public sealed class ReportSummary
    {
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public DateTime GeneratedAt { get; init; }
        public int QuotesCreated { get; init; }
        public int AcceptedQuotes { get; init; }
        public int PendingQuotes { get; init; }
        public int RejectedQuotes { get; init; }
        public int ConfirmedDeliveries { get; init; }
        public int PendingDeliveries { get; init; }
        public decimal ContractedTotal { get; init; }
        public decimal DepositsReceived { get; init; }
        public decimal OutstandingTotal { get; init; }
        public bool HasUnattributedLegacyDeposits { get; init; }
        public List<ReportDeliveryItem> Deliveries { get; init; } = new();

        public string PeriodLabel => StartDate.Date == EndDate.Date
            ? StartDate.ToString("dddd dd 'de' MMMM 'de' yyyy", CultureInfo.GetCultureInfo("es-ES"))
            : $"{StartDate:dd/MM/yyyy} – {EndDate:dd/MM/yyyy}";

        public string AcceptedPercentage => FormatPercentage(AcceptedQuotes);
        public string PendingPercentage => FormatPercentage(PendingQuotes);
        public string RejectedPercentage => FormatPercentage(RejectedQuotes);

        private string FormatPercentage(int count) => QuotesCreated == 0
            ? "0 %"
            : $"{count * 100m / QuotesCreated:N1} %";
    }

    public sealed class ReportDeliveryItem
    {
        public DateTime EventDate { get; init; }
        public string ClientName { get; init; } = "";
        public string Title { get; init; } = "";
        public string Status { get; init; } = "";
        public decimal Total { get; init; }
        public decimal Deposit { get; init; }
        public decimal Balance { get; init; }
    }
}
