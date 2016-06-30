using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WhoIsTweeting
{
    public class UserListBox : ListBox
    {
        private static readonly FontFamily fontFamily = new FontFamily("Segoe UI");
        private static readonly Font boldFont = new Font(fontFamily, 9.0f, FontStyle.Bold);
        private static readonly Font regularFont = new Font(fontFamily, 9.0f, FontStyle.Regular);
        private static readonly Font infoFont = new Font(fontFamily, 8.0f, FontStyle.Regular);
        private static readonly SolidBrush blackBrush = new SolidBrush(Color.FromArgb(51, 51, 51));

        public UserListBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            ItemHeight = 20;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Refresh();
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (DataSource == null) return;
            if (e.Index < 0) return;
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                e = new DrawItemEventArgs(e.Graphics, e.Font, e.Bounds, e.Index,
                    e.State ^ DrawItemState.Selected, e.ForeColor, Color.LightGray);

            UserListItem i = Items[e.Index] as UserListItem;

            e.DrawBackground();
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Font itemFont;
            Brush itemTextBrush;
            string additionalInfo;

            switch (i.Status)
            {
                case UserStatus.Online:
                    e.Graphics.FillEllipse(Brushes.MediumSpringGreen, e.Bounds.Left + 5, e.Bounds.Top + 5, 9, 9);
                    itemFont = boldFont;
                    itemTextBrush = blackBrush;
                    additionalInfo = "";
                    break;
                case UserStatus.Away:
                    e.Graphics.FillEllipse(Brushes.LightGray, e.Bounds.Left + 5, e.Bounds.Top + 5, 9, 9);
                    itemFont = regularFont;
                    itemTextBrush = blackBrush;
                    additionalInfo = $"{i.MinutesFromLastTweet} min";
                    break;
                default:
                    e.Graphics.DrawEllipse(Pens.LightGray, e.Bounds.Left + 5, e.Bounds.Top + 5, 9, 9);
                    itemFont = regularFont;
                    itemTextBrush = Brushes.Gray;
                    additionalInfo = i.LastTweet < new DateTime(2002, 1, 1) ?
                        "" : (i.MinutesFromLastTweet <= 1440 ? i.LastTweet.ToString("HH:mm") : i.LastTweet.ToString("yy/MM/dd"));
                    break;
            }
            e.Graphics.DrawString($"@{i.ScreenName}", itemFont, itemTextBrush, e.Bounds.Left + 20, e.Bounds.Top + 2);
            Size infoSize = TextRenderer.MeasureText(e.Graphics, additionalInfo, infoFont);
            e.Graphics.DrawString(additionalInfo, infoFont, Brushes.Gray, e.Bounds.Right - infoSize.Width - 2, e.Bounds.Top + 10 - infoSize.Height / 2.0f);
        }
    }
}
