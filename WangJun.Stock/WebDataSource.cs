using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.Net;
using WangJun.NetLoader;

namespace WangJun.Stock
{
    public class WebDataSource
    {
        public static WebDataSource GetInstance()
        {
            var inst = new WebDataSource();

            return inst;
        }
        public Dictionary<string, string> GetAllStockCode(string source="THS")
        {
            var res = new Dictionary<string, string>();
            if("THS" == source.ToUpper())
            {
                res = DataSourceTHS.CreateInstance().GetAllStockCode();
            }

            return res;
        }

        public static string GetDataFromHttpAgent(string url)
        {
            var httpDownloader = new HTTP();
            var html = httpDownloader.GetGzip2(url, Encoding.UTF8);
            return html;
        }

        #region 获取SINA财务摘要数据
        /// <summary>
        /// 获取SINA财务摘要数据
        /// </summary>
        /// <param name="stockCode"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public object GetCWZY(string stockCode,string source ="SINA")
        {
            var res = new object();
            if ("SINA" == source.ToUpper()&& !string.IsNullOrWhiteSpace(stockCode))//  //WebDataSource.GetDataFromHttpAgent("http://aifuwu.wang/API.ashx?c=WangJun.Stock.DataSourceSINA&m=GetCWZY&p=6018888");//
            {
                var html = DataSourceSINA.CreateInstance().GetCWZY(stockCode);
                var resDict = NodeService.Get("http://localhost:8990", "新浪", "GetDataFromHtml", new { ContentType = "SINA财务摘要", Page=html });
                return resDict;
            }

            return res;
        }

        #endregion

    }
}
