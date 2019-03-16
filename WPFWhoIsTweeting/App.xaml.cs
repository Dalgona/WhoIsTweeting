using System.Windows;
using System.Windows.Data;
using Wit.Core;
using Wit.UI.Core;
using Wit.VM;

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
            WindowManager winManager = new WindowManager();
            ViewModelFactory vmFactory = new ViewModelFactory(winManager);

            BindingOperations.EnableCollectionSynchronization(service.UserList, service.UserListLock);
            BindingOperations.EnableCollectionSynchronization(service.Graph, service.GraphLock);

            winManager.ShowWindow(vmFactory.Create<MainViewModel>());
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Wit.VM.Properties.Settings.Default.Save();
            Wit.Core.Properties.Settings.Default.Save();
        }
    }
}
