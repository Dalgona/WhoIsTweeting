using System.Windows;

namespace WhoIsTweeting
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (WhoIsTweeting.Properties.Settings.Default.UpdateSettings)
            {
                WhoIsTweeting.Properties.Settings.Default.Upgrade();
                WhoIsTweeting.Properties.Settings.Default.UpdateSettings = false;
                WhoIsTweeting.Properties.Settings.Default.Save();
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            WhoIsTweeting.Properties.Settings.Default.Save();
        }
    }
}
