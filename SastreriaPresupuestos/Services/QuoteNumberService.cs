using System;

namespace SastreriaPresupuestos.Services
{
    public static class QuoteNumberService
    {
        public const string QuoteSeries = "P";

        public static string FormatQuoteNumber(int quoteId, DateTime createdAt)
        {
            if (quoteId <= 0)
                return "";

            return $"{createdAt.Year}/{QuoteSeries}/{quoteId:0000}";
        }

        public static string FormatQuoteLabel(int quoteId, DateTime createdAt)
        {
            var quoteNumber = FormatQuoteNumber(quoteId, createdAt);

            return string.IsNullOrWhiteSpace(quoteNumber)
                ? ""
                : $"Presupuesto {quoteNumber}";
        }
    }
}
