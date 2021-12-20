import sys
import os.path
import winreg
import subprocess


class DependLibHelper:
    """相关依赖库"""

    def CheckLib(self):
        """检测是否安装Fw和vc++"""
        try:
            if self.CheckRt():
                self.InstallRt()
                if self.CheckRt():
                    return

            if self.CheckWv():
                self.InstallWv()
                if self.CheckWv():
                    return
            self.RunSetup()
        except WindowsError:
            print("安装依赖库异常")

    def CheckRt(self):
        try:
            # 检测.net 5 runtime
            # Microsoft Windows Desktop Runtime - 5.0.12 (x64) 这个检测不严谨,参考官方文档
            # https://docs.microsoft.com/zh-cn/dotnet/core/install/how-to-detect-installed-versions?pivots=os-windows
            fwkey = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE,
                                   r"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{3C1AF308-DCD3-4116-9A57-A68C7563F9CE}")
            if fwkey is None:
                return True
            return False
        except WindowsError:
            print("注册表Runtime操作错误")
            return True

    def CheckWv(self):
        try:
            # 检测WebView2
            vckey = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE,
                                   r"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}")
            if vckey is None:
                return True
            return False
        except WindowsError:
            print("注册表wb操作错误")
            return True

    def InstallRt(self):
        print("开始安装.net runtime")
        path =os.path.join("res", "windowsdesktop-runtime-5.0.12-win-x64.exe /install /quiet /norestart")
        print(path)
        filename = self.resource_path(path)
        print(filename)
        # os.system(filename)
        subprocess.call(filename, shell=True)
        print("结束.net runtime的安装")

    def InstallWv(self):
        print("开始安装webview2")
        filename = self.resource_path(os.path.join("res", "MicrosoftEdgeWebview2Setup.exe /silent /install"))
        print(filename)
        subprocess.call(filename, shell=True)
        print("结束webview2的安装")

    def RunSetup(self):
        filename = self.resource_path(os.path.join("res", "Setup.exe"))
        print(filename)
        subprocess.call(filename, shell=True)

    def resource_path(self, relative_path):
        """生成资源文件目录访问路径"""

        if getattr(sys, 'frozen', False):  # 是否Bundle Resource
            base_path = sys._MEIPASS
        else:
            base_path = os.path.abspath(".")
        return os.path.join(base_path, relative_path)


DependLibHelper().CheckLib()




