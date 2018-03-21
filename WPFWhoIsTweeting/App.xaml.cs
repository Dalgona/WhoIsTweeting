using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;

namespace WhoIsTweeting
{
    public partial class App : Application
    {
        public MainService Service { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (WhoIsTweeting.Properties.Settings.Default.UpdateSettings)
            {
                WhoIsTweeting.Properties.Settings.Default.Upgrade();
                WhoIsTweeting.Properties.Settings.Default.UpdateSettings = false;
                WhoIsTweeting.Properties.Settings.Default.Save();
            }

            Service = new MainService();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            WhoIsTweeting.Properties.Settings.Default.Save();
        }
    }
}
