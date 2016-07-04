using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WhoIsTweeting
{
    /// <summary>
    /// ConsumerWindow.xaml에 대한 상호 작용 논리
    /// </summary>
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
            if (viewModel.ConsumerKey == "" || viewModel.ConsumerSecret == "")
            {
                MessageBox.Show("Both the consumer key and the consumer secret cannot be left blank.", "Set Consumer Key", MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
