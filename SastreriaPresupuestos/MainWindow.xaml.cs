using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using QuestPDF.Infrastructure;
using SastreriaPresupuestos.Data;
using SastreriaPresupuestos.Models;
using SastreriaPresupuestos.Services;
using SastreriaPresupuestos.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace SastreriaPresupuestos
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<ProductLine> Products =
            new ObservableCollection<ProductLine>();

        private List<Models.Client> AllClients = new();

        private Models.Quote? CurrentQuote = null;

        private string ActiveStatusFilter = "Todos";
        private string ActiveDeliveryFilter = "Todas";

        //private bool MarkAsSaved();
        private bool HasUnsavedChanges = false;
        private bool IsLoadingData = false;
        private bool IsConfirmingDiscard = false;

        private int? LastSelectedClientId = null;
        private int? LastSelectedQuoteId = null;

        private bool IsRevertingSelection = false;
        private bool SuppressSelectionConfirm = false;

        public MainWindow()
        {
            var culture = new CultureInfo("es-ES");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            InitializeComponent();

            QuestPDF.Settings.License = LicenseType.Community;

            PriceService.LoadPrices();

            //var culture = new CultureInfo("es-ES");
            //Thread.CurrentThread.CurrentCulture = culture;
            //Thread.CurrentThread.CurrentUICulture = culture;

            LoadClients();

            ProductsDataGrid.ItemsSource = Products;

            Products.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (ProductLine item in e.NewItems)
                    {
                        item.PropertyChanged += (a, b) =>
                        {
                            UpdateGrandTotal();
                            MarkAsChanged();
                        };
                    }
                }

                UpdateGrandTotal();
                MarkAsChanged();
            };

            AddProductButton.Click += AddProductButton_Click;
            SaveButton.Click += SaveButton_Click;
            NewQuoteButton.Click += NewQuoteButton_Click;
            NewClientButton.Click += NewClientButton_Click;
            SaveClientButton.Click += SaveClientButton_Click;
            DeleteClientButton.Click += DeleteClientButton_Click;
            DuplicateQuoteButton.Click += DuplicateQuoteButton_Click;
            PricesConfigButton.Click += PricesConfigButton_Click;
            BackupButton.Click += BackupButton_Click;
            ExportPdfButton.Click += ExportPdfButton_Click;
            ExportAllQuotesPdfButton.Click += ExportAllQuotesPdfButton_Click;

            DepositTextBox.TextChanged += (s, e) =>
            {
                UpdatePendingAmount();
            };

            ClientsListBox.SelectionChanged += ClientsListBox_SelectionChanged;
            QuotesListBox.SelectionChanged += QuotesListBox_SelectionChanged;
            //ClientSearchTextBox.TextChanged += ClientSearchTextBox_TextChanged;

            ProductsDataGrid.CellEditEnding += ProductsDataGrid_CellEditEnding;

            ClientNameTextBox.TextChanged += (s, e) => MarkAsChanged();
            PhoneTextBox.TextChanged += (s, e) => MarkAsChanged();
            QuoteTitleTextBox.TextChanged += (s, e) => MarkAsChanged();
            QuoteNotesTextBox.TextChanged += (s, e) => MarkAsChanged();
            DepositTextBox.TextChanged += (s, e) => MarkAsChanged();
            DeliveryDatePicker.SelectedDateChanged += (s, e) => MarkAsChanged();
            QuoteStatusComboBox.SelectionChanged += (s, e) => MarkAsChanged();

            UpdateSaveButtonText();
            UpdateUnsavedChangesIndicator();

            UpdateWindowTitle();

            ClientSearchPlaceholderTextBlock.Visibility =
                string.IsNullOrWhiteSpace(ClientSearchTextBox.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            UpdateStatusFilterButtons();
            UpdateDeliveryFilterButtons();

        }

        private List<string> GetTailoringOptions(string product)
        {
            if (product == "Camisa")
                return new List<string> { "Confeccion", "Medida" };

            if (product == "Corbata" ||
                product == "Pañuelo" ||
                product == "Gemelos" ||
                product == "Tirantes" ||
                product == "Zapatos" ||
                product == "Concepto Libre")
            {
                return new List<string>();
            }

            return new List<string> { "Confeccion", "Medida", "Artesanal" };
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            var line = new ProductLine()
            {
                ProductName = "Concepto Libre",
                TailoringType = "",
                Fabric = "",
                BasePrice = 0,
                FabricPrice = 0,
                ManualPrice = 0,
                Quantity = 1
            };

            Products.Add(line);

            UpdateGrandTotal();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateBeforeSave())
                return;

            using var db = new AppDbContext();

            var clientName = ClientNameTextBox.Text.Trim();
            var clientPhone = PhoneTextBox.Text.Trim();
            var quoteTitle = QuoteTitleTextBox.Text.Trim();
            var quoteNotes = QuoteNotesTextBox.Text.Trim();
            var clientNotes = ClientNotesTextBox.Text.Trim();

            var selectedStatus = QuoteStatusComboBox.SelectedItem?.ToString();

            if (string.IsNullOrWhiteSpace(selectedStatus))
                selectedStatus = "Pendiente";

            var selectedClient = ClientsListBox.SelectedItem as Models.Client;
            var currentClientId = selectedClient?.Id;

            var possibleDuplicate = FindPossibleDuplicateClient(
                db,
                clientName,
                clientPhone,
                currentClientId);

            if (possibleDuplicate != null && selectedClient == null)
            {
                var result = MessageBox.Show(
                    $"Ya existe un cliente parecido:\n\n" +
                    $"{possibleDuplicate.Name}\n" +
                    $"{possibleDuplicate.Phone}\n\n" +
                    "¿Quieres crear otro cliente igualmente?",
                    "Posible cliente duplicado",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            // Buscar cliente existente
            var client = db.Clients
                .FirstOrDefault(c => c.Phone == clientPhone);

            // Crear cliente si no existe
            if (client == null)
            {
                client = new Models.Client()
                {
                    Name = clientName,
                    Phone = clientPhone
                };

                db.Clients.Add(client);

                db.SaveChanges();
            }

            else
            {
                client.Name = clientName;
                db.SaveChanges();
            }

            Models.Quote quote;

            // NUEVO PRESUPUESTO
            if (CurrentQuote == null)
            {
                quote = new Models.Quote()
                {
                    ClientId = client.Id,
                    DeliveryDate = DeliveryDatePicker.SelectedDate ?? DateTime.Now,
                    //Title = QuoteTitleTextBox.Text,
                    //Status = QuoteStatusComboBox.SelectedItem?.ToString() ?? "Pendiente",
                    Title = quoteTitle,
                    Status = selectedStatus,
                    Deposit = GetDepositValue(),
                    //Notes = QuoteNotesTextBox.Text,
                    Notes = quoteNotes,
                    ClientNotes = clientNotes,
                    Total = Products.Sum(p => p.Total)
                };

                db.Quotes.Add(quote);

                db.SaveChanges();
            }

            // EDITAR PRESUPUESTO EXISTENTE
            else
            {
                quote = db.Quotes
                    .Include(q => q.Items)
                    .First(q => q.Id == CurrentQuote.Id);

                quote.Total = Products.Sum(p => p.Total);
                quote.DeliveryDate = DeliveryDatePicker.SelectedDate ?? DateTime.Now;
                //quote.Title = QuoteTitleTextBox.Text;
                //quote.Status = QuoteStatusComboBox.SelectedItem?.ToString() ?? "Pendiente";
                //quote.Notes = QuoteNotesTextBox.Text;
                quote.Title = quoteTitle;
                quote.Notes = quoteNotes;
                quote.ClientNotes = clientNotes;
                quote.Status = selectedStatus;
                quote.Deposit = GetDepositValue();
                

                // Eliminar líneas antiguas
                db.QuoteItems.RemoveRange(quote.Items);

                db.SaveChanges();

                quote.Items.Clear();
            }

            // Guardar líneas nuevas
            foreach (var product in Products)
            {
                var item = new Models.QuoteItem()
                {
                    QuoteId = quote.Id,
                    Quantity = product.Quantity,
                    ProductName = product.ProductName,
                    TailoringType = product.TailoringType,
                    Fabric = product.Fabric,
                    BasePrice = product.BasePrice,
                    FabricPrice = product.FabricPrice,
                    ManualPrice = product.ManualPrice,
                    Total = product.Total
                };

                db.QuoteItems.Add(item);
            }

            db.SaveChanges();

            int savedClientId = client.Id;
            int savedQuoteId = quote.Id;

            SuppressSelectionConfirm = true;

            LoadClients();

            var reloadedClient = ((IEnumerable<Models.Client>)ClientsListBox.ItemsSource)
                .FirstOrDefault(c => c.Id == savedClientId);

            if (reloadedClient != null)
            {
                ClientsListBox.SelectedItem = reloadedClient;

                using var refreshDb = new AppDbContext();

                var reloadedQuotes = refreshDb.Quotes
                    .Where(q => q.ClientId == savedClientId)
                    .OrderBy(q => q.DeliveryDate)
                    .ThenBy(q => q.Id)
                    .ToList();

                QuotesListBox.ItemsSource = reloadedQuotes;

                var reloadedQuote = reloadedQuotes
                    .FirstOrDefault(q => q.Id == savedQuoteId);

                if (reloadedQuote != null)
                {
                    QuotesListBox.SelectedItem = reloadedQuote;
                    CurrentQuote = reloadedQuote;

                    UpdateSaveButtonText();
                }
            }

            SuppressSelectionConfirm = false;

            MarkAsSaved();

            UpdateSaveButtonText();

            MessageBox.Show(
                "Presupuesto guardado correctamente",
                "Éxito",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void UpdateGrandTotal()
        {
            decimal total = 0;

            foreach (var item in Products)
            {
                total += item.Total;
            }

            TotalTextBlock.Text = $"{total:N2} €";

            UpdatePendingAmount();
        }

        private void LoadClients()
        {
            using var db = new AppDbContext();

            AllClients = db.Clients
                .Include(c => c.Quotes)
                .ToList()
                .OrderBy(c => c.NextDeliveryDate ?? DateTime.MaxValue)
                .ThenBy(c => c.Name)
                .ToList();

            ApplyClientFilter();
        }

        private void ClientsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (IsRevertingSelection)
                return;

            var client = ClientsListBox.SelectedItem as Models.Client;

            if (client == null)
                return;

            if (!CanChangeSelection())
            {
                RevertClientSelection();
                return;
            }

            IsLoadingData = true;

            ClientNameTextBox.Text = client.Name;
            PhoneTextBox.Text = client.Phone;

            using var db = new AppDbContext();

            var quotes = db.Quotes
                .Where(q => q.ClientId == client.Id)
                .ToList();

            QuotesListBox.ItemsSource = quotes;

            Products.Clear();

            UpdateGrandTotal();

            CurrentQuote = null;

            DeliveryDatePicker.SelectedDate = DateTime.Now;

            QuoteStatusComboBox.SelectedItem = "Pendiente";

            DepositTextBox.Text = "0";
            QuoteNotesTextBox.Text = "";
            ClientNotesTextBox.Text = "";
            QuoteTitleTextBox.Text = "";

            UpdatePendingAmount();

            LastSelectedClientId = client.Id;
            LastSelectedQuoteId = null;

            IsLoadingData = false;
            MarkAsSaved();

            UpdateSaveButtonText();
        }

        private void QuotesListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (IsRevertingSelection)
                return;

            var selectedQuote = QuotesListBox.SelectedItem as Models.Quote;

            if (selectedQuote == null)
                return;

            if (!CanChangeSelection())
            {
                RevertQuoteSelection();
                return;
            }

            IsLoadingData = true;

            using var db = new AppDbContext();

            var quote = db.Quotes
                .Include(q => q.Items)
                .FirstOrDefault(q => q.Id == selectedQuote.Id);

            if (quote == null)
            {
                IsLoadingData = false;
                return;
            }

            CurrentQuote = quote;

            DeliveryDatePicker.SelectedDate = quote.DeliveryDate;
            QuoteTitleTextBox.Text = quote.Title;

            QuoteStatusComboBox.SelectedItem = string.IsNullOrWhiteSpace(quote.Status)
                ? "Pendiente"
                : quote.Status;

            DepositTextBox.Text = $"{quote.Deposit:N2}";
            QuoteNotesTextBox.Text = quote.Notes;
            ClientNotesTextBox.Text = quote.ClientNotes;

            UpdatePendingAmount();

            Products.Clear();

            foreach (var item in quote.Items)
            {
                var line = new ProductLine()
                {
                    ProductName = item.ProductName,
                    TailoringType = item.TailoringType,
                    Fabric = item.Fabric,
                    BasePrice = item.BasePrice,
                    FabricPrice = item.FabricPrice,
                    ManualPrice = item.ManualPrice,
                    Quantity = item.Quantity,
                    Total = item.Total
                };

                line.UpdateTotal();

                Products.Add(line);
            }

            UpdateGrandTotal();

            LastSelectedQuoteId = quote.Id;

            IsLoadingData = false;
            MarkAsSaved();

            UpdateSaveButtonText();
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button)
                return;

            if (button.DataContext is not ProductLine line)
                return;

            Products.Remove(line);

            UpdateGrandTotal();
            MarkAsChanged();
        }

        private void DuplicateProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button)
                return;

            if (button.DataContext is not ProductLine original)
                return;

            var copy = new ProductLine()
            {
                ProductName = original.ProductName,
                TailoringType = original.TailoringType,
                Fabric = original.Fabric,
                BasePrice = original.BasePrice,
                FabricPrice = original.FabricPrice,
                ManualPrice = original.ManualPrice,
                Quantity = original.Quantity
            };

            copy.UpdateTotal();

            Products.Add(copy);

            UpdateGrandTotal();
            MarkAsChanged();
        }

        private void NewQuoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ConfirmDiscardChanges())
                return;

            IsLoadingData = true;

            Products.Clear();

            UpdateGrandTotal();

            CurrentQuote = null;

            QuotesListBox.SelectedItem = null;

            LastSelectedQuoteId = null;

            DeliveryDatePicker.SelectedDate = DateTime.Now;

            QuoteTitleTextBox.Text = "";

            QuoteStatusComboBox.SelectedItem = "Pendiente";

            DepositTextBox.Text = "0";

            QuoteNotesTextBox.Text = "";

            ClientNotesTextBox.Text = "";

            UpdatePendingAmount();

            IsLoadingData = false;
            MarkAsSaved();

            UpdateSaveButtonText();
        }

        private void ProductsDataGrid_CellEditEnding(object? sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            if (e.Row.Item is ProductLine line)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    line.RefreshPrice();
                    UpdateGrandTotal();
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void DeleteQuote_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button)
                return;

            if (button.DataContext is not Models.Quote quoteToDelete)
                return;

            var result = MessageBox.Show(
                "¿Seguro que quieres eliminar este presupuesto?\n\nEsta acción no se puede deshacer.",
                "Eliminar presupuesto",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            using var db = new AppDbContext();

            var quote = db.Quotes
                .Include(q => q.Items)
                .FirstOrDefault(q => q.Id == quoteToDelete.Id);

            if (quote == null)
                return;

            db.QuoteItems.RemoveRange(quote.Items);
            db.Quotes.Remove(quote);
            db.SaveChanges();

            if (CurrentQuote != null && CurrentQuote.Id == quoteToDelete.Id)
            {
                IsLoadingData = true;

                Products.Clear();
                UpdateGrandTotal();

                CurrentQuote = null;
                QuotesListBox.SelectedItem = null;

                DeliveryDatePicker.SelectedDate = DateTime.Now;
                QuoteTitleTextBox.Text = "";
                QuoteStatusComboBox.SelectedItem = "Pendiente";
                DepositTextBox.Text = "0";
                QuoteNotesTextBox.Text = "";
                ClientNotesTextBox.Text = "";

                UpdatePendingAmount();

                IsLoadingData = false;
                MarkAsSaved();
                UpdateSaveButtonText();
            }

            var selectedClient = ClientsListBox.SelectedItem as Models.Client;

            LoadClients();

            if (selectedClient != null)
            {
                var reloadedClient = ((IEnumerable<Models.Client>)ClientsListBox.ItemsSource)
                    .FirstOrDefault(c => c.Id == selectedClient.Id);

                if (reloadedClient != null)
                {
                    ClientsListBox.SelectedItem = reloadedClient;
                }
            }

            UpdateSaveButtonText();
            UpdateUnsavedChangesIndicator();
            UpdateWindowTitle();

            MessageBox.Show(
                "Presupuesto eliminado correctamente.",
                "Eliminado",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void PricesConfigButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new PricesWindow
            {
                Owner = this
            };

            var result = window.ShowDialog();

            if (result == true)
            {
                PriceService.LoadPrices();

                MessageBox.Show(
                    "Los nuevos precios se aplicarán a los productos que añadas o cambies a partir de ahora. Los presupuestos ya abiertos conservarán sus precios actuales.",
                    "Precios actualizados",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void DuplicateQuoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentQuote == null)
            {
                MessageBox.Show(
                    "Primero selecciona un presupuesto para duplicarlo.",
                    "Duplicar presupuesto",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            if (Products.Count == 0)
            {
                MessageBox.Show(
                    "El presupuesto seleccionado no tiene productos para duplicar.",
                    "Duplicar presupuesto",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            var copiedProducts = Products
                .Select(p =>
                {
                    var copy = new ProductLine()
                    {
                        ProductName = p.ProductName,
                        TailoringType = p.TailoringType,
                        Fabric = p.Fabric,
                        BasePrice = p.BasePrice,
                        FabricPrice = p.FabricPrice,
                        ManualPrice = p.ManualPrice,
                        Quantity = p.Quantity
                    };

                    //copy.RefreshPrice();

                    copy.UpdateTotal();

                    return copy;
                })
                .ToList();

            Products.Clear();

            foreach (var product in copiedProducts)
            {
                Products.Add(product);
            }

            var originalTitle = string.IsNullOrWhiteSpace(QuoteTitleTextBox.Text)
                ? $"Presupuesto #{CurrentQuote.Id}"
                : QuoteTitleTextBox.Text;

            QuoteTitleTextBox.Text = $"Copia de {originalTitle}";

            CurrentQuote = null;
            QuotesListBox.SelectedItem = null;

            QuoteStatusComboBox.SelectedItem = "Pendiente";

            UpdateGrandTotal();

            MarkAsChanged();
            UpdateSaveButtonText();

            MessageBox.Show(
                "Presupuesto duplicado. Revisa los datos y pulsa Guardar para crear la nueva versión.",
                "Duplicar presupuesto",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private decimal GetDepositValue()
        {
            if (decimal.TryParse(DepositTextBox.Text, out var deposit))
            {
                if (deposit < 0)
                    deposit = 0;

                return deposit;
            }

            return 0;
        }

        private void UpdatePendingAmount()
        {
            decimal total = Products.Sum(p => p.Total);
            decimal deposit = GetDepositValue();

            decimal pending = total - deposit;

            if (pending < 0)
                pending = 0;

            PendingAmountTextBlock.Text = $"{pending:N2} €";
        }

        private void ApplyClientFilter()
        {
            var search = ClientSearchTextBox.Text?.Trim().ToLower() ?? "";

            IEnumerable<Models.Client> filteredClients = AllClients;

            if (ActiveStatusFilter != "Todos")
            {
                filteredClients = filteredClients
                    .Where(c => c.Quotes.Any(q =>
                        string.Equals(
                            string.IsNullOrWhiteSpace(q.Status) ? "Pendiente" : q.Status,
                            ActiveStatusFilter,
                            StringComparison.OrdinalIgnoreCase)));
            }

            var today = DateTime.Today;

            if (ActiveDeliveryFilter == "Próximas")
            {
                filteredClients = filteredClients
                    .Where(c => c.NextDeliveryDate != null && c.NextDeliveryDate.Value.Date >= today);
            }
            else if (ActiveDeliveryFilter == "Semana")
            {
                var limit = today.AddDays(7);

                filteredClients = filteredClients
                    .Where(c =>
                        c.NextDeliveryDate != null &&
                        c.NextDeliveryDate.Value.Date >= today &&
                        c.NextDeliveryDate.Value.Date <= limit);
            }
            else if (ActiveDeliveryFilter == "Mes")
            {
                var limit = today.AddDays(30);

                filteredClients = filteredClients
                    .Where(c =>
                        c.NextDeliveryDate != null &&
                        c.NextDeliveryDate.Value.Date >= today &&
                        c.NextDeliveryDate.Value.Date <= limit);
            }
            else if (ActiveDeliveryFilter == "Sin presupuesto")
            {
                filteredClients = filteredClients
                    .Where(c => c.Quotes == null || c.Quotes.Count == 0);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                filteredClients = filteredClients
                    .Where(c =>
                        c.Name.ToLower().Contains(search) ||
                        c.Phone.ToLower().Contains(search) ||
                        c.DeliveryDateText.ToLower().Contains(search) ||
                        c.Quotes.Any(q =>
                            (q.Title ?? "").ToLower().Contains(search) ||
                            (string.IsNullOrWhiteSpace(q.Status) ? "pendiente" : q.Status.ToLower()).Contains(search) ||
                            q.DeliveryDate.ToString("dd/MM/yyyy").Contains(search)
                        ));
            }

            ClientsListBox.ItemsSource = filteredClients
                .OrderBy(c => c.NextDeliveryDate ?? DateTime.MaxValue)
                .ThenBy(c => c.Name)
                .ToList();
        }

        private void ClientSearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ClientSearchPlaceholderTextBlock.Visibility =
                string.IsNullOrWhiteSpace(ClientSearchTextBox.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            ApplyClientFilter();
        }

        private bool ValidateBeforeSave()
        {
            if (string.IsNullOrWhiteSpace(ClientNameTextBox.Text))
            {
                MessageBox.Show(
                    "Debes indicar el nombre del cliente.",
                    "Faltan datos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                ClientNameTextBox.Focus();

                return false;
            }

            if (string.IsNullOrWhiteSpace(PhoneTextBox.Text))
            {
                MessageBox.Show(
                    "Debes indicar el teléfono del cliente.",
                    "Faltan datos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                PhoneTextBox.Focus();

                return false;
            }

            if (Products.Count == 0)
            {
                MessageBox.Show(
                    "Debes añadir al menos un producto al presupuesto.",
                    "Presupuesto vacío",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            foreach (var product in Products)
            {
                if (string.IsNullOrWhiteSpace(product.ProductName))
                {
                    MessageBox.Show(
                        "Hay una línea sin producto seleccionado.",
                        "Producto incompleto",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return false;
                }

                if (product.ProductName != "Concepto Libre" &&
                    product.ProductName != "Corbata" &&
                    product.ProductName != "Pañuelo" &&
                    product.ProductName != "Gemelos" &&
                    product.ProductName != "Tirantes" &&
                    product.ProductName != "Zapatos" &&
                    string.IsNullOrWhiteSpace(product.TailoringType))
                {
                    MessageBox.Show(
                        $"El producto \"{product.ProductName}\" necesita tipo de confección.",
                        "Producto incompleto",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return false;
                }

                if (product.Quantity < 1)
                {
                    MessageBox.Show(
                        $"La cantidad del producto \"{product.ProductName}\" debe ser al menos 1.",
                        "Cantidad no válida",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return false;
                }

                if (product.Total <= 0)
                {
                    var result = MessageBox.Show(
                        $"El producto \"{product.ProductName}\" tiene total 0,00 €. ¿Quieres guardar igualmente?",
                        "Producto sin importe",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result != MessageBoxResult.Yes)
                        return false;
                }
            }

            return true;
        }

        private void MarkAsChanged()
        {
            if (IsLoadingData)
                return;

            HasUnsavedChanges = true;

            UpdateUnsavedChangesIndicator();
            UpdateWindowTitle();
        }

        private bool ConfirmDiscardChanges()
        {
            if (!HasUnsavedChanges)
                return true;

            if (IsConfirmingDiscard)
                return false;

            IsConfirmingDiscard = true;

            var result = MessageBox.Show(
                "Hay cambios sin guardar. ¿Quieres continuar sin guardar?",
                "Cambios sin guardar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            IsConfirmingDiscard = false;

            return result == MessageBoxResult.Yes;
        }

        private bool CanChangeSelection()
        {
            if (SuppressSelectionConfirm)
                return true;

            if (IsLoadingData)
                return true;

            if (!HasUnsavedChanges)
                return true;

            return ConfirmDiscardChanges();
        }

        private void RevertClientSelection()
        {
            IsRevertingSelection = true;

            if (LastSelectedClientId == null)
            {
                ClientsListBox.SelectedItem = null;
            }
            else
            {
                var previousClient = ((IEnumerable<Models.Client>)ClientsListBox.ItemsSource)
                    .FirstOrDefault(c => c.Id == LastSelectedClientId.Value);

                ClientsListBox.SelectedItem = previousClient;
            }

            IsRevertingSelection = false;
        }

        private void RevertQuoteSelection()
        {
            IsRevertingSelection = true;

            if (LastSelectedQuoteId == null)
            {
                QuotesListBox.SelectedItem = null;
            }
            else
            {
                var previousQuote = ((IEnumerable<Models.Quote>)QuotesListBox.ItemsSource)
                    .FirstOrDefault(q => q.Id == LastSelectedQuoteId.Value);

                QuotesListBox.SelectedItem = previousQuote;
            }

            IsRevertingSelection = false;
        }

        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!ConfirmDiscardChanges())
            {
                e.Cancel = true;
            }
        }

        private void UpdateSaveButtonText()
        {
            if (CurrentQuote == null)
            {
                SaveButton.Content = "Guardar";
            }
            else
            {
                SaveButton.Content = "Actualizar";
            }
        }

        private void UpdateUnsavedChangesIndicator()
        {
            UnsavedChangesTextBlock.Visibility = HasUnsavedChanges
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void MarkAsSaved()
        {
            HasUnsavedChanges = false;

            UpdateUnsavedChangesIndicator();
            UpdateWindowTitle();
        }

        private void UpdateWindowTitle()
        {
            Title = HasUnsavedChanges
                ? "Sastrería Presupuestos *"
                : "Sastrería Presupuestos";
        }

        private void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dbPath = AppDbContext.GetDatabasePath();

                if (!File.Exists(dbPath))
                {
                    MessageBox.Show(
                        "No se encontró la base de datos para crear la copia de seguridad.",
                        "Copia de seguridad",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                var dialog = new SaveFileDialog
                {
                    Title = "Guardar copia de seguridad",
                    Filter = "Base de datos SQLite (*.db)|*.db",
                    FileName = $"sastreria_backup_{DateTime.Now:yyyy-MM-dd_HH-mm}.db"
                };

                if (dialog.ShowDialog() != true)
                    return;

                File.Copy(dbPath, dialog.FileName, overwrite: true);

                MessageBox.Show(
                    "Copia de seguridad creada correctamente.",
                    "Copia de seguridad",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudo crear la copia de seguridad.\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ExportPdfButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateBeforeSave())
                return;

            var exportQuote = ExportDataService.CreateExportQuote(
                ClientNameTextBox.Text,
                PhoneTextBox.Text,
                DeliveryDatePicker.SelectedDate ?? DateTime.Now,
                QuoteTitleTextBox.Text,
                QuoteStatusComboBox.SelectedItem?.ToString() ?? "Pendiente",
                GetDepositValue(),
                ClientNotesTextBox.Text,
                Products);

            var dialog = new SaveFileDialog
            {
                Title = "Guardar PDF",
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = $"{exportQuote.DisplayTitle}_{DateTime.Now:yyyy-MM-dd}.pdf"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                PdfExportService.ExportQuoteToPdf(exportQuote, dialog.FileName);

                MessageBox.Show(
                    "PDF generado correctamente.",
                    "Exportar PDF",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudo generar el PDF.\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ExportAllQuotesPdfButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedClient = ClientsListBox.SelectedItem as Models.Client;

            if (selectedClient == null)
            {
                MessageBox.Show(
                    "Primero selecciona un cliente.",
                    "Exportar opciones",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            using var db = new AppDbContext();

            var client = db.Clients
                .Include(c => c.Quotes)
                    .ThenInclude(q => q.Items)
                .FirstOrDefault(c => c.Id == selectedClient.Id);

            if (client == null || client.Quotes.Count == 0)
            {
                MessageBox.Show(
                    "El cliente seleccionado no tiene presupuestos para exportar.",
                    "Exportar opciones",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            var exportQuotes = client.Quotes
                .OrderBy(q => q.DeliveryDate)
                .ThenBy(q => q.Id)
                .Select(q =>
                {
                    var productLines = new ObservableCollection<ProductLine>(
                        q.Items.Select(item =>
                        {
                            var line = new ProductLine()
                            {
                                ProductName = item.ProductName,
                                TailoringType = item.TailoringType,
                                Fabric = item.Fabric,
                                BasePrice = item.BasePrice,
                                FabricPrice = item.FabricPrice,
                                ManualPrice = item.ManualPrice,
                                Quantity = item.Quantity,
                                Total = item.Total
                            };

                            line.UpdateTotal();

                            return line;
                        }));

                    return ExportDataService.CreateExportQuote(
                        client.Name,
                        client.Phone,
                        q.DeliveryDate,
                        q.Title,
                        string.IsNullOrWhiteSpace(q.Status) ? "Pendiente" : q.Status,
                        q.Deposit,
                        q.ClientNotes,
                        productLines);
                })
                .ToList();

            var dialog = new SaveFileDialog
            {
                Title = "Guardar PDF con opciones",
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = $"{client.Name}_opciones_{DateTime.Now:yyyy-MM-dd}.pdf"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                PdfExportService.ExportQuotesToPdf(exportQuotes, dialog.FileName);

                MessageBox.Show(
                    "PDF con opciones generado correctamente.",
                    "Exportar opciones",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudo generar el PDF con opciones.\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void StatusFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button)
                return;

            ActiveStatusFilter = button.Tag?.ToString() ?? "Todos";

            ApplyClientFilter();
            UpdateStatusFilterButtons();

            Products.Clear();
            QuotesListBox.ItemsSource = null;
            CurrentQuote = null;

            LastSelectedQuoteId = null;

            UpdateGrandTotal();
            UpdateSaveButtonText();
            MarkAsSaved();
        }

        private void UpdateStatusFilterButtons()
        {
            foreach (var child in StatusFiltersPanel.Children)
            {
                if (child is not System.Windows.Controls.Button button)
                    continue;

                var status = button.Tag?.ToString() ?? "Todos";

                if (status == ActiveStatusFilter)
                {
                    button.Style = (Style)FindResource("ActiveFilterButtonStyle");
                }
                else
                {
                    button.Style = (Style)FindResource("SecondaryButtonStyle");
                }
            }
        }

        private void ClearScreenForNewClient()
        {
            IsLoadingData = true;

            ClientsListBox.SelectedItem = null;
            QuotesListBox.SelectedItem = null;
            QuotesListBox.ItemsSource = null;

            ClientNameTextBox.Text = "";
            PhoneTextBox.Text = "";

            Products.Clear();

            CurrentQuote = null;
            LastSelectedClientId = null;
            LastSelectedQuoteId = null;

            DeliveryDatePicker.SelectedDate = DateTime.Now;
            QuoteTitleTextBox.Text = "";
            QuoteStatusComboBox.SelectedItem = "Pendiente";
            DepositTextBox.Text = "0";
            QuoteNotesTextBox.Text = "";
            ClientNotesTextBox.Text = "";

            UpdateGrandTotal();
            UpdatePendingAmount();

            IsLoadingData = false;

            MarkAsSaved();
            UpdateSaveButtonText();
        }

        private void NewClientButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ConfirmDiscardChanges())
                return;

            ClearScreenForNewClient();

            ClientNameTextBox.Focus();
        }

        private void DeleteClientButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedClient = ClientsListBox.SelectedItem as Models.Client;

            if (selectedClient == null)
            {
                MessageBox.Show(
                    "Primero selecciona un cliente para eliminarlo.",
                    "Eliminar cliente",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            if (!ConfirmDiscardChanges())
                return;

            var result = MessageBox.Show(
                $"¿Seguro que quieres eliminar el cliente \"{selectedClient.Name}\"?\n\n" +
                "Se eliminarán también todos sus presupuestos y esta acción no se puede deshacer.",
                "Eliminar cliente",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            using var db = new AppDbContext();

            var client = db.Clients
                .Include(c => c.Quotes)
                    .ThenInclude(q => q.Items)
                .FirstOrDefault(c => c.Id == selectedClient.Id);

            if (client == null)
                return;

            foreach (var quote in client.Quotes)
            {
                db.QuoteItems.RemoveRange(quote.Items);
            }

            db.Quotes.RemoveRange(client.Quotes);
            db.Clients.Remove(client);

            db.SaveChanges();

            LoadClients();

            ClearScreenForNewClient();

            MessageBox.Show(
                "Cliente eliminado correctamente.",
                "Eliminar cliente",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void SaveClientButton_Click(object sender, RoutedEventArgs e)
        {
            var clientName = ClientNameTextBox.Text.Trim();
            var clientPhone = PhoneTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(clientName))
            {
                MessageBox.Show(
                    "Debes indicar el nombre del cliente.",
                    "Guardar cliente",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                ClientNameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(clientPhone))
            {
                MessageBox.Show(
                    "Debes indicar el teléfono del cliente.",
                    "Guardar cliente",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                PhoneTextBox.Focus();
                return;
            }

            using var db = new AppDbContext();

            var selectedClient = ClientsListBox.SelectedItem as Models.Client;

            Models.Client? client = null;

            var currentClientId = selectedClient?.Id;

            var possibleDuplicate = FindPossibleDuplicateClient(
                db,
                clientName,
                clientPhone,
                currentClientId);

            if (possibleDuplicate != null)
            {
                var result = MessageBox.Show(
                    $"Ya existe un cliente parecido:\n\n" +
                    $"{possibleDuplicate.Name}\n" +
                    $"{possibleDuplicate.Phone}\n\n" +
                    "¿Quieres guardar igualmente?",
                    "Posible cliente duplicado",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            if (selectedClient != null)
            {
                client = db.Clients.FirstOrDefault(c => c.Id == selectedClient.Id);
            }

            if (client == null)
            {
                client = db.Clients.FirstOrDefault(c => c.Phone == clientPhone);
            }

            if (client == null)
            {
                client = new Models.Client
                {
                    Name = clientName,
                    Phone = clientPhone
                };

                db.Clients.Add(client);
            }
            else
            {
                client.Name = clientName;
                client.Phone = clientPhone;
            }

            db.SaveChanges();

            var savedClientId = client.Id;

            SuppressSelectionConfirm = true;

            LoadClients();

            var reloadedClient = ((IEnumerable<Models.Client>)ClientsListBox.ItemsSource)
                .FirstOrDefault(c => c.Id == savedClientId);

            if (reloadedClient != null)
            {
                ClientsListBox.SelectedItem = reloadedClient;
                LastSelectedClientId = savedClientId;
            }

            SuppressSelectionConfirm = false;

            MarkAsSaved();

            MessageBox.Show(
                "Cliente guardado correctamente.",
                "Guardar cliente",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void DeliveryFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button)
                return;

            ActiveDeliveryFilter = button.Tag?.ToString() ?? "Todas";

            ApplyClientFilter();
            UpdateDeliveryFilterButtons();

            Products.Clear();
            QuotesListBox.ItemsSource = null;
            CurrentQuote = null;
            LastSelectedQuoteId = null;

            UpdateGrandTotal();
            UpdateSaveButtonText();
            MarkAsSaved();
        }

        private void UpdateDeliveryFilterButtons()
        {
            foreach (var child in DeliveryFiltersPanel.Children)
            {
                if (child is not System.Windows.Controls.Button button)
                    continue;

                var filter = button.Tag?.ToString() ?? "Todas";

                if (filter == ActiveDeliveryFilter)
                {
                    button.Style = (Style)FindResource("ActiveFilterButtonStyle");
                }
                else
                {
                    button.Style = (Style)FindResource("SecondaryButtonStyle");
                }
            }
        }

        private void ProductsDataGrid_PreparingCellForEdit(
    object? sender,
    System.Windows.Controls.DataGridPreparingCellForEditEventArgs e)
        {
            if (e.EditingElement is not System.Windows.Controls.TextBox textBox)
                return;

            var header = e.Column.Header?.ToString() ?? "";

            textBox.Tag = header;

            if (header == "Cant." ||
                header == "Base" ||
                header == "Precio tejido" ||
                header == "Tejido" ||
                header == "Ajuste")
            {
                textBox.PreviewTextInput -= NumericTextBox_PreviewTextInput;
                textBox.PreviewTextInput += NumericTextBox_PreviewTextInput;

                DataObject.RemovePastingHandler(textBox, NumericTextBox_Pasting);
                DataObject.AddPastingHandler(textBox, NumericTextBox_Pasting);
            }
        }

        private bool ShouldAllowNegative(System.Windows.Controls.TextBox textBox)
        {
            var header = textBox.Tag?.ToString() ?? "";

            return header == "Ajuste";
        }

        private bool IsValidNumericInput(string text, bool allowNegative)
        {
            if (string.IsNullOrWhiteSpace(text))
                return true;

            // Permitimos escribir parcialmente "-" solo en ajuste
            if (text == "-")
                return allowNegative;

            // Permitimos coma o punto decimal mientras se escribe
            var pattern = allowNegative
                ? @"^-?\d*([,.]\d*)?$"
                : @"^\d*([,.]\d*)?$";

            return Regex.IsMatch(text, pattern);
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is not System.Windows.Controls.TextBox textBox)
                return;

            var proposedText = GetProposedText(textBox, e.Text);

            e.Handled = !IsValidNumericInput(proposedText, allowNegative: ShouldAllowNegative(textBox));
        }

        private void NumericTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (sender is not System.Windows.Controls.TextBox textBox)
                return;

            if (!e.DataObject.GetDataPresent(typeof(string)))
            {
                e.CancelCommand();
                return;
            }

            var pasteText = e.DataObject.GetData(typeof(string)) as string ?? "";
            var proposedText = GetProposedText(textBox, pasteText);

            if (!IsValidNumericInput(proposedText, allowNegative: ShouldAllowNegative(textBox)))
            {
                e.CancelCommand();
            }
        }

        private string GetProposedText(System.Windows.Controls.TextBox textBox, string newText)
        {
            var currentText = textBox.Text ?? "";
            var selectionStart = textBox.SelectionStart;
            var selectionLength = textBox.SelectionLength;

            if (selectionLength > 0)
            {
                currentText = currentText.Remove(selectionStart, selectionLength);
            }

            return currentText.Insert(selectionStart, newText);
        }

        private void MoveProductUp_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button)
                return;

            if (button.DataContext is not ProductLine line)
                return;

            var index = Products.IndexOf(line);

            if (index <= 0)
                return;

            Products.Move(index, index - 1);

            ProductsDataGrid.SelectedItem = line;

            MarkAsChanged();
        }

        private void MoveProductDown_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button)
                return;

            if (button.DataContext is not ProductLine line)
                return;

            var index = Products.IndexOf(line);

            if (index < 0 || index >= Products.Count - 1)
                return;

            Products.Move(index, index + 1);

            ProductsDataGrid.SelectedItem = line;

            MarkAsChanged();
        }

        private string NormalizeText(string text)
        {
            return text
                .Trim()
                .ToLower()
                .Replace(" ", "")
                .Replace("-", "")
                .Replace(".", "");
        }

        private Models.Client? FindPossibleDuplicateClient(
    AppDbContext db,
    string clientName,
    string clientPhone,
    int? currentClientId = null)
        {
            var normalizedName = NormalizeText(clientName);
            var normalizedPhone = NormalizeText(clientPhone);

            var clients = db.Clients.ToList();

            foreach (var client in clients)
            {
                if (currentClientId != null && client.Id == currentClientId.Value)
                    continue;

                var existingName = NormalizeText(client.Name);
                var existingPhone = NormalizeText(client.Phone);

                if (!string.IsNullOrWhiteSpace(normalizedPhone) &&
                    existingPhone == normalizedPhone)
                {
                    return client;
                }

                if (!string.IsNullOrWhiteSpace(normalizedName) &&
                    existingName == normalizedName)
                {
                    return client;
                }
            }

            return null;
        }
    }
}