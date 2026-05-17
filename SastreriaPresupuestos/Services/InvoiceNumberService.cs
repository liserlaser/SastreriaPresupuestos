using System;
using System.Linq;
using SastreriaPresupuestos.Data;

namespace SastreriaPresupuestos.Services
{
    public static class InvoiceNumberService
    {
        public static int GetNextNumber(AppDbContext db, string series, DateTime createdAt)
        {
            var year = createdAt.Year;
            var yearStart = new DateTime(year, 1, 1);
            var nextYearStart = yearStart.AddYears(1);

            var lastNumber = db.Invoices
                .Where(invoice => invoice.Series == series
                    && invoice.CreatedAt >= yearStart
                    && invoice.CreatedAt < nextYearStart)
                .Select(invoice => (int?)invoice.Number)
                .Max();

            return (lastNumber ?? 0) + 1;
        }

        public static string Format(DateTime createdAt, string series, int number)
        {
            return $"{createdAt.Year}/{series}/{number:0000}";
        }
    }
}
