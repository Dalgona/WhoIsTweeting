using System.ComponentModel;

namespace Wit.UI.Core
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        protected ViewModelFactory vmFactory;
        protected IWindowManager winManager;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
