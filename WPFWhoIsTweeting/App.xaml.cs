﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WPFWhoIsTweeting
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            WPFWhoIsTweeting.Properties.Settings.Default.Save();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (WPFWhoIsTweeting.Properties.Settings.Default.UpdateSettings)
            {
                WPFWhoIsTweeting.Properties.Settings.Default.Upgrade();
                WPFWhoIsTweeting.Properties.Settings.Default.UpdateSettings = false;
                WPFWhoIsTweeting.Properties.Settings.Default.Save();
            }
        }
    }
}
