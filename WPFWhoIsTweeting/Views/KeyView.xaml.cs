using System;
using System.Windows;
using System.Windows.Controls;

namespace WhoIsTweeting.Views
{
    public partial class KeyView : UserControl
    {
        public KeyView()
        {
            InitializeComponent();
        }

        private void OnOKClicked(object sender, RoutedEventArgs e)
        {
            //if (string.IsNullOrEmpty(viewModel.ConsumerKey)
            //    || string.IsNullOrEmpty(viewModel.ConsumerSecret))
            //{
            //    MessageBox.Show(Strings.Consumer_Empty_Error, Strings.Consumer_Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            //    return;
            //}
            //DialogResult = true;
            //Close();
            throw new NotImplementedException();
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            //DialogResult = false;
            //Close();
            throw new NotImplementedException();
        }
    }
}
