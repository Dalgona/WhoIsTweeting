using System.Windows;
using System.Windows.Data;
using Wit.Core;

namespace WhoIsTweeting
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (Wit.VM.Properties.Settings.Default.UpgradeSettings)
            {
                Wit.VM.Properties.Settings.Default.Upgrade();
                Wit.VM.Properties.Settings.Default.UpgradeSettings = false;
                Wit.VM.Properties.Settings.Default.Save();
            }

            MainService service = MainService.Instance;

            BindingOperations.EnableCollectionSynchronization(service.UserList, service.UserListLock);
            BindingOperations.EnableCollectionSynchronization(service.Graph, service.GraphLock);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Wit.VM.Properties.Settings.Default.Save();
            Wit.Core.Properties.Settings.Default.Save();
        }
    }
}
