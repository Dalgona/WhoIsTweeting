using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media.Effects;
using Wit.VM;
using Wit.Core;

namespace WhoIsTweeting
{
    public partial class MainWindow : Window
    {
        private static BlurEffect blurry = null;

        private MainViewModel viewModel;
        private Properties.Settings appSettings = Properties.Settings.Default;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = viewModel = new MainViewModel();
            viewModel.RefreshUserList += UserListRefreshRequested;

            if (blurry == null)
            {
                blurry = new BlurEffect
                {
                    Radius = 10.0,
                    KernelType = KernelType.Gaussian
                };
            }
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
            TokenViewModel mdl = win.DataContext as TokenViewModel;
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
                    TokenViewModel mdl = win.DataContext as TokenViewModel;
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
            Effect = blurry;
            MessageWindow win = new MessageWindow(MessageWindowType.MentionWindow, viewModel.SelectedItem);
            MessageViewModel mdl = win.DataContext as MessageViewModel;
            win.Owner = this;
            if ((bool)win.ShowDialog())
                viewModel.PostTweet(mdl.Content, (ex) =>
                {
                    MessageBox.Show(Strings.Message_Error_Mention, Strings.Title_Error,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            Effect = null;
        }

        private void Context_OnDirectMessage(object sender, RoutedEventArgs e)
        {
            Effect = blurry;
            MessageWindow win = new MessageWindow(MessageWindowType.DirectMessageWindow, viewModel.SelectedItem);
            MessageViewModel mdl = win.DataContext as MessageViewModel;
            win.Owner = this;
            if ((bool)win.ShowDialog())
                viewModel.SendDirectMessage(mdl.ScreenName, mdl.Content, (ex) =>
                {
                    MessageBox.Show(Strings.Message_Error_DM, Strings.Title_Error,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            Effect = null;
        }

        private void Context_OnProfile(object sender, RoutedEventArgs e)
            => System.Diagnostics.Process.Start($"https://twitter.com/{viewModel.SelectedItem.ScreenName}");

        #endregion

        #region Custom Window Frame Handler

        private bool restoreIfMove = false;

        private void SwitchWindowState()
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
        }

        private void DockPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                SwitchWindowState();
                return;
            }

            else if (WindowState == WindowState.Maximized)
            {
                restoreIfMove = true;
                return;
            }

            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void DockPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            => restoreIfMove = false;

        private void DockPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (restoreIfMove)
            {
                restoreIfMove = false;

                double percentHorizontal = e.GetPosition(this).X / ActualWidth;
                double targetHorizontal = RestoreBounds.Width * percentHorizontal;
                double percentVertical = e.GetPosition(this).Y / ActualHeight;
                double targetVertical = RestoreBounds.Height * percentVertical;

                WindowState = WindowState.Normal;

                NativeMethods.GetCursorPos(out NativeMethods.Point mousePosition);

                Left = mousePosition.X - targetHorizontal;
                Top = mousePosition.Y - targetVertical;

                DragMove();
            }
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = true;

        private void CommandBinding_Closed(object sender, ExecutedRoutedEventArgs e)
            => SystemCommands.CloseWindow(this);

        private void CommandBinding_Restored(object sender, ExecutedRoutedEventArgs e)
            => SwitchWindowState();

        private void CommandBinding_Maximized(object sender, ExecutedRoutedEventArgs e)
            => SwitchWindowState();

        private void CommandBinding_Minimized(object sender, ExecutedRoutedEventArgs e)
            => SystemCommands.MinimizeWindow(this);

        private void ResizeWindow(object sender, MouseButtonEventArgs e)
        {
            int wParam = int.Parse((sender as Grid).Name.Split('_')[1]);
            HwndSource hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
            NativeMethods.SendMessage(hwndSource.Handle, 0x112, (IntPtr)wParam, IntPtr.Zero);
        }

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

    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int X, Y;
            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }
        }
    }
}
