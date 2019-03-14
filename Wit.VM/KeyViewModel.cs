using Wit.UI.Core;

namespace Wit.VM
{
    public class KeyViewModel : WindowViewModel
    {
        private string consumerKey = Core.Properties.Settings.Default.ConsumerKey;
        private string consumerSecret = Core.Properties.Settings.Default.ConsumerSecret;

        private RelayCommand _saveCommand;
        private RelayCommand _cancelCommand;

        public KeyViewModel()
        {
            Width = 480;
            Height = 180;
            CanResize = false;
        }

        public KeyViewModel(ViewModelFactory vmFactory, IWindowManager winManager) : this()
        {
            this.vmFactory = vmFactory;
            this.winManager = winManager;
        }

        public bool Result { get; private set; } = false;

        public string ConsumerKey
        {
            get => consumerKey;
            set
            {
                consumerKey = value;
                OnPropertyChanged(nameof(ConsumerKey));
            }
        }

        public string ConsumerSecret
        {
            get => consumerSecret;
            set
            {
                consumerSecret = value;
                OnPropertyChanged(nameof(ConsumerSecret));
            }
        }

        // TODO: Display the validation message.

        public RelayCommand SaveCommand
            => _saveCommand ?? (_saveCommand = new RelayCommand(() =>
            {
                Result = true;
                winManager.CloseWindow(this);
            }, () => !string.IsNullOrWhiteSpace(ConsumerKey) && !string.IsNullOrWhiteSpace(consumerSecret)));

        public RelayCommand CancelCommand
            => _cancelCommand ?? (_cancelCommand = new RelayCommand(() =>
            {
                Result = false;
                winManager.CloseWindow(this);
            }));
    }
}
