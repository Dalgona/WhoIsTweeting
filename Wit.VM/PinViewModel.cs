using Wit.UI.Core;

namespace Wit.VM
{
    public class PinViewModel : WindowViewModel
    {
        private string _pin = "";

        private RelayCommand _okCommand;

        public bool Result { get; private set; } = false;

        public string Pin
        {
            get => _pin;
            set
            {
                _pin = value;
                OnPropertyChanged(nameof(Pin));
            }
        }

        public PinViewModel()
        {
            Width = 300;
            Height = 130;
            CanResize = false;
        }

        public RelayCommand OkCommand
            => _okCommand ?? (_okCommand = new RelayCommand(() =>
            {
                Result = true;
                WindowManager.CloseWindow(this);
            }, () => !string.IsNullOrWhiteSpace(Pin)));
    }
}
