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
    /// AboutWindow.xaml에 대한 상호 작용 논리
    /// </summary>
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
