using Wit.Core;
using Wit.UI.Core;

namespace Wit.VM
{
    public enum MessageWindowType { MentionWindow, DirectMessageWindow };

    public class MessageViewModel : ViewModelBase
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
            }));

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
        }
    }
}
