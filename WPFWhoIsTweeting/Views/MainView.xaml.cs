﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Wit.Core;
using Wit.VM;

namespace WhoIsTweeting.Views
{
    public partial class MainView : UserControl
    {
        private MainViewModel viewModel;

        public MainView()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == DataContextProperty)
            {
                if (viewModel != null)
                {
                    viewModel.RefreshUserList -= UserListRefreshRequested;
                }

                viewModel = e.NewValue as MainViewModel;

                if (viewModel != null)
                {
                    viewModel.RefreshUserList += UserListRefreshRequested;
                }
            }

            base.OnPropertyChanged(e);
        }

        #region Context Menu Handler

        private void Context_OnMention(object sender, RoutedEventArgs e)
        {
            MessageWindow win = new MessageWindow(MessageWindowType.MentionWindow, viewModel.SelectedItem);
            MessageViewModel mdl = win.DataContext as MessageViewModel;
            // win.Owner = this;
            if ((bool)win.ShowDialog())
                viewModel.PostTweet(mdl.Content, (ex) =>
                {
                    MessageBox.Show(Strings.Message_Error_Mention, Strings.Title_Error,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
        }

        private void Context_OnDirectMessage(object sender, RoutedEventArgs e)
        {
            MessageWindow win = new MessageWindow(MessageWindowType.DirectMessageWindow, viewModel.SelectedItem);
            MessageViewModel mdl = win.DataContext as MessageViewModel;
            // win.Owner = this;
            if ((bool)win.ShowDialog())
                viewModel.SendDirectMessage(mdl.User.ScreenName, mdl.Content, (ex) =>
                {
                    MessageBox.Show(Strings.Message_Error_DM, Strings.Title_Error,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
        }

        private void Context_OnProfile(object sender, RoutedEventArgs e)
            => System.Diagnostics.Process.Start($"https://twitter.com/{viewModel.SelectedItem.ScreenName}");

        #endregion

        private void OnTryAgainClicked(object sender, RoutedEventArgs e)
            => viewModel.TryResume();

        private void FilterUserList(object sender, FilterEventArgs e)
        {
            if (e.Item is UserListItem item)
            {
                e.Accepted = (viewModel.ShowAway || item.Status != UserStatus.Away) && (viewModel.ShowOffline || item.Status != UserStatus.Offline);
            }
        }

        private void UserListRefreshRequested(object sender, EventArgs e)
        {
            (FindResource("UserListSource") as CollectionViewSource)?.View?.Refresh();
        }
    }
}
