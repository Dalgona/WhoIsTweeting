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

namespace WPFWhoIsTweeting
{
    enum ApplicationStatus { Initial, NeedConsumerKey, LoginRequired, Ready, Running, Updating };

    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private static BlurEffect blurry = null;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private API api;
        private ApplicationStatus status = ApplicationStatus.Initial;

        private User me;
        private HashSet<string> idSet;
        private List<UserListItem> UserList = new List<UserListItem>();

        private BackgroundWorker listUpdateWorker;

        MainViewModel viewModel;

        Properties.Settings AppSettings = Properties.Settings.Default;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = viewModel = new MainViewModel(this);

            listUpdateWorker = new BackgroundWorker();
            listUpdateWorker.DoWork += listUpdateWorker_DoWork;

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
                viewModel.ShowAway = AppSettings.ShowAway;
                viewModel.ShowOffline = AppSettings.ShowOffline;
            }));

            api = new API(AppSettings.ConsumerKey, AppSettings.ConsumerSecret);
            api.Token = AppSettings.Token;
            api.TokenSecret = AppSettings.TokenSecret;
            api.OAuthCallback = "oob";

            if (api.ConsumerKey == "" || api.ConsumerSecret == "")
            {
                SetStatus(ApplicationStatus.NeedConsumerKey);
                return;
            }

            SetStatus(ApplicationStatus.LoginRequired);
            Task.Factory.StartNew(async () =>
            {
                if (await ValidateUser())
                {
                    SetStatus(ApplicationStatus.Ready);
                    Run();
                }
            });
        }

        private async Task<bool> ValidateUser()
        {
            if (api.Token == "" || api.TokenSecret == "") return false;
            try
            {
                me = await api.Get<User>("/1.1/account/verify_credentials.json");
                return true;
            }
            catch (APIException) { return false; }
        }

        private void SetStatus(ApplicationStatus newStatus)
        {
            ApplicationStatus oldStatus = status;
            Dispatcher.Invoke(new Action(() =>
                {
                    switch (newStatus)
                    {
                        case ApplicationStatus.Initial:
                            if (oldStatus != ApplicationStatus.Initial)
                                throw new Exception("Invalid status change");
                            break;

                        case ApplicationStatus.NeedConsumerKey:
                            viewModel.UserMenuText = "Consumer Key Required";
                            menuItemSignIn.IsEnabled = false;
                            break;

                        case ApplicationStatus.LoginRequired:
                            viewModel.UserMenuText = "Please sign in";
                            menuItemSignIn.IsEnabled = true;
                            break;

                        case ApplicationStatus.Ready:
                            if (oldStatus == ApplicationStatus.Initial)
                                throw new Exception("Invalid status change");
                            if (oldStatus == ApplicationStatus.LoginRequired)
                            {
                                viewModel.UserMenuText = $"@{me.screen_name}";
                                menuItemSignIn.IsEnabled = false;
                            }
                            else if (listUpdateWorker.IsBusy)
                                listUpdateWorker.CancelAsync();
                            break;

                        case ApplicationStatus.Running:
                            if (oldStatus < ApplicationStatus.Ready)
                                throw new Exception("Invalid status change");
                            viewModel.StatUpdating = false;
                            break;

                        case ApplicationStatus.Updating:
                            if (oldStatus != ApplicationStatus.Running)
                                throw new Exception("Invalid status change");
                            viewModel.StatUpdating = true;
                            break;
                    }
                }));
            status = newStatus;
        }

        private void Run()
        {
            if (status >= ApplicationStatus.Running) return;
            SetStatus(ApplicationStatus.Running);
            listUpdateWorker.RunWorkerAsync();
        }

        public async void UpdateUserList()
        {
            if (status < ApplicationStatus.Ready) return;

            // Populate initial data
            if (idSet == null)
            {
                CursoredIdStrings ids = await api.Get<CursoredIdStrings>("/1.1/friends/ids.json",
                    new NameValueCollection
                    {
                        { "user_id", me.id_str },
                        { "stringify_id", "true" }
                    });
                idSet = new HashSet<string>(ids.ids);
            }

            if (status == ApplicationStatus.Updating) return;
            SetStatus(ApplicationStatus.Updating);

            int nAway, nOffline;
            UserListItem.lastUpdated = DateTime.Now;
            HashSet<string> tmpSet = new HashSet<string>(idSet);
            List<UserListItem> list = new List<UserListItem>();

            do
            {
                HashSet<string> _ = new HashSet<string>(tmpSet.Take(100));
                tmpSet.ExceptWith(_);
                string data = string.Join(",", _);
                List<User> tmp = api.Post<List<User>>("/1.1/users/lookup.json", null, new NameValueCollection
                    {
                        { "user_id", data },
                        { "include_entities", "true" }
                    }).Result;
                foreach (var x in tmp) list.Add(new UserListItem(x.id_str, x.name, x.screen_name, x.status));
            } while (tmpSet.Count != 0);

            nAway = list.Count(x => x.Status == UserStatus.Away);
            nOffline = list.Count(x => x.Status == UserStatus.Offline);

            viewModel.StatAway = nAway;
            viewModel.StatOffline = nOffline;
            viewModel.StatOnline = (idSet.Count - nAway - nOffline);

            list.Sort((x, y) => x.MinutesFromLastTweet - y.MinutesFromLastTweet);
            lock (viewModel.userListLock)
            {
                viewModel.UserList.Clear();
                foreach (var x in list) viewModel.UserList.Add(x);
            }

            SetStatus(ApplicationStatus.Running);
        }

        private void listUpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            while (true)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                UpdateUserList();
                System.Threading.Thread.Sleep(TimeSpan.FromMinutes(0.5));
            }
        }

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

        #endregion

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
                if (!(mdl.ConsumerKey == AppSettings.ConsumerKey
                    && mdl.ConsumerSecret == AppSettings.ConsumerSecret))
                {
                    AppSettings.ConsumerKey = mdl.ConsumerKey;
                    AppSettings.ConsumerSecret = mdl.ConsumerSecret;
                    AppSettings.Save();
                    if (listUpdateWorker.IsBusy) listUpdateWorker.CancelAsync();

                    api = new API(AppSettings.ConsumerKey, AppSettings.ConsumerSecret);
                    api.OAuthCallback = "oob";
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
                        AppSettings.Token = api.Token;
                        AppSettings.TokenSecret = api.TokenSecret;
                        AppSettings.Save();
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

        private void Menu_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            viewModel.HideBorder = !viewModel.HideBorder;
        }
    }
}
