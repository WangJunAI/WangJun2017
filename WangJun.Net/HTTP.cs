using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WangJun.Tools;

namespace WangJun.Net
{

    /// <summary>
    /// Http 通信器
    /// </summary>
    public class HTTP
    {
        private WebClient http = new WebClient();

        public event EventHandler EventException = null;

        /// <summary>
        /// 事件触发
        /// </summary>
        public void TriggerEvent(EventHandler handler , object sender,EventArgs e)
        {
            if(null != handler)
            {
                handler(sender, e);
            }
        }

        public HTTP()
        {
            this.http.Encoding = Encoding.UTF8;
        }
        public HTTP(Encoding coder)
        {
            this.http.Encoding = coder;
        }

        public HTTP (string coder)
        {
            this.http.Encoding = Encoding.GetEncoding(coder);
        }


        /// <summary>
        /// 通过Get方式获取一份文件
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public  byte[] GetFile(string url)
        {
            byte[] buffer = this.http.DownloadData(url);
            return buffer;
        }

        /// <summary>
        /// 保存一个文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="filePath"></param>
        public void SaveFile(string url ,string filePath)
        {
            byte[] buffer = this.GetFile(url);
            File.WriteAllBytes(filePath, buffer);
        }

        /// <summary>
        /// 保存一个文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="filePath"></param>
        public void SaveText(string url, string filePath)
        {
            string text = this.http.DownloadString(url);
            File.WriteAllText(filePath, text, Encoding.UTF8);
        }

        /// <summary>
        /// 通过Get方式下载字符串
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string GetString(string url)
        {
            string data = this.http.DownloadString(url);
            return data;
        }
        #region

        #endregion  以GZip的格式进行下载
        /// <summary>
        /// 以GZip的格式进行下载
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string GetGzip(string url)
        {
            try
            {
                this.http.Headers.Clear();
                this.http.Headers.Add("Accept-Encoding", "gzip,deflate");
                this.http.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.101 Safari/537.36");
                byte[] byteArray = this.http.DownloadData(url);
                // 处理　gzip   
                string sContentEncoding = this.http.ResponseHeaders["Content-Encoding"];
                if (sContentEncoding == "gzip")
                {
                    var sourceStream = new MemoryStream(byteArray);
                    var targetStream = new MemoryStream();
                    int count = 0;
                    // 解压  
                    GZipStream gzip = new GZipStream(sourceStream, CompressionMode.Decompress);
                    byte[] buf = new byte[512];
                    while ((count = gzip.Read(buf, 0, buf.Length)) > 0)
                    {
                        targetStream.Write(buf, 0, count);
                    }
                    var res = Encoding.GetEncoding("gbk").GetString(targetStream.GetBuffer());
                    sourceStream.Close();
                    targetStream.Close();

                    return res;
                 }
            }
            catch(Exception e)
            {
                EventProc.TriggerEvent(this.EventException, this, EventProcEventArgs.Create(new { Url = url, Exception = e, CreateTime = DateTime.Now }));
            }
            return string.Empty;
        } 
    }
}
