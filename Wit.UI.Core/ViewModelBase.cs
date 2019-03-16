using System.ComponentModel;

namespace Wit.UI.Core
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public IWindowManager WindowManager { get; set; }
        public IStringProvider StringProvider { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
