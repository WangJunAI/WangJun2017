using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WangJun.Data;
using WangJun.DB;
using WangJun.Net;
using WangJun.Tools;

namespace WangJun.NetLoader
{
    public class SinaFin
    {
        protected HTTP http = new HTTP("gbk");

        protected MongoDB mongo = MongoDB.GetInst("mongodb://192.168.0.140:27017");

        protected Dictionary<string, string> stockCodeDict = new Dictionary<string, string>();

        public void GetPageDaDa()
        {
             
            while (true)
            {
                if ((DateTime.Today.AddHours(16) < DateTime.Now )///每日收盘后
                    && !(DateTime.Now.DayOfWeek == DayOfWeek.Sunday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday)) 
                {
                    #region
                    var httpDownloader = new HTTP("GBK");
                    var headers = new Dictionary<string, string>();
                    headers.Add("Accept", "*/*");
                    headers.Add("Accept-Encoding", "gzip,deflate");
                    headers.Add("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4");
                    headers.Add("Content-type", "application/x-www-form-urlencoded");
                    headers.Add("Host", "vip.stock.finance.sina.com.cn");
                    headers.Add("Referer", "http://vip.stock.finance.sina.com.cn/quotes_service/view/cn_bill_all.php");
                    headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");

                    var volume = 100;///一手
                    string pageCountUrl = string.Format("http://vip.stock.finance.sina.com.cn/quotes_service/api/json_v2.php/CN_Bill.GetBillListCount?num=100&page=1363&sort=ticktime&asc=0&volume={0}&type=0", volume);//(new String("283802"))
                    string pageCountText = httpDownloader.GetGzip(pageCountUrl, Encoding.GetEncoding("GBK"), headers);
                    var pageCount = 0;
                    var pageSize = 100;

                    if (pageCountText.Contains("(new String("))
                    {
                        pageCountText = pageCountText.Replace("(new String(", string.Empty).Replace(")", string.Empty).Replace("\"", string.Empty).Trim('\0');
                        pageCount = (0 < int.Parse(pageCountText) % volume) ? int.Parse(pageCountText) / pageSize + 1 : int.Parse(pageCountText) / pageSize;

                    }

                    for (int k = 1; k <= pageCount; k++)
                    {
                        var url = string.Format("http://vip.stock.finance.sina.com.cn/quotes_service/api/json_v2.php/CN_Bill.GetBillList?num=100&page={0}&sort=ticktime&asc=0&volume={1}&type=0", k, volume);
                        var pageContent = httpDownloader.GetGzip(url, Encoding.GetEncoding("GBK"), headers);
                        var data = new
                        {
                            ContentType = "SINA大单追踪实时数据",
                            Url = url,
                            CreatTime = DateTime.Now,
                            Page = pageContent,
                            MD5 = Convertor.Encode_MD5(pageContent),
                            TaskID = THS.CONST.TaskID,
                            TradingDate = DateTime.Now
                        };
                        mongo.Save("PageSource", "PageDaDan", data);
                        Console.WriteLine("正在下载 {0} 共 {1}页 ", url, pageCount);
                        Thread.Sleep(1000);
                    }
                    #endregion
                }
                else
                {
                    Console.WriteLine("资金流向大单追踪 目前没在开市时间 工作日 9:30 - 15:00 当前时间 " + DateTime.Now);
                    Thread.Sleep(60 * 1000);
                }

            }
        }

        /// <summary>
        /// 获取日K线数据
        /// </summary>
        /// <param name="stockCode"></param>
        /// <param name="year"></param>
        /// <param name="jidu"></param>
        /// <returns></returns>
        public string GetKLine(string stockCode,int year,int jidu)
        {
            var httpdownloader = new HTTP();
            var url = string.Format("http://vip.stock.finance.sina.com.cn/corp/go.php/vMS_MarketHistory/stockid/{0}.phtml?year={1}&jidu={2}", stockCode,year,jidu);
            var headers = new Dictionary<HttpRequestHeader, string>();
            headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4");
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
            headers.Add(HttpRequestHeader.Referer, url);
 
            var strData = httpdownloader.GetGzip2(url, Encoding.GetEncoding("GBK"), headers);
            return strData;
        }

