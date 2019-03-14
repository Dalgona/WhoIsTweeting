using System.Windows;
using Wit.VM;

namespace WhoIsTweeting
{
    public partial class PinInputWindow : Window
    {
        KeyViewModel viewModel;

        public PinInputWindow()
        {
            InitializeComponent();

            DataContext = viewModel = new KeyViewModel();
        }

        private void OnOKClicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(viewModel.Pin)) return;
            DialogResult = true;
            Close();
        }
    }
}
