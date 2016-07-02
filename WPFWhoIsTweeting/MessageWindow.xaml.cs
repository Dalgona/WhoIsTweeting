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

namespace WPFWhoIsTweeting
{
    public enum MessageWindowType { MentionWindow, DirectMessageWindow };

    /// <summary>
    /// MessageWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MessageWindow : Window
    {
        private MessageViewModel viewModel;

        public MessageWindow(MessageWindowType type, UserListItem sendTo)
        {
            InitializeComponent();

            DataContext = viewModel = new MessageViewModel(type, sendTo);
            if (type == MessageWindowType.MentionWindow)
                viewModel.Content = $"@{sendTo.ScreenName} ";
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OnOKClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                content.Focus();
                Keyboard.Focus(content);
                content.Select(int.MaxValue, 0);
            }));
        }
    }
}
