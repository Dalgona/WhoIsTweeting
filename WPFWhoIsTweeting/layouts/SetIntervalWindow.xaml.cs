using System.Windows;
using Wit.VM;

namespace WhoIsTweeting
{
    public partial class SetIntervalWindow : Window
    {
        IntervalViewModel viewModel;

        int oldInterval;

        public SetIntervalWindow()
        {
            InitializeComponent();
            DataContext = viewModel = new IntervalViewModel();
            oldInterval = viewModel.Interval;
        }

        private void OnDownClicked(object sender, RoutedEventArgs e)
            => viewModel.Interval -= 5;

        private void OnUpClicked(object sender, RoutedEventArgs e)
            => viewModel.Interval += 5;

        private void OnOKClicked(object sender, RoutedEventArgs e)
        {
            if (viewModel.Interval != oldInterval)
                viewModel.CommitInterval();
            DialogResult = true;
            Close();
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
