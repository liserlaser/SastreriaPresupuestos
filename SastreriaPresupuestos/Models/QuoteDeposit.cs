using System;

namespace SastreriaPresupuestos.Models
{
    public class QuoteDeposit
    {
        public int Id { get; set; }

        public int QuoteId { get; set; }

        public Quote? Quote { get; set; }

        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
