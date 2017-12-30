using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.Data;
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
                var resDict = NodeService.Get(CONST.NodeServiceUrl, "新浪", "GetDataFromHtml", new { ContentType = "SINA财务摘要", Page=html });
                return resDict;
            }

            return res;
        }

        #endregion

        #region 获取SINA公司简介数据
        /// <summary>
        /// 获取SINA公司简介数据
        /// </summary>
        /// <param name="stockCode"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public object GetGSJJ(string stockCode, string source = "SINA")
        {
            var res = new object();

            if ("SINA" == source.ToUpper() && !string.IsNullOrWhiteSpace(stockCode)) //
            {
                var html = DataSourceSINA.CreateInstance().GetGSJJ(stockCode);
                if(string.IsNullOrWhiteSpace(html))
                {
                    html = WebDataSource.GetDataFromHttpAgent(string.Format("http://aifuwu.wang/API.ashx?c=WangJun.Stock.DataSourceSINA&m=GetGSJJ&p={0}", stockCode));
                }
                
                var resDict = NodeService.Get(CONST.NodeServiceUrl, "新浪", "GetDataFromHtml", new { ContentType = "SINA公司简介", Page = html });
                return resDict;
            }

            return res;
        }
        #endregion

        #region 获取SINA大单数据
        public object GetSINADaDan()
        {
            return "";
        }
        #endregion

        #region 获取SINA日线数据
        /// <summary>
        /// 获取SINA日线数据
        /// </summary>
        /// <param name="stockCode"></param>
        /// <param name="year"></param>
        /// <param name="jidu"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public Dictionary<string,object> GetSINAKLineDay(string stockCode, int year ,int jidu,string source = "SINA")
        {
            var html = DataSourceSINA.CreateInstance().GetLSJY(stockCode, year, jidu);
            var res = NodeService.Get(CONST.NodeServiceUrl, "新浪", "GetDataFromHtml", new { ContentType = "SINA历史交易", Page = html });// 
            var resDict= res as Dictionary<string, object>;
            return resDict["PageData"] as Dictionary<string, object>;
        }
        #endregion

        #region 获取SINA板块概念
        /// <summary>
        /// 获取SINA板块概念
        /// </summary>
        /// <param name="stockCode"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public object GetBKGN(string stockCode, string source = "SINA")
        {
            var res = new object();

            if ("SINA" == source.ToUpper() && !string.IsNullOrWhiteSpace(stockCode)) //
            {
                var html = DataSourceSINA.CreateInstance().GetBKGN(stockCode);
                if (string.IsNullOrWhiteSpace(html))
                {
                    html = WebDataSource.GetDataFromHttpAgent(string.Format("http://aifuwu.wang/API.ashx?c=WangJun.Stock.DataSourceSINA&m=GetBKGN&p={0}", stockCode));
                }

                var resDict = NodeService.Get(CONST.NodeServiceUrl, "新浪", "GetDataFromHtml", new { ContentType = "SINA板块概念", Page = html });
                return resDict;
            }

            return res;
        }
        #endregion

        #region 获取THS资金流向
        /// <summary>
        /// 获取THS资金流向
        /// </summary>
        /// <param name="stockCode"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public object GetZJLX(string stockCode, string source = "THS")
        {
            var res = new object();

            if ("THS" == source.ToUpper() && !string.IsNullOrWhiteSpace(stockCode)) //
            {
                var html = DataSourceTHS.CreateInstance().GetJZLX(stockCode);
                if (string.IsNullOrWhiteSpace(html))
                {
                    html = WebDataSource.GetDataFromHttpAgent(string.Format("http://aifuwu.wang/API.ashx?c=WangJun.Stock.DataSourceTHS&m=GetZJLX&p={0}", stockCode));
                }

                var resDict = NodeService.Get(CONST.NodeServiceUrl, "同花顺", "GetDataFromHtml", new { ContentType = "THS资金流向", Page = html });
                return resDict;
            }

            return res;
        }
        #endregion

        #region 获取SINA股票雷达
        /// <summary>
        /// 获取SINA股票雷达
        /// </summary>
        /// <returns></returns>
        public List<Dictionary<string,object>> GetStockRadar()
        {
            var tradingDate = Convertor.CalTradingDate(DateTime.Now, "00:00:00");
            var listHtml = DataSourceSINA.CreateInstance().GetStockRadar();
            var listData = new List<Dictionary<string, object>>();
            foreach (var html in listHtml)
            {
                var resDict = NodeService.Get(CONST.NodeServiceUrl, "新浪", "GetDataFromHtml", new { ContentType = "SINA股市雷达", Page = html }) as Dictionary<string,object>;
                var pageData = resDict["PageData"] as ArrayList;
                if(pageData is ArrayList)
                {
                    var itemList = pageData as ArrayList;
                    foreach (Dictionary<string,object> item in itemList)
                    {
                        var dict = item;
                        dict["异动时间"] = Convertor.CalTradingDate(DateTime.Now, item["异动时间"].ToString());
                        listData.Add(dict);
                    }
                }
            }

            return listData; 
        }
        #endregion
    }
}
