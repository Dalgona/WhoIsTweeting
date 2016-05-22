using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PicoBird;
using PicoBird.Objects;
using System.Collections.Specialized;
using System.Drawing.Drawing2D;

namespace WhoIsTweeting
{
    public partial class Form1 : Form
    {
        private API api;
        private UserListBox listBox;

        private User me;
        private HashSet<string> idSet;
        private List<UserListItem> followings = new List<UserListItem>();

        public Form1()
        {
            InitializeComponent();

            // TODO: REMOVE HARDCODED TOKENS
            api = new API("Your Consumer Key", "Your Consumer Key Secret");
            api.Token = "Your OAuth Token";
            api.TokenSecret = "Your OAuth Token Secret";

            listBox = new UserListBox();
            listBox.Margin = new Padding(0);
            listBox.Dock = DockStyle.Fill;
            listBox.BorderStyle = BorderStyle.None;
            mainLayout.Controls.Add(listBox);

            if (api.Token.Equals("") || api.TokenSecret.Equals(""))
            {
                mainMenu_user.Text = "Please Login";
            }

            new Task(() =>
            {
                while (true)
                {
                    UpdateUserList();
                    System.Threading.Thread.Sleep(TimeSpan.FromMinutes(0.5));
                }
            }).Start();
        }

        private async Task UpdateMyInfo()
        {
            if (mainMenu.InvokeRequired)
            {
                mainMenu.Invoke(new Func<Task>(UpdateMyInfo));
            }
            else
            {
                try
                {
                    me = await api.Get<User>("/1.1/account/verify_credentials.json");
                    mainMenu_user.Text = "@" + me.screen_name;
                }
                catch (APIException e)
                {
                    MessageBox.Show(e.Info.errors[0].message);
                }
            }
        }

        private async void UpdateUserList()
        {
            if (listBox.InvokeRequired)
                listBox.Invoke(new Action(UpdateUserList));
            else
            {
                // Populate initial data
                if (idSet == null)
                {
                    await UpdateMyInfo();
                    CursoredIdStrings ids = await api.Get<CursoredIdStrings>("/1.1/friends/ids.json",
                        new NameValueCollection
                        {
                            { "user_id", me.id_str },
                            { "stringify_id", "true" }
                        });
                    idSet = new HashSet<string>(ids.ids);
                }

                HashSet<string> tmpSet = new HashSet<string>(idSet);
                followings.Clear();
                do
                {
                    HashSet<string> _ = new HashSet<string>(tmpSet.Take(100));
                    tmpSet.ExceptWith(_);
                    string data = string.Join(",", _);
                    foreach (var x in await api.Post<User[]>("/1.1/users/lookup.json", null, new NameValueCollection
                    {
                        { "user_id", data },
                        { "include_entities", "true" }
                    }))
                    {
                        followings.Add(new UserListItem(x.id_str, x.name, x.screen_name, x.status));
                    }
                } while (tmpSet.Count != 0);
                UserListItem.lastUpdated = DateTime.Now;

                followings.Sort((x, y) => x.MinutesFromLastTweet - y.MinutesFromLastTweet);
                listBox.DataSource = null;
                listBox.DataSource = followings;
            }
        }
        // end of UpdateUserList()
    }

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

            if (i.MinutesFromLastTweet <= 5)
            {
                e.Graphics.FillEllipse(Brushes.MediumSpringGreen, e.Bounds.Left + 5, e.Bounds.Top + 5, 9, 9);
                itemFont = new Font(itemFontFamily, 9f, FontStyle.Bold);
                itemTextBrush = new SolidBrush(Color.FromArgb(51, 51, 51));
                additionalInfo = "";
            }
            else if (i.MinutesFromLastTweet <= 15)
            {
                e.Graphics.FillEllipse(Brushes.LightGray, e.Bounds.Left + 5, e.Bounds.Top + 5, 9, 9);
                itemFont = new Font(itemFontFamily, 9f, FontStyle.Regular);
                itemTextBrush = new SolidBrush(Color.FromArgb(51, 51, 51));
                additionalInfo = $"({i.MinutesFromLastTweet} minutes ago)";
            }
            else
            {
                e.Graphics.DrawEllipse(Pens.LightGray, e.Bounds.Left + 5, e.Bounds.Top + 5, 9, 9);
                itemFont = new Font(itemFontFamily, 9f, FontStyle.Regular);
                itemTextBrush = new SolidBrush(Color.Gray);
                additionalInfo = $"{i.LastTweet.ToString("(yy/MM/dd HH:mm)")}";
            }
            e.Graphics.DrawString($"{i.Name} (@{i.ScreenName}) {additionalInfo}", itemFont, itemTextBrush, e.Bounds.Left + 20, e.Bounds.Top + 2);
        }
    }

    public class UserListItem
    {
        public string ID { get; private set; }
        public string Name { get; private set; }
        public string ScreenName { get; private set; }
        public DateTime LastTweet { get; private set; }

        public static DateTime lastUpdated;

        public int MinutesFromLastTweet
        {
            get
            {
                return (int)(lastUpdated - LastTweet).TotalMinutes;
            }
        }

        public UserListItem(string id_str, string name, string screenName, Tweet lastTweet)
        {
            ID = id_str;
            Name = name;
            ScreenName = screenName;
            if (lastTweet == null)
                LastTweet = DateTime.FromBinary(0);
            else
                LastTweet = lastTweet.created_at.ToLocalTime();
        }
    }
}
