using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Uninstall
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 需要修改的地方

        private const string KeyName = "Lumina";
        private const string SetupExe = "Lumina.exe";
        private const string AppExe = "Lumina live.exe";
        private const string LnkName = "Lumina live.lnk";

        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        public void UnRegisterInstallInfo()
        {
            try
            {
                RegistryKey? key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall", true);
                if (key == null)
                {
                    return;
                }

                key.DeleteSubKey(KeyName);
                key.Close();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 获取程序目录
        /// </summary>
        /// <returns></returns>
        public static string GetInstallPath()
        {
            try
            {
                RegistryKey? key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" + KeyName);
                if (key == null) return null;

                return key.GetValue("InstallLocation").ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void DelLnk()
        {
            var dt = Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
            File.Delete(System.IO.Path.Combine(dt, LnkName));
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                e.Handled = true;
                var win = Window.GetWindow(sender as Rectangle);
                win.DragMove();
            }
            catch (Exception)
            {
            }
        }

        private void BtnClose_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Environment.Exit(0);
        }

        private void UninstallBtn_Click(object sender, RoutedEventArgs e)
        {
            UninstallPanel.Visibility = Visibility.Collapsed;
            ProPanel.Visibility = Visibility.Visible;

            Thread setupThread = new Thread(Uninstall);
            setupThread.Start();
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Uninstall()
        {
            try
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    MsgTb.Text = "Uninstalling";
                }));
                ProcessHelper.KillProcessAwait(SetupExe);
                ProcessHelper.KillProcessAwait(AppExe);

                var inspath = GetInstallPath();
                GoPro(10);
                if (!string.IsNullOrEmpty(inspath) && Directory.Exists(inspath))
                {
                    Directory.Delete(inspath, true);
                }
                GoPro(80);
                UnRegisterInstallInfo();
                DelLnk();
                GoPro(100);

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    FinishTb.Text = "Uninstall succeeded";
                    ProPanel.Visibility = Visibility.Collapsed;
                    FinishPanel.Visibility = Visibility.Visible;
                }));
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    MsgTb.Text = ex.Message;
                }));
            }
        }

        int Process = 0;

        private void GoPro(int pro)
        {
            while (Process < pro)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Pro.Value = Process;
                }));
                Thread.Sleep(30);
                Process++;
            }
        }
    }
}
