using System;
using System.Windows;
using System.Windows.Input;

namespace WhoIsTweeting
{
    public enum MessageWindowType { MentionWindow, DirectMessageWindow };

    public partial class MessageWindow : Window
    {
        private MessageViewModel viewModel;

        public MessageWindow(MessageWindowType type, UserListItem sendTo)
        {
            InitializeComponent();

            DataContext = viewModel = new MessageViewModel(type, sendTo);
            if (type == MessageWindowType.MentionWindow)
                viewModel.Content = $"@{sendTo?.ScreenName} ";
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
