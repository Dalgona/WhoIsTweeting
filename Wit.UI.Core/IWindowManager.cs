namespace Wit.UI.Core
{
    public interface IWindowManager
    {
        void ShowWindow(ViewModelBase viewModel);
        void ShowModalWindow(ViewModelBase viewModel, ViewModelBase owner);
        void CloseWindow(ViewModelBase viewModel);
    }
}
