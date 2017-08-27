using System;
using System.Collections.Generic;
using System.IO;
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

            #region SQLServer 注册
            SQLServer.Register("140", @"Data Source=192.168.0.140\sql2016;Initial Catalog=WJBigData;Persist Security Info=True;User ID=sa;Password=111qqq!!!");
            var mssql = SQLServer.GetInstance("140");
            #endregion

            #region MySQL注册
            MySQL.Register("140", @"server=192.168.0.140;user=root;database=WJBigData;port=3306;password=111qqq!!!");
            var mysql = MySQL.GetInstance("140");
            #endregion

            #region   MongoDB注册

            #endregion

            ///事件订阅
            //loader.EventUrlEnqueue += Loader_EventUrlEnqueue;
            //loader.EventDownloadCompleted += Loader_EventDownloadCompleted;


            //loader.Run();

            var arr = File.ReadLines(@"E:\zhwiki-20170720-pages-articles-multistream.xml\zhwiki-20170720-pages-articles-multistream.xml");
            var page = new StringBuilder();
            var count = 0;
            foreach (var item in arr)
            {

                if ("<page>" == item.ToLower().Trim()) ///若是文章开始
                {
                    page.Clear();
                    page.AppendLine(item);
                }
                else if ("</page>" == item.ToLower().Trim()) ///若是文章结束
                {
                    page.AppendLine(item);
                    string content = page.ToString();

                    //Console.Clear();
                    //Console.WriteLine(content);
                    page.Clear();
                    //mongo.Save("wiki", "page", new { Content = content });
                    mssql.Save("INSERT INTO [Page] ([Page]) VALUES (@Page)", new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("@Page", content) });
                    Console.WriteLine("已存储 " + (++count) + "  " );
                }
                else if (6 <= page.Length)
                {
                    page.AppendLine(item);
                }
            }



            //var arr = File.ReadLines(@"E:\zhwiki-20170720-pages-articles-multistream-index.txt\zhwiki-20170720-pages-articles-multistream-index.txt");
            //var page = new StringBuilder();
            //var count = 0;
            //foreach (var item in arr)
            //{

            //    //mongo.Save("wiki", "index", new { Content = item }); 保存到MongoDB数据库中
            //    var v = item.Split(new char[] { ':' }, 3);
            //    mssql.Save("INSERT INTO [Keyword] ([Keyword],[V1],[V2],[V3]) VALUES (@Keyword,@V1,@V2,@V3)", new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("@Keyword", item), new KeyValuePair<string, object>("@V1", v[0].Trim()) ,new KeyValuePair<string, object>("@V2", v[1].Trim()), new KeyValuePair<string, object>("@V3", v[2].Trim()) });
            //    //mysql.Save("INSERT INTO Keyword (Keyword) VALUES (@Keyword)", new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("@Keyword", item) });
            //    Console.WriteLine("已存储 " + (++count) + "  " + item);

            //}


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
