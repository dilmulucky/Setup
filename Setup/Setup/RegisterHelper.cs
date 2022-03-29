using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Setup
{
    public class RegisterHelper
    {
        /// <summary>
        /// 获取程序目录
        /// </summary>
        /// <returns></returns>
        public static string GetSourcePath()
        {
            try
            {
                RegistryKey? key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" + SetupInfo.KeyName);
                if (key == null) return null;

                return key.GetValue("InstallSource").ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string GetInstallPath()
        {
            try
            {
                RegistryKey? key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" + SetupInfo.KeyName);
                if (key == null) return null;

                return key.GetValue("InstallLocation").ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 注册应用信息
        /// </summary>
        /// <param name="setupPath">安装路径</param>
        public static void RegisterInstallInfo(string installPath, string fileName, string targetPath, string unfileName, string ver)
        {
            try
            {
                RegistryKey? key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall", true);
                if (key == null) return;

                RegistryKey software = key.CreateSubKey(SetupInfo.KeyName);

                // 图标
                software.SetValue("DisplayIcon", Path.Combine(installPath, fileName));

                // 显示名
                software.SetValue("DisplayName", SetupInfo.DisplayName);

                // 版本
                software.SetValue("DisplayVersion", ver);

                // 程序发行公司
                software.SetValue("Publisher", SetupInfo.Publisher);

                software.SetValue("InstallSource", targetPath);

                // 安装位置
                software.SetValue("InstallLocation", installPath);

                // 帮助电话
                // software.SetValue("HelpTelephone", "123456789");

                // 卸载完整路径
                software.SetValue("UninstallString", unfileName);

                //software.Close();
                key.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        /// <summary>
        /// 注册自定义协议
        /// </summary>
        /// <param name="runName">文件名.exe</param>
        /// <param name="fileFullName">完整文件名</param>
        public static void SetUnProtocol(string runName, string fileFullName)
        {
            try
            {
                var doub = "\"";
                string cmdrun = string.Format("{0}{1}{2} {3}{4}{5}", doub, fileFullName, doub, doub, "%1", doub);

                RegistryKey key = Registry.ClassesRoot;
                RegistryKey protocol = key.CreateSubKey(SetupInfo.UnProtocolKey);
                protocol.SetValue("", string.Format("URL:{0} protocol", SetupInfo.UnProtocolKey));
                protocol.SetValue("URL Protocol", "");
                RegistryKey def = protocol.CreateSubKey("DefaultIcon");
                def.SetValue("", runName);
                var shell = protocol.CreateSubKey("shell");
                var open = shell.CreateSubKey("open");
                var command = open.CreateSubKey("command");
                command.SetValue("", cmdrun);
                key.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("设置通用协议错误:" + ex.Message);
            }
        }
    }
}
