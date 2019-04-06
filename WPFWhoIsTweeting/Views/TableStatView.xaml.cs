using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Wit.Core;

namespace WhoIsTweeting.Views
{
    public partial class TableStatView : UserControl
    {
        private MainService service = MainService.Instance;

        public TableStatView()
        {
            InitializeComponent();
        }

        private void OnExportClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                Title = Strings.Stat_Export_Title,
                Filter = Strings.Stat_Export_Filter
            };
            dlg.FileOk += OnExportDlgOK;
            dlg.ShowDialog();
        }

        private void OnExportDlgOK(object sender, CancelEventArgs e)
        {
            SaveFileDialog dlg = sender as SaveFileDialog;
            string extension = dlg.FileName.Split('.').Last().ToLower(CultureInfo.InvariantCulture);
            if (extension != "csv")
                MessageBox.Show(
                    Strings.Stat_Export_InvalidType, Strings.Title_Error,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            try
            {
                using (FileStream fs = new FileStream(dlg.FileName, FileMode.Create))
                using (BufferedStream bfs = new BufferedStream(fs, 1024))
                {
                    byte[] utfstr;

                    foreach (var x in service.StatData)
                    {
                        string date = x.Date.ToString("yyyy-MM-dd HH:mm:ss");
                        utfstr = Encoding.UTF8.GetBytes($"\"{date}\",{x.OnlineCount},{x.AwayCount},{x.OfflineCount}{Environment.NewLine}");

                        fs.Write(utfstr, 0, utfstr.Length);
                    }
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show(
                    string.Format(Strings.Stat_Export_Exception, ex.Message),
                    Strings.Title_Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
