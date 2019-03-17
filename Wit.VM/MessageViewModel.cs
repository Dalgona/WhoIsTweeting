using Wit.Core;
using Wit.UI.Core;

namespace Wit.VM
{
    public enum MessageWindowType { MentionWindow, DirectMessageWindow };

    public class MessageViewModel : WindowViewModel
    {
        private string _content;
        private RelayCommand _sendCommand;
        private RelayCommand _cancelCommand;

        public MessageWindowType Type { get; }
        public UserListItem User { get; }
        public bool Result { get; set; } = false;

        public int MaxChars
        {
            get
            {
                if (Type == MessageWindowType.MentionWindow) return 140;
                else return 10000;
            }
        }

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
            }, () => string.IsNullOrWhiteSpace(Content)));

        public RelayCommand CancelCommand
            => _cancelCommand ?? (_cancelCommand = new RelayCommand(() =>
            {
                Result = false;
                WindowManager.CloseWindow(this);
            }));

        public MessageViewModel(MessageWindowType type, UserListItem user)
        {
            Type = type;
            User = user;

            if (type == MessageWindowType.MentionWindow)
            {
                Content = $"@{user.ScreenName} ";
            }

            Width = 400;
            Height = 160;
            MinWidth = 400;
            MinHeight = 160;
            MaxWidth = 400;
        }
    }
}
