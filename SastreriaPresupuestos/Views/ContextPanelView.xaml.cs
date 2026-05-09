using System.Windows;
using System.Windows.Controls;

namespace SastreriaPresupuestos.Views
{
    public partial class ContextPanelView : UserControl
    {
        public ContextPanelView()
        {
            InitializeComponent();
        }

        public TextBlock TitleTextBlock => ContextPanelTitleTextBlock;
        public TextBlock SubtitleTextBlock => ContextPanelSubtitleTextBlock;
        public TextBlock ClientTextBlock => ContextClientTextBlock;
        public TextBlock QuoteTextBlock => ContextQuoteTextBlock;
        public TextBlock StatusTextBlock => ContextStatusTextBlock;
        public TextBlock DeliveryTextBlock => ContextDeliveryTextBlock;
        public TextBlock EventTextBlock => ContextEventTextBlock;
        public TextBlock TotalTextBlock => ContextTotalTextBlock;
        public TextBlock PendingTextBlock => ContextPendingTextBlock;
        public TextBlock NotesTextBlock => ContextNotesTextBlock;

        public event RoutedEventHandler? SaveRequested;
        public event RoutedEventHandler? MarkDeliveredRequested;
        public event RoutedEventHandler? ExportPdfRequested;
        public event RoutedEventHandler? DocumentsRequested;

        private void SaveButton_Click(object sender, RoutedEventArgs e) => SaveRequested?.Invoke(sender, e);
        private void MarkDeliveredButton_Click(object sender, RoutedEventArgs e) => MarkDeliveredRequested?.Invoke(sender, e);
        private void ExportPdfButton_Click(object sender, RoutedEventArgs e) => ExportPdfRequested?.Invoke(sender, e);
        private void DocumentsButton_Click(object sender, RoutedEventArgs e) => DocumentsRequested?.Invoke(sender, e);
    }
}
