using System.Windows;
using System.Windows.Input;

namespace WhoIsTweeting
{
    static class HoverHelper
    {
        public static readonly DependencyProperty HoverCommandProperty =
            DependencyProperty.RegisterAttached(
                "HoverCommand",
                typeof(ICommand),
                typeof(HoverHelper),
                new PropertyMetadata(null, HoverCommandChanged)
            );

        public static ICommand GetHoverCommand(DependencyObject obj) => (ICommand)obj.GetValue(HoverCommandProperty);

        public static void SetHoverCommand(DependencyObject obj, ICommand value) => obj.SetValue(HoverCommandProperty, value);

        private static void HoverCommandChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is FrameworkElement element))
            {
                return;
            }

            if (e.OldValue != null)
            {
                element.MouseEnter -= OnMouseEnter;
                element.MouseLeave -= OnMouseLeave;
            }

            if (e.NewValue != null)
            {
                element.MouseEnter += OnMouseEnter;
                element.MouseLeave += OnMouseLeave;
            }
        }

        private static void OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (!(sender is FrameworkElement element))
            {
                return;
            }

            ICommand command = GetHoverCommand(element);
            object param = element.DataContext;

            if (command?.CanExecute(param) ?? false)
            {
                command.Execute(param);
            }
        }

        private static void OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (!(sender is FrameworkElement element))
            {
                return;
            }

            ICommand command = GetHoverCommand(element);

            if (command?.CanExecute(null) ?? false)
            {
                command.Execute(null);
            }
        }
    }
}
