using System;
using System.Collections.Generic;
using System.Windows;

namespace Wit.UI.Core
{
    public class WindowManager : IWindowManager
    {
        private readonly Dictionary<ViewModelBase, Window> _windows = new Dictionary<ViewModelBase, Window>();

        public void ShowWindow(ViewModelBase viewModel)
        {
            if (_windows.TryGetValue(viewModel, out Window win))
            {
                System.Diagnostics.Debug.WriteLine($"[WindowManager::ShowWindow] Activating an existing window for {viewModel.GetType().Name}");
                win.Activate();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[WindowManager::ShowWindow] Creating a new window for {viewModel.GetType().Name}");
                Window newWin = CreateWindow(viewModel);
                newWin.Closed += OnWindowClosed;

                _windows.Add(viewModel, newWin);
                newWin.Show();
            }
        }

        public void ShowModalWindow(ViewModelBase viewModel)
        {
            CreateWindow(viewModel).ShowDialog();
        }

        public void CloseWindow(ViewModelBase viewModel)
        {
            System.Diagnostics.Debug.WriteLine($"[WindowManager::CloseWindow] Attempting to close a window for {viewModel.GetType().Name}");

            if (_windows.TryGetValue(viewModel, out Window win))
            {
                win.Close();
            }
        }

        private Window CreateWindow(ViewModelBase viewModel)
        {
            return new Window()
            {
                Content = viewModel,
                DataContext = viewModel
            };
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
