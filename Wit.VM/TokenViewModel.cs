using System.ComponentModel;

namespace Wit.VM
{
    public class TokenViewModel : INotifyPropertyChanged
    {
        private string consumerKey, consumerSecret;
        private string pin;

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

        public string Pin
        {
            get => pin;
            set
            {
                pin = value;
                OnPropertyChanged(nameof(Pin));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
