using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Setup
{
    /// <summary>
    /// 安装及配置信息,需要修改所有Lumina及Beacon字样
    /// </summary>
    public class SetupInfo
    {
        /// <summary>
        /// 安装程序唯一标识(必需修改)
        /// </summary>
        public const string Unique = "FB082AF4-D460-4EF8-B6D4-938C2175771C";

        /// <summary>
        /// 配置文件请求地址
        /// </summary>
        public const string VerUrl = "/api/v1/appVersion/configInfo?client=Windows&isNew=true";

        /// <summary>
        /// 此程序用于更新/卸载时所在的目录,建议存放在local下,ProgramFiles目录存在权限问题且放在相同目录卸载时文件占用
        /// 客户端程序需要知道此路径,需要更新时调用此路径下Setup.exe,下载更新包进行更新
        /// </summary>
        public static readonly string UpdatePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Lumina\Update\");

        /// <summary>
        /// 注册名
        /// </summary>
        public const string KeyName = "Lumina";

        /// <summary>
        /// 显示名
        /// </summary>
        public const string DisplayName = "Lumina live";

        /// <summary>
        /// 发行公司
        /// </summary>
        public const string Publisher = "Beacon Education";

        /// <summary>
        /// 安装程序的启动名
        /// </summary>
        public const string AppExe = "Lumina live.exe";

        /// <summary>
        /// 卸载程序名(无需修改)
        /// </summary>
        public const string UninstallExe = "Uninstall.exe";

        /// <summary>
        /// 隐私政策
        /// </summary>
        public const string PrivacyPolicy = "http://lms.baedu.vip/privacy_policy";
        public const string PrivacyPolicyEn = "http://lms.baedu.vip/en/privacy_policy";

        /// <summary>
        /// 用户协议
        /// </summary>
        public const string UserAgreement = "http://lms.baedu.vip/user_agreement";
        public const string UserAgreementEn = "http://lms.baedu.vip/en/user_agreement";

        #region UnProtocol

        /// <summary>
        /// 是否使用通用协议
        /// </summary>
        public const bool IsUseUnProtocol = true;

        /// <summary>
        /// 通用协议key
        /// </summary>
        public const string UnProtocolKey = "beacon";

        #endregion
    }

    [Serializable]
    [XmlRoot("UserConfig")]
    public class UserConfig
    {
        private static readonly string FileFullName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Lumina\Data\userconfig.xml");

        private static readonly object lockobj = new object();

        private static UserConfig instance;

        /// <summary>
        /// 用户配置的实例
        /// </summary>
        /// <returns></returns>
        public static UserConfig Instance()
        {
            lock (lockobj)
            {
                if (instance == null)
                {

                    instance = LoadXML(FileFullName);
                }

                return instance;
            }
        }

        // 为方便测试,所以服务器地址修改为动态从配置文件中获取
        private string serverUrl = "https://www.luminaedu.com";

        /// <summary>
        /// 服务器访问地址(默认线上环境)
        /// </summary>
        public string ServerUrl
        {
            get { return serverUrl; }
            set
            {
                serverUrl = value;
            }
        }

        public static UserConfig LoadXML(string fileFullName)
        {
            using (FileStream fs = new FileStream(fileFullName, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer xs = new XmlSerializer(typeof(UserConfig));
                var listpers = xs.Deserialize(fs) as UserConfig;
                return listpers;
            }
        }
    }
}
