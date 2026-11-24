using Microsoft.EntityFrameworkCore;
using SastreriaPresupuestos.Data;
using SastreriaPresupuestos.Models;
using SastreriaPresupuestos.ViewModels;
using System;
using System.Linq;

namespace SastreriaPresupuestos.Services
{
    public static class ReportsService
    {
        public static ReportSummary Build(DateTime startDate, DateTime endDate, string reportType = "General", bool acceptedOnly = false, string periodType = "Semana")
        {
            var start = startDate.Date;
            var end = endDate.Date;

            using var db = new AppDbContext();
            var quotes = db.Quotes
                .AsNoTracking()
                .Include(q => q.Client)
                .Include(q => q.Deposits)
                .Include(q => q.Items)
                .ToList();

            var createdQuotes = quotes
                .Where(q => q.CreatedAt.Date >= start && q.CreatedAt.Date <= end)
                .ToList();

            string EffectiveStatus(Quote quote)
            {
                var status = string.IsNullOrWhiteSpace(quote.Status) ? "Pendiente" : quote.Status.Trim();
                if (string.Equals(status, "Pendiente", StringComparison.OrdinalIgnoreCase) &&
                    quote.EventDate.HasValue && quote.EventDate.Value.Date < DateTime.Today)
                    return "Rechazado";

                return status;
            }

            var createdStatuses = createdQuotes.Select(EffectiveStatus).ToList();
            var periodDeliveries = quotes
                .Where(q => q.EventDate.HasValue && q.EventDate.Value.Date >= start && q.EventDate.Value.Date <= end)
                .Where(q =>
                {
                    var status = EffectiveStatus(q);
                    return string.Equals(status, "Aceptado", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(status, "Entregado", StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(status, "Pendiente", StringComparison.OrdinalIgnoreCase);
                })
                .OrderBy(q => q.EventDate)
                .ThenBy(q => q.Client?.Name)
                .ToList();

            var eventPeriodClosedQuotes = periodDeliveries
                .Where(q => string.Equals(EffectiveStatus(q), "Aceptado", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(EffectiveStatus(q), "Entregado", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var displayedDeliveries = acceptedOnly || string.Equals(reportType, "Taller", StringComparison.OrdinalIgnoreCase)
                ? periodDeliveries.Where(q => string.Equals(EffectiveStatus(q), "Aceptado", StringComparison.OrdinalIgnoreCase)).ToList()
                : periodDeliveries;

            return new ReportSummary
            {
                StartDate = start,
                EndDate = end,
                ReportType = reportType,
                PeriodType = periodType,
                GeneratedAt = DateTime.Now,
                QuotesCreated = createdQuotes.Count,
                AcceptedQuotes = createdStatuses.Count(s => string.Equals(s, "Aceptado", StringComparison.OrdinalIgnoreCase)),
                PendingQuotes = createdStatuses.Count(s => string.Equals(s, "Pendiente", StringComparison.OrdinalIgnoreCase)),
                RejectedQuotes = createdStatuses.Count(s => string.Equals(s, "Rechazado", StringComparison.OrdinalIgnoreCase)),
                ConfirmedDeliveries = eventPeriodClosedQuotes.Count,
                PendingDeliveries = periodDeliveries.Count(q => string.Equals(EffectiveStatus(q), "Pendiente", StringComparison.OrdinalIgnoreCase)),
                ContractedTotal = eventPeriodClosedQuotes.Sum(q => q.Total),
                DepositsReceived = quotes.SelectMany(q => q.Deposits ?? new())
                    .Where(d => d.CreatedAt.Date >= start && d.CreatedAt.Date <= end)
                    .Sum(d => d.Amount),
                OutstandingTotal = eventPeriodClosedQuotes.Sum(q => q.PendingAmount),
                HasUnattributedLegacyDeposits = quotes.Any(q =>
                    q.Deposit > (q.Deposits?.Sum(d => d.Amount) ?? 0m)),
                Deliveries = displayedDeliveries.Select(q => new ReportDeliveryItem
                {
                    EventDate = q.EventDate!.Value.Date,
                    ClientName = q.Client?.Name ?? "Cliente",
                    Title = q.DisplayTitle,
                    FirstProduct = q.Items.OrderBy(item => item.Id).Select(item => string.IsNullOrWhiteSpace(item.TailoringType) ? item.ProductName : $"{item.ProductName} {item.TailoringType}").FirstOrDefault() ?? "Sin productos",
                    GroupLabel = GetGroupLabel(q.EventDate!.Value.Date, periodType),
                    Status = EffectiveStatus(q),
                    Total = q.Total,
                    Deposit = q.Deposit,
                    Balance = q.PendingAmount
                }).ToList()
            };
        }

        private static string GetGroupLabel(DateTime eventDate, string periodType)
        {
            var es = System.Globalization.CultureInfo.GetCultureInfo("es-ES");
            if (periodType == "Año")
                return eventDate.ToString("MMMM yyyy", es);
            if (periodType == "Mes")
            {
                var monday = eventDate.AddDays(-((7 + (int)eventDate.DayOfWeek - (int)DayOfWeek.Monday) % 7));
                var sunday = monday.AddDays(6);
                return $"Semana del {monday:dd/MM} al {sunday:dd/MM}";
            }
            return "";
        }
    }
}
