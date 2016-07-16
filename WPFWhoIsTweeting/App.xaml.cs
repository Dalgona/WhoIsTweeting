using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;

namespace WhoIsTweeting
{
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

            SelectCulture(Thread.CurrentThread.CurrentUICulture.ToString());

            Service = new MainService();
            MainViewModel = new MainViewModel();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            WhoIsTweeting.Properties.Settings.Default.Save();
        }

        private static void SelectCulture(string culture)
        {
            // thanks to http://stackoverflow.com/questions/814600

            if (string.IsNullOrEmpty(culture))
                return;

            var dictionaryList = Current.Resources.MergedDictionaries.ToList();

            string requestedCulture = string.Format("Resources/Strings.{0}.xaml", culture);
            var resourceDictionary = dictionaryList.
                FirstOrDefault(d => d.Source.OriginalString == requestedCulture);

            if (resourceDictionary == null)
            {
                requestedCulture = "Resources/Strings.xaml";
                resourceDictionary = dictionaryList.
                    FirstOrDefault(d => d.Source.OriginalString == requestedCulture);
            }

            if (resourceDictionary != null)
            {
                Current.Resources.MergedDictionaries.Remove(resourceDictionary);
                Current.Resources.MergedDictionaries.Add(resourceDictionary);
            }

            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
        }
    }
}
