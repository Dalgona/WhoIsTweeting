using Microsoft.VisualBasic;
using PicoBird;
using PicoBird.Objects;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
 * TODO: Stop updating loop when status goes to NeedConsumerKey or LoginRequired
 * TODO: Add command for pausing the main loop
 */

namespace WhoIsTweeting
{
    enum ApplicationStatus { Initial, NeedConsumerKey, LoginRequired, Ready, Running, Updating };

    public partial class MainForm : Form
    {
        private API api;
        private UserListBox listBox;

        private ApplicationStatus status = ApplicationStatus.Initial;
        private Task mainLoop;

        private User me;
        private HashSet<string> idSet;
        private List<UserListItem> followings = new List<UserListItem>();

        Properties.Settings AppSettings = Properties.Settings.Default;

        private bool showAway;
        private bool showOffline;

        public MainForm()
        {
            InitializeComponent();

            showAway = AppSettings.ShowAway;
            showOffline = AppSettings.ShowOffline;

            // Create User ListBox
            listBox = new UserListBox();
            listBox.Margin = new Padding(0);
            listBox.Dock = DockStyle.Fill;
            listBox.BorderStyle = BorderStyle.None;
            mainLayout.Controls.Add(listBox);

            menuItemAway.Checked = showAway;
            menuItemOffline.Checked = showOffline;

            // TODO
            api = new API(AppSettings.ConsumerKey, AppSettings.ConsumerSecret);
            api.Token = AppSettings.Token;
            api.TokenSecret = AppSettings.TokenSecret;
            api.OAuthCallback = "oob";

            if (api.ConsumerKey == "" || api.ConsumerSecret == "")
            {
                SetStatus(ApplicationStatus.NeedConsumerKey);
                return;
            }

            SetStatus(ApplicationStatus.LoginRequired);
            Task.Factory.StartNew(async () =>
            {
                if (await ValidateUser())
                {
                    SetStatus(ApplicationStatus.Ready);
                    Run();
                }
            });
        }

        private async Task<bool> ValidateUser()
        {
            if (api.Token == "" || api.TokenSecret == "") return false;
            try
            {
                me = await api.Get<User>("/1.1/account/verify_credentials.json");
                return true;
            }
            catch (APIException) { return false; }
        }

        private void SetStatus(ApplicationStatus newStatus)
        {
            if (InvokeRequired)
                Invoke(new Action<ApplicationStatus>(SetStatus), newStatus);
            else
            {
                ApplicationStatus oldStatus = status;

                switch (newStatus)
                {
                    case ApplicationStatus.Initial:
                        if (oldStatus != ApplicationStatus.Initial)
                            throw new Exception("Invalid status change");
                        break;

                    case ApplicationStatus.NeedConsumerKey:
                        menuItemUser.Text = "Consumer Key Required";
                        menuItemSignIn.Enabled = false;
                        break;

                    case ApplicationStatus.LoginRequired:
                        listBox.DataSource = null;
                        listBox.Items.Clear();
                        menuItemUser.Text = "Please sign in";
                        menuItemSignIn.Enabled = true;
                        break;

                    case ApplicationStatus.Ready:
                        if (oldStatus == ApplicationStatus.Initial)
                            throw new Exception("Invalid status change");
                        if (oldStatus == ApplicationStatus.LoginRequired)
                        {
                            menuItemUser.Text = $"@{me.screen_name}";
                            menuItemSignIn.Enabled = false;
                        }
                        else
                        {
                            // from "Running/Updating" status
                            // TODO: put some code that stops the main loop task
                        }
                        break;

                    case ApplicationStatus.Running:
                        if (oldStatus < ApplicationStatus.Ready)
                            throw new Exception("Invalid status change");
                        break;

                    case ApplicationStatus.Updating:
                        if (oldStatus != ApplicationStatus.Running)
                            throw new Exception("Invalid status change");
                        break;
                }
#if DEBUG
                Debug.WriteLine(newStatus);
#endif

                status = newStatus;
            }
        }

        private void Run()
        {
            if (status >= ApplicationStatus.Running) return;
            SetStatus(ApplicationStatus.Running);

            // TODO: implement task cancellation.
            if (mainLoop == null)
            {
                mainLoop = new Task(() =>
                {
                    while (true)
                    {
                        UpdateUserList();
                        System.Threading.Thread.Sleep(TimeSpan.FromMinutes(0.5));
                    }
                });
            }
            mainLoop.Start();
        }

