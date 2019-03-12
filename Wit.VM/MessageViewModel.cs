using System.ComponentModel;
using Wit.Core;

namespace Wit.VM
{
    public enum MessageWindowType { MentionWindow, DirectMessageWindow };

    public class MessageViewModel : INotifyPropertyChanged
    {
        private UserListItem user;
        private string content;

        public MessageWindowType Type { get; }

        public MessageViewModel(MessageWindowType type, UserListItem user)
        {
            Type = type;
            this.user = user;
        }

        public int MaxChars
        {
            get
            {
                if (Type == MessageWindowType.MentionWindow) return 140;
                else return 10000;
            }
        }

        public string ScreenName => user.ScreenName;

        public string ScreenNameColor
            => user.Status == UserStatus.Online ? "MediumSpringGreen"
                : (user.Status == UserStatus.Away ? "LightGray" : "Gray");

        public string Content
        {
            get => content;
            set
            {
                content = value;
                OnPropertyChanged(nameof(Content));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
