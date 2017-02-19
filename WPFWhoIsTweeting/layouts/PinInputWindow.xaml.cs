﻿using System.Windows;

namespace WhoIsTweeting
{
    public partial class PinInputWindow : Window
    {
        TokenViewModel viewModel;

        public PinInputWindow()
        {
            InitializeComponent();

            DataContext = viewModel = new TokenViewModel();
        }

        private void OnOKClicked(object sender, RoutedEventArgs e)
        {
            if (viewModel.PIN == "") return;
            DialogResult = true;
            Close();
        }
    }
}