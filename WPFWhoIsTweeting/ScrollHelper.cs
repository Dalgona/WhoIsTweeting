using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WhoIsTweeting
{
    static class ScrollHelper
    {
        public enum AutoScrollDirection
        {
            Top,
            Bottom,
            LeftEnd,
            RightEnd
        }

        public static readonly DependencyProperty ShiftWheelHorizontalScrollProperty =
            DependencyProperty.RegisterAttached(
                "ShiftWheelHorizontalScroll",
                typeof(bool),
                typeof(ScrollHelper),
                new UIPropertyMetadata(false, ShiftWheelHorizontalScrollPropertyChanged)
            );

        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached(
                "AutoScroll",
                typeof(bool),
                typeof(ScrollHelper),
                new UIPropertyMetadata(false, AutoScrollPropertyChanged)
            );

        public static readonly DependencyProperty AutoScrollDirectionProperty =
            DependencyProperty.RegisterAttached(
                "AutoScrollDirection",
                typeof(AutoScrollDirection),
                typeof(ScrollHelper),
                new UIPropertyMetadata(AutoScrollDirection.Top)
            );

        public static bool GetShiftWheelHorizontalScroll(DependencyObject d) => (bool)d.GetValue(ShiftWheelHorizontalScrollProperty);
        public static bool GetAutoScroll(DependencyObject d) => (bool)d.GetValue(AutoScrollProperty);
        public static AutoScrollDirection GetAutoScrollDirection(DependencyObject d) => (AutoScrollDirection)d.GetValue(AutoScrollDirectionProperty);

        public static void SetShiftWheelHorizontalScroll(DependencyObject d, bool value) => d.SetValue(ShiftWheelHorizontalScrollProperty, value);
        public static void SetAutoScroll(DependencyObject d, bool value) => d.SetValue(AutoScrollProperty, value);
        public static void SetAutoScrollDirection(DependencyObject d, AutoScrollDirection value) => d.SetValue(AutoScrollDirectionProperty, value);

        private static void ShiftWheelHorizontalScrollPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer sv)
            {
                if ((bool)e.NewValue)
                {
                    sv.PreviewMouseWheel += OnMouseWheel;
                }
                else
                {
                    sv.PreviewMouseWheel -= OnMouseWheel;
                }
            }
        }

        private static void AutoScrollPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer sv)
            {
                if ((bool)e.NewValue)
                {
                    sv.ScrollChanged += OnScrollChanged;
                }
                else
                {
                    sv.ScrollChanged -= OnScrollChanged;
                }
            }
        }

        private static void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer sv && Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (e.Delta < 0)
                {
                    sv.LineRight();
                }
                else
                {
                    sv.LineLeft();
                }

                e.Handled = true;
            }
        }

        private static void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is ScrollViewer sv && (e.ExtentWidthChange != 0 || e.ExtentHeightChange != 0))
            {
                switch (GetAutoScrollDirection(sv))
                {
                    case AutoScrollDirection.Top:
                        sv.ScrollToTop();
                        break;

                    case AutoScrollDirection.Bottom:
                        sv.ScrollToBottom();
                        break;

                    case AutoScrollDirection.LeftEnd:
                        sv.ScrollToLeftEnd();
                        break;

                    case AutoScrollDirection.RightEnd:
                        sv.ScrollToRightEnd();
                        break;
                }

                e.Handled = true;
            }
        }
    }
}
