using System.Windows;
using System.Windows.Controls;
using Wit.Core;

namespace WhoIsTweeting.Views
{
    public partial class ErrorMessageView : UserControl
    {
        public static readonly DependencyProperty AuthStatusProperty =
            DependencyProperty.Register(
                nameof(AuthStatus),
                typeof(AuthStatus),
                typeof(ErrorMessageView),
                new UIPropertyMetadata(AuthStatus.NeedConsumerKey)
            );

        public static readonly DependencyProperty LastErrorProperty =
            DependencyProperty.Register(
                nameof(LastError),
                typeof(TwitterErrorType),
                typeof(ErrorMessageView),
                new UIPropertyMetadata(TwitterErrorType.None)
            );

        public AuthStatus AuthStatus
        {
            get => (AuthStatus)GetValue(AuthStatusProperty);
            set => SetValue(AuthStatusProperty, value);
        }

        public TwitterErrorType LastError
        {
            get => (TwitterErrorType)GetValue(LastErrorProperty);
            set => SetValue(LastErrorProperty, value);
        }

        public ErrorMessageView()
        {
            InitializeComponent();
        }
    }
}
