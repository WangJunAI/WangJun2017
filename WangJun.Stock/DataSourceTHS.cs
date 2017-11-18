using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WangJun.Net;

namespace WangJun.Stock
{
    /// <summary>
    /// 同花顺数据源
    /// </summary>
    public  class DataSourceTHS
    {
        public static DataSourceTHS CreateInstance()
        {
            return new DataSourceTHS();
        }

        #region 从网页获取所有的股票代码
        /// <summary>
        /// 从网页获取所有的股票代码
        /// </summary>
        public Dictionary<string, string> GetAllStockCode()
        {
            var stockCodeDict = new Dictionary<string, string>();
            var httpdownloader = new HTTP();

            var headers = new Dictionary<string, string>();
            headers.Add("Accept", "text/html,*/*; q=0.01");
            headers.Add("Accept-Encoding", "gzip, deflate");
            headers.Add("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4");
            headers.Add("Host", "q.10jqka.com.cn");
            headers.Add("Referer", "http://q.10jqka.com.cn/");
            headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
            headers.Add("X-Requested-With", "XMLHttpRequest");

            #region 获取分页数 
            var pageCount = 0;
            var pagerUrl = string.Format(@"http://q.10jqka.com.cn/index/index/board/all/field/zdf/order/desc/page/{0}/ajax/1/", 1);
            var pagerHtml = httpdownloader.GetGzip(pagerUrl, Encoding.GetEncoding("GBK"), headers);
            if (100 < pagerHtml.Length)
            {
                pageCount = int.Parse(pagerHtml.Substring(pagerHtml.LastIndexOf("</span>") - 3, 3)); ///页码数
                Console.WriteLine("股票代码页数{0}\t{1}", pageCount, DateTime.Now);
            }
            #endregion

            var stringBuilder = new StringBuilder();
            for (int i = 1; i <= pageCount; i++)
            {
                var url = string.Format(@"http://q.10jqka.com.cn/index/index/board/all/field/zdf/order/desc/page/{0}/ajax/1/", i);
                var html = httpdownloader.GetGzip(url, Encoding.GetEncoding("GBK"), headers);

                #region 股票代码处理
                string[] trArray = html.Substring(html.IndexOf("<tbody>") + "<tbody>".Length).Split(new string[] { "<tr>", "</tr>" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var tr in trArray)
                {
                    var tdArray = tr.Split(new string[] { "<td>", "</td>" }, StringSplitOptions.RemoveEmptyEntries);
                    if (10 < tdArray.Length)
                    {
                        var stockCode = tdArray[3].Substring(tdArray[3].IndexOf("target=\"_blank\">") + "target=\"_blank\">".Length).Replace("</a>", string.Empty).Replace("&#032;", "");
                        var stockName = tdArray[5].Substring(tdArray[5].IndexOf("target=\"_blank\">") + "target=\"_blank\">".Length).Replace("</a>", string.Empty).Replace("&#032;", "");
                        if (!stockCodeDict.ContainsKey(stockCode) && !stockCodeDict.ContainsValue(stockName))
                        {
                            stockCodeDict.Add(stockCode, stockName);
                        }
                        else
                        {
                            throw new Exception("数据重复");
                        }
                        Console.WriteLine("正在添加 {0}\t{1}", stockCode, stockName);
                        
                    }
                }
                #endregion
                Thread.Sleep(2000);
            }

            return stockCodeDict;
        }
        #endregion

        #region 下载指定股票的首页概览
        /// <summary>
        /// 下载指定股票的首页概览
        /// </summary>
        /// <param name="stockcode">股票代码</param>
        /// <returns></returns>
        public string GetSYGL(string stockcode)
        {
            var httpdownloader = new HTTP();
            var headers = new Dictionary<HttpRequestHeader, string>();
            headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
            headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4");
            headers.Add(HttpRequestHeader.Host, "stockpage.10jqka.com.cn");
            headers.Add(HttpRequestHeader.Referer, "http://www.10jqka.com.cn/");
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
            var url = string.Format("http://stockpage.10jqka.com.cn/{0}/", stockcode);
            var html = httpdownloader.GetGzip2(url, Encoding.UTF8, headers);

            return html;
        }
        #endregion

    }
}
