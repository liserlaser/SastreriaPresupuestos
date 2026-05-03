using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SastreriaPresupuestos.Models
{
    public class PriceItem
    {
        public int Id { get; set; }

        public string ProductName { get; set; } = "";

        public string TailoringType { get; set; } = "";

        public decimal Price { get; set; }

        public bool IsAccessory { get; set; }
    }
}