using System.Windows.Controls;
using System.Windows;

namespace SastreriaPresupuestos.Views
{
    public partial class AjustesView : UserControl
    {
        public AjustesView()
        {
            InitializeComponent();
        }

        public void SetBankDetails(string? holder, string? iban, string? bank, string? bic)
        {
            BankAccountHolderTextBox.Text = holder ?? "";
            BankIbanTextBox.Text = iban ?? "";
            BankNameTextBox.Text = bank ?? "";
            BankBicTextBox.Text = bic ?? "";
        }

        private void OpenBankDetailsPopup_Click(object sender, RoutedEventArgs e) => BankDetailsPopup.IsOpen = true;
        private void CloseBankDetailsPopup_Click(object sender, RoutedEventArgs e) => BankDetailsPopup.IsOpen = false;
        private void OpenExportFoldersPopup_Click(object sender, RoutedEventArgs e) => ExportFoldersPopup.IsOpen = true;
        private void CloseExportFoldersPopup_Click(object sender, RoutedEventArgs e) => ExportFoldersPopup.IsOpen = false;
    }
}
