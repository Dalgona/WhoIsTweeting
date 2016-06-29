using System;
using System.Drawing;
using System.Windows.Forms;

namespace WhoIsTweeting
{
    public partial class MentionForm : Form
    {
        public string IdStr { get; private set; }
        public string MentionText { get { return mentionText.Text; } }

        public MentionForm(UserListItem user)
        {
            InitializeComponent();
            IdStr = user.ID;
            lblScreenName.Text = "@" + user.ScreenName;
            lblScreenName.ForeColor = user.Status == UserStatus.Online ? Color.MediumSpringGreen
                : (user.Status == UserStatus.Away ? Color.LightGray : Color.Gray);
            mentionText.Text = $"@{user.ScreenName} ";
            mentionText.SelectionStart = mentionText.Text.Length;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Left = Owner.Left + Owner.Width / 2 - Width / 2;
            Top = Owner.Top + Owner.Height / 2 - Height / 2;
        }
    }
}
