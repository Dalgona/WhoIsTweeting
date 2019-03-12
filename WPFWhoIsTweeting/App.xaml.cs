using System.Windows;
using System.Windows.Data;
using Wit.Core;

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

            MainService service = MainService.Instance;

            BindingOperations.EnableCollectionSynchronization(service.UserList, service.UserListLock);
            BindingOperations.EnableCollectionSynchronization(service.Graph, service.GraphLock);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            WhoIsTweeting.Properties.Settings.Default.Save();
            Wit.Core.Properties.Settings.Default.Save();
        }
    }
}
