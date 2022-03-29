# Setup
使用WPF实现自定义win客户端安装(更新)卸载程序


## 目录
***Setup***为wpf实现的安装(更新)和卸载包  
***LuminaSetup***为python实现的最终安装包  

## 描述
具体实现逻辑博客园里有写:https://www.cnblogs.com/mcc7/p/5019341.html

## 流程

- 以WPF实现的Setup程序,用来进行安装和更新
- 以WPF实现的Uninstall程序,用来进行卸载
- Python为最终打包,因WPF需要.net5来运行,且生成多个文件,所以最终使用pyinstaller来检测并安装环境并进行封装
---
- 运行最终生成的安装程序时会把Setup和Uninstall程序解压到Local目录,把可执行程序安装到指定目录
    * 当可执行程序需要更新时,调用Local目录下的Setup进行更新,Setup会自动读取安装目录进行更新
- 最终程序运行时如果程序已经安装则走更新步骤
  
  
## 使用说明
1. 修改Setup里SetupInfo.cs中对应的信息,原则上都需要修改为自己项目的  
2. 修改Uninstall里MainWindow.xaml.cs中对应的信息,同SetupInfo中的部分  
3. 编译并把生成的安装卸载程序复制到python项目下的res目录下  
4. 修改python项目下的SetupShell.py文件,修改对应需要检测的环境等  
5. 修改SetupShell.spec,该文件为pyinstaller生成包的配置,具体用法自行搜索  
6. 最终生成命令为(python版本为3.8.8,需要安装pyinstaller  
      `pyinstaller -F --uac-admin -r Main.exe.mainfest,1 SetupShell.spec`

