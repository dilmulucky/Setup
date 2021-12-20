# Setup
使用WPF实现自定义win客户端安装卸载程序


目录Setup为wpf实现的安装(更新)和卸载包

目录LuminaSetup为python实现的最终安装包



具体实现逻辑博客园里有写:https://www.cnblogs.com/mcc7/p/5019341.html



本身wpf的实现是需要依赖.net5来运行的,所以需要利用pyinstall来检测并安装环境并进行封装



Setp:

1,修改Setup里SetupInfo.cs中对应的信息,原则上都需要修改为自己项目的

2,修改Uninstall里MainWindow.xaml.cs中对应的信息,同SetupInfo中的部分

3,编译并把生成的安装卸载程序复制到python项目下的res目录下

4,修改python项目下的SetupShell.py文件,修改对应需要检测的环境等

5,修改SetupShell.spec,该文件为pyinstaller生成包的配置,具体用法自行搜索

6,最终生成命令为(python版本为3.8.8,需要安装pyinstaller):
pyinstaller -F --uac-admin -r Main.exe.mainfest,1 SetupShell.spec



最终安装完成后,假设安装在d:\app目录下,则安装程序存在于d:\app\Setup.exe,程序存在于d:\app\Release\目录下.则程序检测到需要更新时调用自身目录的上一级目录的Setup.exe进行更新
