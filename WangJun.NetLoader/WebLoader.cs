using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.Data;
using WangJun.DB;
using WangJun.Net;
using WangJun.Tools;

namespace WangJun.NetLoader
{
    /// <summary>
    /// Web下载器
    /// </summary>
    public class WebLoader
    {
        protected HTTP http = new HTTP(); ///HTTP通信器

        protected MongoDB mongo = null;

        protected Queue<Dictionary<string,object>> queueUrl = new Queue<Dictionary<string, object>>(); ///要下载的链接队列

        public event EventHandler EventDownloadCompleted = null; ///下载完毕事件

        public event EventHandler EventDownloadError = null; ///下载出错事件

        public event EventHandler EventUrlEnqueue = null; ///下载队列添加事件

        ///<summary>
        ///下载链接入队
        /// </summary>
        public void UrlEnqueue(List<Dictionary<string,object>> list)
        {
            if(null != list)
            {
                foreach (var item in list)
                {
                    this.UrlEnqueue(item);
                }
            }
        }

        ///<summary>
        ///下载链接入队
        /// </summary>
        public void UrlEnqueue(Dictionary<string, object> dict)
        {
            lock (this.queueUrl)
            {
                this.queueUrl.Enqueue(dict);
            }
        }

        ///<summary>
        ///下载链接入队
        /// </summary>
        public void UrlEnqueue(string url)
        {
            if(StringChecker.IsHttpUrl(url)) ///若数据符合要求
            {
                var dict = new Dictionary<string, object>();
                dict["Url"] = url;
                this.UrlEnqueue(dict);
            }
        }


        /// <summary>
        /// 下载队列是否为空
        /// </summary>
        public bool IsQueueURLEmpty
        {
            get
            {
                var res  = false;
                lock(this.queueUrl)
                {
                    res = (null == this.queueUrl) || (0 == this.queueUrl.Count);
                }
                return res;
            }
        }

        /// <summary>
        /// 获取一个实例
        /// </summary>
        /// <returns></returns>
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

            if(!this.IsQueueURLEmpty) ///若下载队列不为空
            {
                while (0 < this.queueUrl.Count)
                {
                    var item = this.queueUrl.Dequeue();
                    var url = item["Url"].ToString();
                    var html = this.http.GetGzip(url);  ///获取数据
                     
                    var dict = CollectionTools.CloneDict(item);
                    dict["UpdateTime"] = DateTime.Now;
                    dict["Status"] = "Downloaded";
                    dict["PageData"] = html;
                    dict["FileType"] = "html";
                    EventProc.TriggerEvent(this.EventDownloadCompleted, this, EventProcEventArgs.Create(dict));


                    //this.mongo.Save("ths", "news", dict);
                    Console.WriteLine("已处理\t{0}", url);
                }
            }


        }



        #region 准备数据
        /// <summary>
        /// 准备数据
        /// </summary>
        protected void PrepareData()
        {
            EventProc.TriggerEvent(this.EventUrlEnqueue, this, null);
        }
        #endregion



    }
}
