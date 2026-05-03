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

        public string ProductName
        {
            get => _productName;
            set
            {
                _productName = value;

                UpdateTailoringOptions();

                Notify(nameof(ProductName));
                Notify(nameof(AvailableTailoringTypes));

                RefreshPrice();
            }
        }

        public string TailoringType
        {
            get => _tailoringType;
            set
            {
                _tailoringType = value;
                Notify(nameof(TailoringType));

                RefreshPrice();
            }
        }

        public string Fabric
        {
            get => _fabric;
            set
            {
                _fabric = value;
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
            var calculatedTotal = (BasePrice + FabricPrice + ManualPrice) * Quantity;

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
            UpdateBasePrice();
            UpdateTotal();
        }

        public ObservableCollection<string> AvailableTailoringTypes { get; set; } = new();

        private void UpdateTailoringOptions()
        {
            AvailableTailoringTypes.Clear();

            if (ProductName == "Camisa")
            {
                AvailableTailoringTypes.Add("Confeccion");
                AvailableTailoringTypes.Add("Medida");

                if (TailoringType != "Confeccion" && TailoringType != "Medida")
                    TailoringType = "Confeccion";

                return;
            }

            if (ProductName == "Corbata" ||
                ProductName == "Pañuelo" ||
                ProductName == "Gemelos" ||
                ProductName == "Tirantes" ||
                ProductName == "Zapatos" ||
                ProductName == "Concepto Libre")
            {
                TailoringType = "";
                return;
            }

            AvailableTailoringTypes.Add("Confeccion");
            AvailableTailoringTypes.Add("Medida");
            AvailableTailoringTypes.Add("Artesanal");

            if (string.IsNullOrWhiteSpace(TailoringType))
                TailoringType = "Confeccion";
        }
    }
}