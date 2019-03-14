﻿using System.Reflection;
using Wit.UI.Core;

namespace Wit.VM
{
    public class AboutViewModel : WindowViewModel
    {
        private RelayCommand _closeCommand;
        private RelayCommand _emailCommand;
        private RelayCommand _websiteCommand;
        private RelayCommand _pbCommand;

        public string Version { get; }

        public RelayCommand CloseCommand
            => _closeCommand ?? (_closeCommand = new RelayCommand(() => winManager.CloseWindow(this)));

        public RelayCommand EmailCommand
            => _emailCommand ?? (_emailCommand = new RelayCommand(() => System.Diagnostics.Process.Start("mailto:dalgona@hontou.moe")));

        public RelayCommand WebsiteCommand
            => _websiteCommand ?? (_websiteCommand = new RelayCommand(() => System.Diagnostics.Process.Start("https://dalgona.github.io")));

        public RelayCommand PbCommand
            => _pbCommand ?? (_pbCommand = new RelayCommand(() => System.Diagnostics.Process.Start("https://github.com/Dalgona/PicoBird")));

        public AboutViewModel()
        {
            Width = 380;
            Height = 400;
            CanResize = false;

            var appVer = Assembly.GetEntryAssembly().GetName().Version;
            Version = $"{appVer.Major}.{appVer.Minor}.{appVer.Build}";
        }

        public AboutViewModel(ViewModelFactory vmFactory, IWindowManager winManager) : this()
        {
            this.vmFactory = vmFactory;
            this.winManager = winManager;
        }
    }
}
