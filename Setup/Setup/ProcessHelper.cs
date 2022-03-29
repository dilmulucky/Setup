using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Setup
{
    public class ProcessHelper
    {
        /// <summary>
        /// 启动进程
        /// </summary>
        /// <param name="fileName">进程完整路径</param>
        /// <returns></returns>
        public static Process StartProcess(string fileName, string arg = null)
        {
            try
            {
                var ps = Process.Start(fileName, arg);
                return ps;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 启动exe,使用cmd中介,可摆脱权限及管理员问题
        /// </summary>
        /// <param name="filefullname"></param>
        public static void CmdRunExe(string filefullname)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.UseShellExecute = false;   //是否使用操作系统shell启动 
            process.StartInfo.CreateNoWindow = true;   //是否在新窗口中启动该进程的值 (不显示程序窗口)
            process.StartInfo.RedirectStandardInput = true;
            process.Start(); //等待程序执行完退出进程
            process.StandardInput.WriteLine(filefullname + "&exit");
            process.WaitForExit();
            process.Close();
        }

        /// <summary>
        /// 关闭指定进行并等待
        /// </summary>
        /// <param name="proName"></param>
        /// <param name="await"></param>
        /// <returns></returns>
        public static bool KillProcessAwait(string proName, bool await = true)
        {
            var result = KillProcess(proName);
            if (result)
            {
                if (!await)
                {
                    return result;
                }

                Process pro = FindPro(proName);
                if (pro != null)
                {
                    pro.WaitForExit();
                }
            }

            return result;
        }

        /// <summary>
        /// 关闭指定进程
        /// </summary>
        /// <param name="proName"></param>
        /// <returns></returns>
        public static bool KillProcess(string proName)
        {
            try
            {
                Process pro = FindPro(proName);
                if (pro != null)
                {
                    pro.Kill();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 查找指定进程
        /// </summary>
        /// <param name="proName">进程名称</param>
        /// <returns></returns>
        public static Process FindPro(string proName)
        {
            Process pro = Process.GetProcesses().FirstOrDefault(o => o.ProcessName.ToUpper() == proName.ToUpper());
            return pro;
        }
    }
}
