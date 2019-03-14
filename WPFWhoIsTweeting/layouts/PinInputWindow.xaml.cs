using System.Windows;
using Wit.VM;

namespace WhoIsTweeting
{
    public partial class PinInputWindow : Window
    {
        PinViewModel viewModel;

        public PinInputWindow()
        {
            InitializeComponent();

            DataContext = viewModel = new PinViewModel();
        }

        private void OnOKClicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(viewModel.Pin)) return;
            DialogResult = true;
            Close();
        }
    }
}
