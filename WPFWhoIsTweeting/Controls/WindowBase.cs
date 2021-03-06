﻿using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using WhoIsTweeting;

namespace Wit.Controls
{
    public class WindowBase : Window
    {
        private static readonly ResizeModeConverter _resizeModeConverter = new ResizeModeConverter();

        public WindowBase() : base()
        {
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindowExecuted, CanExecuteMinimizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, OnMaximizeWindowExecuted, CanExecuteMaximizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, OnRestoreWindowExecuted, CanExecuteRestoreWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseWindowExecuted, CanExecuteCloseWindow));

            if (FindResource("MainWindowStyle") is Style s)
            {
                Style = s;
            }

            SetBinding(TitleProperty, new Binding("Title"));
            SetBinding(WidthProperty, new Binding("Width") { Mode = BindingMode.TwoWay });
            SetBinding(MinWidthProperty, new Binding("MinWidth") { Mode = BindingMode.TwoWay });
            SetBinding(MaxWidthProperty, new Binding("MaxWidth") { Mode = BindingMode.TwoWay });
            SetBinding(HeightProperty, new Binding("Height") { Mode = BindingMode.TwoWay });
            SetBinding(MinHeightProperty, new Binding("MinHeight") { Mode = BindingMode.TwoWay });
            SetBinding(MaxHeightProperty, new Binding("MaxHeight") { Mode = BindingMode.TwoWay });
            SetBinding(ResizeModeProperty, new Binding("CanResize") { Converter = _resizeModeConverter });
            SetBinding(TopmostProperty, new Binding("AlwaysOnTop") { Mode = BindingMode.TwoWay });

            System.Windows.Media.TextOptions.SetTextFormattingMode(this, System.Windows.Media.TextFormattingMode.Display);
            System.Windows.Media.TextOptions.SetTextRenderingMode(this, System.Windows.Media.TextRenderingMode.ClearType);
        }

        protected virtual void OnMinimizeWindowExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        protected virtual void OnMaximizeWindowExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        protected virtual void OnRestoreWindowExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        protected virtual void OnCloseWindowExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        protected virtual void CanExecuteMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e != null)
            {
                e.CanExecute = WindowState != WindowState.Minimized;
            }
        }

        protected virtual void CanExecuteMaximizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e != null)
            {
                e.CanExecute = ResizeMode > ResizeMode.CanMinimize && WindowState != WindowState.Maximized;
            }
        }

        protected virtual void CanExecuteRestoreWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e != null)
            {
                e.CanExecute = ResizeMode > ResizeMode.CanMinimize && WindowState != WindowState.Normal;
            }
        }

        protected virtual void CanExecuteCloseWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e != null)
            {
                e.CanExecute = true;
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            CommandManager.InvalidateRequerySuggested();
            base.OnStateChanged(e);
        }
    }
}
