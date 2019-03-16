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
            DepsInjector ioc = DepsInjector.Default;

            BindingOperations.EnableCollectionSynchronization(service.UserList, service.UserListLock);
            BindingOperations.EnableCollectionSynchronization(service.Graph, service.GraphLock);

            ioc.Register<IWindowManager, WindowManager>();
            ioc.Register<ViewModelBase>();

            IWindowManager winManager = ioc.GetInstance<IWindowManager>();

            winManager.ShowWindow(ioc.Create<MainViewModel>());
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Wit.VM.Properties.Settings.Default.Save();
            Wit.Core.Properties.Settings.Default.Save();
        }
    }
}
