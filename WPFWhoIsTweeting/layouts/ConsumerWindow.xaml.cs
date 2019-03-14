using System;
using System.Windows;
using System.Windows.Input;
using Wit.VM;

namespace WhoIsTweeting
{
    public partial class ConsumerWindow : Window
    {
        KeyViewModel viewModel;

        public ConsumerWindow()
        {
            InitializeComponent();

            DataContext = viewModel = new KeyViewModel();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                var coreSettings = Wit.Core.Properties.Settings.Default;

                viewModel.ConsumerKey = coreSettings.ConsumerKey;
                viewModel.ConsumerSecret = coreSettings.ConsumerSecret;

                consumerKey.Focus();
                Keyboard.Focus(consumerKey);
            }));
        }

        private void OnOKClicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(viewModel.ConsumerKey)
                || string.IsNullOrEmpty(viewModel.ConsumerSecret))
            {
                MessageBox.Show(Strings.Consumer_Empty_Error, Strings.Consumer_Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
