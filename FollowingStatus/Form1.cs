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

        private bool showAway = true;
        private bool showOffline = true;

        public Form1()
        {
            InitializeComponent();

            // TODO: REMOVE HARDCODED TOKENS
            api = new API("", "");
            api.Token = "";
            api.TokenSecret = "";

            // Create User ListBox
            listBox = new UserListBox();
            listBox.Margin = new Padding(0);
            listBox.Dock = DockStyle.Fill;
            listBox.BorderStyle = BorderStyle.None;
            mainLayout.Controls.Add(listBox);

            if (api.Token.Equals("") || api.TokenSecret.Equals(""))
            {
                menuItemUser.Text = "Please Login";
            }

            menuItemAway.Checked = showAway;
            menuItemOffline.Checked = showOffline;

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
                    menuItemUser.Text = "@" + me.screen_name;
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

                UserListItem.lastUpdated = DateTime.Now;
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
                        UserListItem i = new UserListItem(x.id_str, x.name, x.screen_name, x.status);
                        if (!showAway && i.Status == UserStatus.Away) continue;
                        if (!showOffline && i.Status == UserStatus.Offline) continue;
                        followings.Add(i);
                    }
                } while (tmpSet.Count != 0);

                followings.Sort((x, y) => x.MinutesFromLastTweet - y.MinutesFromLastTweet);
                listBox.DataSource = null;
                listBox.DataSource = followings;
            }
        }
        // end of UpdateUserList()

        private void OnQuitClick(object sender, EventArgs e)
            => Application.Exit();

        private void OnAwayClick(object sender, EventArgs e)
        {
            menuItemAway.Checked = showAway = !showAway;
            if (!showAway)
            {
                List<UserListItem> tmp = new List<UserListItem>(followings);
                foreach (UserListItem c in tmp)
                    if (c.Status == UserStatus.Away)
                        followings.Remove(c);
                listBox.DataSource = null;
                listBox.DataSource = followings;
            }
            else UpdateUserList();
        }

        private void OnOfflineClick(object sender, EventArgs e)
        {
            menuItemOffline.Checked = showOffline = !showOffline;
            if (!showOffline)
            {
                List<UserListItem> tmp = new List<UserListItem>(followings);
                foreach (UserListItem c in tmp)
                    if (c.Status == UserStatus.Offline)
                        followings.Remove(c);
                listBox.DataSource = null;
                listBox.DataSource = followings;
            }
        }
    }
}
