using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Setup
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        protected override void OnStartup(StartupEventArgs e)
        {

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            base.OnStartup(e);
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            try
            {
                Exception ex = e.Exception;
                Console.WriteLine("异常捕获于App--App_DispatcherUnhandledException");
                Console.WriteLine("警告！！该异常造成系统的终止！！！");
                if (ex != null)
                {
                    Console.WriteLine(ex);
                }
                if (ex != null && ex.InnerException != null)
                {
                    Console.WriteLine("InnerException" + ex.InnerException.Source + "   " + ex.Message);
                }
                if (ex is InvalidOperationException)
                {
                    Console.WriteLine("无效");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = (Exception)e.ExceptionObject;
                Console.WriteLine("异常捕获于App--CurrentDomain_UnhandledException");
                Console.WriteLine("警告！！该异常造成系统的终止！！！");
                if (ex != null)
                {
                    Console.WriteLine(ex);
                }
                if (ex != null && ex.InnerException != null)
                {
                    Console.WriteLine("InnerException" + ex.InnerException.Source + "   " + ex.Message);
                }
                if (ex is InvalidOperationException)
                {
                    Console.WriteLine("无效");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        #region ISingleInstanceApp Members

        [STAThread]
        public static void Main()
        {
            try
            {
                if (SingleInstance<App>.InitializeAsFirstInstance(SetupInfo.Unique))
                {
                    var application = new App();
                    application.InitializeComponent();
                    application.Run();

                    // Allow single instance code to perform cleanup operations
                    SingleInstance<App>.Cleanup();
                }
            }
            catch (Exception)
            {
            }

        }

        public bool SignalExternalCommandLineArgs(string arg)
        {
            // Bring window to foreground
            //if (this.MainWindow.WindowState == WindowState.Minimized)
            //{
            this.MainWindow.WindowState = WindowState.Normal;
            this.MainWindow.Visibility = Visibility.Visible;
            //}

            this.MainWindow.Activate();

            // Handle command line arguments of second instance
            return true;
        }
        #endregion
    }
}
