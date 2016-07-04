using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PicoBird;
using PicoBird.Objects;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media.Effects;

namespace WhoIsTweeting
{
    enum ApplicationStatus { Initial, NeedConsumerKey, LoginRequired, Ready, Running, Updating };

    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private static BlurEffect blurry = null;

        private MainViewModel viewModel;
        private Properties.Settings appSettings = Properties.Settings.Default;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = viewModel = new MainViewModel(this);

            if (blurry == null)
            {
                blurry = new BlurEffect();
                blurry.Radius = 10.0;
                blurry.KernelType = KernelType.Gaussian;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                viewModel.ShowAway = appSettings.ShowAway;
                viewModel.ShowOffline = appSettings.ShowOffline;
            }));
        }

        #region Main Menu Handler

        private void Menu_OnQuit(object sender, RoutedEventArgs e)
            => Application.Current.Shutdown();

        private void Menu_OnAbout(object sender, RoutedEventArgs e)
        {
            Effect = blurry;
            AboutWindow win = new AboutWindow();
            win.Owner = this;
            win.ShowDialog();
            Effect = null;
        }

        private void Menu_OnConsumer(object sender, RoutedEventArgs e)
        {
            Effect = blurry;
            ConsumerWindow win = new ConsumerWindow();
            TokenViewModel mdl = win.DataContext as TokenViewModel;
            win.Owner = this;
            if ((bool)win.ShowDialog())
                if (!(mdl.ConsumerKey == appSettings.ConsumerKey
                    && mdl.ConsumerSecret == appSettings.ConsumerSecret))
                {
                    appSettings.ConsumerKey = mdl.ConsumerKey;
                    appSettings.ConsumerSecret = mdl.ConsumerSecret;
                    appSettings.Save();
                    //if (listUpdateWorker.IsBusy) listUpdateWorker.CancelAsync();

                    api.ConsumerKey = appSettings.ConsumerKey;
                    api.ConsumerSecret = appSettings.ConsumerSecret;
                    SetStatus(ApplicationStatus.LoginRequired);
                }
            Effect = null;
        }

        private void Menu_OnSignIn(object sender, RoutedEventArgs e)
        {
            MessageBoxResult cont = MessageBox.Show("If you click OK, a web browser will be opened and it will show you the PIN required for authentication.", "Sign in with Twitter", MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if (cont == MessageBoxResult.OK)
            {
                Effect = blurry;
                api.Token = api.TokenSecret = "";
                Task requestTask = api.RequestToken(url =>
                {
                    PinInputWindow win = new PinInputWindow();
                    TokenViewModel mdl = win.DataContext as TokenViewModel;
                    System.Diagnostics.Process.Start(url);
                    win.Owner = this;
                    win.ShowDialog();
                    return mdl.PIN;
                });
                Task onSuccess = requestTask.ContinueWith(async (_) =>
                {
                    if (await ValidateUser())
                    {
                        appSettings.Token = api.Token;
                        appSettings.TokenSecret = api.TokenSecret;
                        appSettings.Save();
                        SetStatus(ApplicationStatus.Ready);
                        Run();
                    }
                    else
                        MessageBox.Show("Unable to retrieve user data.", "Sign in with Twitter", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }, TaskContinuationOptions.NotOnFaulted);
                Task onFailure = requestTask.ContinueWith((_) =>
                {
                    MessageBox.Show("Invalid PIN was provided. Please try again.");
                }, TaskContinuationOptions.OnlyOnFaulted);
                Effect = null;
            }
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
            {
                Task postTask = api.Post("/1.1/statuses/update.json", null, new NameValueCollection
                {
                    { "status", mdl.Content }
                });
                Task onError = postTask.ContinueWith(task =>
                {
                    MessageBox.Show($"Could not send a mention.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
            Effect = null;
        }

        private void Context_OnDirectMessage(object sender, RoutedEventArgs e)
        {
            Effect = blurry;
            MessageWindow win = new MessageWindow(MessageWindowType.DirectMessageWindow, viewModel.SelectedItem);
            MessageViewModel mdl = win.DataContext as MessageViewModel;
            win.Owner = this;
            if ((bool)win.ShowDialog())
            {
                Task postTask = api.Post("/1.1/direct_messages/new.json", null, new NameValueCollection
                {
                    { "screen_name", mdl.ScreenName },
                    { "text", mdl.Content }
                });
                Task onError = postTask.ContinueWith(task =>
                {
                    APIException ex = task.Exception.InnerException as APIException;
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    System.Diagnostics.Debug.WriteLine(ex.Info.errors[0].message);
                    System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                    MessageBox.Show("Could not send a direct message to specied user.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
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

                POINT mousePosition;
                GetCursorPos(out mousePosition);

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
            => SystemCommands.RestoreWindow(this);

        private void CommandBinding_Maximized(object sender, ExecutedRoutedEventArgs e)
            => SystemCommands.MaximizeWindow(this);

        private void CommandBinding_Minimized(object sender, ExecutedRoutedEventArgs e)
            => SystemCommands.MinimizeWindow(this);

        private void ResizeWindow(object sender, MouseButtonEventArgs e)
        {
            int wParam = int.Parse((sender as Grid).Name.Split('_')[1]);
            HwndSource hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
            SendMessage(hwndSource.Handle, 0x112, (IntPtr)wParam, IntPtr.Zero);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X, Y;
            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        private void Menu_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            viewModel.HideBorder = !viewModel.HideBorder;
        }

        #endregion
    }
}
