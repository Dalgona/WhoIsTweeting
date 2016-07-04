using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WhoIsTweeting
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        public MainService Service { get; private set; }
        public MainViewModel MainViewModel { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (WhoIsTweeting.Properties.Settings.Default.UpdateSettings)
            {
                WhoIsTweeting.Properties.Settings.Default.Upgrade();
                WhoIsTweeting.Properties.Settings.Default.UpdateSettings = false;
                WhoIsTweeting.Properties.Settings.Default.Save();
            }

            Service = new MainService();
            MainViewModel = new MainViewModel(null); // TODO
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            WhoIsTweeting.Properties.Settings.Default.Save();
        }
    }
}
