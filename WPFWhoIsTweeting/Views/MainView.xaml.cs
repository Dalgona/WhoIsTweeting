using System;
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
