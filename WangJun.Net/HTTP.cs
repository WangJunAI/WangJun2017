using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.Net
{

    /// <summary>
    /// Http 通信器
    /// </summary>
    public class HTTP
    {
        private WebClient http = new WebClient();

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

        //public string UploadFile()
        //{

        //}
    }
}
