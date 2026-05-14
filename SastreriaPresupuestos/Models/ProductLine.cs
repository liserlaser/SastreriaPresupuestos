using System.Collections.ObjectModel;
using System.ComponentModel;
using SastreriaPresupuestos.Services;

namespace SastreriaPresupuestos.Models
{
    public class ProductLine : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _productName = "";
        private string _tailoringType = "";
        private string _fabric = "";
        private decimal _basePrice;
        private decimal _fabricPrice;
        private decimal _manualPrice;
        private decimal _total;
        private int _quantity = 1;
        private bool _suppressPriceRefresh;

        public string ProductName
        {
            get => _productName;
            set
            {
                _productName = value ?? string.Empty;

                UpdateTailoringOptions();

                Notify(nameof(ProductName));
                Notify(nameof(AvailableTailoringTypes));

                if (!_suppressPriceRefresh)
                    RefreshPrice();
            }
        }

        public string TailoringType
        {
            get => _tailoringType;
            set
            {
                _tailoringType = value ?? string.Empty;
                Notify(nameof(TailoringType));

                if (!_suppressPriceRefresh)
                    RefreshPrice();
            }
        }

        public string Fabric
        {
            get => _fabric;
            set
            {
                _fabric = value ?? string.Empty;
                Notify(nameof(Fabric));

                // aquí puedes añadir lógica futura de tejidos
            }
        }

        public decimal BasePrice
        {
            get => _basePrice;
            set
            {
                _basePrice = value < 0 ? 0 : value;

                UpdateTotal();

                Notify(nameof(BasePrice));
            }
        }

        public decimal FabricPrice
        {
            get => _fabricPrice;
            set
            {
                _fabricPrice = value < 0 ? 0 : value;

                UpdateTotal();

                Notify(nameof(FabricPrice));
            }
        }

        public decimal ManualPrice
        {
            get => _manualPrice;
            set
            {
                _manualPrice = value;

                UpdateTotal();

                Notify(nameof(ManualPrice));
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (value < 1)
                    value = 1;

                _quantity = value;

                UpdateTotal();

                Notify(nameof(Quantity));
            }
        }

            // se sugirió cambiar por:
            //set
            //{
            //    _quantity = value;
            //    Notify(nameof(Quantity));
            //    RefreshPrice();
            //}
        

        public decimal Total
        {
            get => _total;
            set
            {
                _total = value;
                Notify(nameof(Total));
            }
        }

        private void UpdateBasePrice()
        {
            if (ProductName == "Concepto Libre")
            {
                BasePrice = 0;
                return;
            }

            BasePrice = PriceService.GetPrice(ProductName, TailoringType);
        }

        public void UpdateTotal()
        {
            // El campo Precio es editable y sustituye al antiguo Ajuste.
            // El total se calcula directamente desde Precio + Tejido, sin una segunda celda manual.
            var calculatedTotal = (BasePrice + FabricPrice) * Quantity;

            if (calculatedTotal < 0)
                calculatedTotal = 0;

            Total = calculatedTotal;
        }

        private void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RefreshPrice()
        {
            // Solo debe llamarse cuando cambia el producto o el tipo de confección.
            // No debe ejecutarse al editar cantidad/precio, porque sobrescribiría
            // el precio final introducido manualmente con la tarifa base.
            UpdateBasePrice();
            UpdateTotal();
        }

        public ObservableCollection<string> AvailableTailoringTypes { get; set; } = new();

        public static ProductLine FromQuoteItem(QuoteItem item)
        {
            var line = new ProductLine();

            line.LoadPersistedValues(
                item.ProductName,
                item.TailoringType,
                item.Fabric,
                item.ManualPrice > 0 ? item.ManualPrice : item.BasePrice,
                item.FabricPrice,
                item.Quantity,
                item.Total);

            return line;
        }

        public void LoadPersistedValues(
            string? productName,
            string? tailoringType,
            string? fabric,
            decimal basePrice,
            decimal fabricPrice,
            int quantity,
            decimal total)
        {
            _suppressPriceRefresh = true;

            _productName = productName ?? string.Empty;
            _tailoringType = tailoringType ?? string.Empty;
            _fabric = fabric ?? string.Empty;
            _basePrice = basePrice < 0 ? 0 : basePrice;
            _fabricPrice = fabricPrice < 0 ? 0 : fabricPrice;
            _manualPrice = 0;
            _quantity = quantity < 1 ? 1 : quantity;
            _total = total < 0 ? 0 : total;

            RefreshTailoringOptionsWithoutChangingSelection();

            Notify(nameof(ProductName));
            Notify(nameof(TailoringType));
            Notify(nameof(AvailableTailoringTypes));
            Notify(nameof(Fabric));
            Notify(nameof(BasePrice));
            Notify(nameof(FabricPrice));
            Notify(nameof(ManualPrice));
            Notify(nameof(Quantity));
            Notify(nameof(Total));

            _suppressPriceRefresh = false;
        }

        private void UpdateTailoringOptions()
        {
            AvailableTailoringTypes.Clear();

            var tailoringTypes = PriceService.GetTailoringTypes(ProductName);

            foreach (var tailoringType in tailoringTypes)
                AvailableTailoringTypes.Add(tailoringType);

            if (tailoringTypes.Count == 0)
            {
                TailoringType = "";
                return;
            }

            if (!tailoringTypes.Contains(TailoringType))
                TailoringType = tailoringTypes[0];
        }

        private void RefreshTailoringOptionsWithoutChangingSelection()
        {
            AvailableTailoringTypes.Clear();

            var tailoringTypes = PriceService.GetTailoringTypes(ProductName);

            foreach (var tailoringType in tailoringTypes)
                AvailableTailoringTypes.Add(tailoringType);
        }
    }
}