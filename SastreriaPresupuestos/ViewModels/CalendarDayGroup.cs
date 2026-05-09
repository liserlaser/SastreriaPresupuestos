using System;
using System.Collections.ObjectModel;
using System.Globalization;

namespace SastreriaPresupuestos.ViewModels
{
    public class CalendarDayGroup
    {
        public DateTime Date { get; set; }

        public string DayTitle =>
            CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                Date.ToString("dddd dd/MM"));

        public ObservableCollection<WeeklyDeliveryItem> Deliveries { get; set; } =
            new ObservableCollection<WeeklyDeliveryItem>();

        public bool HasDeliveries => Deliveries.Count > 0;
    }
}