        /// <summary>
        /// KLIine
        /// </summary>
        /// <param name="mongodbConnectionString"></param>
        /// <param name="mongodbDBName"></param>
        /// <param name="mongodbCollectionName"></param>
        /// <param name="sqlserverConnectionString"></param>
        /// <param name="sqlserverDBName"></param>
        /// <param name="sqlserverCollectionName"></param>
        public void ConvertKLine2D(string mongodbConnectionString, string mongodbDBName, string mongodbCollectionName, string sqlserverConnectionString, string sqlserverDBName, string sqlserverCollectionName)
        {
            #region SQLServer 注册
            SQLServer.Register("140", @"Data Source=192.168.0.140\sql2016;Initial Catalog=WJStock;Persist Security Info=True;User ID=sa;Password=111qqq!!!");
            var mssql = SQLServer.GetInstance("140");
            #endregion

            #region   MongoDB注册
            MongoDB mongo = MongoDB.GetInst("mongodb://192.168.0.140:27017");
            #endregion
            var count = 0;
            mongo.EventTraverse += (object sender, EventArgs e) =>
            {
                var ee = e as EventProcEventArgs;
                var dict = ee.Default as Dictionary<string, object>;
                var list = dict["Rows"] as Array;
                var lscjmx = dict["历史成交明细"] as Array;
                if (list is Array)
                {
                    var itemArrayIndex = 0;
                    string sql = "INSERT INTO  [DataDaDan2D]  ([dbItemID] ,[StockCode] ,[StockName] ,[TradingDate] ,[Price] ,[Volume] ,[PrevPrice] ,[Kind],[ItemArrayIndex])  VALUES (@dbItemID ,@StockCode ,@StockName ,@TradingDate ,@Price ,@Volume  ,@PrevPrice  ,@Kind,@ItemArrayIndex)";

                    foreach (var item in list)
                    {
                        var srcItem = item as Dictionary<string, object>;
                        var svItem = new Dictionary<string, object>();
                        svItem["dbItemID"] = dict["_id"].ToString();
                        svItem["StockCode"] = dict["StockCode"];
                        svItem["StockName"] = dict["StockName"];
                        svItem["日期"] = Convert.ToDateTime(srcItem["日期"]);
                        svItem["开盘价"] = float.Parse(srcItem["开盘价"].ToString());
                        svItem["最高价"] = float.Parse(srcItem["最高价"].ToString());
                        svItem["收盘价"] = float.Parse(srcItem["收盘价"].ToString());
                        svItem["最低价"] = float.Parse(srcItem["最低价"].ToString());
                        svItem["交易量(股)"] = long.Parse(srcItem["交易量(股)"].ToString());
                        svItem["交易金额(元)"] = long.Parse(srcItem["交易金额(元)"].ToString());
                        svItem["ItemArrayIndex"] = itemArrayIndex++;
                        svItem["Source"] = "SINA日线";

                        mongo.Save("DataSource2D", "DataKLine2D", svItem);

                        //var paramList = new List<KeyValuePair<string, object>>();
                        //paramList.Add(new KeyValuePair<string, object>("@dbItemID", svItem["dbItemID"]));
                        //paramList.Add(new KeyValuePair<string, object>("@StockCode", svItem["StockCode"]));
                        //paramList.Add(new KeyValuePair<string, object>("@StockName", svItem["StockName"]));
                        //paramList.Add(new KeyValuePair<string, object>("@TradingDate", svItem["TradingDate"]));
                        //paramList.Add(new KeyValuePair<string, object>("@Price", svItem["Price"]));
                        //paramList.Add(new KeyValuePair<string, object>("@Volume", svItem["Volume"]));
                        //paramList.Add(new KeyValuePair<string, object>("@PrevPrice", svItem["PrevPrice"]));
                        //paramList.Add(new KeyValuePair<string, object>("@Kind", svItem["Kind"]));
                        //paramList.Add(new KeyValuePair<string, object>("@ItemArrayIndex", svItem["ItemArrayIndex"]));

                        //mssql.Save(sql, paramList);
                    }
                     
                }

                if(lscjmx is Array)
                {
                    var itemArrayIndex = 0;
 
                    foreach (var item in lscjmx)
                    { 
                        var svItem = new Dictionary<string, object>();
                        svItem["dbItemID"] = dict["_id"].ToString();
                        svItem["StockCode"] = dict["StockCode"];
                        svItem["StockName"] = dict["StockName"]; 
                        svItem["ItemArrayIndex"] = itemArrayIndex++; 
                        svItem["Source"] = "要下载的SINA日线历史成交明细页面";
                        svItem["下载URL"] = item;
                        svItem["父页面URL"] = dict["Url"];
                        svItem["当前状态"] = "未开始下载页面";
                        svItem["下一步任务"] = "下载页面";
                        svItem["作业创建日期"] =DateTime.Now;

                        mongo.Save("Jobs", "DownloadQueue", svItem);
 
                    }
                }

                Console.WriteLine("成功插入" + dict["_id"] + " " + (++count));

            };
            mongo.Find("DataSource", "DataKLine", "{}");
        }



        /// <summary>
        /// 进行任务检查
        /// </summary>
        /// <returns></returns>
        public bool CheckDaDanTask()
        {
            ///检查今日数据是否已经下载,联网获取页数,比较数据中的数据量
            return false;
        }
        public void Run()
        {


        }
    }
}
