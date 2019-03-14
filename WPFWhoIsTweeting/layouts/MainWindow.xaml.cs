using System;
using System.Windows;
using System.Windows.Data;
using Wit.VM;
using Wit.Controls;
using Wit.Core;

namespace WhoIsTweeting
{
    public partial class MainWindow : WindowBase
    {
        private MainViewModel viewModel;
        private Properties.Settings appSettings = Properties.Settings.Default;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = viewModel = new MainViewModel();
            viewModel.RefreshUserList += UserListRefreshRequested;
        }

        #region Main Menu Handler

        private void Menu_OnQuit(object sender, RoutedEventArgs e)
            => Application.Current.Shutdown();

        private void Menu_OnAbout(object sender, RoutedEventArgs e)
        {
            AboutWindow win = new AboutWindow { Owner = this };
            win.ShowDialog();
        }

        private void Menu_OnConsumer(object sender, RoutedEventArgs e)
        {
            ConsumerWindow win = new ConsumerWindow();
            KeyViewModel mdl = win.DataContext as KeyViewModel;
            var coreSettings = Wit.Core.Properties.Settings.Default;

            win.Owner = this;
            if ((bool)win.ShowDialog())
                if (!(mdl.ConsumerKey == coreSettings.ConsumerKey
                    && mdl.ConsumerSecret == coreSettings.ConsumerSecret))
                    viewModel.SetConsumerKey(mdl.ConsumerKey, mdl.ConsumerSecret);
        }

        private void Menu_OnSignIn(object sender, RoutedEventArgs e)
        {
            MessageBoxResult cont = MessageBox.Show(Strings.SignIn_Confirm, Strings.SignIn_Title,
                MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if (cont == MessageBoxResult.OK)
                viewModel.SignIn((url) =>
                {
                    PinInputWindow win = new PinInputWindow();
                    PinViewModel mdl = win.DataContext as PinViewModel;
                    System.Diagnostics.Process.Start(url);
                    win.Owner = this;
                    win.ShowDialog();
                    return mdl.Pin;
                }, (ex)=>
                {
                    MessageBox.Show(Strings.SignIn_Error, Strings.Title_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                });
        }

        private void Menu_OnStatistics(object sender, RoutedEventArgs e)
        {
            GraphWindow win = new GraphWindow { Owner = this };
            win.Show();
        }

        private void Menu_OnSetInterval(object sender, RoutedEventArgs e)
        {
            SetIntervalWindow win = new SetIntervalWindow { Owner = this };
            win.ShowDialog();
        }

        #endregion

        #region Context Menu Handler

        private void Context_OnMention(object sender, RoutedEventArgs e)
        {
            MessageWindow win = new MessageWindow(MessageWindowType.MentionWindow, viewModel.SelectedItem);
            MessageViewModel mdl = win.DataContext as MessageViewModel;
            win.Owner = this;
            if ((bool)win.ShowDialog())
                viewModel.PostTweet(mdl.Content, (ex) =>
                {
                    MessageBox.Show(Strings.Message_Error_Mention, Strings.Title_Error,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
        }

        private void Context_OnDirectMessage(object sender, RoutedEventArgs e)
        {
            MessageWindow win = new MessageWindow(MessageWindowType.DirectMessageWindow, viewModel.SelectedItem);
            MessageViewModel mdl = win.DataContext as MessageViewModel;
            win.Owner = this;
            if ((bool)win.ShowDialog())
                viewModel.SendDirectMessage(mdl.ScreenName, mdl.Content, (ex) =>
                {
                    MessageBox.Show(Strings.Message_Error_DM, Strings.Title_Error,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
        }

        private void Context_OnProfile(object sender, RoutedEventArgs e)
            => System.Diagnostics.Process.Start($"https://twitter.com/{viewModel.SelectedItem.ScreenName}");

        #endregion

        private void OnTryAgainClicked(object sender, RoutedEventArgs e)
            => viewModel.TryResume();

        private void FilterUserList(object sender, FilterEventArgs e)
        {
            if (e.Item is UserListItem item)
            {
                e.Accepted = (viewModel.ShowAway || item.Status != UserStatus.Away) && (viewModel.ShowOffline || item.Status != UserStatus.Offline);
            }
        }

        private void UserListRefreshRequested(object sender, EventArgs e)
        {
            (FindResource("UserListSource") as CollectionViewSource)?.View?.Refresh();
        }
    }
}
