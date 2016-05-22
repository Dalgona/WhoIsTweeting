using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WhoIsTweeting
{
    public class UserListBox : ListBox
    {
        public UserListBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            ItemHeight = 20;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                e = new DrawItemEventArgs(e.Graphics, e.Font, e.Bounds, e.Index,
                    e.State ^ DrawItemState.Selected, e.ForeColor, Color.FromArgb(240, 240, 240));

            UserListItem i = Items[e.Index] as UserListItem;

            e.DrawBackground();
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            FontFamily itemFontFamily = new FontFamily("Segoe UI");
            Font itemFont;
            SolidBrush itemTextBrush;
            string additionalInfo;

            switch (i.Status)
            {
                case UserStatus.Online:
                    e.Graphics.FillEllipse(Brushes.MediumSpringGreen, e.Bounds.Left + 5, e.Bounds.Top + 5, 9, 9);
                    itemFont = new Font(itemFontFamily, 9f, FontStyle.Bold);
                    itemTextBrush = new SolidBrush(Color.FromArgb(51, 51, 51));
                    additionalInfo = "";
                    break;
                case UserStatus.Away:
                    e.Graphics.FillEllipse(Brushes.LightGray, e.Bounds.Left + 5, e.Bounds.Top + 5, 9, 9);
                    itemFont = new Font(itemFontFamily, 9f, FontStyle.Regular);
                    itemTextBrush = new SolidBrush(Color.FromArgb(51, 51, 51));
                    additionalInfo = $"({i.MinutesFromLastTweet} minutes ago)";
                    break;
                default:
                    e.Graphics.DrawEllipse(Pens.LightGray, e.Bounds.Left + 5, e.Bounds.Top + 5, 9, 9);
                    itemFont = new Font(itemFontFamily, 9f, FontStyle.Regular);
                    itemTextBrush = new SolidBrush(Color.Gray);
                    additionalInfo = $"{i.LastTweet.ToString("(yy/MM/dd HH:mm)")}";
                    break;
            }
            e.Graphics.DrawString($"{i.Name} (@{i.ScreenName}) {additionalInfo}", itemFont, itemTextBrush, e.Bounds.Left + 20, e.Bounds.Top + 2);
        }
    }
}
