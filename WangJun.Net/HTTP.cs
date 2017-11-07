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
        private WebClientWithCookie http = new WebClientWithCookie();

        public event EventHandler EventException = null;

         public void SetHeaders(Dictionary<string,string> headers)
        {
            if(null !=headers)
            {
                this.http.Headers.Clear();
                foreach (var item in headers)
                {
                    this.http.Headers.Add(item.Key, item.Value);
                }
            }

        }

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
            
            var headers = new Dictionary<string, string>();
            headers.Add("Host","d.10jqka.com.cn");
            headers.Add("User-Agent", "Mozilla/5.0(Windows NT 10.0; Win64; x64) AppleWebKit/537.36(KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
            headers.Add("Accept", "*/*");
            headers.Add("Referer", "http://data.10jqka.com.cn/funds/ggzjl/");
            headers.Add("Accept-Encoding", "gzip,deflate");
            headers.Add("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4");



            //this.SetHeaders(headers);
            this.http.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
            this.http.Headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4");
            this.http.Headers.Add(HttpRequestHeader.Accept, "*/*");
            this.http.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            this.http.Headers.Add(HttpRequestHeader.Referer, "http://data.10jqka.com.cn/funds/ggzjl/");
            this.http.Headers.Add(HttpRequestHeader.Cookie, "_ga=GA1.3.1308637288.1480656705;Hm_lvt_f79b64788a4e377c608617fba4c736e2=1490269470; __utma=156575163.1308637288.1480656705.1497148324.1500515050.22; __utmz=156575163.1500515050.22.16.utmcsr=10jqka.com.cn|utmccn=(referral)|utmcmd=referral|utmcct=/; searchGuide=sg; spversion=20130314; Hm_lvt_78c58f01938e4d85eaf619eae71b4ed1=1509877496,1509944925,1509953463,1509994430; historystock=002156%7C*%7C600807%7C*%7C300708%7C*%7C002333; log=; v=AcUnjYDRMInlvheYhhW9Klcw1Ar7gngcwzddZMcqgyALoet8j9KJ5FOGbS5X");
            string data = this.http.DownloadString(url);
            return data;
        }
         

        #region 通过POST方法获取结果
        public string PostGZip(string url,string postData)
        {
            string res = string.Empty;
            try
            {
                this.http.Headers.Clear();
                this.http.Headers.Add("Accept-Encoding", "gzip,deflate");
                this.http.Headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");
                this.http.Headers.Add("X-Requested-With", "XMLHttpRequest");
                this.http.Headers.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
                this.http.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8,en;q=0.6");
                this.http.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.101 Safari/537.36");
                byte[] byteArray = this.http.UploadData(url,Encoding.UTF8.GetBytes(postData));
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
                    res = Encoding.UTF8.GetString(targetStream.GetBuffer());
                    sourceStream.Close();
                    targetStream.Close();

                    return res;
                }
            }
            catch (Exception e)
            {
                var dict = new Dictionary<string, object>();
                dict["Url"] = url;
                dict["Exception"] = e.Message;
                dict["CreateTime"] = DateTime.Now;
                EventProc.TriggerEvent(this.EventException, this, EventProcEventArgs.Create(dict));
                 
            }
            return string.Empty;

        }
        #endregion


        /// <summary>
        /// 以GZip的格式进行下载
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string GetGzip(string url,Encoding encoding,Dictionary<string,string> headers=null)
        {
            try
            {
                this.SetHeaders(headers);

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
                    var res = encoding.GetString(targetStream.GetBuffer());
                    sourceStream.Close();
                    targetStream.Close();

                    return res;
                }
                else
                {
                    var p= 0;
                }
            }
            catch (Exception e)
            {
                var dict = new Dictionary<string, object>();
                dict["Url"] = url;
                dict["Exception"] = e.Message;
                dict["CreateTime"] = DateTime.Now;
                EventProc.TriggerEvent(this.EventException, this, EventProcEventArgs.Create(dict));
            }
            return string.Empty;
        }

 
    }
 
         
}
