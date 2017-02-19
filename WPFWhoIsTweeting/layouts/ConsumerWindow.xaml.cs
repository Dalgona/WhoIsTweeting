using System;
using System.Windows;
using System.Windows.Input;

namespace WhoIsTweeting
{
    public partial class ConsumerWindow : Window
    {
        TokenViewModel viewModel;

        public ConsumerWindow()
        {
            InitializeComponent();

            DataContext = viewModel = new TokenViewModel();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                viewModel.ConsumerKey = Properties.Settings.Default.ConsumerKey;
                viewModel.ConsumerSecret = Properties.Settings.Default.ConsumerSecret;
                consumerKey.Focus();
                Keyboard.Focus(consumerKey);
            }));
        }

        private void OnOKClicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(viewModel.ConsumerKey)
                || string.IsNullOrEmpty(viewModel.ConsumerSecret))
            {
                MessageBox.Show(Application.Current.FindResource("Consumer_Empty_Error").ToString(),
                    Application.Current.FindResource("Consumer_Title").ToString(),
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            DialogResult = true;
            Close();
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
