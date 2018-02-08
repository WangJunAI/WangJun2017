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
    public class DataSourceTHS
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
                        Console.WriteLine("正在添加 {0}\t{1} 当前第{2}页 共{3}页", stockCode, stockName, i + 1, pageCount);

                    }
                }
                #endregion
                Thread.Sleep(new Random().Next(1000, 3000));
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

        #region 下载指定股票的资金流向
        /// <summary>
        /// 下载指定股票的资金流向
        /// </summary>
        /// <param name="stockcode"></param>
        /// <returns></returns>
        public string GetJZLX(string stockcode)
        {
            var httpdownloader = new HTTP();
            string url = string.Format("http://stockpage.10jqka.com.cn/{0}/funds/", stockcode);

            var headers = new Dictionary<HttpRequestHeader, string>();
            headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
            headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4");
            headers.Add(HttpRequestHeader.Host, "stockpage.10jqka.com.cn");
            headers.Add(HttpRequestHeader.Referer, "http://www.10jqka.com.cn/");
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
            var html = httpdownloader.GetGzip2(url, Encoding.UTF8, headers);

            return html;
        }
        #endregion

        #region 获取指定日期的新闻列表
        /// <summary>
        /// 获取指定日期的新闻列表
        /// </summary>
        /// <param name="dateTime">格式：20171129</param>
        /// <returns></returns>
        public string GetNewsListCJYW(DateTime dateTime)
        {
            var httpdownloader = new HTTP();
            string url = string.Format("http://news.10jqka.com.cn/today_list/{0}/", string.Format("{0:yyyyMMdd}", dateTime));

            var headers = new Dictionary<HttpRequestHeader, string>();
            headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            headers.Add(HttpRequestHeader.Host, "news.10jqka.com.cn");
            headers.Add(HttpRequestHeader.Referer, "http://news.10jqka.com.cn/today_list/");
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
            var html = httpdownloader.GetGzip2(url, Encoding.GetEncoding("GBK"), headers);
            if(string.IsNullOrWhiteSpace(html))
            {

            }
            return html;
        }
        #endregion

        #region 下载指定的新闻内容
        /// <summary>
        /// 下载指定的新闻内容
        /// </summary>
        /// <param name="articleUrl">http://news.10jqka.com.cn/20171129/c601825811.shtml</param>
        /// <param name="parentUrl">http://news.10jqka.com.cn/today_list/20171129/</param>
        /// <returns></returns>
        public string GetNewsArticle(string articleUrl,string parentUrl)
        {
            var httpdownloader = new HTTP();

            var headers = new Dictionary<HttpRequestHeader, string>();
            headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            headers.Add(HttpRequestHeader.Host, "news.10jqka.com.cn");
            headers.Add(HttpRequestHeader.Referer, parentUrl);
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
            var html = httpdownloader.GetGzip2(articleUrl, Encoding.GetEncoding("GBK"), headers);

            return html;
        }

        #endregion

        #region 下载个股龙虎榜
        /// <summary>
        /// 下载个股龙虎榜
        /// </summary>
        /// <returns></returns>
        public string GetGGLHB(string stockcode)
        {
            var httpdownloader = new HTTP();

            var headers = new Dictionary<HttpRequestHeader, string>();
            headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4");
            headers.Add(HttpRequestHeader.Host, "data.10jqka.com.cn");
            headers.Add(HttpRequestHeader.Referer, "http://data.10jqka.com.cn/market/longhu/");
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");

            var url = string.Format("http://data.10jqka.com.cn/market/lhbgg/code/{0}/", stockcode);
            var html = httpdownloader.GetGzip2(url, Encoding.GetEncoding("GBK"), headers);///下载的个股龙虎榜页面
            return html;
        }
        #endregion

        #region 
        public List<string> GetUrlGGLHBMX(string pagegglhbHtml)
        {
            var list = new List<string>();
            #region 分析个股龙虎榜明细
            var tableStartIndex = pagegglhbHtml.IndexOf("<tbody>"); ///个股龙虎榜表格数据开始位置
            var tableEndIndex = pagegglhbHtml.IndexOf("</tbody>");///个股龙虎榜表格数据结束位置
            var tableHtml = pagegglhbHtml.Substring(tableStartIndex, tableEndIndex - tableStartIndex);
            var tdArray = tableHtml.Split(new string[] { "<td>", "</td>" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var td in tdArray)
            {
                if (td.Contains("rid") && td.Contains("date"))
                {
                    var paramArray = td.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    var date = string.Empty;
                    var rid = string.Empty;
                    var code = string.Empty;
                    foreach (var param in paramArray)
                    {
                        if (param.Contains("code="))
                        {
                            code = param.Substring(6, 6);
                        }
                        else if (param.Contains("date="))
                        {
                            date = param.Substring(6, 10);
                        }
                        else if (param.Contains("rid="))
                        {
                            rid = param.Substring(5, 2).Replace("\"", string.Empty);
                        }

                        if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(date) && !string.IsNullOrWhiteSpace(rid))
                        {
                            var urlgglhbmx = string.Format("http://data.10jqka.com.cn/ifmarket/getnewlh/code/{0}/date/{1}/rid/{2}/", code, date, rid); ///个股龙虎榜明细
                            list.Add(urlgglhbmx);
                        }
                    }
                }
            }
            #endregion

            return list;
        }

        #endregion

        #region 下载个股龙虎榜明细
        /// <summary>
        /// 下载个股龙虎榜明细
        /// </summary>
        /// <param name="stockcode"></param>
        /// <param name="date"></param>
        /// <param name="rid"></param>
        /// <returns></returns>
        public string GetGGLHBMX(string stockcode,string url)
        {
            var httpdownloader = new HTTP();

            //var url = string.Format("http://data.10jqka.com.cn/ifmarket/getnewlh/code/{0}/date/{1}/rid/{2}/", stockcode, date, rid); ///个股龙虎榜明细
            var headers = new Dictionary<string, string>();
            headers.Add("Accept", "*/*");
            headers.Add("Accept-Encoding", "gzip, deflate");
            headers.Add("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4");
            headers.Add("Host", "data.10jqka.com.cn");
            headers.Add("Referer", string.Format("http://data.10jqka.com.cn/market/lhbgg/code/{0}/", stockcode));
            headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
            headers.Add("X-Requested-With", "XMLHttpRequest");

            ///下载明细
            var html = httpdownloader.GetGzip(url, Encoding.GetEncoding("GBK"), headers);
            return html;
        }
        #endregion

        #region 获取页面
        /// <summary>
        /// 获取页面
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="stockcode"></param>
        /// <param name="url"></param>
        /// <param name="exData"></param>
        /// <returns></returns>
        public string GetPage(string contentType , string stockcode, string url=null, Dictionary<string, object> exData = null)
        {
            var html = string.Empty;
            if("首页概览" == contentType)
            {
                html = this.GetSYGL(stockcode);
            }
            else if ("资金流向" == contentType)
            {
                html = this.GetJZLX(stockcode);
            }
            else if ("个股龙虎榜" == contentType)
            {
                html = this.GetGGLHB(stockcode);
            }
            else if ("个股龙虎榜明细" == contentType)
            {
                html = this.GetGGLHBMX(stockcode,url);
            }
            else if("SINA个股历史交易" == contentType)
            {
                var sina = DataSourceSINA.GetInstance();
                var year = (int)exData["Year"];
                var jidu = (int)exData["JiDu"];
                html = sina.GetLSJY(stockcode, year, jidu);
            }
            else if ("SINA个股历史交易明细" == contentType)
            {
                var sina = DataSourceSINA.GetInstance();
                var year = (int)exData["Year"];
                var jidu = (int)exData["JiDu"];
                html = sina.GetLSJY(stockcode, year, jidu);
            }
            else if("THS财经要闻列表" == contentType)
            {

            }
            return html;
        }
        #endregion
    }
}
