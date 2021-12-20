using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Setup
{
    public class SetupInfo
    {
        /// <summary>
        /// 安装程序唯一标识(必需修改)
        /// </summary>
        public const string Unique = "FB082AF4-D460-4EF8-B6D4-938C2175771C";

        /// <summary>
        /// 配置文件请求地址
        /// </summary>
        public const string VerUrl = "https://www.luminaedu.com/api/v1/appVersion/configInfo?client=Windows";

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
}
