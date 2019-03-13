using Wit.Core;
using Wit.UI.Core;

namespace Wit.VM
{
    public enum MessageWindowType { MentionWindow, DirectMessageWindow };

    public class MessageViewModel : ViewModelBase
    {
        private UserListItem user;
        private string content;

        public MessageWindowType Type { get; }

        public MessageViewModel(MessageWindowType type, UserListItem user)
        {
            Type = type;
            this.user = user;
        }

        public MessageViewModel(ViewModelFactory vmFactory, IWindowManager winManager, MessageWindowType type, UserListItem user) : this(type, user)
        {
            this.vmFactory = vmFactory;
            this.winManager = winManager;
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
    }
}
