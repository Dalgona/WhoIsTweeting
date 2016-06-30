using System;
using System.Drawing;
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
            Rectangle sc = Screen.FromControl(this).Bounds;
            Left = Owner.Left + Owner.Width / 2 - Width / 2;
            Top = Owner.Top + Owner.Height / 2 - Height / 2;
            if (Left < 0) Left = 0;
            if (Top < 0) Top = 0;
            if (sc.Width - Left < Width) Left = sc.Width - Width;
            if (sc.Height - Top < Height) Top = sc.Height - Height;
        }
    }
}
