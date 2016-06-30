using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace WhoIsTweeting
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
            LinkLabel.Link email = new LinkLabel.Link();
            email.LinkData = "mailto:dalgona@hontou.moe";
            lnkEmail.Links.Add(email);
            LinkLabel.Link website = new LinkLabel.Link();
            website.LinkData = "http://dalgona.github.io";
            lnkWebsite.Links.Add(website);
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

        private void LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            => Process.Start(e.Link.LinkData as string);

        private void OnPbLogoClicked(object sender, System.EventArgs e)
        {
            Process.Start("https://github.com/Dalgona/PicoBird");
        }
    }
}
