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
    }
}
