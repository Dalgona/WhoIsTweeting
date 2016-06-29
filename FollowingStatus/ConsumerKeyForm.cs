using System;
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

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Left = Owner.Left + Owner.Width / 2 - Width / 2;
            Top = Owner.Top + Owner.Height / 2 - Height / 2;
        }
    }
}
