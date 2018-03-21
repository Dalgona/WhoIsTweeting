using System.ComponentModel;

namespace WhoIsTweeting
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
                OnPropertyChanged("ConsumerKey");
            }
        }

        public string ConsumerSecret
        {
            get => consumerSecret;
            set
            {
                consumerSecret = value;
                OnPropertyChanged("ConsumerSecret");
            }
        }

        public string Pin
        {
            get => pin;
            set
            {
                pin = value;
                OnPropertyChanged("Pin");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
