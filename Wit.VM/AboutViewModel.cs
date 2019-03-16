using System.Reflection;
using Wit.UI.Core;

namespace Wit.VM
{
    public class AboutViewModel : WindowViewModel
    {
        private RelayCommand _closeCommand;
        private RelayCommand _emailCommand;
        private RelayCommand _websiteCommand;

        public string Version { get; }

        public RelayCommand CloseCommand
            => _closeCommand ?? (_closeCommand = new RelayCommand(() => WindowManager.CloseWindow(this)));

        public RelayCommand EmailCommand
            => _emailCommand ?? (_emailCommand = new RelayCommand(() => System.Diagnostics.Process.Start("mailto:dalgona@hontou.moe")));

        public RelayCommand WebsiteCommand
            => _websiteCommand ?? (_websiteCommand = new RelayCommand(() => System.Diagnostics.Process.Start("https://dalgona.github.io")));

        public AboutViewModel()
        {
            Width = 350;
            Height = 360;
            CanResize = false;

            var appVer = Assembly.GetEntryAssembly().GetName().Version;
            Version = $"{appVer.Major}.{appVer.Minor}.{appVer.Build}";
        }
    }
}
