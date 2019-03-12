using System.ComponentModel;
using System.Windows;
using Wit.Core;

namespace WhoIsTweeting
{
    class MessageViewModel : INotifyPropertyChanged
    {
        private MessageWindowType type;
        private UserListItem user;
        private string content;

        public MessageViewModel(MessageWindowType type, UserListItem user)
        {
            this.type = type;
            this.user = user;
        }

        public string WindowHeader
        {
            get
            {
                if (type == MessageWindowType.MentionWindow)
                    return Strings.Message_Header_Mention;
                else return Strings.Message_Header_DM;
            }
        }

        public int MaxChars
        {
            get
            {
                if (type == MessageWindowType.MentionWindow) return 140;
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
