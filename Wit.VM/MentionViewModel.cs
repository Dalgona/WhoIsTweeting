using Wit.Core;
using Wit.UI.Core;

namespace Wit.VM
{
    public class MentionViewModel : WindowViewModel
    {
        private UserListItem _user;
        private string _content;
        private RelayCommand _sendCommand;
        private RelayCommand _cancelCommand;

        public bool Result { get; set; } = false;

        public UserListItem User
        {
            get => _user;
            set
            {
                _user = value;
                Content = _user == null ? "" : $"@{User.ScreenName} ";
                OnPropertyChanged(nameof(User));
            }
        }

        public int MaxChars { get; } = 140;

        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged(nameof(Content));
            }
        }

        public RelayCommand SendCommand
            => _sendCommand ?? (_sendCommand = new RelayCommand(() =>
            {
                Result = true;
                WindowManager.CloseWindow(this);
            }, () => !string.IsNullOrWhiteSpace(Content)));

        public RelayCommand CancelCommand
            => _cancelCommand ?? (_cancelCommand = new RelayCommand(() =>
            {
                Result = false;
                WindowManager.CloseWindow(this);
            }));

        public MentionViewModel()
        {
            Width = 400;
            Height = 160;
            MinWidth = 400;
            MinHeight = 160;
            MaxWidth = 400;
        }
    }
}
