using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace Setup
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 选择的安装目录
        /// 安装后安装包目录
        /// </summary>
        private string TargetPath;

        /// <summary>
        /// 安装的实际目录
        /// 位于TargetPath目录下
        /// 如不需要在TargetPath目录下存放其他文件,也可直接安装在TargetPath目录
        /// </summary>
        private string InstallPath;

        private bool IsUpdate;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            TargetPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), SetupInfo.Publisher, SetupInfo.KeyName);
            SetupPathTb.Text = TargetPath;
            InstallPath = System.IO.Path.Combine(TargetPath, "App");

            // 如果发现该程序已经安装,则直接执行更新操作
            var inspath = RegisterHelper.GetInstallPath();
            if (!string.IsNullOrEmpty(inspath) && Directory.Exists(inspath))
            {
                IsUpdate = true;

                TargetPath = RegisterHelper.GetSourcePath();
                InstallPath = inspath;

                // 隐藏安装界面,直接更新
                SetupPanel.Visibility = Visibility.Collapsed;
                SelectPanel.Visibility = Visibility.Collapsed;

                BtnClose.Visibility = Visibility.Collapsed;
                ProPanel.Visibility = Visibility.Visible;

                Thread setupThread = new Thread(Setup);
                setupThread.Start();
            }
        }

        public void Setup()
        {
            SetPro(0);

            try
            {
                ProcessHelper.KillProcessAwait(SetupInfo.AppExe);

                SetMsg("Fetching package info");
                var config = HttpHelper.GetConfig(UserConfig.Instance().ServerUrl + SetupInfo.VerUrl);
                if (config == null || string.IsNullOrEmpty(config.Version) || string.IsNullOrEmpty(config.Url))
                {
                    SetMsg("Failed to fetch package info");
                    return;
                }

                SetPro(1);

                // download package
                var packageName = "Lumina.zip";
                SetMsg("Downloading package");
                var packageFullName = HttpHelper.DownLoadFile(config.Url, AppDomain.CurrentDomain.BaseDirectory, packageName, (offset, len) =>
                {
                    SetPro(offset / (float)len * 100);
                }, true);

                SetPro(50);

                if (string.IsNullOrEmpty(packageFullName) || !System.IO.File.Exists(packageFullName))
                {
                    SetErrMsg("Failed to download package");
                    return;
                }

                SetMsg("Unzip package");
                var iszip = ZipHelper.UnZipFile(packageFullName, InstallPath, pro =>
                {
                    SetPro(pro);
                });
                if (!iszip)
                {
                    SetErrMsg("Failed to unzip package");
                    return;
                }

                // register
                SetMsg("Register program");
                SetPro(90);

                if (SetupInfo.IsUseUnProtocol)
                {
                    RegisterHelper.SetUnProtocol(SetupInfo.AppExe, System.IO.Path.Combine(InstallPath, SetupInfo.AppExe));
                }

                RegisterHelper.RegisterInstallInfo(InstallPath, SetupInfo.AppExe, TargetPath, System.IO.Path.Combine(SetupInfo.UpdatePath, SetupInfo.UninstallExe), config.Version);
                if (!IsUpdate)
                {
                    CopyExeFile();
                    // create .lnk 
                    CreateLnk(InstallPath, true);
                }

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    SetPro(100);
                    if (IsUpdate)
                    {
                        FinishTb.Text = "Update succeeded";
                    }
                    else
                    {
                        FinishTb.Text = "Install succeeded";
                    }

                    BtnClose.Visibility = Visibility.Visible;
                    ProPanel.Visibility = Visibility.Collapsed;
                    FinishPanel.Visibility = Visibility.Visible;
                }));
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    SetErrMsg(ex.Message);
                }));
                Console.WriteLine(ex.Message);
            }
        }

        private void SetMsg(string msg)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                MsgTb.Text = msg;
            }));
        }

        private void SetErrMsg(string msg)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                RetryTb.Text = msg;
                BtnClose.Visibility = Visibility.Visible;
                ProPanel.Visibility = Visibility.Collapsed;
                RetryPanel.Visibility = Visibility.Visible;
            }));
        }

        private void SetPro(double pro)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Pro.Value = pro;
            }));
        }

        /// <summary>
        /// 复制当前exe和卸载到指定目录
        /// </summary>
        public void CopyExeFile()
        {
            try
            {
                if (!Directory.Exists(SetupInfo.UpdatePath))
                    Directory.CreateDirectory(SetupInfo.UpdatePath);

                var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory);
                foreach (var file in files)
                {
                    var fileName = file.Split('\\').LastOrDefault();
                    System.IO.File.Copy(file, System.IO.Path.Combine(SetupInfo.UpdatePath, fileName), true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 创建快捷方式
        /// </summary>
        /// <param name="setupPath">安装目录</param>
        /// <param name="isDesktop">是否创建到桌面(默认创建到安装目录)</param>
        public void CreateLnk(string setupPath, bool isDesktop)
        {
            string path = setupPath;

            if (isDesktop)
            {
                path = Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
            }


            // 注：如果桌面有现准备创建的快捷键方式，当程序执行创建语句时会修改桌面已有快捷键方式，程序不会出现异常

            WshShell shell = new WshShell();

            // 快捷键方式创建的位置、名称
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(path + "\\" + SetupInfo.DisplayName + ".lnk");

            // 目标文件
            shortcut.TargetPath = setupPath + "\\" + SetupInfo.AppExe;

            // 该属性指定应用程序的工作目录，当用户没有指定一个具体的目录时，快捷方式的目标应用程序将使用该属性所指定的目录来装载或保存文件。
            shortcut.WorkingDirectory = setupPath;// System.Environment.CurrentDirectory;

            // 目标应用程序的窗口状态分为普通、最大化、最小化【1,3,7】
            // shortcut.WindowStyle = 1; 

            // 描述
            //shortcut.Description = Description;

            // 快捷方式图标
            //shortcut.IconLocation = setupPath + "\\" + AppIco;

            shortcut.Arguments = "";

            // 快捷键
            //shortcut.Hotkey = "CTRL+ALT+F11"; 

            // 保存
            shortcut.Save();
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.CmdRunExe(System.IO.Path.Combine(InstallPath, SetupInfo.AppExe));
            this.Close();
        }

        private void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            BtnClose.Visibility = Visibility.Collapsed;
            ProPanel.Visibility = Visibility.Visible;
            RetryPanel.Visibility = Visibility.Collapsed;
            Thread setupThread = new Thread(Setup);
            setupThread.Start();
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

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            SetupPanel.Visibility = Visibility.Visible;
            SelectPanel.Visibility = Visibility.Collapsed;
        }

        private void More_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetupPanel.Visibility = Visibility.Collapsed;
            SelectPanel.Visibility = Visibility.Visible;
        }

        private void SelectPathBtn_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserEx.FolderBrowserDialog folderBrowserDialog = new FolderBrowserEx.FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SetupPathTb.Text = folderBrowserDialog.SelectedFolder;
                TargetPath = folderBrowserDialog.SelectedFolder;
                InstallPath = System.IO.Path.Combine(TargetPath, "App");
            }
        }

        private void SetupBtn_Click(object sender, RoutedEventArgs e)
        {
            BtnClose.Visibility = Visibility.Collapsed;
            SetupPanel.Visibility = Visibility.Collapsed;
            SelectPanel.Visibility = Visibility.Collapsed;

            Pro.Visibility = Visibility.Visible;
            RetryBtn.Visibility = Visibility.Collapsed;
            ProPanel.Visibility = Visibility.Visible;

            Thread setupThread = new Thread(Setup);
            setupThread.Start();
        }

        [DllImport("kernel32.dll")]
        public static extern UInt16 GetUserDefaultLangID();

        private void pp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (GetUserDefaultLangID() == 2052)
            {
                // 中文
                ProcessHelper.StartProcess(SetupInfo.PrivacyPolicy);
                return;
            }

            ProcessHelper.StartProcess(SetupInfo.PrivacyPolicyEn);
        }

        private void la_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (GetUserDefaultLangID() == 2052)
            {
                ProcessHelper.StartProcess(SetupInfo.UserAgreement);
                return;
            }
            ProcessHelper.StartProcess(SetupInfo.UserAgreementEn);
        }
    }
}
