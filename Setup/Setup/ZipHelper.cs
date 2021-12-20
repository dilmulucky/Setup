using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Setup
{
    public class ZipHelper
    {
        /// <summary>
        /// 功能：解压zip格式的文件
        /// </summary>
        /// <param name="zipFilePath">压缩文件路径</param>
        /// <param name="unZipDir">解压文件存放路径,为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹</param>
        /// <param name="pro"></param>
        /// <returns>解压是否成功</returns>
        public static bool UnZipFile(string zipFilePath, string unZipDir, Action<double> pro)
        {
            if (zipFilePath == string.Empty)
            {
                return false;
            }
            if (!File.Exists(zipFilePath))
            {
                return false;
            }
            // 解压文件夹为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹
            if (unZipDir == string.Empty)
                unZipDir = zipFilePath.Replace(Path.GetFileName(zipFilePath),
                    Path.GetFileNameWithoutExtension(zipFilePath));
            if (!unZipDir.EndsWith("//"))
                unZipDir += "//";
            if (!Directory.Exists(unZipDir))
                Directory.CreateDirectory(unZipDir);
            try
            {
                FileStream file = File.OpenRead(zipFilePath);
                long totalBytes = file.Length;
                long totalDownloadedByte = 0;
                using (ZipInputStream s = new ZipInputStream(file))
                {
                    ZipEntry theEntry;
                    while ((theEntry = s.GetNextEntry()) != null)
                    {
                        string directoryName = Path.GetDirectoryName(theEntry.Name);
                        string fileName = Path.GetFileName(theEntry.Name);
                        // ReSharper disable once PossibleNullReferenceException
                        if (directoryName.Length > 0)
                        {
                            Directory.CreateDirectory(unZipDir + directoryName);
                        }
                        if (!directoryName.EndsWith("//"))
                            // ReSharper disable once RedundantAssignment
                            directoryName += "//";
                        if (fileName == string.Empty) continue;

                        using (FileStream streamWriter = File.Create(unZipDir + theEntry.Name))
                        {
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                var size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    totalDownloadedByte = size + totalDownloadedByte;
                                    streamWriter.Write(data, 0, size);
                                    if (pro != null)
                                    {
                                        pro.Invoke(totalDownloadedByte / (float)totalBytes * 100);
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                    file.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("解压失败:" + zipFilePath + "  " + ex.Message);
                return false;
            }
            return true;
        }
    }
}
