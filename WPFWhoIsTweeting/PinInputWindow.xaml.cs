using System.Windows;

namespace WPFWhoIsTweeting
{
    /// <summary>
    /// PinInputWindow.xaml에 대한 상호 작용 논리
    /// </summary>
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
            if (viewModel.PIN == "") return;
            DialogResult = true;
            Close();
        }
    }
}
