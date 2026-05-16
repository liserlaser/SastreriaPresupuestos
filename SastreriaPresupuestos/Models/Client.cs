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

        public string Dni { get; set; } = "";

        public List<Quote> Quotes { get; set; } = new();

        public DateTime? NextEventDate
        {
            get
            {
                if (Quotes == null || Quotes.Count == 0)
                    return null;

                return Quotes
                    .Where(q => q.EventDate.HasValue)
                    .OrderBy(q => q.EventDate.GetValueOrDefault())
                    .FirstOrDefault()
                    ?.EventDate;
            }
        }

        public string EventDateText
        {
            get
            {
                if (NextEventDate == null)
                    return "Sin evento";

                return NextEventDate.Value.ToString("dd/MM/yyyy");
            }
        }

        public string DisplayPhone
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Phone))
                    return "";

                var digits = new string(Phone.Where(char.IsDigit).ToArray());

                // Si viene como +34 600 12 34 56, quitamos el prefijo español
                if (digits.Length == 11 && digits.StartsWith("34"))
                {
                    digits = digits.Substring(2);
                }

                if (digits.Length == 9)
                {
                    return $"{digits.Substring(0, 3)} {digits.Substring(3, 2)} {digits.Substring(5, 2)} {digits.Substring(7, 2)}";
                }

                return Phone;
            }
        }

    }
}