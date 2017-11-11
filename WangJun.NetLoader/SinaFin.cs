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

namespace WangJun.NetLoader
{
    public class SinaFin
    {
        protected HTTP http = new HTTP("gbk");

        protected MongoDB mongo = MongoDB.GetInst("mongodb://192.168.0.140:27017");

        protected Dictionary<string, string> stockCodeDict = new Dictionary<string, string>();

        public void GetLargeFundsTracking()
        {
             
            while (true)
            {
                if ((DateTime.Today.AddHours(16) < DateTime.Now )
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
                            ContentType = "大单追踪实时数据",
                            Url = url,
                            CreatTime = DateTime.Now,
                            Page = pageContent,
                            MD5 = Convertor.Encode_MD5(pageContent),
                            TaskID = THS.CONST.TaskID,
                            TradingDate = DateTime.Now
                        };
                        mongo.Save("SINA", "DaDan", data);
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
        /// 进行任务检查
        /// </summary>
        /// <returns></returns>
        public bool CheckTask()
        {
            return false;
        }
        public void Run()
        {


        }
    }
}
