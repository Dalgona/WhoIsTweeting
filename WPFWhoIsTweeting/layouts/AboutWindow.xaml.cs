using System.Windows;
using System.Windows.Input;

namespace WhoIsTweeting
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void OnCloseButtonClicked(object sender, RoutedEventArgs e)
            => Close();

        private void OnEmailClicked(object sender, MouseButtonEventArgs e)
            => System.Diagnostics.Process.Start("mailto:dalgona@hontou.moe");

        private void OnWebsiteClicked(object sender, MouseButtonEventArgs e)
            => System.Diagnostics.Process.Start("http://dalgona.github.io");

        private void OnPicoBirdLogoClicked(object sender, MouseButtonEventArgs e)
            => System.Diagnostics.Process.Start("https://github.com/Dalgona/PicoBird");
    }
}
