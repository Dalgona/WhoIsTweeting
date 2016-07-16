using System.ComponentModel;

namespace WhoIsTweeting
{
    public class TokenViewModel : INotifyPropertyChanged
    {
        private string consumerKey, consumerSecret;
        private string pin;

        public TokenViewModel() { }

        public string ConsumerKey
        {
            get
            {
                return consumerKey;
            }
            set
            {
                consumerKey = value;
                OnPropertyChanged("ConsumerKey");
            }
        }

        public string ConsumerSecret
        {
            get
            {
                return consumerSecret;
            }
            set
            {
                consumerSecret = value;
                OnPropertyChanged("ConsumerSecret");
            }
        }

        public string PIN
        {
            get
            {
                return pin;
            }
            set
            {
                pin = value;
                OnPropertyChanged("PIN");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
