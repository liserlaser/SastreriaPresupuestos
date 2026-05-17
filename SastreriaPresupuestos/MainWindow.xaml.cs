using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using QuestPDF.Infrastructure;
using SastreriaPresupuestos.Data;
using SastreriaPresupuestos.Export;
using SastreriaPresupuestos.Messages;
using SastreriaPresupuestos.Models;
using SastreriaPresupuestos.Services;
using SastreriaPresupuestos.ViewModels;
using SastreriaPresupuestos.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MediaColor = System.Windows.Media.Color;

namespace SastreriaPresupuestos
{
    internal enum WorkspaceSaveState
    {
        Saved,
        Saving,
        Dirty,
        SaveFailed
    }

    public partial class MainWindow : Window
    {

        private readonly DashboardView DashboardView = new();
        private readonly ClientesView ClientesView = new();
        private readonly PresupuestoView PresupuestoView = new();
        private readonly ProductosView ProductosView = new();
        private readonly FacturasView FacturasView = new();
        private readonly AjustesView AjustesView = new();

        public ObservableCollection<string> AvailableProductNames { get; } = new();

        private TextBlock WeekTitleTextBlock => DashboardView.WeekTitleTextBlock;
        private Button PreviousCalendarButton => DashboardView.PreviousCalendarButton;
        private Button TodayCalendarButton => DashboardView.TodayCalendarButton;
        private Button NextCalendarButton => DashboardView.NextCalendarButton;
        private Button ListViewButton => DashboardView.ListViewButton;
        private Button WeekViewButton => DashboardView.WeekViewButton;
        private Button MonthViewButton => DashboardView.MonthViewButton;
        private TextBlock DashboardTodayCountTextBlock => DashboardView.DashboardTodayCountTextBlock;
        private TextBlock DashboardNext7CountTextBlock => DashboardView.DashboardNext7CountTextBlock;
        private TextBlock DashboardPendingCountTextBlock => DashboardView.DashboardPendingCountTextBlock;
        private TextBlock DashboardMonthCountTextBlock => DashboardView.DashboardMonthCountTextBlock;
        private ListBox DeliveriesListBox => DashboardView.DeliveriesListBox;
        private TextBlock EmptyWeekTextBlock => DashboardView.EmptyWeekTextBlock;
        private ItemsControl WeeklyColumnsItemsControl => DashboardView.WeeklyColumnsItemsControl;
        private ListBox CalendarGroupsListBox => DashboardView.CalendarGroupsListBox;
        private Button NewClientButton => ClientesView.NewClientButton;
        private Button DeleteClientButton => ClientesView.DeleteClientButton;
        private TextBox ClientSearchTextBox => ClientesView.ClientSearchTextBox;
        private TextBlock ClientSearchPlaceholderTextBlock => ClientesView.ClientSearchPlaceholderTextBlock;
        private WrapPanel StatusFiltersPanel => ClientesView.StatusFiltersPanel;
        private WrapPanel DeliveryFiltersPanel => ClientesView.DeliveryFiltersPanel;
        private ListBox ClientsListBox => ClientesView.ClientsListBox;
        private TextBlock ClientDetailNameTextBlock => ClientesView.ClientDetailNameTextBlock;
        private TextBlock ClientDetailMetaTextBlock => ClientesView.ClientDetailMetaTextBlock;
        private Button NewQuoteFromClientButton => ClientesView.NewQuoteFromClientButton;
        private Button OpenSelectedClientQuoteButton => ClientesView.OpenSelectedClientQuoteButton;
        private TextBlock ClientDetailPhoneTextBlock => ClientesView.ClientDetailPhoneTextBlock;
        private TextBlock ClientDetailDniTextBlock => ClientesView.ClientDetailDniTextBlock;
        private TextBlock ClientDetailNextDeliveryTextBlock => ClientesView.ClientDetailNextDeliveryTextBlock;
        private TextBlock ClientDetailSummaryTextBlock => ClientesView.ClientDetailSummaryTextBlock;
        private ListBox QuotesListBox => ClientesView.QuotesListBox;
        private TextBlock BudgetWorkspaceTitleTextBlock => PresupuestoView.BudgetWorkspaceTitleTextBlock;
        private TextBlock BudgetWorkspaceClientTextBlock => PresupuestoView.BudgetWorkspaceClientTextBlock;
        private TextBlock BudgetWorkspaceStatusTextBlock => PresupuestoView.BudgetWorkspaceStatusTextBlock;
        private TextBlock BudgetWorkspaceDeliveryTextBlock => PresupuestoView.BudgetWorkspaceDeliveryTextBlock;
        private TextBlock BudgetWorkspaceTotalTextBlock => PresupuestoView.BudgetWorkspaceTotalTextBlock;
        private Button BudgetQuickSummaryButton => PresupuestoView.BudgetQuickSummaryButton;
        private Button BudgetQuickProductsButton => PresupuestoView.BudgetQuickProductsButton;
        private Button BudgetQuickHistoryButton => PresupuestoView.BudgetQuickHistoryButton;
        private Button BudgetQuickDocumentsButton => PresupuestoView.BudgetQuickDocumentsButton;
        private Button BudgetQuickExportPdfButton => PresupuestoView.BudgetQuickExportPdfButton;
        private TabControl PresupuestoWorkspaceTabs => PresupuestoView.PresupuestoWorkspaceTabs;
        private ContentControl ProductsInlineHost => PresupuestoView.ProductsInlineHost;
        private FrameworkElement WorkspaceDashboard => PresupuestoView.WorkspaceDashboard;
        private TextBlock QuoteCreatedAtTextBox => PresupuestoView.QuoteCreatedAtTextBox;
        private Button OpenQuoteFolderButton => PresupuestoView.OpenQuoteFolderButton;
        private Button OpenClientSheetFolderButton => PresupuestoView.OpenClientSheetFolderButton;
        private ScrollViewer WorkspaceScrollViewer => PresupuestoView.WorkspaceScrollViewer;
        private Border ActivityTimelineBorder => PresupuestoView.ActivityTimelineBorder;
        private Border PresupuestoEmptyHintBorder => PresupuestoView.PresupuestoEmptyHintBorder;
        private TextBox ClientNameTextBox => PresupuestoView.ClientNameTextBox;
        private ComboBox ClientPredictiveComboBox => PresupuestoView.ClientPredictiveComboBox;
        private TextBox PhoneTextBox => PresupuestoView.PhoneTextBox;
        private TextBlock PhoneDisplayTextBlock => PresupuestoView.PhoneDisplayTextBlock;
        private TextBox DniTextBox => PresupuestoView.DniTextBox;
        private Button SaveClientButton => PresupuestoView.SaveClientButton;
        private Button ClientSheetButton => PresupuestoView.ClientSheetButton;
        private Button EditJobButton => PresupuestoView.EditJobButton;
        private DatePicker DeliveryDatePicker => PresupuestoView.DeliveryDatePicker;
        private DatePicker EventDatePicker => PresupuestoView.EventDatePicker;
        private TextBox QuoteTitleTextBox => PresupuestoView.QuoteTitleTextBox;
        private TextBlock QuoteTitlePlaceholderTextBlock => PresupuestoView.QuoteTitlePlaceholderTextBlock;
        private ComboBox QuoteStatusComboBox => PresupuestoView.QuoteStatusComboBox;
        private Border QuoteStatusBadgeBorder => PresupuestoView.QuoteStatusBadgeBorder;
        private TextBlock QuoteStatusBadgeTextBlock => PresupuestoView.QuoteStatusBadgeTextBlock;
        private TextBlock EventDateDisplayTextBlock => PresupuestoView.EventDateDisplayTextBlock;
        private TextBox DepositTextBox => PresupuestoView.DepositTextBox;
        private TextBlock PendingAmountTextBlock => PresupuestoView.PendingAmountTextBlock;
        private TextBox QuoteNotesTextBox => PresupuestoView.QuoteNotesTextBox;
        private TextBlock QuoteNotesPlaceholderTextBlock => PresupuestoView.QuoteNotesPlaceholderTextBlock;
        private TextBox ClientNotesTextBox => PresupuestoView.ClientNotesTextBox;
        private TextBlock ClientNotesPlaceholderTextBlock => PresupuestoView.ClientNotesPlaceholderTextBlock;
        private ListBox BudgetQuotesListBox => PresupuestoView.BudgetQuotesListBox;
        private TextBox WorkSearchTextBox => PresupuestoView.WorkSearchTextBox;
        private TextBlock WorkSearchPlaceholderTextBlock => PresupuestoView.WorkSearchPlaceholderTextBlock;
        private TextBlock UnsavedChangesTextBlock => PresupuestoView.UnsavedChangesTextBlock;
        private Button NewQuoteButton => PresupuestoView.NewQuoteButton;
        private Button DuplicateQuoteButton => PresupuestoView.DuplicateQuoteButton;
        private Button SaveButton => PresupuestoView.SaveButton;
        private Border ProductsEmptyHintBorder => ProductosView.ProductsEmptyHintBorder;
        private TextBlock ProductsEmptyHintTextBlock => ProductosView.ProductsEmptyHintTextBlock;
        private TextBlock ProductsUnsavedChangesTextBlock => ProductosView.ProductsUnsavedChangesTextBlock;
        private Button AddProductButton => ProductosView.AddProductButton;
        private Button SaveProductsButton => ProductosView.SaveProductsButton;
        private Button BackToJobDashboardButton => ProductosView.BackToJobDashboardButton;
        private DataGrid ProductsDataGrid => ProductosView.ProductsDataGrid;
        private TextBlock TotalTextBlock => ProductosView.TotalTextBlock;
        private Border DocumentsEmptyHintBorder => PresupuestoView.DocumentsEmptyHintBorder;
        private TextBlock DocumentsEmptyHintTextBlock => PresupuestoView.DocumentsEmptyHintTextBlock;
        private Border DocumentsLastPdfStatusBorder => PresupuestoView.DocumentsLastPdfStatusBorder;
        private TextBlock DocumentsLastPdfStatusTextBlock => PresupuestoView.DocumentsLastPdfStatusTextBlock;
        private Button OpenLastPdfButton => PresupuestoView.OpenLastPdfButton;
        private Button ExportPdfButton => PresupuestoView.ExportPdfButton;
        private Button ExportClientSheetButton => PresupuestoView.ExportClientSheetButton;
        private Button ExportAllQuotesPdfButton => PresupuestoView.ExportAllQuotesPdfButton;
        private Button ConvertQuoteToInvoiceButton => PresupuestoView.CreateInvoiceButton;
        private Button ViewQuoteInvoiceButton => PresupuestoView.ViewInvoiceButton;
        private TextBox InvoiceSearchTextBox => FacturasView.InvoiceSearchTextBox;
        private Button InvoiceCreateButton => FacturasView.CreateInvoiceButton;
        private Button InvoiceExportButton => FacturasView.ExportInvoiceButton;
        private Button InvoicePrintButton => FacturasView.PrintInvoiceButton;
        private Button CreateCorrectiveInvoiceButton => FacturasView.CreateCorrectiveInvoiceButton;
        private ListBox InvoicesListBox => FacturasView.InvoicesListBox;
        private DataGrid InvoiceItemsDataGrid => FacturasView.InvoiceItemsDataGrid;
        private TextBlock InvoiceDetailNumberTextBlock => FacturasView.InvoiceDetailNumberTextBlock;
        private TextBlock InvoiceDetailMetaTextBlock => FacturasView.InvoiceDetailMetaTextBlock;
        private TextBlock InvoiceDetailClientTextBlock => FacturasView.InvoiceDetailClientTextBlock;
        private TextBlock InvoiceDetailSourceTextBlock => FacturasView.InvoiceDetailSourceTextBlock;
        private TextBlock InvoiceDetailTotalTextBlock => FacturasView.InvoiceDetailTotalTextBlock;
        private Button PricesConfigButton => AjustesView.PricesConfigButton;
        private Button BackupButton => AjustesView.BackupButton;
        private TextBlock ClientSheetFolderTextBlock => AjustesView.ClientSheetFolderTextBlock;
        private Button ClientSheetFolderButton => AjustesView.ClientSheetFolderButton;
        private TextBlock QuoteFolderTextBlock => AjustesView.QuoteFolderTextBlock;
        private Button QuoteFolderButton => AjustesView.QuoteFolderButton;
        private TextBlock InvoiceFolderTextBlock => AjustesView.InvoiceFolderTextBlock;
        private Button InvoiceFolderButton => AjustesView.InvoiceFolderButton;


        private ObservableCollection<ProductLine> Products =
            new ObservableCollection<ProductLine>();

        private ObservableCollection<WeeklyDeliveryItem> WeeklyDeliveries =
            new ObservableCollection<WeeklyDeliveryItem>();

        private ObservableCollection<CalendarDayGroup> CalendarDayGroups =
            new ObservableCollection<CalendarDayGroup>();

        private ObservableCollection<WeeklyDeliveryItem> UpcomingDeliveries =
            new ObservableCollection<WeeklyDeliveryItem>();

        private ObservableCollection<GlobalSearchResult> GlobalSearchResults =
            new ObservableCollection<GlobalSearchResult>();

        private ObservableCollection<InvoiceListItem> Invoices =
            new ObservableCollection<InvoiceListItem>();

        private ObservableCollection<Models.Client> PredictiveClientResults =
            new ObservableCollection<Models.Client>();

        private List<Models.Client> AllClients = new();

        private Models.Quote? CurrentQuote = null;

        private string ActiveStatusFilter = "Todos";

        private bool IsSidebarCollapsed = false;
        private string ActiveDeliveryFilter = "Todas";

        private DateTime CalendarReferenceDate = DateTime.Today;
        private string CalendarViewMode = "Lista";

        private void MountWorkspaceViews()
        {
            DashboardHost.Content = DashboardView;
            ClientesHost.Content = ClientesView;
            PresupuestoHost.Content = PresupuestoView;
            ProductsInlineHost.Content = ProductosView;
            FacturasHost.Content = FacturasView;
            AjustesHost.Content = AjustesView;
        }

        private string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Cliente";

            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(invalidChar, '_');
            }

