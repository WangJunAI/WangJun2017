using System;
using System.Collections.Generic;
using System.Linq;
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

            var httpDownloader = new HTTP("GBK");
            var headers = new Dictionary<string, string>();
            headers.Add("Accept", "*/*");
            headers.Add("Accept-Encoding", "gzip,deflate");
            headers.Add("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4");
            headers.Add("Content-type", "application/x-www-form-urlencoded");
            headers.Add("Host", "vip.stock.finance.sina.com.cn");
            headers.Add("Referer", "http://vip.stock.finance.sina.com.cn/quotes_service/view/cn_bill_all.php");
            headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");

            var volume = 100;
            string pageCountUrl = string.Format("http://vip.stock.finance.sina.com.cn/quotes_service/api/json_v2.php/CN_Bill.GetBillListCount?num=100&page=1363&sort=ticktime&asc=0&volume={0}&type=0",volume);//(new String("283802"))
            string pageCountText = httpDownloader.GetGzip(pageCountUrl, Encoding.GetEncoding("GBK"), headers);
            var pageCount = 0;

            if(pageCountText.Contains("(new String("))
            {
                pageCountText = pageCountText.Replace("(new String(",string.Empty).Replace(")",string.Empty).Replace("\"",string.Empty).Trim('\0');
                pageCount = (0 < int.Parse(pageCountText) % volume) ? int.Parse(pageCountText) + 1 : int.Parse(pageCountText);
                
            }

            for (int k= 1; k <= pageCount; k++)
            {
                var url = string.Format("http://vip.stock.finance.sina.com.cn/quotes_service/api/json_v2.php/CN_Bill.GetBillList?num=100&page={0}&sort=ticktime&asc=0&volume={1}&type=0",k,volume);
                var pageContent = httpDownloader.GetGzip(url, Encoding.GetEncoding("GBK"), headers);
                var data = new
                {
                    ContentType = "大单追踪实时数据",
                    Url = url,
                    CreatTime = DateTime.Now,
                    Page = pageContent,
                    MD5 = Convertor.Encode_MD5(pageContent),
                    TaskID = THS.CONST.TaskID
                };
                mongo.Save("SINA", "DaDan", data);
                Console.WriteLine("正在下载 {0} 共 {1}页 ", url, pageCount);
                Thread.Sleep(1000);
            }

        }
    }
}
