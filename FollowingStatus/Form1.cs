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
using Microsoft.VisualBasic;
using System.Diagnostics;

namespace WhoIsTweeting
{
    public partial class Form1 : Form
    {
        private API api;
        private UserListBox listBox;

        private bool loggedIn = false;
        private bool running = false;
        private bool updating = false;
        private User me;
        private HashSet<string> idSet;
        private List<UserListItem> followings = new List<UserListItem>();

        private bool showAway = Properties.Settings.Default.ShowAway;
        private bool showOffline = Properties.Settings.Default.ShowOffline;

        public Form1()
        {
            InitializeComponent();

            // TODO
            api = new API("", "");
            api.OAuthCallback = "oob";

            api.Token = Properties.Settings.Default.Token;
            api.TokenSecret = Properties.Settings.Default.TokenSecret;

            // Create User ListBox
            listBox = new UserListBox();
            listBox.Margin = new Padding(0);
            listBox.Dock = DockStyle.Fill;
            listBox.BorderStyle = BorderStyle.None;
            mainLayout.Controls.Add(listBox);

            menuItemAway.Checked = showAway;
            menuItemOffline.Checked = showOffline;

            if (api.Token.Equals("") || api.TokenSecret.Equals(""))
            {
                menuItemUser.Text = "Please sign in";
                return;
            }

            Run();
        }

        private void Run(Task _ = null)
        {
            if (!running)
            {
                running = true;
                UpdateMyInfo()
                    .ContinueWith((x) =>
                    {
                        while (true)
                        {
                            UpdateUserList();
                            System.Threading.Thread.Sleep(TimeSpan.FromMinutes(0.5));
                        }
                    }, TaskContinuationOptions.NotOnFaulted);
            }
        }

        private async Task UpdateMyInfo()
        {
            if (mainMenu.InvokeRequired)
                await (Task)mainMenu.Invoke(new Func<Task>(UpdateMyInfo));
            else
            {
                try
                {
                    me = await api.Get<User>("/1.1/account/verify_credentials.json");
                    loggedIn = true;
                    menuItemUser.Text = "@" + me.screen_name;
                    menuItemSignIn.Enabled = false;
                    Properties.Settings.Default.Token = api.Token;
                    Properties.Settings.Default.TokenSecret = api.TokenSecret;
                    Properties.Settings.Default.Save();
                }
                catch (APIException e)
                {
                    MessageBox.Show($"{e.Message}\n\nMessage: {e.Info.errors[0].message} [{e.Info.errors[0].code}]");
                    api.Token = api.TokenSecret = "";
                    throw e;
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
                    CursoredIdStrings ids = await api.Get<CursoredIdStrings>("/1.1/friends/ids.json",
                        new NameValueCollection
                        {
                            { "user_id", me.id_str },
                            { "stringify_id", "true" }
                        });
                    idSet = new HashSet<string>(ids.ids);
                }

                if (updating) return;
                updating = true;

                UserListItem.lastUpdated = DateTime.Now;
                HashSet<string> tmpSet = new HashSet<string>(idSet);
                followings.Clear();
                do
                {
                    HashSet<string> _ = new HashSet<string>(tmpSet.Take(100));
                    tmpSet.ExceptWith(_);
                    string data = string.Join(",", _);
                    Debug.Write("(");
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
                        Debug.Write("*");
                    }
                    Debug.WriteLine(")");
                } while (tmpSet.Count != 0);

                followings.Sort((x, y) => x.MinutesFromLastTweet - y.MinutesFromLastTweet);
                listBox.DataSource = null;
                listBox.DataSource = followings;

                updating = false;
            }
        }
        // end of UpdateUserList()

        private void OnQuitClick(object sender, EventArgs e)
            => Application.Exit();

        private void OnAwayClick(object sender, EventArgs e)
        {
            menuItemAway.Checked = showAway = !showAway;
            Properties.Settings.Default.ShowAway = showAway;
            Properties.Settings.Default.Save();
            if (!showAway)
            {
                List<UserListItem> tmp = new List<UserListItem>(followings);
                foreach (UserListItem c in tmp)
                    if (c.Status == UserStatus.Away)
                        followings.Remove(c);
                listBox.DataSource = null;
                listBox.DataSource = followings;
            }
            else if (loggedIn) UpdateUserList();
        }

        private void OnOfflineClick(object sender, EventArgs e)
        {
            menuItemOffline.Checked = showOffline = !showOffline;
            Properties.Settings.Default.ShowOffline = showOffline;
            Properties.Settings.Default.Save();
            if (!showOffline)
            {
                List<UserListItem> tmp = new List<UserListItem>(followings);
                foreach (UserListItem c in tmp)
                    if (c.Status == UserStatus.Offline)
                        followings.Remove(c);
                listBox.DataSource = null;
                listBox.DataSource = followings;
            }
            else if (loggedIn) UpdateUserList();
        }

        private void OnSignInClick(object sender, EventArgs e)
        {
            if (!loggedIn)
            {
                api.RequestToken((url) =>
                {
                    Process.Start(url);
                    return Interaction.InputBox("Input PIN", "Sign in with Twitter");
                }).ContinueWith(Run);
            }
        }
    }
}
