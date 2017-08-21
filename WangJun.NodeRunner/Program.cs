using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WangJun.BIZ;
using WangJun.Data;
using WangJun.DB;
using WangJun.NetLoader;
using WangJun.Tools;

namespace WangJun.NodeRunner
{
    class Program
    {
        protected static DateTime StartTime = DateTime.Now; ///开始运行时间
        protected static MongoDB mongo = MongoDB.GetInst("mongodb://192.168.0.140:27017");
        protected static WebLoader loader = WebLoader.GetInstance();
        static void Main(string[] args)
        {
            SetConsoleInfo();
 
            ///事件订阅
            loader.EventUrlEnqueue += Loader_EventUrlEnqueue;
            loader.EventDownloadCompleted += Loader_EventDownloadCompleted;


            loader.Run();

            Console.WriteLine("全部结束");
            Console.ReadKey();
        }

        /// <summary>
        /// 下载成功事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Loader_EventDownloadCompleted(object sender, EventArgs e)
        {
            if(e is EventProcEventArgs)
            {
                var ee = e as EventProcEventArgs;
                var dict =  ee.Default as Dictionary<string,object>;
                Console.WriteLine("下载完成{0}",dict["Url"]);
                //var task = (dict.ContainsKey("Task")&&null != dict["Task"])?dict["Task"].ToString():string.Empty; ///提取链接
                //if ("ExtractLink" == task)
                //{
                //    dict["Task"] = "Done";
                //}
                //else
                //{
                //    dict["Task"] = "ExtractLink";
                //}

                mongo.Save("ths", "news", dict);
            }
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Loader_EventUrlEnqueue(object sender, EventArgs e)
        {
            Console.WriteLine("Loader_EventUrlEnqueue");
            var res = mongo.Find("ths", "news", "{ Status:'NotDownloaded' }"); ///要下载的列表页面
            if(CollectionTools.CanTraverse(res)) ///若允许遍历
            {
                foreach (var item in res)
                {
                    if (null != item["Url"] && item["Url"].ToString().ToLower().StartsWith("http://")) ///若数据有效
                    {
                        loader.UrlEnqueue(item);
                    }
                }
            }
            else
            {
                var root = new Dictionary<string, object>();
                root["Url"] = "http://news.10jqka.com.cn/today_list/";
                root["Status"] = "NotDownloaded";
                root["Task"] = "ExtractLink"; ///尚未下载,需要分析链接
                root["EntryPoint"] = "True"; ///是否是进入节点
                loader.UrlEnqueue(root);
            }



        }
 

        #region 设置程序的基本信息
        static void SetConsoleInfo()
        {
            Console.Title = string.Format("本地文件遍历器  开始时间:{0}",Program.StartTime);
        }
        #endregion
    }
}
