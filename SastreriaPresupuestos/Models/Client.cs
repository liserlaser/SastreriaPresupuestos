using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace SastreriaPresupuestos.Models
{
    public class Client
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public string Phone { get; set; } = "";

        public List<Quote> Quotes { get; set; } = new();

        public DateTime? NextDeliveryDate
        {
            get
            {
                if (Quotes == null || Quotes.Count == 0)
                    return null;

                return Quotes
                    .OrderBy(q => q.DeliveryDate)
                    .First()
                    .DeliveryDate;
            }
        }

        public string DeliveryDateText
        {
            get
            {
                if (NextDeliveryDate == null)
                    return "Sin fecha";

                return NextDeliveryDate.Value.ToString("dd/MM/yyyy");
            }
        }

    }
}