using System.Windows;
using Wit.VM;

namespace WhoIsTweeting
{
    public partial class PinInputWindow : Window
    {
        TokenViewModel viewModel;

        public PinInputWindow()
        {
            InitializeComponent();

            DataContext = viewModel = new TokenViewModel();
        }

        private void OnOKClicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(viewModel.Pin)) return;
            DialogResult = true;
            Close();
        }
    }
}
