using System.ComponentModel;
using System.Windows;

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
                    return Application.Current.FindResource("Message_Header_Mention").ToString();
                else return Application.Current.FindResource("Message_Header_DM").ToString();
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

        public string ScreenName { get { return user.ScreenName; } }

        public string ScreenNameColor
        {
            get
            {
                return user.Status == UserStatus.Online ? "MediumSpringGreen"
                    : (user.Status == UserStatus.Away ? "LightGray" : "Gray");
            }
        }

        public string Content
        {
            get
            {
                return content;
            }
            set
            {
                content = value;
                OnPropertyChanged("Content");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