            return name.Trim();
        }

        private bool HasUnsavedChanges = false;
        private WorkspaceSaveState CurrentSaveState = WorkspaceSaveState.Saved;
        private DateTime? LastSavedAt = null;
        private bool IsLoadingData = false;
        private bool IsConfirmingDiscard = false;
        private bool IsAutoSaving = false;
        private readonly DispatcherTimer AutoSaveTimer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMilliseconds(1500)
        };

        private readonly DispatcherTimer SaveStatusRefreshTimer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMinutes(1)
        };

        private int? LastSelectedClientId = null;
        private int? LastSelectedQuoteId = null;
        private int? CurrentClientId = null;
        private string? LastGeneratedPdfPath = null;
        private AppSettings CurrentSettings = AppSettingsService.Load();
        private const int MaxWorkspaceUndoSteps = 30;
        private readonly Stack<WorkspaceUndoSnapshot> WorkspaceUndoStack = new();
        private WorkspaceUndoSnapshot? LastWorkspaceUndoSnapshot = null;

        private bool IsRevertingSelection = false;
        private bool SuppressSelectionConfirm = false;
        private bool IsUpdatingClientPredictiveComboBox = false;
        private bool IsRestoringUndoSnapshot = false;
        private string? ShellBreadcrumbOverride = null;
        private int? PendingNavigationTargetTab = null;
        private int? PreviousNavigationTab = null;
        private bool NavigateToProductsAfterJobEdit = false;

        private const int TabSemana = 0;
        private const int TabClientes = 1;
        private const int TabPresupuesto = 2;
        private const int TabProductos = 3;
        private const int TabFacturas = 4;
        private const int TabAjustes = 5;
        private const int InnerTabResumen = 0;
        private const int InnerTabProductos = 1;
        private const int InnerTabDocumentos = 2;
        private const int InnerTabHistorial = 3;

        private static readonly IReadOnlyList<string> DefaultTailoringOptions =
            new[] { "Confeccion", "Medida", "Artesanal" };

        private static readonly IReadOnlyList<string> ShirtTailoringOptions =
            new[] { "Confeccion", "Medida" };

        private static readonly HashSet<string> ProductsWithoutTailoring = new(StringComparer.OrdinalIgnoreCase)
        {
            "Corbata",
            "Pañuelo",
            "Gemelos",
            "Tirantes",
            "Zapatos",
            "Concepto Libre"
        };

        public MainWindow()
        {
            ConfigureCulture();

            InitializeComponent();

            MountWorkspaceViews();
            RegisterMessengerHandlers();

            QuestPDF.Settings.License = LicenseType.Community;

            InitializeDataSources();
            LoadInitialData();
            WireEvents();
            InitializeUiState();
            UpdateExportFolderSettingsUi();
        }

        protected override void OnClosed(EventArgs e)
        {
            WeakReferenceMessenger.Default.UnregisterAll(this);
            base.OnClosed(e);
        }

        private void RegisterMessengerHandlers()
        {
            WeakReferenceMessenger.Default.Register<ProductsTotalChangedMessage>(
                this,
                (_, __) => UpdateGrandTotal());

            WeakReferenceMessenger.Default.Register<ProductLineChangedMessage>(
                this,
                (_, __) =>
                {
                    UpdateWorkflowState();
                    UpdateActiveContext();
                });

            WeakReferenceMessenger.Default.Register<WorkspaceDirtyMessage>(
                this,
                (_, __) =>
                {
                    if (!IsLoadingData)
                        MarkAsChanged();
                });

            WeakReferenceMessenger.Default.Register<ClientSelectedMessage>(
                this,
                (_, message) => Dispatcher.BeginInvoke(
                    new Action(() => SelectPredictiveClient(message.ClientId)),
                    DispatcherPriority.Background));
        }

        private List<string> GetTailoringOptions(string product)
        {
            if (string.Equals(product, "Camisa", StringComparison.OrdinalIgnoreCase))
                return ShirtTailoringOptions.ToList();

            if (ProductsWithoutTailoring.Contains(product))
                return new List<string>();

            return DefaultTailoringOptions.ToList();
        }

        private static ProductLine CreateDefaultProductLine()
        {
            return new ProductLine
            {
                ProductName = "Concepto Libre",
                TailoringType = string.Empty,
                Fabric = string.Empty,
                BasePrice = 0,
                FabricPrice = 0,
                ManualPrice = 0,
                Quantity = 1
            };
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentClientId == null)
            {
                MessageBox.Show(
                    "Primero debes guardar o seleccionar un cliente antes de añadir productos.",
                    "Añadir producto",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                GoToTab(TabPresupuesto);
                ClientNameTextBox.Focus();

                return;
            }

            Products.Add(CreateDefaultProductLine());

            UpdateGrandTotal();

            OpenProductsInlinePanel();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            AutoSaveTimer.Stop();

            if (CurrentClientId == null)
            {
                MessageBox.Show(
                    "Primero debes guardar el cliente antes de guardar un trabajo.",
                    "Guardar trabajo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                GoToTab(TabPresupuesto);
                ClientNameTextBox.Focus();

                return;
            }

            if (!ValidateBeforeSave())
                return;

            SetSaveWorkflowState(WorkspaceSaveState.Saving);
            Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Background);

            using var db = new AppDbContext();

            var clientName = ClientNameTextBox.Text.Trim();
            var clientPhone = PhoneTextBox.Text.Trim();
            var clientDni = DniTextBox.Text.Trim();
            var quoteTitle = QuoteTitleTextBox.Text.Trim();
            var quoteNotes = QuoteNotesTextBox.Text.Trim();
            var clientNotes = ClientNotesTextBox.Text.Trim();

            var selectedStatus = GetSelectedQuoteStatus();

            var selectedClient = ClientsListBox.SelectedItem as Models.Client;
            var currentClientId = CurrentClientId ?? selectedClient?.Id;

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

            Models.Client client;

            if (CurrentClientId != null)
            {
                client = db.Clients.First(c => c.Id == CurrentClientId.Value);

                client.Name = clientName;
                client.Phone = clientPhone;
                client.Dni = clientDni;

                db.SaveChanges();
            }
            else
            {
                client = new Models.Client()
                {
                    Name = clientName,
                    Phone = clientPhone,
                    Dni = clientDni
                };

                db.Clients.Add(client);
                db.SaveChanges();

                CurrentClientId = client.Id;
            }

            Models.Quote quote;

            if (CurrentQuote == null)
            {
                quote = new Models.Quote()
                {
                    ClientId = client.Id,
                    DeliveryDate = GetEffectiveDeliveryDate(),
                    EventDate = EventDatePicker.SelectedDate,
                    Title = quoteTitle,
                    Status = selectedStatus,
                    Deposit = GetDepositValue(),
                    Notes = quoteNotes,
                    ClientNotes = clientNotes,
                    Total = Products.Sum(p => p.Total)
                };

                db.Quotes.Add(quote);

                db.SaveChanges();
            }

            else
            {
                quote = db.Quotes
                    .Include(q => q.Items)
                    .First(q => q.Id == CurrentQuote.Id);

                quote.Total = Products.Sum(p => p.Total);
                quote.DeliveryDate = GetEffectiveDeliveryDate();
                quote.EventDate = EventDatePicker.SelectedDate;
                quote.Title = quoteTitle;
                quote.Notes = quoteNotes;
                quote.ClientNotes = clientNotes;
                quote.Status = selectedStatus;
                quote.Deposit = GetDepositValue();


                db.QuoteItems.RemoveRange(quote.Items);

                db.SaveChanges();

                quote.Items.Clear();
            }

            AddQuoteItems(db, quote.Id);

            db.SaveChanges();

            int savedClientId = client.Id;
            int savedQuoteId = quote.Id;

            SuppressSelectionConfirm = true;

            LoadClients();

            LoadCalendarDeliveries();

            var reloadedClient = ((IEnumerable<Models.Client>)ClientsListBox.ItemsSource)
                .FirstOrDefault(c => c.Id == savedClientId);

            if (reloadedClient != null)
            {
                ClientsListBox.SelectedItem = reloadedClient;
                SyncPredictiveClientSelection(savedClientId);

                using var refreshDb = new AppDbContext();

                var reloadedQuotes = refreshDb.Quotes
                    .Where(q => q.ClientId == savedClientId)
                    .OrderBy(q => q.EventDate ?? DateTime.MaxValue)
                    .ThenBy(q => q.Id)
                    .ToList();

                QuotesListBox.ItemsSource = reloadedQuotes;
                BudgetQuotesListBox.ItemsSource = reloadedQuotes;

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
                "Trabajo guardado correctamente",
                "Éxito",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private string GetSelectedQuoteStatus()
        {
            var selectedStatus = QuoteStatusComboBox.SelectedItem?.ToString();

            return string.IsNullOrWhiteSpace(selectedStatus)
                ? "Pendiente"
                : selectedStatus;
        }

        private void AddQuoteItems(AppDbContext db, int quoteId)
        {
            foreach (var product in Products)
            {
                db.QuoteItems.Add(CreateQuoteItem(product, quoteId));
            }
        }

        private static Models.QuoteItem CreateQuoteItem(ProductLine product, int quoteId)
        {
            return new Models.QuoteItem
            {
                QuoteId = quoteId,
                Quantity = product.Quantity,
                ProductName = product.ProductName,
                TailoringType = product.TailoringType,
                Fabric = product.Fabric,
                BasePrice = product.BasePrice,
                FabricPrice = product.FabricPrice,
                ManualPrice = 0,
                Total = product.Total
            };
        }

        private void UpdateGrandTotal()
        {
            var total = Products.Sum(item => item.Total);

            TotalTextBlock.Text = $"{total:N2} €";

            UpdatePendingAmount();
            UpdateActiveContext();
            UpdateWorkflowState();
        }

        private void LoadClients()
        {
            using var db = new AppDbContext();

            AllClients = db.Clients
                .Include(c => c.Quotes)
                .ToList()
                .OrderBy(c => c.NextEventDate ?? DateTime.MaxValue)
                .ThenBy(c => c.Name)
                .ToList();

            ApplyClientFilter();
            RefreshPredictiveClientResults(ClientPredictiveComboBox.Text);
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
            DniTextBox.Text = client.Dni;

            using var db = new AppDbContext();

            var quotes = db.Quotes
                .Include(q => q.Items)
                .Where(q => q.ClientId == client.Id)
                .OrderBy(q => q.EventDate ?? DateTime.MaxValue)
                .ThenBy(q => q.Id)
                .ToList();

            QuotesListBox.ItemsSource = quotes;
            BudgetQuotesListBox.ItemsSource = quotes;
            ApplyBudgetQuoteFilter();
            UpdateClientDetailPanel(client, quotes);

            Products.Clear();

            UpdateGrandTotal();

            CurrentQuote = null;

            DeliveryDatePicker.SelectedDate = null;
            EventDatePicker.SelectedDate = null;

            QuoteStatusComboBox.SelectedItem = "Pendiente";

            DepositTextBox.Text = "0";
            QuoteNotesTextBox.Text = "";
            ClientNotesTextBox.Text = "";
            QuoteTitleTextBox.Text = "";

            UpdatePendingAmount();

            LastSelectedClientId = client.Id;
            LastSelectedQuoteId = null;
            CurrentClientId = client.Id;
            SyncPredictiveClientSelection(client.Id);
            UpdateWorkflowState();

            IsLoadingData = false;

            if (quotes.Count == 1)
            {
                SuppressSelectionConfirm = true;
                QuotesListBox.SelectedItem = quotes[0];
                BudgetQuotesListBox.SelectedItem = quotes[0];
                SuppressSelectionConfirm = false;
                return;
            }

            MarkAsSaved();

            UpdateSaveButtonText();
            UpdateActiveContext();
            UpdateSecondaryPlaceholders();
            UpdateGlobalSearchPlaceholder();
            UpdateShellNavigationState();
            UpdateWorkspaceButtonState();
        }

        private void UpdateClientDetailPanel(Models.Client? client, IEnumerable<Models.Quote>? quotes)
        {
            if (ClientDetailNameTextBlock == null)
                return;

            if (client == null)
            {
                ClientDetailNameTextBlock.Text = "Selecciona un cliente";
                ClientDetailMetaTextBlock.Text = "Busca o selecciona un cliente para ver su ficha rápida.";
                ClientDetailPhoneTextBlock.Text = "—";
                ClientDetailDniTextBlock.Text = "—";
                ClientDetailNextDeliveryTextBlock.Text = "—";
                ClientDetailSummaryTextBlock.Text = "Sin cliente seleccionado.";
                OpenSelectedClientQuoteButton.IsEnabled = false;
                NewQuoteFromClientButton.IsEnabled = false;
                return;
            }

            var quoteList = quotes?.ToList() ?? new List<Models.Quote>();
            var nextQuote = quoteList
                .Where(q => q.EventDate.HasValue && q.EventDate.Value.Date >= DateTime.Today)
                .OrderBy(q => q.EventDate.GetValueOrDefault())
                .FirstOrDefault();

            ClientDetailNameTextBlock.Text = client.Name;
            ClientDetailMetaTextBlock.Text = quoteList.Count == 1
                ? "1 trabajo asociado"
                : $"{quoteList.Count} trabajos asociados";

            ClientDetailPhoneTextBlock.Text = string.IsNullOrWhiteSpace(client.DisplayPhone) ? "—" : client.DisplayPhone;
            ClientDetailDniTextBlock.Text = string.IsNullOrWhiteSpace(client.Dni) ? "—" : client.Dni;
            ClientDetailNextDeliveryTextBlock.Text = nextQuote == null
                ? "Sin próximos eventos"
                : nextQuote.EventDate.GetValueOrDefault().ToString("dd/MM/yyyy");

            ClientDetailSummaryTextBlock.Text = nextQuote == null
                ? "No hay eventos próximos para este cliente."
                : $"Próximo evento: {nextQuote.DisplayTitle} · {nextQuote.StatusLabelText} · {nextQuote.Total:N2} €";

            OpenSelectedClientQuoteButton.IsEnabled = quoteList.Count > 0;
            NewQuoteFromClientButton.IsEnabled = true;
        }

        private void OpenSelectedClientQuoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsListBox.SelectedItem == null)
            {
                MessageBox.Show(
                    "Selecciona un cliente para abrir sus trabajos.",
                    "Abrir trabajo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            if (QuotesListBox.SelectedItem is Models.Quote selectedQuote)
            {
                OpenQuoteById(selectedQuote.Id, TabPresupuesto, "Trabajos");
                return;
            }

            if (QuotesListBox.Items.Count > 0 && QuotesListBox.Items[0] is Models.Quote firstQuote)
            {
                OpenQuoteById(firstQuote.Id, TabPresupuesto, "Trabajos");
                return;
            }

            GoToTab(TabPresupuesto);
        }

        private void NewQuoteFromClientButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsListBox.SelectedItem == null)
            {
                MessageBox.Show(
                    "Selecciona un cliente antes de crear un trabajo.",
                    "Nuevo trabajo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            NewQuoteButton_Click(sender, e);
        }

        internal void ClientsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ClientsListBox.SelectedItem == null)
                return;

            OpenSelectedClientQuoteButton_Click(sender, e);
        }

        private void QuotesListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (IsRevertingSelection)
                return;

            if (BudgetQuotesListBox.SelectedItem != QuotesListBox.SelectedItem)
                BudgetQuotesListBox.SelectedItem = QuotesListBox.SelectedItem;

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

            CurrentClientId = quote.ClientId;
            UpdateWorkflowState();

            DeliveryDatePicker.SelectedDate = quote.DeliveryDate;
            EventDatePicker.SelectedDate = quote.EventDate;
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
                var line = ProductLine.FromQuoteItem(item);

                Products.Add(line);
            }

            UpdateGrandTotal();

            LastSelectedQuoteId = quote.Id;

            IsLoadingData = false;
            MarkAsSaved();

            UpdateSaveButtonText();
            UpdateActiveContext();
            UpdateSecondaryPlaceholders();

            var targetTab = PendingNavigationTargetTab ?? TabPresupuesto;
            PendingNavigationTargetTab = null;
            GoToTab(targetTab);
        }

        public void BudgetQuotesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BudgetQuotesListBox.SelectedItem == null)
                return;

            if (QuotesListBox.SelectedItem != BudgetQuotesListBox.SelectedItem)
                QuotesListBox.SelectedItem = BudgetQuotesListBox.SelectedItem;
        }

        public void BudgetQuotesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (BudgetQuotesListBox.SelectedItem is not Models.Quote quote)
                return;

            OpenQuoteById(quote.Id, TabProductos, "Trabajos");
        }

        internal void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button)
                return;

            if (button.DataContext is not ProductLine line)
                return;

            Products.Remove(line);

            UpdateGrandTotal();
            MarkAsChanged();
        }

        internal void DuplicateProduct_Click(object sender, RoutedEventArgs e)
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
            UpdateContextBreadcrumb("Trabajos", TabPresupuesto);
            GoToTab(TabPresupuesto);

            if (!ConfirmDiscardChanges())
                return;

            if (!PrepareClientForNewQuote())
                return;

            IsLoadingData = true;

            Products.Clear();

            UpdateGrandTotal();

            CurrentQuote = null;

            QuotesListBox.SelectedItem = null;

            BudgetQuotesListBox.SelectedItem = null;

            LastSelectedQuoteId = null;

            DeliveryDatePicker.SelectedDate = null;
            EventDatePicker.SelectedDate = null;

            QuoteTitleTextBox.Text = "";

            QuoteStatusComboBox.SelectedItem = "Pendiente";

            DepositTextBox.Text = "0";

            QuoteNotesTextBox.Text = "";

            ClientNotesTextBox.Text = "";

            UpdatePendingAmount();

            IsLoadingData = false;
            MarkAsSaved();

            UpdateSaveButtonText();
            UpdateActiveContext();
            UpdateSecondaryPlaceholders();
            UpdateWorkflowState();

            Dispatcher.BeginInvoke(new Action(() =>
            {
                NavigateToProductsAfterJobEdit = true;
                EditJobButton_Click(sender, e);
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void ProductsDataGrid_CellEditEnding(object? sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            if (e.Row.Item is ProductLine)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    // No recalculamos el precio base aquí: el campo "Precio" es editable
                    // y debe mantenerse como precio final cuando cambia la cantidad.
                    // ProductName/TailoringType ya actualizan el precio por defecto desde ProductLine.
                    UpdateGrandTotal();
                    MarkAsChanged();
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void ProductsDataGrid_CurrentCellChanged(object? sender, EventArgs e)
        {
            if (IsLoadingData)
                return;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdateGrandTotal();
                MarkAsChanged();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        internal void DeleteQuote_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button)
                return;

            if (button.DataContext is not Models.Quote quoteToDelete)
                return;

            var result = MessageBox.Show(
                "¿Seguro que quieres eliminar este trabajo?\n\nEsta acción no se puede deshacer.",
                "Eliminar trabajo",
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

                DeliveryDatePicker.SelectedDate = null;
                EventDatePicker.SelectedDate = null;
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
            LoadCalendarDeliveries();

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
                "Trabajo eliminado correctamente.",
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
                RefreshAvailableProductNames();

                MessageBox.Show(
                    "Los productos y precios actualizados se aplicarán a los productos que añadas o cambies a partir de ahora. Los trabajos ya abiertos conservarán sus precios actuales.",
                    "Productos actualizados",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void DuplicateQuoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentQuote == null)
            {
                MessageBox.Show(
                    "Primero selecciona un trabajo para duplicarlo.",
                    "Duplicar trabajo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            if (Products.Count == 0)
            {
                MessageBox.Show(
                    "El trabajo seleccionado no tiene productos para duplicar.",
                    "Duplicar trabajo",
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
                ? QuoteNumberService.FormatQuoteNumber(CurrentQuote.Id, CurrentQuote.CreatedAt)
                : QuoteTitleTextBox.Text;

            QuoteTitleTextBox.Text = $"Copia de {originalTitle}";

            CurrentQuote = null;
            QuotesListBox.SelectedItem = null;

            QuoteStatusComboBox.SelectedItem = "Pendiente";

            UpdateGrandTotal();

            MarkAsChanged();
            UpdateSaveButtonText();

            MessageBox.Show(
                "Trabajo duplicado. Revisa los datos y pulsa Guardar para crear la nueva versión.",
                "Duplicar trabajo",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private decimal GetDepositValue()
        {
            var text = DepositTextBox.Text
                .Replace("€", "")
                .Trim();

            return TryParseNonNegativeDecimal(text, CultureInfo.CurrentCulture, out var deposit) ||
                TryParseNonNegativeDecimal(text, CultureInfo.InvariantCulture, out deposit)
                    ? deposit
                    : 0;
        }

        private static bool TryParseNonNegativeDecimal(string text, CultureInfo cultureInfo, out decimal value)
        {
            if (!decimal.TryParse(text, NumberStyles.Number, cultureInfo, out value))
                return false;

            value = Math.Max(0, value);
            return true;
        }

        private void UpdatePendingAmount()
        {
            var total = Products.Sum(p => p.Total);
            var deposit = GetDepositValue();
            var pending = Math.Max(0, total - deposit);

            PendingAmountTextBlock.Text = $"{pending:N2} €";
        }

        private DateTime GetEffectiveDeliveryDate()
        {
            if (DeliveryDatePicker.SelectedDate.HasValue)
                return DeliveryDatePicker.SelectedDate.Value.Date;

            if (EventDatePicker.SelectedDate.HasValue)
                return EventDatePicker.SelectedDate.Value.Date.AddDays(-3);

            return DateTime.Today;
        }

        private void ApplyClientFilter()
        {
            var selectedClientId = (ClientsListBox.SelectedItem as Models.Client)?.Id;
            var search = NormalizeSearchText(ClientSearchTextBox.Text ?? string.Empty);

            var filteredClients = AllClients
                .Where(MatchesActiveStatusFilter)
                .Where(MatchesActiveDeliveryFilter);

            if (!string.IsNullOrWhiteSpace(search))
                filteredClients = filteredClients.Where(client => MatchesClientSearch(client, search));

            var orderedClients = filteredClients
                .OrderBy(c => c.NextEventDate ?? DateTime.MaxValue)
                .ThenBy(c => c.Name)
                .ToList();

            ClientsListBox.ItemsSource = orderedClients;

            if (selectedClientId == null)
                return;

            var selectedClient = orderedClients.FirstOrDefault(c => c.Id == selectedClientId.Value);
            if (selectedClient != null)
            {
                if (!Equals(ClientsListBox.SelectedItem, selectedClient))
                {
                    IsRevertingSelection = true;
                    ClientsListBox.SelectedItem = selectedClient;
                    IsRevertingSelection = false;
                }

                return;
            }

            ClearVisibleClientSelection();
        }

        private void ClearVisibleClientSelection()
        {
            IsRevertingSelection = true;
            ClientsListBox.SelectedItem = null;
            QuotesListBox.SelectedItem = null;
            BudgetQuotesListBox.SelectedItem = null;
            IsRevertingSelection = false;

            QuotesListBox.ItemsSource = null;
            BudgetQuotesListBox.ItemsSource = null;
            UpdateClientDetailPanel(null, null);
        }

        private bool MatchesActiveStatusFilter(Models.Client client)
        {
            if (ActiveStatusFilter == "Todos")
                return true;

            return client.Quotes.Any(q =>
                string.Equals(GetQuoteStatusOrDefault(q), ActiveStatusFilter, StringComparison.OrdinalIgnoreCase));
        }

        private bool MatchesActiveDeliveryFilter(Models.Client client)
        {
            var today = DateTime.Today;
            var nextEventDate = client.NextEventDate?.Date;

            return ActiveDeliveryFilter switch
            {
                "Próximas" => nextEventDate != null && nextEventDate >= today,
                "Semana" => nextEventDate != null && nextEventDate >= today && nextEventDate <= today.AddDays(7),
                "Mes" => nextEventDate != null && nextEventDate >= today && nextEventDate <= today.AddDays(30),
                "Sin trabajo" => client.Quotes == null || client.Quotes.Count == 0,
                _ => true
            };
        }

        private static bool MatchesClientSearch(Models.Client client, string search)
        {
            return NormalizeSearchText(client.Name).Contains(search) ||
                NormalizeSearchText(client.Phone).Contains(search) ||
                NormalizeSearchText(client.Dni).Contains(search) ||
                NormalizeSearchText(client.EventDateText).Contains(search) ||
                client.Quotes.Any(q => MatchesQuoteSearch(q, search));
        }

        private void RefreshPredictiveClientResults(string? searchText)
        {
            var search = NormalizeSearchText(searchText ?? string.Empty);
            var currentClientId = CurrentClientId;

            var matches = AllClients.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
                matches = matches.Where(client => MatchesClientSearch(client, search));

            var ordered = matches
                .OrderBy(c => c.NextEventDate ?? DateTime.MaxValue)
                .ThenBy(c => c.Name)
                .Take(25)
                .ToList();

            if (currentClientId.HasValue && ordered.All(c => c.Id != currentClientId.Value))
            {
                var currentClient = AllClients.FirstOrDefault(c => c.Id == currentClientId.Value);
                if (currentClient != null)
                    ordered.Insert(0, currentClient);
            }

            PredictiveClientResults.Clear();

            foreach (var client in ordered)
                PredictiveClientResults.Add(client);
        }

        private void SyncPredictiveClientSelection(int? clientId)
        {
            IsUpdatingClientPredictiveComboBox = true;

            try
            {
                RefreshPredictiveClientResults(string.Empty);

                if (!clientId.HasValue)
                {
                    ClientPredictiveComboBox.SelectedItem = null;
                    ClientPredictiveComboBox.Text = string.Empty;
                    return;
                }

                var selectedClient = PredictiveClientResults
                    .FirstOrDefault(c => c.Id == clientId.Value);

                ClientPredictiveComboBox.SelectedItem = selectedClient;

                if (selectedClient != null)
                    ClientPredictiveComboBox.Text = selectedClient.Name;
            }
            finally
            {
                IsUpdatingClientPredictiveComboBox = false;
            }
        }

        private void ClientPredictiveComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsUpdatingClientPredictiveComboBox)
                return;

            RefreshPredictiveClientResults(ClientPredictiveComboBox.Text);

            if (ClientPredictiveComboBox.IsKeyboardFocusWithin && PredictiveClientResults.Count > 0)
                ClientPredictiveComboBox.IsDropDownOpen = true;
        }

        private void ClientPredictiveComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsUpdatingClientPredictiveComboBox)
                return;

            if (ClientPredictiveComboBox.SelectedItem is not Models.Client client)
                return;

            WeakReferenceMessenger.Default.Send(new ClientSelectedMessage(client.Id));
        }

        private void SelectPredictiveClient(int clientId)
        {
            if (CurrentClientId == clientId)
            {
                SyncPredictiveClientSelection(clientId);
                return;
            }

            if (!ConfirmDiscardChanges())
            {
                SyncPredictiveClientSelection(CurrentClientId);
                return;
            }

            SuppressSelectionConfirm = true;
            ShellBreadcrumbOverride = null;

            LoadClients();
            var wasLoaded = LoadClientWorkspaceById(clientId);

            SuppressSelectionConfirm = false;

            if (!wasLoaded)
            {
                SyncPredictiveClientSelection(CurrentClientId);
                return;
            }

            NavigateToSection(TabPresupuesto);
            UpdateContextBreadcrumb("Cliente", TabPresupuesto);
            UpdateShellNavigationState();
        }

        private static bool MatchesQuoteSearch(Models.Quote quote, string search)
        {
            return NormalizeSearchText(quote.Title ?? string.Empty).Contains(search) ||
                NormalizeSearchText(GetQuoteStatusOrDefault(quote)).Contains(search) ||
                (quote.EventDate.HasValue && quote.EventDate.Value.ToString("dd/MM/yyyy").Contains(search));
        }

        private static string GetQuoteStatusOrDefault(Models.Quote quote)
        {
            return string.IsNullOrWhiteSpace(quote.Status)
                ? "Pendiente"
                : quote.Status;
        }

        internal void ClientSearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
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
                return ShowValidationError("Debes indicar el nombre del cliente.", "Faltan datos", ClientNameTextBox);

            if (string.IsNullOrWhiteSpace(PhoneTextBox.Text))
                return ShowValidationError("Debes indicar el teléfono del cliente.", "Faltan datos", PhoneTextBox);

            if (Products.Count == 0)
                return ShowValidationError("Debes añadir al menos un producto al trabajo.", "Trabajo vacío");

            foreach (var product in Products)
            {
                if (!ValidateProductLine(product))
                    return false;
            }

            return true;
        }

        private static bool ShowValidationError(string message, string title, Control? controlToFocus = null)
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            controlToFocus?.Focus();

            return false;
        }

        private bool ValidateProductLine(ProductLine product)
        {
            if (string.IsNullOrWhiteSpace(product.ProductName))
                return ShowValidationError("Hay una línea sin producto seleccionado.", "Producto incompleto");

            if (PriceService.HasTailoringTypes(product.ProductName) &&
                string.IsNullOrWhiteSpace(product.TailoringType))
            {
                return ShowValidationError(
                    $"El producto \"{product.ProductName}\" necesita tipo de confección.",
                    "Producto incompleto");
            }

            if (product.Quantity < 1)
            {
                return ShowValidationError(
                    $"La cantidad del producto \"{product.ProductName}\" debe ser al menos 1.",
                    "Cantidad no válida");
            }

            if (product.Total > 0)
                return true;

            var result = MessageBox.Show(
                $"El producto \"{product.ProductName}\" tiene total 0,00 €. ¿Quieres guardar igualmente?",
                "Producto sin importe",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            return result == MessageBoxResult.Yes;
        }

        private void MarkAsChanged()
        {
            if (IsLoadingData || IsRestoringUndoSnapshot)
                return;

            CaptureWorkspaceUndoStep();

            HasUnsavedChanges = true;

            SetSaveWorkflowState(WorkspaceSaveState.Dirty);
            ScheduleAutoSave();
        }

        private void ResetWorkspaceUndoHistory()
        {
            WorkspaceUndoStack.Clear();
            LastWorkspaceUndoSnapshot = CaptureWorkspaceUndoSnapshot();
        }

        private void CaptureWorkspaceUndoStep()
        {
            var currentSnapshot = CaptureWorkspaceUndoSnapshot();

            if (LastWorkspaceUndoSnapshot == null)
            {
                LastWorkspaceUndoSnapshot = currentSnapshot;
                return;
            }

            if (AreWorkspaceUndoSnapshotsEqual(LastWorkspaceUndoSnapshot, currentSnapshot))
                return;

            WorkspaceUndoStack.Push(LastWorkspaceUndoSnapshot);
            TrimWorkspaceUndoStack();
            LastWorkspaceUndoSnapshot = currentSnapshot;
        }

        private void TrimWorkspaceUndoStack()
        {
            if (WorkspaceUndoStack.Count <= MaxWorkspaceUndoSteps)
                return;

            var retained = WorkspaceUndoStack
                .Take(MaxWorkspaceUndoSteps)
                .Reverse()
                .ToList();

            WorkspaceUndoStack.Clear();

            foreach (var snapshot in retained)
                WorkspaceUndoStack.Push(snapshot);
        }

        private WorkspaceUndoSnapshot CaptureWorkspaceUndoSnapshot()
        {
            return new WorkspaceUndoSnapshot(
                ClientNameTextBox.Text,
                PhoneTextBox.Text,
                DniTextBox.Text,
                DeliveryDatePicker.SelectedDate,
                EventDatePicker.SelectedDate,
                QuoteTitleTextBox.Text,
                GetSelectedQuoteStatus(),
                DepositTextBox.Text,
                QuoteNotesTextBox.Text,
                ClientNotesTextBox.Text,
                Products.Select(ProductUndoSnapshot.FromProductLine).ToList());
        }

        private static bool AreWorkspaceUndoSnapshotsEqual(
            WorkspaceUndoSnapshot left,
            WorkspaceUndoSnapshot right)
        {
            return left.ClientName == right.ClientName &&
                left.Phone == right.Phone &&
                left.Dni == right.Dni &&
                left.DeliveryDate == right.DeliveryDate &&
                left.EventDate == right.EventDate &&
                left.QuoteTitle == right.QuoteTitle &&
                left.QuoteStatus == right.QuoteStatus &&
                left.Deposit == right.Deposit &&
                left.QuoteNotes == right.QuoteNotes &&
                left.ClientNotes == right.ClientNotes &&
                left.Products.SequenceEqual(right.Products);
        }

        private void UndoLastWorkspaceChange()
        {
            if (WorkspaceUndoStack.Count == 0)
                return;

            AutoSaveTimer.Stop();

            var snapshot = WorkspaceUndoStack.Pop();

            IsRestoringUndoSnapshot = true;
            IsLoadingData = true;

            try
            {
                ApplyWorkspaceUndoSnapshot(snapshot);
            }
            finally
            {
                IsLoadingData = false;
                IsRestoringUndoSnapshot = false;
            }

            LastWorkspaceUndoSnapshot = CaptureWorkspaceUndoSnapshot();
            HasUnsavedChanges = true;
            SetSaveWorkflowState(WorkspaceSaveState.Dirty);
            ScheduleAutoSave();
        }

        private void ApplyWorkspaceUndoSnapshot(WorkspaceUndoSnapshot snapshot)
        {
            ClientNameTextBox.Text = snapshot.ClientName;
            PhoneTextBox.Text = snapshot.Phone;
            DniTextBox.Text = snapshot.Dni;
            DeliveryDatePicker.SelectedDate = snapshot.DeliveryDate;
            EventDatePicker.SelectedDate = snapshot.EventDate;
            QuoteTitleTextBox.Text = snapshot.QuoteTitle;
            QuoteStatusComboBox.SelectedItem = string.IsNullOrWhiteSpace(snapshot.QuoteStatus)
                ? "Pendiente"
                : snapshot.QuoteStatus;
            DepositTextBox.Text = snapshot.Deposit;
            QuoteNotesTextBox.Text = snapshot.QuoteNotes;
            ClientNotesTextBox.Text = snapshot.ClientNotes;

            Products.Clear();

            foreach (var productSnapshot in snapshot.Products)
                Products.Add(productSnapshot.ToProductLine());

            UpdateGrandTotal();
            UpdatePendingAmount();
            UpdateSaveButtonText();
            UpdateActiveContext();
            UpdateSecondaryPlaceholders();
            UpdateWorkflowState();
            UpdateWorkspaceButtonState();
        }

        private void ScheduleAutoSave()
        {
            if (IsLoadingData || IsAutoSaving)
                return;

            if (CurrentClientId == null || CurrentQuote == null)
                return;

            AutoSaveTimer.Stop();
            AutoSaveTimer.Start();
        }

        private void AutoSaveTimer_Tick(object? sender, EventArgs e)
        {
            AutoSaveTimer.Stop();
            TryAutoSaveCurrentWorkspace();
        }

        private void SaveStatusRefreshTimer_Tick(object? sender, EventArgs e)
        {
            if (CurrentSaveState != WorkspaceSaveState.Saved)
            {
                SaveStatusRefreshTimer.Stop();
                return;
            }

            UpdateShellSaveStateIndicator();
        }

        private bool CanAutoSaveCurrentWorkspace()
        {
            return HasUnsavedChanges &&
                CurrentClientId != null &&
                CurrentQuote != null &&
                HasRequiredClientData() &&
                Products.Count > 0 &&
                Products.All(IsProductLineReadyForAutoSave);
        }

        private bool HasRequiredClientData()
        {
            return !string.IsNullOrWhiteSpace(ClientNameTextBox.Text) &&
                !string.IsNullOrWhiteSpace(PhoneTextBox.Text);
        }

        private static bool IsProductLineReadyForAutoSave(ProductLine product)
        {
            if (string.IsNullOrWhiteSpace(product.ProductName))
                return false;

            if (PriceService.HasTailoringTypes(product.ProductName) &&
                string.IsNullOrWhiteSpace(product.TailoringType))
            {
                return false;
            }

            return product.Quantity >= 1;
        }

        private void TryAutoSaveCurrentWorkspace()
        {
            if (!CanAutoSaveCurrentWorkspace())
                return;

            try
            {
                IsAutoSaving = true;
                SetSaveWorkflowState(WorkspaceSaveState.Saving);
                Dispatcher.Invoke(() => { }, DispatcherPriority.Background);

                if (CurrentClientId is not int clientId || CurrentQuote is not { } currentQuote)
                    return;

                using var db = new AppDbContext();

                var client = db.Clients.FirstOrDefault(c => c.Id == clientId);
                var quote = db.Quotes
                    .Include(q => q.Items)
                    .FirstOrDefault(q => q.Id == currentQuote.Id);

                if (client == null || quote == null)
                {
                    SetSaveWorkflowState(WorkspaceSaveState.Dirty);
                    return;
                }

                client.Name = ClientNameTextBox.Text.Trim();
                client.Phone = PhoneTextBox.Text.Trim();
                client.Dni = DniTextBox.Text.Trim();

                quote.Title = QuoteTitleTextBox.Text.Trim();
                quote.Notes = QuoteNotesTextBox.Text.Trim();
                quote.ClientNotes = ClientNotesTextBox.Text.Trim();
                quote.Status = GetSelectedQuoteStatus();
                quote.Deposit = GetDepositValue();
                quote.DeliveryDate = GetEffectiveDeliveryDate();
                quote.EventDate = EventDatePicker.SelectedDate;
                quote.Total = Products.Sum(p => p.Total);

                db.QuoteItems.RemoveRange(quote.Items);
                AddQuoteItems(db, quote.Id);

                db.SaveChanges();

                currentQuote.Title = quote.Title;
                currentQuote.Notes = quote.Notes;
                currentQuote.ClientNotes = quote.ClientNotes;
                currentQuote.Status = quote.Status;
                currentQuote.Deposit = quote.Deposit;
                currentQuote.DeliveryDate = quote.DeliveryDate;
                currentQuote.EventDate = quote.EventDate;
                currentQuote.Total = quote.Total;

                LoadCalendarDeliveries();
                UpdateActiveContext();
                UpdateWorkflowState();
                MarkAsSaved();
            }
            catch
            {
                SetSaveWorkflowState(WorkspaceSaveState.SaveFailed);
            }
            finally
            {
                IsAutoSaving = false;
            }
        }

        private bool TryFlushPendingChangesBeforeWorkflowAction(string actionName)
        {
            if (!HasUnsavedChanges)
                return true;

            if (CanAutoSaveCurrentWorkspace())
            {
                TryAutoSaveCurrentWorkspace();

                if (!HasUnsavedChanges)
                    return true;
            }

            var result = MessageBox.Show(
                $"Hay cambios pendientes antes de {actionName}.\n\n" +
                "Sí: guardar ahora.\n" +
                "No: continuar con los datos visibles sin guardar.\n" +
                "Cancelar: volver al trabajo.",
                "Cambios pendientes",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Cancel)
                return false;

            if (result == MessageBoxResult.No)
                return true;

            SaveButton_Click(this, new RoutedEventArgs());

            return !HasUnsavedChanges;
        }

        private void MarkCurrentQuoteAsDeliveredFromQuickAction()
        {
            if (CurrentQuote == null)
                return;

            QuoteStatusComboBox.SelectedItem = "Entregado";
            MarkAsChanged();
            UpdateActiveContext();

            if (CanAutoSaveCurrentWorkspace())
                TryAutoSaveCurrentWorkspace();
        }

        private bool ConfirmDiscardChanges()
        {
            if (!HasUnsavedChanges)
                return true;

            if (CanAutoSaveCurrentWorkspace())
            {
                TryAutoSaveCurrentWorkspace();

                if (!HasUnsavedChanges)
                    return true;
            }

            if (IsConfirmingDiscard)
                return false;

            IsConfirmingDiscard = true;

            var result = MessageBox.Show(
                "Hay cambios sin guardar que no se han podido guardar automáticamente. ¿Quieres continuar sin guardar?",
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

            GoToTab(TabClientes);
            FocusSelectedClient();
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

            GoToTab(TabClientes);
            FocusSelectedQuote();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
                return;

            switch (e.Key)
            {
                case Key.S:
                    e.Handled = true;
                    SaveCurrentWorkspaceFromShortcut();
                    break;

                case Key.Z:
                    if (WorkspaceUndoStack.Count > 0)
                    {
                        e.Handled = true;
                        UndoLastWorkspaceChange();
                    }
                    break;

                case Key.P:
                    e.Handled = true;
                    ExportPdfButton_Click(sender, e);
                    break;

                case Key.F:
                    e.Handled = true;
                    FocusGlobalSearchFromShortcut();
                    break;

                case Key.D:
                    e.Handled = true;
                    DuplicateCurrentQuoteFromShortcut();
                    break;

                case Key.D1:
                case Key.NumPad1:
                    e.Handled = true;
                    NavigateToWorkspaceSectionFromShortcut(TabPresupuesto);
                    break;

                case Key.D2:
                case Key.NumPad2:
                    e.Handled = true;
                    NavigateToWorkspaceSectionFromShortcut(TabProductos);
                    break;

                case Key.D3:
                case Key.NumPad3:
                    e.Handled = true;
                    NavigateToWorkspaceDocumentsFromShortcut();
                    break;

                case Key.D4:
                case Key.NumPad4:
                    e.Handled = true;
                    NavigateToWorkspaceSectionFromShortcut(TabClientes);
                    break;
            }
        }

        private void SaveCurrentWorkspaceFromShortcut()
        {
            if (CurrentSaveState == WorkspaceSaveState.Saving)
                return;

            AutoSaveTimer.Stop();
            SaveButton_Click(this, new RoutedEventArgs());
        }

        private void FocusGlobalSearchFromShortcut()
        {
            GlobalSearchTextBox.Focus();
            GlobalSearchTextBox.SelectAll();
        }

        private void DuplicateCurrentQuoteFromShortcut()
        {
            if (DuplicateQuoteButton.IsEnabled)
                DuplicateQuoteButton_Click(this, new RoutedEventArgs());
        }

        private void NavigateToWorkspaceSectionFromShortcut(int sectionIndex)
        {
            if (sectionIndex == TabProductos)
            {
                if (!TryFlushPendingChangesBeforeWorkflowAction("abrir productos"))
                    return;

                OpenProductsInlinePanel();
                return;
            }

            UpdateContextBreadcrumb("Trabajos", sectionIndex);
            NavigateToSection(sectionIndex);
        }

        private void NavigateToWorkspaceDocumentsFromShortcut()
        {
            if (!TryFlushPendingChangesBeforeWorkflowAction("abrir documentos"))
                return;

            OpenBudgetDocumentsTab();
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
            var text = CurrentQuote == null
                ? "Guardar"
                : "Actualizar";

            SaveButton.Content = text;
            SaveProductsButton.Content = text;
        }

        private void UpdateUnsavedChangesIndicator()
        {
            var visibility = HasUnsavedChanges
                ? Visibility.Visible
                : Visibility.Collapsed;

            UnsavedChangesTextBlock.Visibility = visibility;
            ProductsUnsavedChangesTextBlock.Visibility = visibility;
        }

        private void MarkAsSaved()
        {
            AutoSaveTimer.Stop();
            HasUnsavedChanges = false;
            LastSavedAt = DateTime.Now;

            SetSaveWorkflowState(WorkspaceSaveState.Saved);

            if (!IsAutoSaving)
                ResetWorkspaceUndoHistory();
        }

        private void SetSaveWorkflowState(WorkspaceSaveState state)
        {
            CurrentSaveState = state;

            if (state == WorkspaceSaveState.Dirty)
                HasUnsavedChanges = true;
            else if (state == WorkspaceSaveState.Saved)
                HasUnsavedChanges = false;

            UpdateSaveStatusRefreshTimer();
            UpdateUnsavedChangesIndicator();
            UpdateShellSaveStateIndicator();
            UpdateWindowTitle();
        }

        private void UpdateSaveStatusRefreshTimer()
        {
            if (CurrentSaveState == WorkspaceSaveState.Saved && LastSavedAt != null)
                SaveStatusRefreshTimer.Start();
            else
                SaveStatusRefreshTimer.Stop();
        }

        private void UpdateShellSaveStateIndicator()
        {
            if (ShellSaveStateTextBlock == null || ShellSaveStateDot == null || ShellSaveStateBadge == null)
                return;

            string text;
            SolidColorBrush dotBrush;
            SolidColorBrush foregroundBrush;
            SolidColorBrush backgroundBrush;

            switch (CurrentSaveState)
            {
                case WorkspaceSaveState.SaveFailed:
                    text = "Error al guardar";
                    dotBrush = new SolidColorBrush(MediaColor.FromRgb(168, 64, 52));
                    foregroundBrush = new SolidColorBrush(MediaColor.FromRgb(120, 44, 36));
                    backgroundBrush = new SolidColorBrush(MediaColor.FromRgb(252, 238, 235));
                    break;

                case WorkspaceSaveState.Saving:
                    text = "Guardando…";
                    dotBrush = new SolidColorBrush(MediaColor.FromRgb(168, 178, 161));
                    foregroundBrush = new SolidColorBrush(MediaColor.FromRgb(47, 51, 45));
                    backgroundBrush = new SolidColorBrush(MediaColor.FromRgb(238, 242, 234));
                    break;

                case WorkspaceSaveState.Dirty:
                    text = "Cambios pendientes";
                    dotBrush = new SolidColorBrush(MediaColor.FromRgb(180, 128, 74));
                    foregroundBrush = new SolidColorBrush(MediaColor.FromRgb(95, 65, 32));
                    backgroundBrush = new SolidColorBrush(MediaColor.FromRgb(250, 244, 235));
                    break;

                default:
                    text = GetSavedStateText();
                    dotBrush = new SolidColorBrush(MediaColor.FromRgb(100, 191, 35));
                    foregroundBrush = new SolidColorBrush(MediaColor.FromRgb(109, 116, 104));
                    backgroundBrush = new SolidColorBrush(MediaColor.FromRgb(238, 242, 234));
                    break;
            }

            ShellSaveStateTextBlock.Text = text;
            ShellSaveStateTextBlock.Foreground = foregroundBrush;
            ShellSaveStateDot.Fill = dotBrush;
            ShellSaveStateBadge.Background = backgroundBrush;
            ShellSaveStateBadge.ToolTip = GetSaveStateTooltip();

            if (ShellRetrySaveButton != null)
            {
                ShellRetrySaveButton.Visibility = CurrentSaveState == WorkspaceSaveState.SaveFailed
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }


        private string GetSavedStateText()
        {
            if (LastSavedAt == null)
                return "Guardado";

            var elapsed = DateTime.Now - LastSavedAt.Value;

            if (elapsed.TotalSeconds < 45)
                return "Guardado ahora";

            if (elapsed.TotalMinutes < 60)
                return $"Guardado hace {(int)Math.Max(1, elapsed.TotalMinutes)} min";

            return $"Guardado {LastSavedAt.Value:HH:mm}";
        }

        private string GetSaveStateTooltip()
        {
            return CurrentSaveState switch
            {
                WorkspaceSaveState.SaveFailed => "No se ha podido guardar automáticamente. Revisa los datos y pulsa Reintentar.",
                WorkspaceSaveState.Saving => "Guardando los cambios del trabajo activo…",
                WorkspaceSaveState.Dirty => "Hay cambios pendientes de guardar.",
                _ when LastSavedAt != null => $"Último guardado: {LastSavedAt.Value:dd/MM/yyyy HH:mm:ss}",
                _ => "Estado de guardado del trabajo activo"
            };
        }

        private void UpdateWindowTitle()
        {
            Title = HasUnsavedChanges
                ? "Sastrería Presupuestos *"
                : "Sastrería Presupuestos";
        }

        private void UpdateExportFolderSettingsUi()
        {
            ClientSheetFolderTextBlock.Text = FormatExportFolder(CurrentSettings.ClientSheetExportFolder);
            QuoteFolderTextBlock.Text = FormatExportFolder(CurrentSettings.QuoteExportFolder);
            InvoiceFolderTextBlock.Text = FormatExportFolder(CurrentSettings.InvoiceExportFolder);
        }

        private string FormatExportFolder(string? folder)
        {
            return string.IsNullOrWhiteSpace(folder)
                ? "Sin carpeta seleccionada"
                : folder;
        }

        private void SelectExportFolder(string title, Action<string> updateSetting)
        {
            var dialog = new OpenFolderDialog
            {
                Title = title,
                Multiselect = false
            };

            if (dialog.ShowDialog(this) != true)
                return;

            updateSetting(dialog.FolderName);
            AppSettingsService.Save(CurrentSettings);
            UpdateExportFolderSettingsUi();
        }

        private void ClientSheetFolderButton_Click(object sender, RoutedEventArgs e)
        {
            SelectExportFolder(
                "Seleccionar carpeta para fichas de cliente",
                folder => CurrentSettings.ClientSheetExportFolder = folder);
        }

        private void QuoteFolderButton_Click(object sender, RoutedEventArgs e)
        {
            SelectExportFolder(
                "Seleccionar carpeta para presupuestos",
                folder => CurrentSettings.QuoteExportFolder = folder);
        }

        private void InvoiceFolderButton_Click(object sender, RoutedEventArgs e)
        {
            SelectExportFolder(
                "Seleccionar carpeta para facturas",
                folder => CurrentSettings.InvoiceExportFolder = folder);
        }

        private void OpenQuoteFolderButton_Click(object sender, RoutedEventArgs e)
        {
            OpenConfiguredExportFolder(CurrentSettings.QuoteExportFolder, "presupuestos");
        }

        private void OpenClientSheetFolderButton_Click(object sender, RoutedEventArgs e)
        {
            OpenConfiguredExportFolder(CurrentSettings.ClientSheetExportFolder, "fichas de cliente");
        }

        private void OpenConfiguredExportFolder(string? folder, string label)
        {
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                MessageBox.Show(
                    $"Configura primero la carpeta de {label} en Ajustes.",
                    "Abrir carpeta",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = folder,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudo abrir la carpeta. {ex.Message}",
                    "Abrir carpeta",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadInvoices()
        {
            Invoices.Clear();

            using var db = new AppDbContext();
            var search = NormalizeSearchText(InvoiceSearchTextBox?.Text ?? "");

            var invoices = db.Invoices
                .Include(i => i.Client)
                .Include(i => i.Items)
                .AsNoTracking()
                .OrderByDescending(i => i.CreatedAt)
                .ThenByDescending(i => i.Id)
                .ToList();

            foreach (var invoice in invoices)
            {
                if (!string.IsNullOrWhiteSpace(search) &&
                    !NormalizeSearchText(invoice.InvoiceNumber).Contains(search) &&
                    !NormalizeSearchText(invoice.Client?.Name ?? "").Contains(search) &&
                    !NormalizeSearchText(invoice.Summary).Contains(search))
                {
                    continue;
                }

                Invoices.Add(new InvoiceListItem
                {
                    InvoiceId = invoice.Id,
                    InvoiceNumber = invoice.DisplayTitle,
                    CreatedAt = invoice.CreatedAt,
                    ClientName = invoice.Client?.Name ?? "",
                    Summary = invoice.Summary,
                    Total = invoice.Total,
                    Series = invoice.Series
                });
            }
        }

        private void InvoiceSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadInvoices();
        }

        private void InvoicesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InvoicesListBox.SelectedItem is InvoiceListItem item)
                LoadInvoiceDetail(item.InvoiceId);
            else
                ClearInvoiceDetail();
        }

        private void ClearInvoiceDetail()
        {
            InvoiceDetailNumberTextBlock.Text = "Selecciona una factura";
            InvoiceDetailMetaTextBlock.Text = "Las facturas emitidas se visualizan en modo solo lectura.";
            InvoiceDetailClientTextBlock.Text = "Cliente: —";
            InvoiceDetailSourceTextBlock.Text = "Origen: —";
            InvoiceDetailTotalTextBlock.Text = "Total: 0,00 €";
            InvoiceItemsDataGrid.ItemsSource = null;
            CreateCorrectiveInvoiceButton.IsEnabled = false;
            InvoiceExportButton.IsEnabled = false;
            InvoicePrintButton.IsEnabled = false;
        }

        private void LoadInvoiceDetail(int invoiceId)
        {
            using var db = new AppDbContext();
            var invoice = db.Invoices
                .Include(i => i.Client)
                .Include(i => i.SourceQuote)
                .Include(i => i.Items)
                .AsNoTracking()
                .FirstOrDefault(i => i.Id == invoiceId);

            if (invoice == null)
            {
                ClearInvoiceDetail();
                return;
            }

            InvoiceDetailNumberTextBlock.Text = invoice.DisplayTitle;
            InvoiceDetailMetaTextBlock.Text = $"{invoice.CreatedAt:dd/MM/yyyy} · Serie {invoice.Series} · {invoice.Status}";
            InvoiceDetailClientTextBlock.Text = $"Cliente: {invoice.Client?.Name ?? "—"}";
            InvoiceDetailSourceTextBlock.Text = invoice.SourceQuote == null
                ? "Origen: factura directa"
                : $"Origen: {invoice.SourceQuote.DisplayTitle}";
            InvoiceDetailTotalTextBlock.Text = $"Total: {invoice.Total:N2} €";
            InvoiceItemsDataGrid.ItemsSource = invoice.Items.ToList();
            CreateCorrectiveInvoiceButton.IsEnabled = true;
            InvoiceExportButton.IsEnabled = true;
            InvoicePrintButton.IsEnabled = true;
        }

        private void InvoiceCreateButton_Click(object sender, RoutedEventArgs e)
        {
            var clientId = SelectClientForInvoiceWizard();
            if (clientId == null)
                return;

            using var db = new AppDbContext();
            var client = db.Clients
                .Include(c => c.Quotes)
                .ThenInclude(q => q.Items)
                .FirstOrDefault(c => c.Id == clientId.Value);

            if (client == null)
                return;

            var quote = SelectQuoteForInvoiceWizard(client);
            if (quote != null)
            {
                CreateInvoiceFromQuote(quote.Id);
                return;
            }

            var products = SelectStandaloneInvoiceProducts();
            if (products == null || products.Count == 0)
                return;

            CreateStandaloneInvoice(client.Id, products);
        }

        private int? SelectClientForInvoiceWizard()
        {
            using var db = new AppDbContext();
            var clients = db.Clients.AsNoTracking().OrderBy(c => c.Name).ToList();
            var dialog = CreateContextEditDialog("Crear factura", "Selecciona un cliente existente o crea uno nuevo.");
            dialog.AcceptButton.Content = "Continuar";

            var combo = new ComboBox
            {
                Height = 36,
                FontSize = 14,
                ItemsSource = clients,
                DisplayMemberPath = "Name",
                SelectedValuePath = "Id",
                BorderBrush = (Brush)FindResource("BorderSoftBrush"),
                BorderThickness = new Thickness(1),
                Background = Brushes.White
            };
            var nameBox = CreateDialogTextBox("");
            var phoneBox = CreateDialogTextBox("");
            var dniBox = CreateDialogTextBox("");

            AddDialogField(dialog.ContentPanel, "Cliente existente", combo);
            AddDialogField(dialog.ContentPanel, "Nuevo cliente - nombre", nameBox);
            AddDialogField(dialog.ContentPanel, "Nuevo cliente - teléfono", phoneBox);
            AddDialogField(dialog.ContentPanel, "Nuevo cliente - DNI/CIF", dniBox);

            int? selectedClientId = null;
            dialog.AcceptButton.Click += (_, _) =>
            {
                if (combo.SelectedValue is int existingClientId)
                {
                    selectedClientId = existingClientId;
                    dialog.Window.DialogResult = true;
                    return;
                }

                if (string.IsNullOrWhiteSpace(nameBox.Text))
                {
                    MessageBox.Show("Selecciona un cliente o indica el nombre del nuevo cliente.", "Crear factura", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using var createDb = new AppDbContext();
                var client = new Models.Client
                {
                    Name = nameBox.Text.Trim(),
                    Phone = phoneBox.Text.Trim(),
                    Dni = dniBox.Text.Trim()
                };
                createDb.Clients.Add(client);
                createDb.SaveChanges();
                selectedClientId = client.Id;
                LoadClients();
                dialog.Window.DialogResult = true;
            };

            return dialog.Window.ShowDialog() == true ? selectedClientId : null;
        }

        private Models.Quote? SelectQuoteForInvoiceWizard(Models.Client client)
        {
            var quotes = client.Quotes.OrderBy(q => q.EventDate ?? DateTime.MaxValue).ThenBy(q => q.Id).ToList();
            if (quotes.Count == 0)
                return null;

            var dialog = CreateContextEditDialog("Presupuesto", "Elige un presupuesto o continúa sin presupuesto.");
            dialog.AcceptButton.Content = "Continuar";

            var combo = new ComboBox
            {
                Height = 36,
                FontSize = 14,
                ItemsSource = quotes,
                DisplayMemberPath = "DisplayTitle",
                BorderBrush = (Brush)FindResource("BorderSoftBrush"),
                BorderThickness = new Thickness(1),
                Background = Brushes.White
            };
            var withoutQuoteCheckBox = new CheckBox { Content = "Crear factura sin presupuesto" };
            AddDialogField(dialog.ContentPanel, "Presupuesto", combo);
            dialog.ContentPanel.Children.Add(withoutQuoteCheckBox);

            Models.Quote? selectedQuote = null;
            dialog.AcceptButton.Click += (_, _) =>
            {
                selectedQuote = withoutQuoteCheckBox.IsChecked == true ? null : combo.SelectedItem as Models.Quote;
                dialog.Window.DialogResult = true;
            };

            return dialog.Window.ShowDialog() == true ? selectedQuote : null;
        }

        private List<InvoiceItem>? SelectStandaloneInvoiceProducts()
        {
            var dialog = CreateContextEditDialog("Productos", "Añade los productos que aparecerán en la factura.");
            dialog.AcceptButton.Content = "Crear";
            var productCombo = new ComboBox
            {
                Height = 36,
                FontSize = 14,
                ItemsSource = PriceService.GetProductNames(),
                BorderBrush = (Brush)FindResource("BorderSoftBrush"),
                BorderThickness = new Thickness(1),
                Background = Brushes.White
            };
            var quantityBox = CreateDialogTextBox("1");
            var priceBox = CreateDialogTextBox("0");
            var selectedItems = new ObservableCollection<InvoiceItem>();
            var itemsListBox = new ListBox
            {
                Height = 116,
                ItemsSource = selectedItems,
                DisplayMemberPath = "Description",
                BorderBrush = (Brush)FindResource("BorderSoftBrush"),
                BorderThickness = new Thickness(1),
                Background = Brushes.White
            };
            var addButton = new Button
            {
                Content = "Añadir producto",
                Height = 34,
                MinWidth = 118,
                Style = (Style)FindResource("SecondaryButtonStyle"),
                Margin = new Thickness(0, 8, 0, 0)
            };
            AddDialogField(dialog.ContentPanel, "Producto", productCombo);
            AddDialogField(dialog.ContentPanel, "Cantidad", quantityBox);
            AddDialogField(dialog.ContentPanel, "Precio unitario", priceBox);
            dialog.ContentPanel.Children.Add(addButton);
            AddDialogField(dialog.ContentPanel, "Productos añadidos", itemsListBox);

            addButton.Click += (_, _) =>
            {
                var productName = productCombo.SelectedItem?.ToString() ?? "";
                if (string.IsNullOrWhiteSpace(productName) ||
                    !int.TryParse(quantityBox.Text, out var quantity) ||
                    !decimal.TryParse(priceBox.Text, out var price))
                {
                    MessageBox.Show("Completa producto, cantidad y precio.", "Crear factura", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var normalizedQuantity = Math.Max(1, quantity);
                var normalizedPrice = Math.Max(0, price);
                selectedItems.Add(new InvoiceItem
                {
                    ProductName = productName,
                    Description = $"{productName} · {normalizedQuantity} x {normalizedPrice:N2} €",
                    Quantity = normalizedQuantity,
                    UnitPrice = normalizedPrice,
                    Total = normalizedQuantity * normalizedPrice
                });

                productCombo.SelectedIndex = -1;
                quantityBox.Text = "1";
                priceBox.Text = "0";
            };

            List<InvoiceItem>? result = null;
            dialog.AcceptButton.Click += (_, _) =>
            {
                if (selectedItems.Count == 0)
                {
                    MessageBox.Show("Añade al menos un producto.", "Crear factura", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                result = selectedItems
                    .Select(item => new InvoiceItem
                    {
                        ProductName = item.ProductName,
                        Description = item.Description,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Total = item.Total
                    })
                    .ToList();
                dialog.Window.DialogResult = true;
            };

            return dialog.Window.ShowDialog() == true ? result : null;
        }

        private void CreateInvoiceFromQuote(int quoteId, string? forcedSeries = null, int? correctsInvoiceId = null)
        {
            using var db = new AppDbContext();
            var quote = db.Quotes.Include(q => q.Client).Include(q => q.Items).FirstOrDefault(q => q.Id == quoteId);
            if (quote == null)
                return;

            var existingInvoice = db.Invoices.FirstOrDefault(i => i.SourceQuoteId == quoteId && i.CorrectsInvoiceId == null);
            if (existingInvoice != null && correctsInvoiceId == null)
            {
                MessageBox.Show($"Este presupuesto ya tiene factura: {existingInvoice.InvoiceNumber}.", "Crear factura", MessageBoxButton.OK, MessageBoxImage.Information);
                OpenInvoice(existingInvoice.Id);
                return;
            }

            var series = forcedSeries ?? SelectInvoiceSeries(correctsInvoiceId != null);
            if (string.IsNullOrWhiteSpace(series))
                return;

            var invoice = BuildInvoice(db, quote.ClientId, quote.Items.Select(CreateInvoiceItemFromQuoteItem).ToList(), series, quote.Id, correctsInvoiceId);
            invoice.Notes = quote.Notes;
            if (correctsInvoiceId != null)
            {
                foreach (var item in invoice.Items)
                {
                    item.UnitPrice *= -1;
                    item.Total *= -1;
                }

                invoice.Total = invoice.Items.Sum(i => i.Total);
                invoice.Status = "Rectificativa";
                invoice.Notes = $"Rectifica la factura #{correctsInvoiceId}. {invoice.Notes}".Trim();
            }

            db.Invoices.Add(invoice);
            db.SaveChanges();
            LoadInvoices();
            OpenInvoice(invoice.Id);
            MessageBox.Show($"Factura {invoice.InvoiceNumber} creada correctamente.", "Facturas", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CreateStandaloneInvoice(int clientId, List<InvoiceItem> items)
        {
            using var db = new AppDbContext();
            var series = SelectInvoiceSeries(false);
            if (string.IsNullOrWhiteSpace(series))
                return;

            var invoice = BuildInvoice(db, clientId, items, series, null, null);

            db.Invoices.Add(invoice);
            db.SaveChanges();
            LoadInvoices();
            OpenInvoice(invoice.Id);
        }

        private Invoice BuildInvoice(AppDbContext db, int clientId, List<InvoiceItem> items, string series, int? sourceQuoteId, int? correctsInvoiceId)
        {
            var createdAt = DateTime.Now;
            var number = InvoiceNumberService.GetNextNumber(db, series, createdAt);
            return new Invoice
            {
                CreatedAt = createdAt,
                Series = series,
                Number = number,
                InvoiceNumber = InvoiceNumberService.Format(createdAt, series, number),
                ClientId = clientId,
                SourceQuoteId = sourceQuoteId,
                CorrectsInvoiceId = correctsInvoiceId,
                Total = items.Sum(i => i.Total),
                Items = items
            };
        }

        private static InvoiceItem CreateInvoiceItemFromQuoteItem(QuoteItem item)
        {
            var quantity = item.Quantity < 1 ? 1 : item.Quantity;
            return new InvoiceItem
            {
                ProductName = item.ProductName,
                Description = string.Join(" · ", new[] { item.TailoringType, item.Fabric }.Where(v => !string.IsNullOrWhiteSpace(v))),
                Quantity = quantity,
                UnitPrice = item.Total / quantity,
                Total = item.Total
            };
        }

        private string? SelectInvoiceSeries(bool isCorrective)
        {
            var dialog = CreateContextEditDialog("Serie de factura", "Selecciona la serie que se usará para la numeración.");
            dialog.AcceptButton.Content = "Aceptar";
            var combo = new ComboBox
            {
                Height = 36,
                FontSize = 14,
                ItemsSource = isCorrective
                    ? new[] { InvoiceSeries.CorrectiveSimplified, InvoiceSeries.CorrectiveCompany }
                    : new[] { InvoiceSeries.Customer, InvoiceSeries.Simplified, InvoiceSeries.Company },
                SelectedIndex = 0,
                BorderBrush = (Brush)FindResource("BorderSoftBrush"),
                BorderThickness = new Thickness(1),
                Background = Brushes.White
            };
            AddDialogField(dialog.ContentPanel, "Serie", combo);
            string? result = null;
            dialog.AcceptButton.Click += (_, _) =>
            {
                result = combo.SelectedItem?.ToString();
                dialog.Window.DialogResult = true;
            };
            return dialog.Window.ShowDialog() == true ? result : null;
        }

        private void ConvertQuoteToInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentQuote == null)
            {
                MessageBox.Show("Selecciona o guarda un presupuesto antes de convertirlo en factura.", "Facturas", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!TryFlushPendingChangesBeforeWorkflowAction("crear factura"))
                return;

            CreateInvoiceFromQuote(CurrentQuote.Id);
        }

        private void ViewQuoteInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentQuote == null)
                return;

            using var db = new AppDbContext();
            var invoice = db.Invoices.AsNoTracking().FirstOrDefault(i => i.SourceQuoteId == CurrentQuote.Id);
            if (invoice == null)
            {
                MessageBox.Show("Este presupuesto todavía no tiene factura.", "Facturas", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            OpenInvoice(invoice.Id);
        }

        private void OpenInvoice(int invoiceId)
        {
            NavigateToSection(TabFacturas);
            LoadInvoices();
            InvoicesListBox.SelectedItem = Invoices.FirstOrDefault(i => i.InvoiceId == invoiceId);
            LoadInvoiceDetail(invoiceId);
        }

        private void InvoiceExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (InvoicesListBox.SelectedItem is InvoiceListItem item && ExportInvoicePdf(item.InvoiceId) != null)
                MessageBox.Show("PDF de factura generado correctamente.", "Facturas", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void InvoicePrintButton_Click(object sender, RoutedEventArgs e)
        {
            if (InvoicesListBox.SelectedItem is not InvoiceListItem item)
                return;

            var pdfPath = ExportInvoicePdf(item.InvoiceId);
            if (pdfPath == null)
                return;

            Process.Start(new ProcessStartInfo { FileName = pdfPath, UseShellExecute = true });
        }

        private string? ExportInvoicePdf(int invoiceId)
        {
            using var db = new AppDbContext();
            var invoice = db.Invoices.Include(i => i.Client).Include(i => i.Items).AsNoTracking().FirstOrDefault(i => i.Id == invoiceId);
            if (invoice == null)
                return null;

            var exportInvoice = new ExportInvoice
            {
                InvoiceNumber = invoice.InvoiceNumber,
                Series = invoice.Series,
                CreatedAt = invoice.CreatedAt,
                ClientName = invoice.Client?.Name ?? "",
                ClientPhone = invoice.Client?.Phone ?? "",
                ClientDni = invoice.Client?.Dni ?? "",
                Notes = invoice.Notes,
                Items = invoice.Items.Select(i => new ExportInvoiceItem
                {
                    ProductName = i.ProductName,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Total = i.Total
                }).ToList()
            };

            var fileName = $"{MakeSafeFileName(invoice.InvoiceNumber)}.pdf";
            if (!TryBuildExportFilePath(CurrentSettings.InvoiceExportFolder, fileName, "facturas", out var pdfPath))
                return null;

            InvoicePdfService.ExportInvoiceToPdf(exportInvoice, pdfPath);
            LastGeneratedPdfPath = pdfPath;
            return pdfPath;
        }

        private void CreateCorrectiveInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (InvoicesListBox.SelectedItem is not InvoiceListItem item)
                return;

            using var db = new AppDbContext();
            var invoice = db.Invoices.AsNoTracking().FirstOrDefault(i => i.Id == item.InvoiceId);
            if (invoice == null || invoice.SourceQuoteId == null)
                return;

            var correctiveSeries = invoice.Series == InvoiceSeries.Company
                ? InvoiceSeries.CorrectiveCompany
                : InvoiceSeries.CorrectiveSimplified;

            CreateInvoiceFromQuote(invoice.SourceQuoteId.Value, correctiveSeries, invoice.Id);
        }


        private bool TryBuildExportFilePath(string? folder, string fileName, string configurationLabel, out string filePath)
        {
            filePath = string.Empty;

            if (string.IsNullOrWhiteSpace(folder))
            {
                MessageBox.Show(
                    $"Configura primero la carpeta de guardado para {configurationLabel} en Ajustes.",
                    "Carpeta no configurada",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                NavigateToSection(TabAjustes);
                return false;
            }

            try
            {
                Directory.CreateDirectory(folder);
                filePath = Path.Combine(folder, fileName);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudo preparar la carpeta de guardado para {configurationLabel}.\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }
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

            if (!TryFlushPendingChangesBeforeWorkflowAction("generar el PDF"))
                return;

            var exportQuote = ExportDataService.CreateExportQuote(
                ClientNameTextBox.Text,
                PhoneTextBox.Text,
                GetEffectiveDeliveryDate(),
                EventDatePicker.SelectedDate,
                QuoteTitleTextBox.Text,
                QuoteStatusComboBox.SelectedItem?.ToString() ?? "Pendiente",
                GetDepositValue(),
                ClientNotesTextBox.Text,
                Products,
                CurrentQuote?.Id,
                CurrentQuote?.CreatedAt);

            var safeClientName = MakeSafeFileName(exportQuote.ClientName);
            var exportDateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
            var fileName = $"{safeClientName}_{exportDateTime}.pdf";

            if (!TryBuildExportFilePath(CurrentSettings.QuoteExportFolder, fileName, "presupuestos", out var pdfPath))
                return;

            try
            {
                PdfExportService.ExportQuoteToPdf(exportQuote, pdfPath);

                LastGeneratedPdfPath = pdfPath;
                UpdatePdfWorkflowStatus(pdfPath);

                MessageBox.Show(
                    "PDF generado correctamente. Puedes abrirlo desde la pestaña Documentos.",
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


        private void UpdatePdfWorkflowStatus(string filePath)
        {
            if (DocumentsLastPdfStatusBorder == null || DocumentsLastPdfStatusTextBlock == null)
                return;

            var fileName = Path.GetFileName(filePath);
            DocumentsLastPdfStatusTextBlock.Text = $"{fileName} · generado ahora. Puedes abrirlo directamente desde aquí.";
            DocumentsLastPdfStatusBorder.Visibility = Visibility.Visible;
            OpenLastPdfButton.IsEnabled = File.Exists(filePath);
        }

        private void OpenLastPdfButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LastGeneratedPdfPath) || !File.Exists(LastGeneratedPdfPath))
            {
                MessageBox.Show(
                    "No se encuentra el último PDF generado en esta sesión.",
                    "Abrir PDF",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = LastGeneratedPdfPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudo abrir el PDF.\n\n{ex.Message}",
                    "Abrir PDF",
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
                    "El cliente seleccionado no tiene trabajos para exportar.",
                    "Exportar opciones",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            var exportQuotes = client.Quotes
                .OrderBy(q => q.EventDate ?? DateTime.MaxValue)
                .ThenBy(q => q.Id)
                .Select(q =>
                {
                    var productLines = new ObservableCollection<ProductLine>(
                        q.Items.Select(item =>
                        {
                            return ProductLine.FromQuoteItem(item);
                        }));

                    return ExportDataService.CreateExportQuote(
                        client.Name,
                        client.Phone,
                        q.DeliveryDate,
                        q.EventDate,
                        q.Title,
                        string.IsNullOrWhiteSpace(q.Status) ? "Pendiente" : q.Status,
                        q.Deposit,
                        q.ClientNotes,
                        productLines,
                        q.Id,
                        q.CreatedAt);
                })
                .ToList();

            var fileName = $"{MakeSafeFileName(client.Name)}_opciones_{DateTime.Now:yyyy-MM-dd_HH-mm}.pdf";

            if (!TryBuildExportFilePath(CurrentSettings.QuoteExportFolder, fileName, "presupuestos", out var pdfPath))
                return;

            try
            {
                PdfExportService.ExportQuotesToPdf(exportQuotes, pdfPath);

                LastGeneratedPdfPath = pdfPath;
                UpdatePdfWorkflowStatus(pdfPath);

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

        public void ExportClientSheetButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentQuote == null)
            {
                MessageBox.Show(
                    "Primero selecciona un trabajo guardado.",
                    "Exportar ficha cliente",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var status = QuoteStatusComboBox.SelectedItem?.ToString() ?? "";

            if (!string.Equals(status, "Aceptado", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "La ficha interna solo se puede generar cuando el trabajo está aceptado.",
                    "Exportar ficha cliente",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var fileName = $"{SanitizeFileName(ClientNameTextBox.Text)}_ficha_{DateTime.Now:yyyy-MM-dd_HH-mm}.pdf";

            if (!TryBuildExportFilePath(CurrentSettings.ClientSheetExportFolder, fileName, "fichas de cliente", out var pdfPath))
                return;

            var sheet = new ExportClientSheet
            {
                QuoteId = CurrentQuote.Id,
                CreatedAt = CurrentQuote.CreatedAt,
                ClientName = ClientNameTextBox.Text.Trim(),
                ClientDni = DniTextBox.Text.Trim(),
                ClientPhone = PhoneTextBox.Text.Trim(),
                OrderTitle = BuildClientSheetOrderSummary(),
                EventDate = EventDatePicker.SelectedDate ?? DeliveryDatePicker.SelectedDate ?? DateTime.Now,
                Deposit = GetDepositValue(),
                Observations = QuoteNotesTextBox.Text.Trim()
            };

            ClientSheetPdfService.ExportClientSheetToPdf(sheet, pdfPath);

            LastGeneratedPdfPath = pdfPath;
            UpdatePdfWorkflowStatus(pdfPath);

            MessageBox.Show(
                "Ficha interna generada correctamente.",
                "Exportar ficha cliente",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private string BuildClientSheetOrderSummary()
        {
            var parts = new List<string>();

            if (CurrentQuote != null)
                parts.Add(QuoteNumberService.FormatQuoteNumber(CurrentQuote.Id, CurrentQuote.CreatedAt));

            var firstProduct = Products.FirstOrDefault();

            if (firstProduct != null)
            {
                var productSummary = firstProduct.ProductName?.Trim() ?? "";

                if (!string.IsNullOrWhiteSpace(firstProduct.TailoringType))
                    productSummary += $" {NormalizeDisplayText(firstProduct.TailoringType)}";

                if (!string.IsNullOrWhiteSpace(productSummary))
                    parts.Add(productSummary.Trim());
            }

            var title = QuoteTitleTextBox.Text.Trim();

            if (!string.IsNullOrWhiteSpace(title) &&
                !parts.Any(p => string.Equals(p, title, StringComparison.OrdinalIgnoreCase)))
            {
                parts.Add(title);
            }

            return string.Join(" · ", parts);
        }

        private string NormalizeDisplayText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            return value.Trim()
                .Replace("Confeccion", "Confección");
        }

        internal void StatusFilterButton_Click(object sender, RoutedEventArgs e)
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
            UpdateClientDetailPanel(null, null);

            ClientNameTextBox.Text = "";
            PhoneTextBox.Text = "";
            DniTextBox.Text = "";

            Products.Clear();

            CurrentQuote = null;
            CurrentClientId = null;
            LastSelectedClientId = null;
            LastSelectedQuoteId = null;
            SyncPredictiveClientSelection(null);

            DeliveryDatePicker.SelectedDate = null;
            EventDatePicker.SelectedDate = null;
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
            UpdateActiveContext();
            UpdateWorkflowState();
            UpdateSecondaryPlaceholders();
            UpdateClientDetailPanel(null, null);
        }

        private void NewClientButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ConfirmDiscardChanges())
                return;

            ClearScreenForNewClient();

            GoToTab(TabPresupuesto);

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
                "Se eliminarán también todos sus trabajos y esta acción no se puede deshacer.",
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
            LoadCalendarDeliveries();

            ClearScreenForNewClient();

            MessageBox.Show(
                "Cliente eliminado correctamente.",
                "Eliminar cliente",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void SaveClientButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ShowClientEditDialog())
                return;

            SaveClientFromContextFields();
        }

        private bool PrepareClientForNewQuote()
        {
            var result = MessageBox.Show(
                "¿Quieres crear un cliente nuevo para este presupuesto?\n\n" +
                "Sí: crear cliente nuevo.\n" +
                "No: elegir un cliente existente.",
                "Crear presupuesto",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Cancel)
                return false;

            if (result == MessageBoxResult.Yes)
            {
                ClearScreenForNewClient();

                if (!ShowClientEditDialog())
                    return false;

                SaveClientFromContextFields();
                return CurrentClientId != null;
            }

            var selectedClientId = ShowClientSelectionDialog();
            if (selectedClientId == null)
                return false;

            SelectClientForNewQuote(selectedClientId.Value);
            return CurrentClientId != null;
        }

        private int? ShowClientSelectionDialog()
        {
            using var db = new AppDbContext();

            var clients = db.Clients
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ThenBy(c => c.Phone)
                .ToList();

            if (clients.Count == 0)
            {
                MessageBox.Show(
                    "Todavía no hay clientes guardados. Crea un cliente nuevo para continuar.",
                    "Elegir cliente",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return null;
            }

            var dialog = CreateContextEditDialog(
                "Elegir cliente",
                "Selecciona el cliente al que quieres asociar el nuevo presupuesto.");

            var comboBox = new ComboBox
            {
                Height = 38,
                FontSize = 14,
                ItemsSource = clients,
                DisplayMemberPath = "Name",
                SelectedValuePath = "Id",
                SelectedValue = CurrentClientId,
                BorderBrush = (Brush)FindResource("BorderSoftBrush"),
                BorderThickness = new Thickness(1),
                Background = Brushes.White
            };

            if (comboBox.SelectedItem == null)
                comboBox.SelectedIndex = 0;

            var detailsTextBlock = new TextBlock
            {
                FontSize = 12,
                Foreground = (Brush)FindResource("MutedTextBrush"),
                Margin = new Thickness(0, 6, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };

            void UpdateDetails()
            {
                if (comboBox.SelectedItem is Models.Client client)
                {
                    var phone = string.IsNullOrWhiteSpace(client.Phone) ? "Sin teléfono" : client.DisplayPhone;
                    var dni = string.IsNullOrWhiteSpace(client.Dni) ? "Sin DNI" : client.Dni;
                    detailsTextBlock.Text = $"{phone} · {dni}";
                }
                else
                {
                    detailsTextBlock.Text = "Selecciona un cliente.";
                }
            }

            comboBox.SelectionChanged += (_, _) => UpdateDetails();
            UpdateDetails();

            AddDialogField(dialog.ContentPanel, "Cliente existente", comboBox);
            dialog.ContentPanel.Children.Add(detailsTextBlock);

            int? selectedClientId = null;
            dialog.AcceptButton.Click += (_, _) =>
            {
                if (comboBox.SelectedItem is not Models.Client client)
                {
                    MessageBox.Show(
                        "Selecciona un cliente para continuar.",
                        "Elegir cliente",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                selectedClientId = client.Id;
                dialog.Window.DialogResult = true;
            };

            comboBox.Focus();

            return dialog.Window.ShowDialog() == true ? selectedClientId : null;
        }

        private void SelectClientForNewQuote(int clientId)
        {
            SuppressSelectionConfirm = true;

            LoadClients();

            var reloadedClient = ((IEnumerable<Models.Client>)ClientsListBox.ItemsSource)
                .FirstOrDefault(c => c.Id == clientId);

            if (reloadedClient != null)
            {
                ClientsListBox.SelectedItem = reloadedClient;
                LastSelectedClientId = clientId;
                CurrentClientId = clientId;
                SyncPredictiveClientSelection(clientId);
            }

            SuppressSelectionConfirm = false;

            if (reloadedClient != null)
            {
                // Fuerza la carga visual de la ficha/lista asociada al cliente seleccionado.
                ClientsListBox_SelectionChanged(ClientsListBox, new SelectionChangedEventArgs(System.Windows.Controls.Primitives.Selector.SelectionChangedEvent, new List<object>(), new List<object> { reloadedClient }));
            }

            UpdateActiveContext();
            UpdateWorkflowState();
            UpdateShellNavigationState();
        }

        private bool ShowClientEditDialog()
        {
            var dialog = CreateContextEditDialog(
                "Editar cliente",
                "Actualiza los datos básicos del cliente sin salir del trabajo actual.");

            var nameTextBox = CreateDialogTextBox(ClientNameTextBox.Text);
            var phoneTextBox = CreateDialogTextBox(PhoneTextBox.Text);
            var dniTextBox = CreateDialogTextBox(DniTextBox.Text);

            AddDialogField(dialog.ContentPanel, "Nombre", nameTextBox);
            AddDialogField(dialog.ContentPanel, "Teléfono", phoneTextBox);
            AddDialogField(dialog.ContentPanel, "DNI", dniTextBox);

            dialog.AcceptButton.Click += (_, _) =>
            {
                ClientNameTextBox.Text = nameTextBox.Text.Trim();
                PhoneTextBox.Text = phoneTextBox.Text.Trim();
                DniTextBox.Text = dniTextBox.Text.Trim();
                dialog.Window.DialogResult = true;
            };

            nameTextBox.Focus();
            nameTextBox.SelectAll();

            return dialog.Window.ShowDialog() == true;
        }

        private void SaveClientFromContextFields()
        {
            var clientName = ClientNameTextBox.Text.Trim();
            var clientPhone = PhoneTextBox.Text.Trim();
            var clientDni = DniTextBox.Text.Trim();

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

            var currentClientId = CurrentClientId ?? selectedClient?.Id;

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

            if (CurrentClientId != null)
            {
                client = db.Clients.FirstOrDefault(c => c.Id == CurrentClientId.Value);
            }

            if (selectedClient != null)
            {
                client = db.Clients.FirstOrDefault(c => c.Id == selectedClient.Id);
            }

            if (client == null)
            {
                client = new Models.Client
                {
                    Name = clientName,
                    Phone = clientPhone,
                    Dni = clientDni
                };

                db.Clients.Add(client);
            }
            else
            {
                client.Name = clientName;
                client.Phone = clientPhone;
                client.Dni = clientDni;
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
                CurrentClientId = savedClientId;
                SyncPredictiveClientSelection(savedClientId);
                UpdateWorkflowState();
            }

            SuppressSelectionConfirm = false;

            MarkAsSaved();

            MessageBox.Show(
                "Cliente guardado correctamente.",
                "Guardar cliente",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private (Window Window, StackPanel ContentPanel, Button AcceptButton) CreateContextEditDialog(string title, string subtitle)
        {
            var window = new Window
            {
                Title = title,
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                SizeToContent = SizeToContent.WidthAndHeight,
                Background = (Brush)FindResource("AppBackgroundBrush")
            };

            var root = new Border
            {
                Width = 430,
                Background = (Brush)FindResource("CardBackgroundBrush"),
                BorderBrush = (Brush)FindResource("BorderSoftBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18),
                Padding = new Thickness(20)
            };

            var layout = new Grid();
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var header = new StackPanel { Margin = new Thickness(0, 0, 0, 18) };
            header.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("PrimaryBrush")
            });
            header.Children.Add(new TextBlock
            {
                Text = subtitle,
                FontSize = 13,
                Foreground = (Brush)FindResource("MutedTextBrush"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 4, 0, 0)
            });
            Grid.SetRow(header, 0);
            layout.Children.Add(header);

            var contentPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 18) };
            Grid.SetRow(contentPanel, 1);
            layout.Children.Add(contentPanel);

            var actions = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right
            };

            var cancelButton = new Button
            {
                Content = "Cancelar",
                Width = 96,
                Height = 36,
                Margin = new Thickness(0, 0, 8, 0),
                Style = (Style)FindResource("SecondaryButtonStyle")
            };
            cancelButton.Click += (_, _) => window.DialogResult = false;

            var acceptButton = new Button
            {
                Content = "Guardar",
                Width = 102,
                Height = 36,
                Style = (Style)FindResource("PrimaryActionButtonStyle")
            };

            actions.Children.Add(cancelButton);
            actions.Children.Add(acceptButton);
            Grid.SetRow(actions, 2);
            layout.Children.Add(actions);

            root.Child = layout;
            window.Content = root;

            return (window, contentPanel, acceptButton);
        }

        private TextBox CreateDialogTextBox(string value)
        {
            return new TextBox
            {
                Text = value,
                Height = 36,
                FontSize = 14,
                Padding = new Thickness(10, 0, 10, 0),
                VerticalContentAlignment = System.Windows.VerticalAlignment.Center,
                BorderBrush = (Brush)FindResource("BorderSoftBrush"),
                BorderThickness = new Thickness(1),
                Background = Brushes.White
            };
        }

        private DatePicker CreateDialogDatePicker(DateTime? selectedDate)
        {
            return new DatePicker
            {
                SelectedDate = selectedDate,
                Height = 36,
                FontSize = 14,
                BorderBrush = (Brush)FindResource("BorderSoftBrush"),
                BorderThickness = new Thickness(1),
                Background = Brushes.White
            };
        }

        private ComboBox CreateDialogComboBox(string selectedValue)
        {
            var comboBox = new ComboBox
            {
                Height = 36,
                FontSize = 14,
                ItemsSource = new[] { "Pendiente", "Aceptado", "Rechazado", "Entregado" },
                SelectedItem = string.IsNullOrWhiteSpace(selectedValue) ? "Pendiente" : selectedValue,
                BorderBrush = (Brush)FindResource("BorderSoftBrush"),
                BorderThickness = new Thickness(1),
                Background = Brushes.White
            };

            return comboBox;
        }

        private void AddDialogField(Panel parent, string label, Control control)
        {
            var field = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
            field.Children.Add(new TextBlock
            {
                Text = label,
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("MutedTextBrush"),
                Margin = new Thickness(0, 0, 0, 4)
            });
            field.Children.Add(control);
            parent.Children.Add(field);
        }

        internal void DeliveryFilterButton_Click(object sender, RoutedEventArgs e)
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

        internal void ProductsDataGrid_PreparingCellForEdit(
            object? sender,
            System.Windows.Controls.DataGridPreparingCellForEditEventArgs e)
        {
            if (e.EditingElement is not System.Windows.Controls.TextBox textBox)
                return;

            var header = e.Column.Header?.ToString() ?? "";

            textBox.Tag = header;

            if (header == "Precio" ||
                header == "Precio Tejido" ||
                header == "Tejido")
            {
                textBox.Text = textBox.Text
                    .Replace("€", "")
                    .Trim();

                if (e.EditingEventArgs is not TextCompositionEventArgs)
                {
                    textBox.SelectAll();
                }
                else
                {
                    textBox.CaretIndex = textBox.Text.Length;
                }
            }

            if (header == "Cant." ||
                header == "Precio" ||
                header == "Precio Tejido" ||
                header == "Tejido")
            {
                textBox.PreviewTextInput -= NumericTextBox_PreviewTextInput;
                textBox.PreviewTextInput += NumericTextBox_PreviewTextInput;

                DataObject.RemovePastingHandler(textBox, NumericTextBox_Pasting);
                DataObject.AddPastingHandler(textBox, NumericTextBox_Pasting);
            }
        }

        private bool ShouldAllowNegative(System.Windows.Controls.TextBox textBox)
        {
            return false;
        }

        private bool IsValidNumericInput(string text, bool allowNegative)
        {
            if (string.IsNullOrWhiteSpace(text))
                return true;

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

        internal void MoveProductUp_Click(object sender, RoutedEventArgs e)
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

        internal void MoveProductDown_Click(object sender, RoutedEventArgs e)
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

        private void LoadCalendarDeliveries()
        {
            WeeklyDeliveries.Clear();
            CalendarDayGroups.Clear();

            using var db = new AppDbContext();

            DateTime startDate;
            DateTime endDate;

            if (CalendarViewMode == "Mes")
            {
                startDate = new DateTime(CalendarReferenceDate.Year, CalendarReferenceDate.Month, 1);
                endDate = startDate.AddMonths(1).AddDays(-1);

                WeekTitleTextBlock.Text = $"Mes de {CalendarReferenceDate:MMMM yyyy}";
            }
            else if (CalendarViewMode == "Semana")
            {
                int diff = (7 + (CalendarReferenceDate.DayOfWeek - DayOfWeek.Monday)) % 7;
                startDate = CalendarReferenceDate.AddDays(-diff).Date;
                endDate = startDate.AddDays(6).Date;

                WeekTitleTextBlock.Text = $"Semana del {startDate:dd/MM/yyyy} al {endDate:dd/MM/yyyy}";
            }
            else
            {
                startDate = DateTime.Today;
                endDate = DateTime.Today.AddMonths(6);

                WeekTitleTextBlock.Text = "Trabajos pendientes ordenados por fecha de evento";
            }

            var deliveriesQuery = db.Quotes
                .Include(q => q.Client)
                .AsQueryable();

            if (CalendarViewMode == "Lista")
            {
                deliveriesQuery = deliveriesQuery
                    .Where(q => q.Status != "Entregado");
            }
            else
            {
                deliveriesQuery = deliveriesQuery
                    .Where(q => q.EventDate.HasValue &&
                        q.EventDate.Value.Date >= startDate &&
                        q.EventDate.Value.Date <= endDate);
            }

            var deliveries = deliveriesQuery
                .OrderBy(q => q.EventDate.HasValue ? 0 : 1)
                .ThenBy(q => q.EventDate ?? DateTime.MaxValue)
                .ThenBy(q => q.Client != null ? q.Client.Name : "")
                .ToList();

            foreach (var quote in deliveries)
            {
                WeeklyDeliveries.Add(new WeeklyDeliveryItem
                {
                    QuoteId = quote.Id,
                    ClientId = quote.ClientId,
                    CreatedAt = quote.CreatedAt,
                    DeliveryDate = quote.DeliveryDate,
                    EventDate = quote.EventDate,
                    ClientName = quote.Client?.Name ?? "",
                    ClientPhone = quote.Client?.Phone ?? "",
                    QuoteTitle = quote.Title,
                    Status = string.IsNullOrWhiteSpace(quote.Status) ? "Pendiente" : quote.Status,
                    Deposit = quote.Deposit,
                    Total = quote.Total
                });
            }

            if (CalendarViewMode != "Lista")
            {
                var currentDate = startDate;

                while (currentDate <= endDate)
                {
                    var group = new CalendarDayGroup
                    {
                        Date = currentDate
                    };

                    var dayDeliveries = WeeklyDeliveries
                        .Where(d => d.EventDate.HasValue && d.EventDate.Value.Date == currentDate.Date)
                        .OrderBy(d => d.ClientName)
                        .ToList();

                    foreach (var delivery in dayDeliveries)
                    {
                        group.Deliveries.Add(delivery);
                    }

                    var isWeekend =
                        currentDate.DayOfWeek == DayOfWeek.Saturday ||
                        currentDate.DayOfWeek == DayOfWeek.Sunday;

                    var shouldShowDay =
                        group.HasDeliveries ||
                        (CalendarViewMode == "Semana" && !isWeekend);

                    if (shouldShowDay)
                    {
                        CalendarDayGroups.Add(group);
                    }

                    currentDate = currentDate.AddDays(1);
                }
            }

            EmptyWeekTextBlock.Text = CalendarViewMode switch
            {
                "Mes" => "No hay eventos programados para este mes.",
                "Semana" => "No hay eventos programados para esta semana.",
                _ => "No hay trabajos pendientes."
            };

            EmptyWeekTextBlock.Visibility = WeeklyDeliveries.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;

            UpdateDashboardSummary();
        }

        private void LoadUpcomingDeliveries()
        {
            UpcomingDeliveries.Clear();

            using var db = new AppDbContext();

            var today = DateTime.Today;
            var limit = today.AddDays(21);

            var deliveries = db.Quotes
                .AsNoTracking()
                .Include(q => q.Client)
                .Where(q => q.EventDate.HasValue
                    && q.EventDate.Value.Date >= today
                    && q.EventDate.Value.Date <= limit
                    && q.Status != "Entregado")
                .OrderBy(q => q.EventDate ?? DateTime.MaxValue)
                .ThenBy(q => q.Client != null ? q.Client.Name : "")
                .Take(8)
                .ToList();

            foreach (var quote in deliveries)
            {
                UpcomingDeliveries.Add(new WeeklyDeliveryItem
                {
                    QuoteId = quote.Id,
                    ClientId = quote.ClientId,
                    CreatedAt = quote.CreatedAt,
                    DeliveryDate = quote.DeliveryDate,
                    EventDate = quote.EventDate,
                    ClientName = quote.Client?.Name ?? "",
                    ClientPhone = quote.Client?.Phone ?? "",
                    QuoteTitle = quote.Title,
                    Status = string.IsNullOrWhiteSpace(quote.Status) ? "Pendiente" : quote.Status,
                    Deposit = quote.Deposit,
                    Total = quote.Total
                });
            }

        }

        private void UpdateDashboardSummary()
        {
            using var db = new AppDbContext();

            var today = DateTime.Today;
            var next7Limit = today.AddDays(7);
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var quotes = db.Quotes
                .AsNoTracking()
                .ToList();

            var todayCount = quotes.Count(q => q.EventDate.HasValue && q.EventDate.Value.Date == today);
            var next7Count = quotes.Count(q =>
                q.EventDate.HasValue &&
                q.EventDate.Value.Date >= today &&
                q.EventDate.Value.Date <= next7Limit);
            var pendingCount = quotes.Count(q =>
                !string.Equals(q.Status, "Entregado", StringComparison.OrdinalIgnoreCase));
            var monthCount = quotes.Count(q =>
                q.EventDate.HasValue &&
                q.EventDate.Value.Date >= monthStart &&
                q.EventDate.Value.Date <= monthEnd);

            DashboardTodayCountTextBlock.Text = todayCount.ToString("N0");
            DashboardNext7CountTextBlock.Text = next7Count.ToString("N0");
            DashboardPendingCountTextBlock.Text = pendingCount.ToString("N0");
            DashboardMonthCountTextBlock.Text = monthCount.ToString("N0");
        }

        private bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday ||
                   date.DayOfWeek == DayOfWeek.Sunday;
        }

        private bool HasDeliveriesOnDate(IEnumerable<WeeklyDeliveryItem> deliveries, DateTime date)
        {
            return deliveries.Any(d => d.EventDate.HasValue && d.EventDate.Value.Date == date.Date);
        }

        private void UpdateActiveContext()
        {
            var clientName = string.IsNullOrWhiteSpace(ClientNameTextBox.Text)
                ? "—"
                : ClientNameTextBox.Text.Trim();

            var phone = FormatPhoneForDisplay(PhoneTextBox.Text);

            var quoteTitle = string.IsNullOrWhiteSpace(QuoteTitleTextBox.Text)
                ? "—"
                : QuoteTitleTextBox.Text.Trim();

            var quoteNumber = CurrentQuote == null
                ? "—"
                : QuoteNumberService.FormatQuoteNumber(CurrentQuote.Id, CurrentQuote.CreatedAt);
            var quoteDisplayTitle = CurrentQuote == null
                ? quoteTitle
                : string.IsNullOrWhiteSpace(QuoteTitleTextBox.Text)
                    ? quoteNumber
                    : quoteTitle;

            var status = QuoteStatusComboBox.SelectedItem?.ToString() ?? "Pendiente";
            var eventText = EventDatePicker.SelectedDate.HasValue
                ? EventDatePicker.SelectedDate.Value.ToString("dd/MM/yyyy")
                : "—";
            var deliveryText = DeliveryDatePicker.SelectedDate.HasValue
                ? DeliveryDatePicker.SelectedDate.Value.ToString("dd/MM/yyyy")
                : "—";
            var deposit = GetDepositValue();
            decimal total = Products.Sum(p => p.Total);

            if (QuoteCreatedAtTextBox != null)
            {
                QuoteCreatedAtTextBox.Text = CurrentQuote == null
                    ? "—"
                    : CurrentQuote.CreatedAt.ToString("dd/MM/yyyy HH:mm");
            }

            ActiveWorkspaceTitle.Text = quoteDisplayTitle == "—" ? "Nuevo trabajo" : quoteDisplayTitle;
            ActiveWorkspaceSubtitle.Text = clientName == "—" ? "Selecciona un cliente o crea un trabajo" : clientName;
            ActiveClientTextBlock.Text = $"Cliente: {clientName}";
            PhoneDisplayTextBlock.Text = phone;
            UpdateQuoteStatusBadge(status);
            EventDateDisplayTextBlock.Text = eventText;

            ActivePhoneTextBlock.Text = $"Teléfono: {phone}";
            ActiveQuoteTextBlock.Text = $"Estado: {status}";
            ActiveDeliveryTextBlock.Text = $"Evento: {eventText}";
            ActiveDepositTextBlock.Text = $"Señal: {deposit:N2} €";
            ActiveTotalTextBlock.Text = $"Total: {total:N2} €";

            if (BudgetWorkspaceTitleTextBlock != null)
                BudgetWorkspaceTitleTextBlock.Text = quoteDisplayTitle == "—" ? "Nuevo trabajo" : quoteDisplayTitle;

            if (BudgetWorkspaceClientTextBlock != null)
                BudgetWorkspaceClientTextBlock.Text = $"Cliente: {clientName}";

            if (BudgetWorkspaceStatusTextBlock != null)
                BudgetWorkspaceStatusTextBlock.Text = $"Estado: {status}";

            if (BudgetWorkspaceDeliveryTextBlock != null)
                BudgetWorkspaceDeliveryTextBlock.Text = $"Evento: {eventText}";

            if (BudgetWorkspaceTotalTextBlock != null)
                BudgetWorkspaceTotalTextBlock.Text = $"{total:N2} €";


            UpdateContextPanel(clientName, quoteTitle, total);

            UpdateEmptyStateMessages();
            UpdateWorkflowState();
        }



        private string FormatPhoneForDisplay(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return "—";

            var digits = new string(phone.Where(char.IsDigit).ToArray());

            if (digits.Length == 9)
                return $"{digits[..3]} {digits.Substring(3, 2)} {digits.Substring(5, 2)} {digits.Substring(7, 2)}";

            return phone.Trim();
        }

        private void UpdateQuoteStatusBadge(string? status)
        {
            var normalizedStatus = string.IsNullOrWhiteSpace(status) ? "Pendiente" : status.Trim();

            QuoteStatusBadgeTextBlock.Text = normalizedStatus.ToUpperInvariant();

            switch (normalizedStatus)
            {
                case "Aceptado":
                    QuoteStatusBadgeBorder.Background = new SolidColorBrush(MediaColor.FromRgb(93, 135, 88));
                    QuoteStatusBadgeTextBlock.Foreground = Brushes.White;
                    break;
                case "Entregado":
                    QuoteStatusBadgeBorder.Background = new SolidColorBrush(MediaColor.FromRgb(120, 130, 112));
                    QuoteStatusBadgeTextBlock.Foreground = Brushes.White;
                    break;
                case "Rechazado":
                    QuoteStatusBadgeBorder.Background = new SolidColorBrush(MediaColor.FromRgb(170, 100, 90));
                    QuoteStatusBadgeTextBlock.Foreground = Brushes.White;
                    break;
                default:
                    QuoteStatusBadgeBorder.Background = new SolidColorBrush(MediaColor.FromRgb(238, 242, 234));
                    QuoteStatusBadgeTextBlock.Foreground = new SolidColorBrush(MediaColor.FromRgb(47, 51, 45));
                    break;
            }
        }

        private void UpdateContextPanel(string clientName, string quoteTitle, decimal total)
        {
            // 12B.3: el panel contextual derecho se ha eliminado.
            // El contexto visible vive ahora en el panel persistente "Trabajo activo" de la sidebar.
        }

        private void ContextSaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveButton_Click(sender, e);
        }

        private void ContextMarkDeliveredButton_Click(object sender, RoutedEventArgs e)
        {
            MarkCurrentQuoteAsDeliveredFromQuickAction();
        }

        private void ContextExportPdfButton_Click(object sender, RoutedEventArgs e)
        {
            ExportPdfButton_Click(sender, e);
        }

        private void ContextDocumentsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TryFlushPendingChangesBeforeWorkflowAction("abrir documentos"))
                return;

            OpenBudgetDocumentsTab();
        }

        private void ShellRetrySaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CanAutoSaveCurrentWorkspace())
            {
                SaveButton_Click(this, new RoutedEventArgs());
                return;
            }

            TryAutoSaveCurrentWorkspace();
        }

        private void ShellQuickProductsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TryFlushPendingChangesBeforeWorkflowAction("abrir productos"))
                return;

            OpenProductsInlinePanel();
        }

        private void ShellQuickExportPdfButton_Click(object sender, RoutedEventArgs e)
        {
            ExportPdfButton_Click(sender, e);
        }

        private void ShellQuickDeliveredButton_Click(object sender, RoutedEventArgs e)
        {
            MarkCurrentQuoteAsDeliveredFromQuickAction();
        }


        private void ClientSheetButton_Click(object sender, RoutedEventArgs e)
        {
            ExportClientSheetButton_Click(sender, e);
        }

        private void EditJobButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = CreateContextEditDialog(
                "Editar trabajo",
                "Modifica los datos principales del trabajo sin perder el contexto actual.");

            var titleTextBox = CreateDialogTextBox(QuoteTitleTextBox.Text);
            var statusComboBox = CreateDialogComboBox(QuoteStatusComboBox.SelectedItem?.ToString() ?? "Pendiente");
            var deliveryDatePicker = CreateDialogDatePicker(DeliveryDatePicker.SelectedDate);
            var eventDatePicker = CreateDialogDatePicker(EventDatePicker.SelectedDate);

            AddDialogField(dialog.ContentPanel, "Título", titleTextBox);
            AddDialogField(dialog.ContentPanel, "Estado", statusComboBox);
            AddDialogField(dialog.ContentPanel, "Fecha del evento", eventDatePicker);
            AddDialogField(dialog.ContentPanel, "Entrega", deliveryDatePicker);

            dialog.AcceptButton.Click += (_, _) =>
            {
                QuoteTitleTextBox.Text = titleTextBox.Text.Trim();
                QuoteStatusComboBox.SelectedItem = statusComboBox.SelectedItem?.ToString() ?? "Pendiente";
                DeliveryDatePicker.SelectedDate = deliveryDatePicker.SelectedDate;
                EventDatePicker.SelectedDate = eventDatePicker.SelectedDate;
                dialog.Window.DialogResult = true;
            };

            titleTextBox.Focus();
            titleTextBox.SelectAll();

            if (dialog.Window.ShowDialog() == true)
            {
                MarkAsChanged();
                UpdateActiveContext();
                UpdateSecondaryPlaceholders();
                UpdateWorkflowState();

                if (NavigateToProductsAfterJobEdit)
                {
                    NavigateToProductsAfterJobEdit = false;
                    OpenProductsInlinePanel();
                }
            }
            else
            {
                NavigateToProductsAfterJobEdit = false;
            }
        }

        private void ConfigureCulture()
        {
            var culture = new CultureInfo("es-ES");

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        private void InitializeDataSources()
        {
            WeeklyColumnsItemsControl.ItemsSource = CalendarDayGroups;
            CalendarGroupsListBox.ItemsSource = CalendarDayGroups;
            DeliveriesListBox.ItemsSource = WeeklyDeliveries;
            GlobalSearchResultsListBox.ItemsSource = GlobalSearchResults;
            ProductsDataGrid.ItemsSource = Products;
            ClientPredictiveComboBox.ItemsSource = PredictiveClientResults;
            InvoicesListBox.ItemsSource = Invoices;
        }

        private void LoadInitialData()
        {
            PriceService.LoadPrices();
            RefreshAvailableProductNames();

            LoadClients();
            LoadCalendarDeliveries();
            LoadInvoices();
        }

        private void RefreshAvailableProductNames()
        {
            AvailableProductNames.Clear();

            foreach (var productName in PriceService.GetProductNames())
                AvailableProductNames.Add(productName);
        }

        private void WireEvents()
        {
            Products.CollectionChanged += Products_CollectionChanged;

            AddProductButton.Click += AddProductButton_Click;
            SaveButton.Click += SaveButton_Click;
            SaveProductsButton.Click += SaveButton_Click;
            BackToJobDashboardButton.Click += BackToJobDashboardButton_Click;
            NewQuoteButton.Click += NewQuoteButton_Click;
            NewQuoteFromClientButton.Click += NewQuoteFromClientButton_Click;
            OpenSelectedClientQuoteButton.Click += OpenSelectedClientQuoteButton_Click;
            NewClientButton.Click += NewClientButton_Click;
            SaveClientButton.Click += SaveClientButton_Click;
            ClientSheetButton.Click += ClientSheetButton_Click;
            EditJobButton.Click += EditJobButton_Click;
            DeleteClientButton.Click += DeleteClientButton_Click;
            DuplicateQuoteButton.Click += DuplicateQuoteButton_Click;
            PricesConfigButton.Click += PricesConfigButton_Click;
            BackupButton.Click += BackupButton_Click;
            ClientSheetFolderButton.Click += ClientSheetFolderButton_Click;
            QuoteFolderButton.Click += QuoteFolderButton_Click;
            InvoiceFolderButton.Click += InvoiceFolderButton_Click;
            InvoiceCreateButton.Click += InvoiceCreateButton_Click;
            InvoiceExportButton.Click += InvoiceExportButton_Click;
            InvoicePrintButton.Click += InvoicePrintButton_Click;
            CreateCorrectiveInvoiceButton.Click += CreateCorrectiveInvoiceButton_Click;
            ConvertQuoteToInvoiceButton.Click += ConvertQuoteToInvoiceButton_Click;
            ViewQuoteInvoiceButton.Click += ViewQuoteInvoiceButton_Click;
            ExportPdfButton.Click += ExportPdfButton_Click;
            OpenLastPdfButton.Click += OpenLastPdfButton_Click;
            ExportAllQuotesPdfButton.Click += ExportAllQuotesPdfButton_Click;
            OpenQuoteFolderButton.Click += OpenQuoteFolderButton_Click;
            OpenClientSheetFolderButton.Click += OpenClientSheetFolderButton_Click;
            PreviousCalendarButton.Click += PreviousCalendarButton_Click;
            TodayCalendarButton.Click += TodayCalendarButton_Click;
            NextCalendarButton.Click += NextCalendarButton_Click;
            ListViewButton.Click += ListViewButton_Click;
            WeekViewButton.Click += WeekViewButton_Click;
            MonthViewButton.Click += MonthViewButton_Click;
            DeliveriesListBox.PreviewMouseLeftButtonUp += CalendarDelivery_MouseLeftButtonUp;
            WeeklyColumnsItemsControl.PreviewMouseLeftButtonUp += CalendarDelivery_MouseLeftButtonUp;
            CalendarGroupsListBox.PreviewMouseLeftButtonUp += CalendarDelivery_MouseLeftButtonUp;
            ToggleSidebarButton.Click += ToggleSidebarButton_Click;
            BudgetQuickSummaryButton.Click += BudgetQuickSummaryButton_Click;
            BudgetQuickProductsButton.Click += BudgetQuickProductsButton_Click;
            BudgetQuickHistoryButton.Click += BudgetQuickHistoryButton_Click;
            BudgetQuickDocumentsButton.Click += BudgetQuickDocumentsButton_Click;
            BudgetQuickExportPdfButton.Click += BudgetQuickExportPdfButton_Click;
            PresupuestoWorkspaceTabs.SelectionChanged += PresupuestoWorkspaceTabs_SelectionChanged;

            ClientsListBox.SelectionChanged += ClientsListBox_SelectionChanged;
            QuotesListBox.SelectionChanged += QuotesListBox_SelectionChanged;
            WorkSearchTextBox.TextChanged += WorkSearchTextBox_TextChanged;
            InvoiceSearchTextBox.TextChanged += InvoiceSearchTextBox_TextChanged;
            InvoicesListBox.SelectionChanged += InvoicesListBox_SelectionChanged;
            ClientPredictiveComboBox.SelectionChanged += ClientPredictiveComboBox_SelectionChanged;
            ClientPredictiveComboBox.AddHandler(
                TextBoxBase.TextChangedEvent,
                new TextChangedEventHandler(ClientPredictiveComboBox_TextChanged));

            ProductsDataGrid.CellEditEnding += ProductsDataGrid_CellEditEnding;
            ProductsDataGrid.CurrentCellChanged += ProductsDataGrid_CurrentCellChanged;
            AutoSaveTimer.Tick += AutoSaveTimer_Tick;
            SaveStatusRefreshTimer.Tick += SaveStatusRefreshTimer_Tick;

            ClientNameTextBox.TextChanged += ClientNameTextBox_TextChanged;
            QuoteTitleTextBox.TextChanged += QuoteTitleTextBox_TextChanged;
            PhoneTextBox.TextChanged += AnyEditableField_Changed;
            DniTextBox.TextChanged += (_, __) => MarkAsChanged();
            QuoteNotesTextBox.TextChanged += AnyEditableField_Changed;
            ClientNotesTextBox.TextChanged += AnyEditableField_Changed;
            DepositTextBox.TextChanged += DepositTextBox_TextChanged;
            DepositTextBox.PreviewTextInput += DepositTextBox_PreviewTextInput;
            DepositTextBox.PreviewMouseLeftButtonDown += DepositTextBox_PreviewMouseLeftButtonDown;
            DepositTextBox.GotKeyboardFocus += DepositTextBox_GotKeyboardFocus;
            DepositTextBox.LostKeyboardFocus += DepositTextBox_LostKeyboardFocus;

            DataObject.AddPastingHandler(DepositTextBox, DepositTextBox_Pasting);

            DeliveryDatePicker.SelectedDateChanged += AnyEditableField_Changed;
            EventDatePicker.SelectedDateChanged += (_, __) => MarkAsChanged();
            QuoteStatusComboBox.SelectionChanged += AnyEditableField_Changed;
        }

        private void InitializeUiState()
        {
            UpdateSaveButtonText();
            UpdateUnsavedChangesIndicator();
            UpdateShellSaveStateIndicator();
            UpdateWindowTitle();

            ClientSearchPlaceholderTextBlock.Visibility =
                string.IsNullOrWhiteSpace(ClientSearchTextBox.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            UpdateStatusFilterButtons();
            UpdateDeliveryFilterButtons();
            UpdateActiveContext();
            UpdateEmptyStateMessages();
            UpdateWorkflowState();
            UpdateCalendarViewButtons();
            UpdateSecondaryPlaceholders();
        }

        private void Products_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new ProductsTotalChangedMessage(Products.Sum(p => p.Total)));
            WeakReferenceMessenger.Default.Send(new WorkspaceDirtyMessage("Products collection changed"));
        }

        private void ProductLine_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new ProductLineChangedMessage(e.PropertyName ?? string.Empty));
        }

        private void ClientNameTextBox_TextChanged(object? sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            MarkAsChanged();
            UpdateActiveContext();
        }

        private void QuoteTitleTextBox_TextChanged(object? sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            MarkAsChanged();
            UpdateActiveContext();
            UpdateSecondaryPlaceholders();
        }

        private void DepositTextBox_TextChanged(object? sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdatePendingAmount();
            MarkAsChanged();
            UpdateActiveContext();
        }

        private void AnyEditableField_Changed(object? sender, EventArgs e)
        {
            MarkAsChanged();
            UpdateActiveContext();
            UpdateSecondaryPlaceholders();
        }

        private void NavigateToSection(int sectionIndex)
        {
            if (MainTabs == null)
                return;

            if (sectionIndex < 0 || sectionIndex >= MainTabs.Items.Count)
                return;

            if (MainTabs.SelectedIndex != sectionIndex)
                PreviousNavigationTab = MainTabs.SelectedIndex;

            MainTabs.SelectedIndex = sectionIndex;
            UpdateShellNavigationState();
        }

        private void GoToTab(int tabIndex) => NavigateToSection(tabIndex);

        private void BackNavigationButton_Click(object sender, RoutedEventArgs e)
        {
            if (PreviousNavigationTab.HasValue)
            {
                var targetTab = PreviousNavigationTab.Value;
                PreviousNavigationTab = MainTabs?.SelectedIndex;
                ShellBreadcrumbOverride = null;
                NavigateToSection(targetTab);
            }
            else
            {
                ShellBreadcrumbOverride = null;
                NavigateToSection(TabSemana);
            }
        }

        private void SidebarNavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            if (button.Tag == null)
                return;

            if (int.TryParse(button.Tag.ToString(), out var tabIndex))
            {
                ShellBreadcrumbOverride = null;
                GoToTab(tabIndex);
            }
        }

        private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != MainTabs)
                return;

            UpdateShellNavigationState();
        }

        private (string Title, string Subtitle, string Breadcrumb) GetSectionInfo(int sectionIndex)
        {
            var baseInfo = sectionIndex switch
            {
                0 => ("Inicio", "Próximas entregas, vista semanal y vista mensual", "🏠"),
                1 => ("Trabajos", "Clientes y trabajos se gestionan desde el workspace contextual", "🏠 > Trabajos"),
                2 => ("Trabajos", "Resumen del trabajo activo y datos principales", "🏠 > Trabajos"),
                3 => ("Trabajos · Productos", "Líneas, prendas y conceptos del trabajo activo", "🏠 > Trabajos > Productos"),
                4 => ("Facturas", "Emisión, búsqueda y archivo de facturas", "🏠 > Facturas"),
                5 => ("Ajustes", "Tarifas, datos de empresa y configuración", "🏠 > Ajustes"),
                _ => ("Sastrería Martínez Mor", "Gestión de trabajos, PDFs y entregas", "🏠")
            };

            return (baseInfo.Item1, baseInfo.Item2, BuildContextBreadcrumb(sectionIndex, baseInfo.Item3));
        }

        private string BuildContextBreadcrumb(int sectionIndex, string fallback)
        {
            if (!string.IsNullOrWhiteSpace(ShellBreadcrumbOverride))
                return ShellBreadcrumbOverride;

            var clientName = string.IsNullOrWhiteSpace(ClientNameTextBox?.Text)
                ? null
                : ClientNameTextBox.Text.Trim();

            var quoteTitle = string.IsNullOrWhiteSpace(QuoteTitleTextBox?.Text)
                ? null
                : QuoteTitleTextBox.Text.Trim();

            if (sectionIndex == TabClientes && clientName != null)
                return $"🏠 > Trabajos > {clientName}";

            if ((sectionIndex == TabPresupuesto || sectionIndex == TabProductos) && clientName != null)
            {
                var childSection = sectionIndex == TabProductos ? "Productos" : null;

                var basePath = quoteTitle == null
                    ? $"🏠 > Trabajos > {clientName}"
                    : $"🏠 > Trabajos > {clientName} > {quoteTitle}";

                return childSection == null
                    ? basePath
                    : $"{basePath} > {childSection}";
            }

            return fallback;
        }

        private void UpdateShellNavigationState()
        {
            if (MainTabs == null)
                return;

            var sectionInfo = GetSectionInfo(MainTabs.SelectedIndex);

            if (ShellSectionTitleTextBlock != null)
                ShellSectionTitleTextBlock.Text = sectionInfo.Title;

            if (ShellSectionSubtitleTextBlock != null)
                ShellSectionSubtitleTextBlock.Text = sectionInfo.Subtitle;

            if (ShellBreadcrumbTextBlock != null)
                ShellBreadcrumbTextBlock.Text = sectionInfo.Breadcrumb;

            UpdateSidebarButtonState();
        }

        private void UpdateSidebarButtonState()
        {
            if (MainTabs == null)
                return;

            SetSidebarButtonActive(DashboardNavButton, MainTabs.SelectedIndex == TabSemana);
            SetSidebarButtonActive(ClientsNavButton, MainTabs.SelectedIndex == TabClientes);
            SetSidebarButtonActive(QuotesNavButton, MainTabs.SelectedIndex == TabPresupuesto || MainTabs.SelectedIndex == TabProductos);
            SetSidebarButtonActive(InvoicesNavButton, MainTabs.SelectedIndex == TabFacturas);
            SetSidebarButtonActive(SettingsNavButton, MainTabs.SelectedIndex == TabAjustes);
            UpdateWorkspaceButtonState();
        }

        private void SetSidebarButtonActive(Button button, bool isActive)
        {
            if (button == null)
                return;

            button.Background = isActive ? new SolidColorBrush(MediaColor.FromRgb(216, 226, 209)) : Brushes.Transparent;
            button.BorderBrush = Brushes.Transparent;
            button.BorderThickness = new Thickness(0);
        }

        private void UpdateWorkspaceButtonState()
        {
            if (MainTabs == null)
                return;

            SetWorkspaceButtonActive(ActiveSummaryButton, MainTabs.SelectedIndex == TabPresupuesto && PresupuestoWorkspaceTabs.SelectedIndex == InnerTabResumen && ProductsInlineHost.Visibility != Visibility.Visible);
            SetWorkspaceButtonActive(ActiveProductsButton, MainTabs.SelectedIndex == TabPresupuesto && ProductsInlineHost.Visibility == Visibility.Visible);
            SetWorkspaceButtonActive(ActiveDocumentsButton, MainTabs.SelectedIndex == TabPresupuesto && PresupuestoWorkspaceTabs.SelectedIndex == InnerTabDocumentos);
        }

        private void SetWorkspaceButtonActive(Button button, bool isActive)
        {
            if (button == null)
                return;

            button.Background = isActive ? new SolidColorBrush(MediaColor.FromRgb(67, 87, 67)) : new SolidColorBrush(MediaColor.FromRgb(238, 242, 234));
            button.Foreground = isActive ? Brushes.White : new SolidColorBrush(MediaColor.FromRgb(67, 87, 67));
            button.BorderBrush = isActive ? new SolidColorBrush(MediaColor.FromRgb(67, 87, 67)) : new SolidColorBrush(MediaColor.FromRgb(200, 209, 194));
        }

        private void WorkspaceSectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            if (button == ActiveDocumentsButton)
            {
                if (!TryFlushPendingChangesBeforeWorkflowAction("abrir documentos"))
                    return;

                OpenBudgetDocumentsTab();
                return;
            }

            if (button == ActiveSummaryButton)
            {
                OpenBudgetSummaryTab();
                return;
            }

            if (button.Tag == null)
                return;

            if (int.TryParse(button.Tag.ToString(), out var targetTab))
            {
                if (targetTab == TabProductos)
                {
                    if (!TryFlushPendingChangesBeforeWorkflowAction("abrir productos"))
                        return;

                    OpenProductsInlinePanel();
                    return;
                }

                ShellBreadcrumbOverride = null;
                GoToTab(targetTab);
                UpdateWorkspaceButtonState();
            }
        }

        private void ToggleSidebarButton_Click(object sender, RoutedEventArgs e)
        {
            IsSidebarCollapsed = !IsSidebarCollapsed;
            ApplySidebarState();
        }

        private void ApplySidebarState()
        {
            if (ShellSidebarColumn == null)
                return;

            ShellSidebarColumn.Width = new GridLength(IsSidebarCollapsed ? 72 : 230);

            var compactVisibility = IsSidebarCollapsed ? Visibility.Collapsed : Visibility.Visible;

            ShellBrandTextBlock.Visibility = compactVisibility;
            ShellSearchPanel.Visibility = compactVisibility;
            // 13A: the Context Bar in the main workspace is now the single source of active client/job context.
            // Keep the old sidebar card loaded for existing bindings/code-behind, but do not show it.
            ActiveWorkspaceBorder.Visibility = Visibility.Collapsed;
            ShellFooterTextBlock.Visibility = compactVisibility;

            ShellLogoImage.Width = IsSidebarCollapsed ? 40 : 60;
            ShellLogoImage.Height = IsSidebarCollapsed ? 40 : 60;

            // Use the same broadly supported glyphs in collapsed and expanded states.
            // Previous MDL2-only glyphs rendered as square boxes on some machines when the sidebar collapsed.
            DashboardNavButton.Content = IsSidebarCollapsed ? "⌂" : "⌂  Inicio";
            ClientsNavButton.Content = IsSidebarCollapsed ? "👤" : "👤  Clientes";
            ClientsNavButton.Visibility = Visibility.Collapsed;
            QuotesNavButton.Content = IsSidebarCollapsed ? "📜" : "📜  Trabajos";
            InvoicesNavButton.Content = IsSidebarCollapsed ? "🧾" : "🧾  Facturas";
            SettingsNavButton.Content = "⚙";
        }


        private void GlobalSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateGlobalSearchPlaceholder();
            RefreshGlobalSearchResults();
        }

        private void UpdateGlobalSearchPlaceholder()
        {
            if (GlobalSearchPlaceholderTextBlock == null || GlobalSearchTextBox == null)
                return;

            GlobalSearchPlaceholderTextBlock.Visibility =
                string.IsNullOrWhiteSpace(GlobalSearchTextBox.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        private void RefreshGlobalSearchResults()
        {
            if (GlobalSearchResultsBorder == null || GlobalSearchResultsListBox == null)
                return;

            GlobalSearchResults.Clear();

            var search = GlobalSearchTextBox.Text?.Trim() ?? string.Empty;
            if (search.Length < 2)
            {
                GlobalSearchResultsBorder.Visibility = Visibility.Collapsed;
                return;
            }

            var term = search.ToLowerInvariant();

            using var db = new AppDbContext();

            var clients = db.Clients
                .Include(c => c.Quotes)
                .AsNoTracking()
                .ToList()
                .Where(c =>
                    ContainsSearch(c.Name, term) ||
                    ContainsSearch(c.Phone, term) ||
                    ContainsSearch(c.Dni, term))
                .OrderBy(c => c.Name)
                .Take(5)
                .Select(c => new GlobalSearchResult
                {
                    ResultType = GlobalSearchResultType.Client,
                    ClientId = c.Id,
                    TypeLabel = "CLIENTE",
                    PrimaryText = string.IsNullOrWhiteSpace(c.Name) ? "Cliente sin nombre" : c.Name,
                    SecondaryText = BuildGlobalClientSummary(c)
                });

            foreach (var result in clients)
                GlobalSearchResults.Add(result);

            var quotes = db.Quotes
                .Include(q => q.Client)
                .AsNoTracking()
                .ToList()
                .Where(q =>
                    ContainsSearch(q.Title, term) ||
                    ContainsSearch(q.DisplayTitle, term) ||
                    ContainsSearch(q.Status, term) ||
                    ContainsSearch(q.Client?.Name, term) ||
                    ContainsSearch(q.Client?.Phone, term) ||
                    ContainsSearch(q.Client?.Dni, term) ||
                    (q.EventDate.HasValue &&
                        (q.EventDate.Value.ToString("dd/MM/yyyy").Contains(term) ||
                         q.EventDate.Value.ToString("dd-MM-yyyy").Contains(term))))
                .OrderBy(q => q.EventDate ?? DateTime.MaxValue)
                .Take(7)
                .Select(q => new GlobalSearchResult
                {
                    ResultType = GlobalSearchResultType.Quote,
                    ClientId = q.ClientId,
                    QuoteId = q.Id,
                    TypeLabel = "PRESUP.",
                    PrimaryText = q.DisplayTitle,
                    SecondaryText = $"{q.Client?.Name ?? "Cliente"} · {q.Status} · Evento {(q.EventDate.HasValue ? q.EventDate.Value.ToString("dd/MM/yyyy") : "—")} · {q.Total:N2} €"
                });

            foreach (var result in quotes)
            {
                if (!GlobalSearchResults.Any(r => r.ResultType == result.ResultType && r.QuoteId == result.QuoteId))
                    GlobalSearchResults.Add(result);
            }

            GlobalSearchResultsBorder.Visibility = GlobalSearchResults.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;

            if (GlobalSearchResults.Count > 0)
                GlobalSearchResultsListBox.SelectedIndex = 0;
        }

        private static bool ContainsSearch(string? value, string term)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return value.ToLowerInvariant().Contains(term);
        }

        private static string BuildGlobalClientSummary(Models.Client client)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(client.DisplayPhone))
                parts.Add(client.DisplayPhone);

            if (!string.IsNullOrWhiteSpace(client.Dni))
                parts.Add(client.Dni);

            var nextDelivery = client.Quotes?
                .Where(q => q.EventDate.HasValue && q.EventDate.Value.Date >= DateTime.Today && q.Status != "Entregado")
                .OrderBy(q => q.EventDate.GetValueOrDefault())
                .FirstOrDefault();

            if (nextDelivery != null)
                parts.Add($"Próximo evento {nextDelivery.EventDate.GetValueOrDefault():dd/MM/yyyy}");

            return parts.Count == 0 ? "Sin teléfono ni eventos próximos" : string.Join(" · ", parts);
        }

        private void GlobalSearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                MoveGlobalSearchSelection(1);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Up)
            {
                MoveGlobalSearchSelection(-1);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape)
            {
                HideGlobalSearchResults();
                e.Handled = true;
                return;
            }

            if (e.Key != Key.Enter)
                return;

            e.Handled = true;

            if (GlobalSearchResultsListBox.SelectedItem is GlobalSearchResult selectedResult)
            {
                OpenGlobalSearchResult(selectedResult);
                return;
            }

            var search = GlobalSearchTextBox.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(search))
                return;

            if (GlobalSearchResults.Count > 0 && GlobalSearchResults[0] is GlobalSearchResult firstResult)
            {
                OpenGlobalSearchResult(firstResult);
                return;
            }

            // Clientes ya no es una pantalla visible: ante una búsqueda sin resultado
            // mantenemos al usuario en el workspace contextual de Trabajos.
            NavigateToSection(TabPresupuesto);
            BudgetQuotesListBox?.Focus();
        }

        private void MoveGlobalSearchSelection(int offset)
        {
            if (GlobalSearchResultsListBox == null || GlobalSearchResults.Count == 0)
                return;

            var nextIndex = GlobalSearchResultsListBox.SelectedIndex + offset;

            if (nextIndex < 0)
                nextIndex = GlobalSearchResults.Count - 1;

            if (nextIndex >= GlobalSearchResults.Count)
                nextIndex = 0;

            GlobalSearchResultsListBox.SelectedIndex = nextIndex;
            GlobalSearchResultsListBox.ScrollIntoView(GlobalSearchResultsListBox.SelectedItem);
        }

        private void GlobalSearchResultsListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (GlobalSearchResultsListBox.SelectedItem is GlobalSearchResult result)
            {
                e.Handled = true;
                OpenGlobalSearchResult(result);
            }
        }

        private void GlobalSearchResultsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GlobalSearchResultsListBox.SelectedItem is GlobalSearchResult result)
                OpenGlobalSearchResult(result);
        }

        private void OpenGlobalSearchResult(GlobalSearchResult result)
        {
            HideGlobalSearchResults(clearText: true);

            if (result.ResultType == GlobalSearchResultType.Quote && result.QuoteId.HasValue)
            {
                OpenQuoteById(result.QuoteId.Value, TabPresupuesto, "Búsqueda");
                return;
            }

            if (result.ResultType == GlobalSearchResultType.Client && result.ClientId.HasValue)
            {
                OpenClientById(result.ClientId.Value);
            }
        }

        private void OpenClientById(int clientId)
        {
            if (!ConfirmDiscardChanges())
                return;

            SuppressSelectionConfirm = true;
            ShellBreadcrumbOverride = null;

            LoadClients();
            var wasLoaded = LoadClientWorkspaceById(clientId);

            SuppressSelectionConfirm = false;

            if (!wasLoaded)
                return;

            // Clientes deja de ser una pantalla principal: una búsqueda de cliente
            // abre directamente el workspace contextual de Trabajos con ese cliente cargado.
            NavigateToSection(TabPresupuesto);
            UpdateContextBreadcrumb("Búsqueda", TabPresupuesto);
            UpdateShellNavigationState();
            BudgetQuotesListBox?.Focus();
        }

        private bool LoadClientWorkspaceById(int clientId, int? quoteId = null)
        {
            using var db = new AppDbContext();

            var client = db.Clients
                .AsNoTracking()
                .FirstOrDefault(c => c.Id == clientId);

            if (client == null)
                return false;

            var quotes = db.Quotes
                .Include(q => q.Items)
                .AsNoTracking()
                .Where(q => q.ClientId == clientId)
                .OrderBy(q => q.EventDate ?? DateTime.MaxValue)
                .ThenBy(q => q.Id)
                .ToList();

            var selectedQuote = quoteId.HasValue
                ? quotes.FirstOrDefault(q => q.Id == quoteId.Value)
                : quotes.Count == 1
                    ? quotes[0]
                    : null;

            IsLoadingData = true;

            ClientNameTextBox.Text = client.Name;
            PhoneTextBox.Text = client.Phone;
            DniTextBox.Text = client.Dni;

            QuotesListBox.ItemsSource = quotes;
            BudgetQuotesListBox.ItemsSource = quotes;
            UpdateClientDetailPanel(client, quotes);

            Products.Clear();

            CurrentClientId = client.Id;
            LastSelectedClientId = client.Id;
            SyncPredictiveClientSelection(client.Id);

            if (selectedQuote == null)
            {
                CurrentQuote = null;
                LastSelectedQuoteId = null;

                DeliveryDatePicker.SelectedDate = null;
                EventDatePicker.SelectedDate = null;
                QuoteStatusComboBox.SelectedItem = "Pendiente";
                DepositTextBox.Text = "0";
                QuoteNotesTextBox.Text = "";
                ClientNotesTextBox.Text = "";
                QuoteTitleTextBox.Text = "";
            }
            else
            {
                CurrentQuote = selectedQuote;
                LastSelectedQuoteId = selectedQuote.Id;

                DeliveryDatePicker.SelectedDate = selectedQuote.DeliveryDate;
                EventDatePicker.SelectedDate = selectedQuote.EventDate;
                QuoteTitleTextBox.Text = selectedQuote.Title;
                QuoteStatusComboBox.SelectedItem = string.IsNullOrWhiteSpace(selectedQuote.Status)
                    ? "Pendiente"
                    : selectedQuote.Status;
                DepositTextBox.Text = $"{selectedQuote.Deposit:N2}";
                QuoteNotesTextBox.Text = selectedQuote.Notes;
                ClientNotesTextBox.Text = selectedQuote.ClientNotes;

                foreach (var item in selectedQuote.Items)
                {
                    Products.Add(ProductLine.FromQuoteItem(item));
                }
            }

            if (!string.IsNullOrWhiteSpace(WorkSearchTextBox.Text))
                WorkSearchTextBox.Text = string.Empty;

            UpdateGrandTotal();
            UpdatePendingAmount();

            IsRevertingSelection = true;
            ClientsListBox.SelectedItem = ((IEnumerable<Models.Client>)ClientsListBox.ItemsSource)
                .FirstOrDefault(c => c.Id == clientId);
            QuotesListBox.SelectedItem = selectedQuote;
            BudgetQuotesListBox.SelectedItem = selectedQuote;
            IsRevertingSelection = false;

            IsLoadingData = false;

            MarkAsSaved();
            UpdateSaveButtonText();
            UpdateActiveContext();
            UpdateWorkflowState();
            UpdateSecondaryPlaceholders();
            UpdateWorkspaceButtonState();

            return true;
        }

        private void HideGlobalSearchResults(bool clearText = false)
        {
            if (clearText && GlobalSearchTextBox != null)
                GlobalSearchTextBox.Text = string.Empty;

            GlobalSearchResults.Clear();

            if (GlobalSearchResultsBorder != null)
                GlobalSearchResultsBorder.Visibility = Visibility.Collapsed;
        }


        private void WorkSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateWorkSearchPlaceholder();
            ApplyBudgetQuoteFilter();
        }

        private void UpdateWorkSearchPlaceholder()
        {
            if (WorkSearchPlaceholderTextBlock == null || WorkSearchTextBox == null)
                return;

            WorkSearchPlaceholderTextBlock.Visibility = string.IsNullOrWhiteSpace(WorkSearchTextBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void ApplyBudgetQuoteFilter()
        {
            if (BudgetQuotesListBox == null || QuotesListBox == null)
                return;

            var allQuotes = QuotesListBox.ItemsSource as IEnumerable<Models.Quote>;
            if (allQuotes == null)
                return;

            var selectedQuote = BudgetQuotesListBox.SelectedItem as Models.Quote;
            var query = WorkSearchTextBox?.Text?.Trim() ?? string.Empty;

            var filtered = string.IsNullOrWhiteSpace(query)
                ? allQuotes
                : allQuotes.Where(q => MatchesWorkSearch(q, query));

            var ordered = filtered
                .OrderBy(q => q.EventDate ?? DateTime.MaxValue)
                .ThenBy(q => q.Id)
                .ToList();

            BudgetQuotesListBox.ItemsSource = ordered;

            if (selectedQuote != null)
            {
                var replacement = ordered.FirstOrDefault(q => q.Id == selectedQuote.Id);
                if (replacement != null)
                {
                    BudgetQuotesListBox.SelectedItem = replacement;
                }
            }
        }

        private static bool MatchesWorkSearch(Models.Quote quote, string query)
        {
            var normalizedQuery = NormalizeSearchText(query);

            bool Contains(string? value) => NormalizeSearchText(value ?? string.Empty).Contains(normalizedQuery);

            return Contains(quote.Title)
                || Contains(quote.DisplayTitle)
                || Contains(quote.Status)
                || Contains(quote.Client?.Name)
                || Contains(quote.Client?.Phone)
                || Contains(quote.Client?.Dni)
                || (quote.EventDate.HasValue && Contains(quote.EventDate.Value.ToString("dd/MM/yyyy")));
        }

        private static string NormalizeSearchText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            return new string(normalized.Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark).ToArray())
                .Normalize(NormalizationForm.FormC);
        }

        private void PresupuestoWorkspaceTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != PresupuestoWorkspaceTabs)
                return;

            UpdateWorkspaceButtonState();
        }

        private void OpenProductsInlinePanel()
        {
            UpdateContextBreadcrumb("Trabajos", TabProductos);
            NavigateToSection(TabPresupuesto);
            PresupuestoWorkspaceTabs.SelectedIndex = InnerTabResumen;

            WorkspaceDashboard.Visibility = Visibility.Collapsed;
            ProductsInlineHost.Visibility = Visibility.Visible;
            UpdateWorkspaceButtonState();

            Dispatcher.BeginInvoke(new Action(() =>
            {
                ProductsInlineHost.BringIntoView();
                ProductsDataGrid.Focus();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void HideProductsInlinePanel()
        {
            if (ProductsInlineHost.Visibility != Visibility.Visible)
                return;

            ProductsInlineHost.Visibility = Visibility.Collapsed;
            WorkspaceDashboard.Visibility = Visibility.Visible;
            UpdateWorkspaceButtonState();
        }

        private void OpenBudgetSummaryTab()
        {
            HideProductsInlinePanel();
            UpdateContextBreadcrumb("Trabajos", TabPresupuesto);
            NavigateToSection(TabPresupuesto);
            PresupuestoWorkspaceTabs.SelectedIndex = InnerTabResumen;
            UpdateWorkspaceButtonState();
        }

        private void OpenBudgetDocumentsTab()
        {
            UpdateContextBreadcrumb("Trabajos", TabPresupuesto);
            NavigateToSection(TabPresupuesto);
            PresupuestoWorkspaceTabs.SelectedIndex = InnerTabDocumentos;
            UpdateWorkspaceButtonState();
        }

        private void BudgetQuickSummaryButton_Click(object sender, RoutedEventArgs e)
        {
            OpenBudgetSummaryTab();
        }

        private void BackToJobDashboardButton_Click(object sender, RoutedEventArgs e)
        {
            HideProductsInlinePanel();
            UpdateContextBreadcrumb("Trabajos", TabPresupuesto);
            NavigateToSection(TabPresupuesto);
        }

        private void BudgetQuickHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateContextBreadcrumb("Trabajos", TabPresupuesto);
            NavigateToSection(TabPresupuesto);
            PresupuestoWorkspaceTabs.SelectedIndex = InnerTabHistorial;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                ActivityTimelineBorder.BringIntoView();
                WorkspaceScrollViewer.ScrollToTop();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void BudgetQuickExportPdfButton_Click(object sender, RoutedEventArgs e)
        {
            ExportPdfButton_Click(sender, e);
        }

        private void BudgetQuickProductsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TryFlushPendingChangesBeforeWorkflowAction("abrir productos"))
                return;

            OpenProductsInlinePanel();
        }

        private void BudgetQuickDocumentsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TryFlushPendingChangesBeforeWorkflowAction("abrir documentos"))
                return;

            OpenBudgetDocumentsTab();
        }

        private void OpenQuoteById(int quoteId, int targetTab = TabPresupuesto, string? breadcrumbOrigin = null)
        {
            if (!ConfirmDiscardChanges())
                return;

            using var db = new AppDbContext();

            var quoteSnapshot = db.Quotes
                .AsNoTracking()
                .FirstOrDefault(q => q.Id == quoteId);

            if (quoteSnapshot == null)
                return;

            SuppressSelectionConfirm = true;
            PendingNavigationTargetTab = targetTab;
            ShellBreadcrumbOverride = null;

            LoadClients();
            var wasLoaded = LoadClientWorkspaceById(quoteSnapshot.ClientId, quoteId);

            SuppressSelectionConfirm = false;

            if (!wasLoaded)
            {
                PendingNavigationTargetTab = null;
                return;
            }

            PendingNavigationTargetTab = null;
            if (targetTab == TabProductos)
            {
                OpenProductsInlinePanel();
            }
            else
            {
                GoToTab(targetTab);
            }

            UpdateContextBreadcrumb(breadcrumbOrigin, targetTab);
            UpdateShellNavigationState();
        }

        private void UpdateContextBreadcrumb(string? origin, int targetTab)
        {
            var clientName = string.IsNullOrWhiteSpace(ClientNameTextBox.Text)
                ? null
                : ClientNameTextBox.Text.Trim();

            var quoteTitle = string.IsNullOrWhiteSpace(QuoteTitleTextBox.Text)
                ? null
                : QuoteTitleTextBox.Text.Trim();

            if (clientName == null)
            {
                ShellBreadcrumbOverride = null;
                return;
            }

            var target = targetTab switch
            {
                TabProductos => "Productos",
                TabClientes => "Trabajos",
                _ => "Trabajos"
            };

            var parts = new List<string> { "🏠" };

            parts.Add("Trabajos");
            parts.Add(clientName);

            if (targetTab != TabClientes && quoteTitle != null)
                parts.Add(quoteTitle);

            {

                if (targetTab == TabProductos)
                    parts.Add(target);
            }

            ShellBreadcrumbOverride = string.Join(" > ", parts);
        }

        private void OpenWeeklyDelivery(WeeklyDeliveryItem delivery)
        {
            if (delivery == null || delivery.QuoteId <= 0)
                return;

            OpenQuoteById(delivery.QuoteId, TabPresupuesto, "Inicio");
        }

        private void CalendarDelivery_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as DependencyObject;

            while (element != null)
            {
                if (element is FrameworkElement frameworkElement &&
                    frameworkElement.DataContext is WeeklyDeliveryItem delivery)
                {
                    OpenWeeklyDelivery(delivery);
                    e.Handled = true;
                    return;
                }

                element = VisualTreeHelper.GetParent(element);
            }
        }

        /*  DESACTIVADO
         private void WeeklyDeliveriesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
         {
             var element = e.OriginalSource as DependencyObject;

             while (element != null)
             {
                 if (element is FrameworkElement frameworkElement &&
                     frameworkElement.DataContext is WeeklyDeliveryItem delivery)
                 {
                     OpenWeeklyDelivery(delivery);
                     return;
                 }

                 element = System.Windows.Media.VisualTreeHelper.GetParent(element);
             }
         }
        */

        private void WeeklyDeliveryCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not FrameworkElement element)
                return;

            if (element.DataContext is not WeeklyDeliveryItem delivery)
                return;

            OpenWeeklyDelivery(delivery);
        }

        internal void UpcomingDeliveryCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not FrameworkElement element)
                return;

            if (element.DataContext is not WeeklyDeliveryItem delivery)
                return;

            OpenWeeklyDelivery(delivery);
        }

        internal void UpcomingDeliveriesMonthButton_Click(object sender, RoutedEventArgs e)
        {
            CalendarViewMode = "Mes";
            CalendarReferenceDate = DateTime.Today;
            LoadCalendarDeliveries();
            UpdateCalendarViewButtons();
        }

        private void UpdateEmptyStateMessages()
        {
            var hasClient =
                !string.IsNullOrWhiteSpace(ClientNameTextBox.Text) ||
                !string.IsNullOrWhiteSpace(PhoneTextBox.Text);

            var hasProducts = Products.Count > 0;

            PresupuestoEmptyHintBorder.Visibility = hasClient
                ? Visibility.Collapsed
                : Visibility.Visible;

            if (!hasClient)
            {
                ProductsEmptyHintBorder.Visibility = Visibility.Visible;
                ProductsEmptyHintTextBlock.Text = "Selecciona o crea un cliente antes de añadir productos.";
            }
            else if (!hasProducts)
            {
                ProductsEmptyHintBorder.Visibility = Visibility.Visible;
                ProductsEmptyHintTextBlock.Text = "Añade productos para construir el trabajo.";
            }
            else
            {
                ProductsEmptyHintBorder.Visibility = Visibility.Collapsed;
            }

            if (!hasClient)
            {
                DocumentsEmptyHintBorder.Visibility = Visibility.Visible;
                DocumentsEmptyHintTextBlock.Text = "Selecciona o crea un cliente antes de exportar documentos.";
            }
            else if (!hasProducts)
            {
                DocumentsEmptyHintBorder.Visibility = Visibility.Visible;
                DocumentsEmptyHintTextBlock.Text = "Añade al menos un producto antes de exportar documentos.";
            }
            else
            {
                DocumentsEmptyHintBorder.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateWorkflowState()
        {
            var hasSavedClient = CurrentClientId != null;
            var hasProducts = Products.Count > 0;

            QuoteTitleTextBox.IsEnabled = hasSavedClient;
            QuoteStatusComboBox.IsEnabled = hasSavedClient;
            DeliveryDatePicker.IsEnabled = hasSavedClient;
            DepositTextBox.IsEnabled = hasSavedClient;
            QuoteNotesTextBox.IsEnabled = hasSavedClient;
            ClientNotesTextBox.IsEnabled = hasSavedClient;

            NewQuoteButton.IsEnabled = true;
            DuplicateQuoteButton.IsEnabled = hasSavedClient;

            ProductsDataGrid.IsEnabled = hasSavedClient;
            AddProductButton.IsEnabled = hasSavedClient;
            SaveProductsButton.IsEnabled = hasSavedClient;

            ExportPdfButton.IsEnabled = hasSavedClient && hasProducts;
            ConvertQuoteToInvoiceButton.IsEnabled = CurrentQuote != null && hasProducts;
            ViewQuoteInvoiceButton.IsEnabled = CurrentQuote != null;
            OpenLastPdfButton.IsEnabled = !string.IsNullOrWhiteSpace(LastGeneratedPdfPath) && File.Exists(LastGeneratedPdfPath);
            ExportAllQuotesPdfButton.IsEnabled = hasSavedClient;
            BudgetQuickSummaryButton.IsEnabled = hasSavedClient;
            BudgetQuickProductsButton.IsEnabled = hasSavedClient;
            BudgetQuickHistoryButton.IsEnabled = hasSavedClient;
            BudgetQuickDocumentsButton.IsEnabled = hasSavedClient;
            BudgetQuickExportPdfButton.IsEnabled = hasSavedClient && hasProducts;

            ShellQuickProductsButton.IsEnabled = hasSavedClient;
            ShellQuickExportPdfButton.IsEnabled = hasSavedClient && hasProducts;
            ShellQuickDeliveredButton.IsEnabled = hasSavedClient;

            SaveButton.IsEnabled = hasSavedClient;
        }

        private void FocusSelectedClient()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (ClientsListBox.SelectedItem != null)
                {
                    ClientsListBox.ScrollIntoView(ClientsListBox.SelectedItem);

                    var item = ClientsListBox.ItemContainerGenerator
                        .ContainerFromItem(ClientsListBox.SelectedItem) as ListBoxItem;

                    item?.Focus();
                }
                else
                {
                    ClientsListBox.Focus();
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void FocusSelectedQuote()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (QuotesListBox.SelectedItem != null)
                {
                    QuotesListBox.ScrollIntoView(QuotesListBox.SelectedItem);

                    var item = QuotesListBox.ItemContainerGenerator
                        .ContainerFromItem(QuotesListBox.SelectedItem) as ListBoxItem;

                    item?.Focus();
                }
                else
                {
                    QuotesListBox.Focus();
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private string MakeSafeFileName(string text)
        {
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                text = text.Replace(invalidChar, '_');
            }

            return text.Trim();
        }

        private void PreviousCalendarButton_Click(object sender, RoutedEventArgs e)
        {
            if (CalendarViewMode == "Mes")
                CalendarReferenceDate = CalendarReferenceDate.AddMonths(-1);
            else if (CalendarViewMode == "Semana")
                CalendarReferenceDate = CalendarReferenceDate.AddDays(-7);

            LoadCalendarDeliveries();
            UpdateCalendarViewButtons();
        }

        private void NextCalendarButton_Click(object sender, RoutedEventArgs e)
        {
            if (CalendarViewMode == "Mes")
                CalendarReferenceDate = CalendarReferenceDate.AddMonths(1);
            else if (CalendarViewMode == "Semana")
                CalendarReferenceDate = CalendarReferenceDate.AddDays(7);

            LoadCalendarDeliveries();
            UpdateCalendarViewButtons();
        }

        private void TodayCalendarButton_Click(object sender, RoutedEventArgs e)
        {
            CalendarReferenceDate = DateTime.Today;

            LoadCalendarDeliveries();
            UpdateCalendarViewButtons();
        }

        private void ListViewButton_Click(object sender, RoutedEventArgs e)
        {
            CalendarViewMode = "Lista";
            CalendarReferenceDate = DateTime.Today;

            LoadCalendarDeliveries();
            UpdateCalendarViewButtons();
        }

        private void WeekViewButton_Click(object sender, RoutedEventArgs e)
        {
            CalendarViewMode = "Semana";

            LoadCalendarDeliveries();
            UpdateCalendarViewButtons();
        }

        private void MonthViewButton_Click(object sender, RoutedEventArgs e)
        {
            CalendarViewMode = "Mes";

            LoadCalendarDeliveries();
            UpdateCalendarViewButtons();
        }

        private void UpdateCalendarViewButtons()
        {
            var isListView = CalendarViewMode == "Lista";
            var isWeekView = CalendarViewMode == "Semana";
            var isMonthView = CalendarViewMode == "Mes";

            ListViewButton.Style = (Style)FindResource(
                isListView
                    ? "ActiveFilterButtonStyle"
                    : "SecondaryButtonStyle");

            WeekViewButton.Style = (Style)FindResource(
                isWeekView
                    ? "ActiveFilterButtonStyle"
                    : "SecondaryButtonStyle");

            MonthViewButton.Style = (Style)FindResource(
                isMonthView
                    ? "ActiveFilterButtonStyle"
                    : "SecondaryButtonStyle");

            DeliveriesListBox.Visibility = isListView
                ? Visibility.Visible
                : Visibility.Collapsed;

            WeeklyColumnsItemsControl.Visibility = isWeekView
                ? Visibility.Visible
                : Visibility.Collapsed;

            CalendarGroupsListBox.Visibility = isMonthView
                ? Visibility.Visible
                : Visibility.Collapsed;

            PreviousCalendarButton.IsEnabled = !isListView;
            NextCalendarButton.IsEnabled = !isListView;
        }

        private void DepositTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            var proposedText = GetProposedText(textBox, e.Text);

            e.Handled = !IsValidNumericInput(proposedText, allowNegative: false);
        }

        private void DepositTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            if (!e.DataObject.GetDataPresent(typeof(string)))
            {
                e.CancelCommand();
                return;
            }

            var pasteText = e.DataObject.GetData(typeof(string)) as string ?? "";
            var proposedText = GetProposedText(textBox, pasteText);

            if (!IsValidNumericInput(proposedText, allowNegative: false))
            {
                e.CancelCommand();
            }
        }

        private void DepositTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not TextBox textBox || textBox.IsKeyboardFocusWithin)
                return;

            e.Handled = true;
            textBox.Focus();
        }

        private void DepositTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            DepositTextBox.Text = DepositTextBox.Text
                .Replace("€", "")
                .Trim();

            if (GetDepositValue() == 0)
            {
                DepositTextBox.Clear();
            }

            DepositTextBox.SelectAll();
        }

        private void DepositTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var deposit = GetDepositValue();

            DepositTextBox.Text = $"{deposit:N2}";

            UpdatePendingAmount();
        }

        private void UpdateSecondaryPlaceholders()
        {
            QuoteTitlePlaceholderTextBlock.Visibility =
                string.IsNullOrWhiteSpace(QuoteTitleTextBox.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            QuoteNotesPlaceholderTextBlock.Visibility =
                string.IsNullOrWhiteSpace(QuoteNotesTextBox.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            ClientNotesPlaceholderTextBlock.Visibility =
                string.IsNullOrWhiteSpace(ClientNotesTextBox.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        internal void CalendarDelivery_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as DependencyObject;

            while (element != null)
            {
                if (element is FrameworkElement frameworkElement &&
                    frameworkElement.DataContext is WeeklyDeliveryItem delivery)
                {
                    OpenWeeklyDelivery(delivery);
                    e.Handled = true;
                    return;
                }

                element = VisualTreeHelper.GetParent(element);
            }
        }


        private enum GlobalSearchResultType
        {
            Client,
            Quote
        }

        private sealed class GlobalSearchResult
        {
            public GlobalSearchResultType ResultType { get; init; }

            public int? ClientId { get; init; }

            public int? QuoteId { get; init; }

            public string TypeLabel { get; init; } = string.Empty;

            public string PrimaryText { get; init; } = string.Empty;

            public string SecondaryText { get; init; } = string.Empty;
        }

        private sealed record WorkspaceUndoSnapshot(
            string ClientName,
            string Phone,
            string Dni,
            DateTime? DeliveryDate,
            DateTime? EventDate,
            string QuoteTitle,
            string QuoteStatus,
            string Deposit,
            string QuoteNotes,
            string ClientNotes,
            IReadOnlyList<ProductUndoSnapshot> Products);

        private sealed record ProductUndoSnapshot(
            string ProductName,
            string TailoringType,
            string Fabric,
            decimal BasePrice,
            decimal FabricPrice,
            decimal ManualPrice,
            int Quantity,
            decimal Total)
        {
            public static ProductUndoSnapshot FromProductLine(ProductLine product)
            {
                return new ProductUndoSnapshot(
                    product.ProductName,
                    product.TailoringType,
                    product.Fabric,
                    product.BasePrice,
                    product.FabricPrice,
                    product.ManualPrice,
                    product.Quantity,
                    product.Total);
            }

            public ProductLine ToProductLine()
            {
                var product = new ProductLine();

                product.LoadPersistedValues(
                    ProductName,
                    TailoringType,
                    Fabric,
                    BasePrice,
                    FabricPrice,
                    Quantity,
                    Total);

                return product;
            }
        }

    }
}
