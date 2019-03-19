using System;
using System.Collections.Generic;
using System.ComponentModel;
using Wit.Core;
using Wit.UI.Core;

namespace Wit.VM
{
    public class MainViewModel : WindowViewModel
    {
        #region Fields

        private readonly MainService _service = MainService.Instance;
        private UserListItem _selectedItem;
        private ViewModelBase _statViewModel;

        private RelayCommand _openStatCommand;
        private RelayCommand _openKeyCommand;
        private RelayCommand _signInCommand;
        private RelayCommand _openIntervalCommand;
        private RelayCommand _openAboutCommand;
        private RelayCommand _openMentionCommand;
        private RelayCommand _openProfileCommand;
        private RelayCommand _quitCommand;

        #endregion

        public event EventHandler RefreshUserList;

        public MainViewModel()
        {
            _service.PropertyChanged += Service_PropertyChanged;

            Title = "WhoIsTweeting";
            Width = 300;
            Height = 600;
            MinWidth = 300;
            MinHeight = 400;
        }

        #region Command Properties

        public RelayCommand OpenStatCommand
            => _openStatCommand ?? (_openStatCommand = new RelayCommand(() =>
            {
                if (_statViewModel == null)
                {
                    _statViewModel = DepsInjector.Default.Create<StatViewModel>();
                }

                WindowManager.ShowWindow(_statViewModel);
            }));

        public RelayCommand OpenKeyCommand
            => _openKeyCommand ?? (_openKeyCommand = new RelayCommand(() =>
            {
                KeyViewModel vm = DepsInjector.Default.Create<KeyViewModel>();
                Core.Properties.Settings coreSettings = Core.Properties.Settings.Default;

                WindowManager.ShowModalWindow(vm, this);

                if (vm.Result && (vm.ConsumerKey != coreSettings.ConsumerKey || vm.ConsumerSecret != coreSettings.ConsumerSecret))
                {
                    _service.SetConsumerKey(vm.ConsumerKey, vm.ConsumerSecret);
                }
            }));

        public RelayCommand SignInCommand
            => _signInCommand ?? (_signInCommand = new RelayCommand(() =>
            {
                string confirmTitle = StringProvider.GetString("SignIn_Title");
                string confirmMessage = StringProvider.GetString("SignIn_Confirm");

                MessageBoxHelper.ShowInfo(confirmTitle, confirmMessage);

                _service.SignIn(url =>
                {
                    PinViewModel vm = DepsInjector.Default.Create<PinViewModel>();

                    System.Diagnostics.Process.Start(url);
                    WindowManager.ShowModalWindow(vm, this);

                    return vm.Pin;
                }, ex =>
                {
                    string errTitle = StringProvider.GetString("Title_Error");
                    string errMessage = StringProvider.GetString("SignIn_Error");

                    MessageBoxHelper.ShowError(errTitle, errMessage);
                });
            }, () => _service.AuthStatus == AuthStatus.NeedSignIn || _service.AuthStatus == AuthStatus.Error));

        public RelayCommand OpenIntervalCommand
            => _openIntervalCommand ?? (_openIntervalCommand = new RelayCommand(() =>
            {
                IntervalViewModel vm = DepsInjector.Default.Create<IntervalViewModel>();

                WindowManager.ShowModalWindow(vm, this);
            }));

        public RelayCommand OpenAboutCommand
            => _openAboutCommand ?? (_openAboutCommand = new RelayCommand(() =>
            {
                AboutViewModel vm = DepsInjector.Default.Create<AboutViewModel>();

                WindowManager.ShowModalWindow(vm, this);
            }));

        public RelayCommand OpenMentionCommand
            => _openMentionCommand ?? (_openMentionCommand = new RelayCommand(param =>
            {
                MentionViewModel vm = DepsInjector.Default.Create<MentionViewModel>();
                vm.User = param as UserListItem;

                WindowManager.ShowModalWindow(vm, this);

                if (vm.Result)
                {
                    _service.PostTweet(vm.Content, ex =>
                    {
                        string errTitle = StringProvider.GetString("Title_Error");
                        string errMessage = StringProvider.GetString("Message_Error_Mention");

                        MessageBoxHelper.ShowError(errTitle, errMessage);
                    });
                }
            }));

        public RelayCommand OpenProfileCommand
            => _openProfileCommand ?? (_openProfileCommand = new RelayCommand(param =>
            {
                if (param is UserListItem user)
                {
                    System.Diagnostics.Process.Start($"https://twitter.com/{user.ScreenName}");
                }
            }));

        public RelayCommand QuitCommand
            => _quitCommand ?? (_quitCommand = new RelayCommand(() => WindowManager.CloseWindow(this)));

        #endregion

        #region Properties

        public AuthStatus AuthStatus => _service.AuthStatus;
        public UserListItem Me => _service.Me;
        public IEnumerable<UserListItem> UserList => _service.UserList;

        public int OnlineCount => _service.OnlineCount;
        public int AwayCount => _service.AwayCount;
        public int OfflineCount => _service.OfflineCount;

        public bool IsUpdating => _service.IsUpdating;
        public TwitterErrorType LastError => _service.LastError;

        //public bool IsErrorSet => _service.State < 0;

        public UserListItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        public override bool AlwaysOnTop
        {
            get => Properties.Settings.Default.AlwaysOnTop;
            set
            {
                Properties.Settings.Default.AlwaysOnTop = value;
                OnPropertyChanged(nameof(AlwaysOnTop));
            }
        }

        public bool ShowAway
        {
            get => Properties.Settings.Default.ShowAway;
            set
            {
                Properties.Settings.Default.ShowAway = value;
                OnPropertyChanged(nameof(ShowAway));
                RefreshUserList?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool ShowOffline
        {
            get => Properties.Settings.Default.ShowOffline;
            set
            {
                Properties.Settings.Default.ShowOffline = value;
                OnPropertyChanged(nameof(ShowOffline));
                RefreshUserList?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion

        private void Service_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }
    }
}
