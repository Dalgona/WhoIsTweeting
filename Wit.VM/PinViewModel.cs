using Wit.UI.Core;

namespace Wit.VM
{
    public class PinViewModel : WindowViewModel
    {
        private string _pin;

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

        public PinViewModel() { }

        public PinViewModel(ViewModelFactory vmFactory, IWindowManager winManager) : this()
        {
            this.vmFactory = vmFactory;
            this.winManager = winManager;
        }

        public RelayCommand OkCommand
            => _okCommand ?? (_okCommand = new RelayCommand(() =>
            {
                Result = true;
                winManager.CloseWindow(this);
            }, () => !string.IsNullOrWhiteSpace(Pin)));
    }
}
