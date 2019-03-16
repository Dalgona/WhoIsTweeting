using System;
using System.Collections.Generic;
using System.Windows;
using Wit.Controls;
using Wit.UI.Core;

namespace WhoIsTweeting
{
    public class WindowManager : IWindowManager
    {
        private readonly Dictionary<ViewModelBase, WindowBase> _windows = new Dictionary<ViewModelBase, WindowBase>();

        public void ShowWindow(ViewModelBase viewModel)
        {
            if (_windows.TryGetValue(viewModel, out WindowBase win))
            {
                System.Diagnostics.Debug.WriteLine($"[WindowManager::ShowWindow] Activating an existing window for {viewModel.GetType().Name}");
                win.Activate();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[WindowManager::ShowWindow] Creating a new window for {viewModel.GetType().Name}");
                WindowBase newWin = CreateWindow(viewModel);

                _windows.Add(viewModel, newWin);
                newWin.Show();
            }
        }

        public void ShowModalWindow(ViewModelBase viewModel, ViewModelBase owner)
        {
            if (_windows.TryGetValue(viewModel, out WindowBase win))
            {
                System.Diagnostics.Debug.WriteLine($"[WindowManager::ShowWindow] Activating an existing modal window for {viewModel.GetType().Name}");
                win.Activate();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[WindowManager::ShowWindow] Creating a new modal window for {viewModel.GetType().Name}");
                WindowBase newWin = CreateWindow(viewModel);

                if (owner != null && _windows.TryGetValue(owner, out WindowBase ownerWin))
                {
                    newWin.Owner = ownerWin;
                    newWin.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }

                _windows.Add(viewModel, newWin);
                newWin.ShowDialog();
            }
        }

        public void CloseWindow(ViewModelBase viewModel)
        {
            System.Diagnostics.Debug.WriteLine($"[WindowManager::CloseWindow] Attempting to close a window for {viewModel.GetType().Name}");

            if (_windows.TryGetValue(viewModel, out WindowBase win))
            {
                win.Close();
            }
        }

        private WindowBase CreateWindow(ViewModelBase viewModel)
        {
            WindowBase newWin = new WindowBase()
            {
                Content = viewModel,
                DataContext = viewModel
            };

            if (viewModel is WindowViewModel wvm)
            {
                newWin.Width = wvm.Width;
                newWin.Height = wvm.Height;
            }

            newWin.Closed += OnWindowClosed;

            return newWin;
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            if (sender is Window win)
            {
                win.Closed -= OnWindowClosed;

                if (win.DataContext is ViewModelBase viewModel)
                {
                    System.Diagnostics.Debug.WriteLine($"[WindowManager::OnWindowClosed] A window for {viewModel.GetType().Name} has closed");
                    _windows.Remove(viewModel);
                }
            }
        }
    }
}