        private async void UpdateUserList()
        {
            if (InvokeRequired)
                Invoke(new Action(UpdateUserList));
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

                if (status == ApplicationStatus.Updating) return;
                SetStatus(ApplicationStatus.Updating);

                int nAway = 0;
                int nOffline = 0;
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
                        if (i.Status == UserStatus.Away) { ++nAway; if (!showAway) continue; }
                        if (i.Status == UserStatus.Offline) { ++nOffline; if (!showOffline) continue; }
                        followings.Add(i);
                    }
                } while (tmpSet.Count != 0);

                statAway.Text = nAway.ToString();
                statOffline.Text = nOffline.ToString();
                statOnline.Text = (idSet.Count - nAway - nOffline).ToString();

                followings.Sort((x, y) => x.MinutesFromLastTweet - y.MinutesFromLastTweet);
                listBox.DataSource = null;
                listBox.DataSource = followings;

                SetStatus(ApplicationStatus.Running);
            }
        }
        // end of UpdateUserList()

        private void OnQuitClick(object sender, EventArgs e)
            => Application.Exit();

        private void OnAwayClick(object sender, EventArgs e)
        {
            menuItemAway.Checked = showAway = !showAway;
            AppSettings.ShowAway = showAway;
            AppSettings.Save();
            if (!showAway)
            {
                List<UserListItem> tmp = new List<UserListItem>(followings);
                foreach (UserListItem c in tmp)
                    if (c.Status == UserStatus.Away)
                        followings.Remove(c);
                listBox.DataSource = null;
                listBox.DataSource = followings;
            }
            else if (status >= ApplicationStatus.Ready) UpdateUserList();
        }

        private void OnOfflineClick(object sender, EventArgs e)
        {
            menuItemOffline.Checked = showOffline = !showOffline;
            AppSettings.ShowOffline = showOffline;
            AppSettings.Save();
            if (!showOffline)
            {
                List<UserListItem> tmp = new List<UserListItem>(followings);
                foreach (UserListItem c in tmp)
                    if (c.Status == UserStatus.Offline)
                        followings.Remove(c);
                listBox.DataSource = null;
                listBox.DataSource = followings;
            }
            else if (status >= ApplicationStatus.Ready) UpdateUserList();
        }

        private void OnConsumerClick(object sender, EventArgs e)
        {
            ConsumerKeyForm form = new ConsumerKeyForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                AppSettings.ConsumerKey = form.ConsumerKey;
                AppSettings.ConsumerSecret = form.ConsumerSecret;
                AppSettings.Save();
                if (form.ConsumerKey == "" || form.ConsumerSecret == "")
                {
                    MessageBox.Show("Both of two fields cannot be left blank.\n"
                        + "You will not be able to use this program before you\n"
                        + "provide valid consumer token and consumer secret.",
                        "Invalid Consumer Key", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    SetStatus(ApplicationStatus.NeedConsumerKey);
                }
                else
                {
                    api = new API(AppSettings.ConsumerKey, AppSettings.ConsumerSecret);
                    api.OAuthCallback = "oob";
                    SetStatus(ApplicationStatus.LoginRequired);
                }
            }
        }

        private void OnSignInClick(object sender, EventArgs e)
        {
            api.Token = "";
            api.TokenSecret = "";
            Task requestTask = api.RequestToken((url) =>
            {
                Process.Start(url);
                return Interaction.InputBox("Input PIN", "Sign in with Twitter");
            });
            Task onSuccess = requestTask.ContinueWith(async (_) =>
            {
                if (await ValidateUser())
                {
                    AppSettings.Token = api.Token;
                    AppSettings.TokenSecret = api.TokenSecret;
                    AppSettings.Save();
                    SetStatus(ApplicationStatus.Ready);
                    Run();
                }
                else
                    MessageBox.Show("Unable to retrieve user data.");
            }, TaskContinuationOptions.NotOnFaulted);
            Task onFailure = requestTask.ContinueWith((_) =>
            {
                MessageBox.Show("Invalid PIN was provided. Please try again.");
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
