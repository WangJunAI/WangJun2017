using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.DB;
using WangJun.Net;

namespace WangJun.NetLoader
{
    /// <summary>
    /// Web下载器
    /// </summary>
    public class WebLoader
    {
        protected HTTP http = new HTTP();

        protected MongoDB mongo = null;

        protected Queue<string> queueUrl = new Queue<string>();

        public event EventHandler EventDownloadCompleted = null;


        public static WebLoader GetInstance()
        {
            var inst = new WebLoader();
            inst.mongo = MongoDB.GetInst("mongodb://192.168.0.140:27017");
            return inst;
        }

        /// <summary>
        /// 启动下载
        /// 从数据库中读取URL,下载文件后,写入其中
        /// </summary>
        public void Run()
        {

            this.PrepareData();

            while (0<this.queueUrl.Count)
            {
                var url = this.queueUrl.Dequeue();
                var html = this.http.GetGzip(url);
                Console.WriteLine(html);
                var dict = new Dictionary<string, object>();
                dict["Url"] = url;
                dict["CreateTime"] = DateTime.Now;
                dict["UpdateTime"] = DateTime.Now;
                dict["Status"] = 0;
                this.mongo.Save("ths", "news", dict);
            }
        }



        #region 准备数据
        /// <summary>
        /// 准备数据
        /// </summary>
        protected void PrepareData()
        {
            this.queueUrl.Enqueue("http://news.10jqka.com.cn/today_list/");
        }
        #endregion



    }
}
