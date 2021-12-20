using Setup.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Setup
{
    public class HttpHelper
    {
        private const string DefaultUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
        private const string Get = "GET";

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="httpUrl"></param>
        /// <param name="savePath"></param>
        /// <param name="fileMd5">为空时不做md5校验</param>
        /// <param name="result">下载结束后执行</param>
        /// <param name="pro">下载进度</param>
        /// <param name="isExistsDel">是否强制删除文件</param>
        /// <param name="isnewName">是否使用新名</param>
        /// <returns></returns>
        public static string DownLoadFile(string httpUrl, string savePath, string fileName, Action<long, long> pro, bool isExistsDel = false, string fileMd5 = "")
        {
            try
            {
                string fileFullPath = Path.Combine(savePath, fileName);

                // 判断文件是否存在
                if (File.Exists(fileFullPath))
                {
                    if (isExistsDel)
                    {
                        File.Delete(fileFullPath);
                    }
                    else
                    {
                        return fileFullPath;
                    }
                }

                // 判断下载地址是否存在
                if (string.IsNullOrEmpty(httpUrl))
                {
                    return string.Empty;
                }

                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                bool isload = LoadFileByGet(fileFullPath, httpUrl, pro);
                // 下载失败
                if (!isload)
                {
                    return string.Empty;
                }

                //if (string.IsNullOrEmpty(fileMd5))
                //{
                return fileFullPath;
                //}

                //string md5 = EncodingHelper.GetMd5HashFromFile(fileFullPath);
                //return fileMd5.ToUpper().Equals(md5.ToUpper()) ? fileFullPath : string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// 根据Url下载文件
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="httpUrl">Url</param>
        /// <param name="pro">下载进度</param>
        /// <returns>反馈信息</returns>
        private static bool LoadFileByGet(string path, string httpUrl, Action<long, long> pro)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(httpUrl) as HttpWebRequest;
                if (request == null)
                {
                    //url + "请求返回值为null";
                    return false;
                }
                request.Method = "GET";
                request.UserAgent = DefaultUserAgent;
                request.ServicePoint.ConnectionLimit = 20;
                WebHeaderCollection headers = request.Headers;

                //发送请求并获取相应回应数据
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                long totalBytes = response.ContentLength;
                if (totalBytes == -1)
                {
                    return false;
                }

                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                Stream responseStream = response.GetResponseStream();
                if (responseStream == null) return false;

                byte[] bArr = new byte[1024];
                int size = responseStream.Read(bArr, 0, bArr.Length);
                //创建本地文件写入流
                Stream stream;
                if (size > 0)
                {
                    stream = new FileStream(path, FileMode.Create);
                }
                else
                {
                    return false;
                }

                long totalDownloadedByte = 0;
                while (size > 0)
                {
                    totalDownloadedByte = size + totalDownloadedByte;
                    stream.Write(bArr, 0, size);
                    size = responseStream.Read(bArr, 0, bArr.Length);

                    if (pro != null)
                    {
                        //pro.Invoke(totalDownloadedByte / (float)totalBytes * 100, totalBytes);
                        pro.Invoke(totalDownloadedByte, totalBytes);
                    }
                }

                stream.Close();
                responseStream.Close();
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("下载文件异常:" + httpUrl + " ex:" + ex.Message);
                return false;
            }
        }



        #region private

        /// <summary>  
        /// 创建GET方式的HTTP请求  
        /// </summary>  
        /// <param name="url">请求的URL</param>  
        /// <param name="timeout">请求的超时时间</param>  
        /// <param name="userAgent">请求的客户端浏览器信息，可以为空</param>  
        /// <param name="cookies">随同HTTP请求发送的Cookie信息，如果不需要身份验证可以为空</param>  
        /// <returns></returns>  
        private static HttpWebResponse CreateGetHttpResponse(string url, int? timeout = null, string userAgent = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            if (request == null)
            {
                Console.WriteLine(url + "请求返回值为null");
                return null;
            }

            request.Method = Get;
            request.UserAgent = DefaultUserAgent;


            if (!string.IsNullOrEmpty(userAgent))
            {
                request.UserAgent = userAgent;
            }
            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value;
            }
            //request.KeepAlive = false;
            request.ServicePoint.ConnectionLimit = 20;

            // var domain = url.Replace("http://", "").Replace("https://", "").Split('/').FirstOrDefault();
            // request.CookieContainer = CreateCookies(Cookies, domain);

            HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();

            return webResponse;
        }

        #endregion

        #region public

        /// <summary>
        /// GET请求获取返回值
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static string HttpWebGet(string url)
        {
            string responseText = string.Empty;
            HttpWebResponse response = CreateGetHttpResponse(url);
            if (response != null)
            {
                StreamReader myreader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                responseText = myreader.ReadToEnd();
                myreader.Close();
                response.Close();
            }

            return responseText;
        }

        #endregion


        public static VersionModel GetConfig(string url)
        {
            try
            {
                var result = HttpWebGet(url);

                if (string.IsNullOrEmpty(result))
                {
                    return null;
                }

                VersionModel versionModel = new VersionModel();

                var res = result.Split('"');
                for (int i = 0; i < res.Length; i++)
                {
                    if (res[i].Equals("version"))
                    {
                        versionModel.Version = res[i + 2];
                        continue;
                    }
                    if (res[i].Equals("file_url"))
                    {
                        versionModel.Url = res[i + 2].Replace("\\", "");
                        break;
                    }
                }

                return versionModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

    }
}
