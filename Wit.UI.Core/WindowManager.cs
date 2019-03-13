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
                win.Activate();
            }
            else
            {
                Window newWin = CreateWindow(viewModel);

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
            if (_windows.TryGetValue(viewModel, out Window win))
            {
                win.Close();
                _windows.Remove(viewModel);
            }
        }

        // TODO: Handle the case when a user closes a window by clicking the system command button.

        private Window CreateWindow(ViewModelBase viewModel)
        {
            return new Window()
            {
                Content = viewModel,
                DataContext = viewModel
            };
        }
    }
}
