namespace Wit.UI.Core
{
    public interface IMessageBoxHelper
    {
        void ShowError(string title, string message);

        void ShowInfo(string title, string message);

        bool ShowYesNo(string title, string message);

        bool? ShowYesNoCancel(string title, string message);
    }
}
