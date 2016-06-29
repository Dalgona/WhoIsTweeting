using Microsoft.VisualBasic;
using PicoBird;
using PicoBird.Objects;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
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

        private ApplicationStatus status = ApplicationStatus.Initial;

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

            menuItemAway.Checked = showAway;
            menuItemOffline.Checked = showOffline;

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
                        else if (listUpdateWorker.IsBusy)
                            listUpdateWorker.CancelAsync();
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

                status = newStatus;
            }
        }

        private void Run()
        {
            if (status >= ApplicationStatus.Running) return;
            SetStatus(ApplicationStatus.Running);

            listUpdateWorker.RunWorkerAsync();
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
                statUpdating.Text = "Updating";

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

                statAway.Text = nAway.ToString() + " away";
                statOffline.Text = nOffline.ToString() + " offline";
                statOnline.Text = (idSet.Count - nAway - nOffline).ToString() + " online";

                followings.Sort((x, y) => x.MinutesFromLastTweet - y.MinutesFromLastTweet);
                listBox.DataSource = null;
                listBox.DataSource = followings;

                SetStatus(ApplicationStatus.Running);
                statUpdating.Text = "";
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
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                AppSettings.ConsumerKey = form.ConsumerKey;
                AppSettings.ConsumerSecret = form.ConsumerSecret;
                AppSettings.Save();
                if (listUpdateWorker.IsBusy) listUpdateWorker.CancelAsync();
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

        private void listBox_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                int idx = listBox.IndexFromPoint(e.Location);
                if (e.Button == MouseButtons.Right)
                {
                    if (idx >= 0)
                        listBox.SelectedIndex = listBox.IndexFromPoint(e.Location);
                    UserListItem item = listBox.SelectedItem as UserListItem;
                    ctxItemNickname.Text = item.Name;
                    ctxItemID.Text = $"(@{item.ScreenName})";
                }
            }
            catch (Exception) { }
        }

        private void OnCtxOpenProfileClick(object sender, EventArgs e)
        {
            Process.Start($"https://twitter.com/{(listBox.SelectedItem as UserListItem).ScreenName}");
        }

        private async void OnCtxMentionClick(object sender, EventArgs e)
        {
            MentionForm form = new MentionForm(listBox.SelectedItem as UserListItem);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                await api.Post("/1.1/statuses/update.json", new NameValueCollection
                {
                    { "status", form.MentionText }
                });
            }
        }

        private void listUpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            while (true)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                UpdateUserList();
                System.Threading.Thread.Sleep(TimeSpan.FromMinutes(0.5));
            }
        }

        private void OnAboutClick(object sender, EventArgs e)
        {
            new AboutForm().ShowDialog(this);
        }
    }
}
