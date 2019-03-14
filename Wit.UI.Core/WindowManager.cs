using System;
using System.Collections.Generic;
using System.Windows;

namespace Wit.UI.Core
{
    public class WindowManager<TWindow> : IWindowManager where TWindow : Window, new()
    {
        private readonly Dictionary<ViewModelBase, TWindow> _windows = new Dictionary<ViewModelBase, TWindow>();

        public void ShowWindow(ViewModelBase viewModel)
        {
            if (_windows.TryGetValue(viewModel, out TWindow win))
            {
                System.Diagnostics.Debug.WriteLine($"[WindowManager::ShowWindow] Activating an existing window for {viewModel.GetType().Name}");
                win.Activate();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[WindowManager::ShowWindow] Creating a new window for {viewModel.GetType().Name}");
                TWindow newWin = CreateWindow(viewModel);

                _windows.Add(viewModel, newWin);
                newWin.Show();
            }
        }

        public void ShowModalWindow(ViewModelBase viewModel)
        {
            if (_windows.TryGetValue(viewModel, out TWindow win))
            {
                System.Diagnostics.Debug.WriteLine($"[WindowManager::ShowWindow] Activating an existing modal window for {viewModel.GetType().Name}");
                win.Activate();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[WindowManager::ShowWindow] Creating a new modal window for {viewModel.GetType().Name}");
                TWindow newWin = CreateWindow(viewModel);

                _windows.Add(viewModel, newWin);
                newWin.ShowDialog();
            }
        }

        public void CloseWindow(ViewModelBase viewModel)
        {
            System.Diagnostics.Debug.WriteLine($"[WindowManager::CloseWindow] Attempting to close a window for {viewModel.GetType().Name}");

            if (_windows.TryGetValue(viewModel, out TWindow win))
            {
                win.Close();
            }
        }

        private TWindow CreateWindow(ViewModelBase viewModel)
        {
            TWindow newWin = new TWindow()
            {
                Content = viewModel,
                DataContext = viewModel
            };

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
