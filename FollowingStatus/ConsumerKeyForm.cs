using System.Windows.Forms;

namespace WhoIsTweeting
{
    public partial class ConsumerKeyForm : Form
    {
        public string ConsumerKey { get { return txtConsumerKey.Text; } }
        public string ConsumerSecret { get { return txtConsumerSecret.Text; } }

        public ConsumerKeyForm()
        {
            InitializeComponent();
            txtConsumerKey.Text = Properties.Settings.Default.ConsumerKey;
            txtConsumerSecret.Text = Properties.Settings.Default.ConsumerSecret;
        }
    }
}
