using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using WangJun.Net;
using WangJun.Utility;

namespace WangJun.Stock
{
    /// <summary>
    /// 新浪数据源
    /// </summary>
    public class DataSourceSINA
    {
        public static DataSourceSINA GetInstance()
        {
            return new DataSourceSINA();
        }

        #region 历史交易
        /// <summary>
        /// 获取历史交易
        /// </summary>
        /// <param name="stockcode"></param>
        /// <param name="year"></param>
        /// <param name="jidu"></param>
        /// <returns></returns>
        /// <example>view-source:http://vip.stock.finance.sina.com.cn/corp/go.php/vMS_MarketHistory/stockid/600617.phtml?year=2017&jidu=1</example>
        public string GetLSJY(string stockcode, int year, int jidu)
        {
            string url = string.Format("http://vip.stock.finance.sina.com.cn/corp/go.php/vMS_MarketHistory/stockid/{0}.phtml?year={1}&jidu={2}", stockcode, year, jidu);
            var httpdownloader = new HTTP();
            var headers = new Dictionary<HttpRequestHeader, string>();
            headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4");
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
            headers.Add(HttpRequestHeader.Referer, url);

            var strData = httpdownloader.GetGzip2(url, Encoding.GetEncoding("GBK"), headers);
            return strData;
        }
        #endregion


        #region 历史成交明细 页数
        /// <summary>
        /// 历史成交明细 页数
        /// http://market.finance.sina.com.cn/transHis.php?date=2017-10-13&symbol=sh600036
        /// </summary>
        public int GetLSCJMXCount(string stockcode, string date)
        {
            string url = string.Format("http://market.finance.sina.com.cn/transHis.php?date={0}&symbol={1}", date, Convertor.AddStockCodePrefix(stockcode));
            var httpdownloader = new HTTP();
            var headers = new Dictionary<HttpRequestHeader, string>();
            headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            headers.Add(HttpRequestHeader.Host, "market.finance.sina.com.cn");
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
            headers.Add(HttpRequestHeader.Cookie, "U_TRS1=000000bc.81346475.5a10967a.97e5dd54; U_TRS2=000000bc.813e6475.5a10967a.ecb434e5; FINANCE2=56e2a29d3a8fe026d3c022d0667dda04");

            var strData = httpdownloader.GetGzip2(url, Encoding.GetEncoding("GBK"), headers);
            if (!strData.Contains("输入的代码有误或没有交易数据"))
            {
                var startIndex = strData.IndexOf("var detailPages=") + "var detailPages=".Length;
                var endIndex = strData.IndexOf("var detailDate =");
                var subString = strData.Substring(startIndex, endIndex - startIndex);
                var array = subString.Replace(";", string.Empty).Replace("[[", string.Empty).Replace("]]", string.Empty).Replace("\r\n", string.Empty).Split(new string[] { "],[" }, StringSplitOptions.RemoveEmptyEntries);
                return array.Length;
            }
            return 0;
        }
        #endregion

        #region  历史成交明细
        /// <summary>
        /// 历史成交明细
        /// http://vip.stock.finance.sina.com.cn/quotes_service/view/vMS_tradehistory.php?symbol=sh600036&date=2017-10-13
        /// </summary>
        /// <param name="stockcode">类似于 sz300668</param>
        /// <param name="date"></param>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        public string GetLSCJMX(string stockcode, string date, int pageNumber)
        {
            string url = string.Format("http://vip.stock.finance.sina.com.cn/quotes_service/view/vMS_tradehistory.php?symbol={0}&date={1}&page={2}", Convertor.AddStockCodePrefix(stockcode), date, pageNumber);
            var httpdownloader = new HTTP();
            var headers = new Dictionary<HttpRequestHeader, string>();
            headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            headers.Add(HttpRequestHeader.Host, "vip.stock.finance.sina.com.cn");
            headers.Add(HttpRequestHeader.Referer, string.Format("http://vip.stock.finance.sina.com.cn/quotes_service/view/vMS_tradehistory.php?symbol={0}&date={1}", stockcode, date));
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
            headers.Add(HttpRequestHeader.Cookie, "vjuids=-3d20ba9f8.150715b95a8.0.9e4d081a; SGUID=1445010838969_24869349; U_TRS1=00000077.eff21405.56211d98.e270c652; SCF=AiNSdX-kSf4x3r5s3OPpIKeILKce6Ob9lRa-jJt11Vh5r8kMpPU24v_1hNWF-ZDmw1aBetJmtrTPFQDDGaqUuMU.; SINAGLOBAL=123.138.24.104_1468410916.956202; sso_info=v02m6alo5qztKWRk5iljpSMpZCToKWRk5SlkJSQpY6TgKWRk5iljpSQpY6ElLGOk6SziaeVqZmDtLKNs4C2jJOct4yTlLA=; visited_uss=gb_wb; UOR=,,; SR_SEL=1_511; lxlrtst=1509781793_o; close_left_xraytg=1; ");

            var strData = httpdownloader.GetGzip2(url, Encoding.GetEncoding("GBK"), headers);
            return strData;
        }
        #endregion

        #region 获取指定股票的历史成交明细
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stockcode"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public List<string> GetLSCJMX(string stockcode, string date)
        {
            List<string> list = new List<string>();
            var pageCount = this.GetLSCJMXCount(stockcode, date);
            for (int k = 1; k < pageCount; k++)
            {
                var html = this.GetLSCJMX(stockcode, date, k);
                list.Add(html);
                ThreadManager.Pause(seconds: 2);
            }
            return list;
        }
        #endregion

        #region 获取大单页码数
        /// <summary>
        /// 获取大单页码数
        /// </summary>
        /// <returns></returns>
        public int GetDaDanPageCount()
        {
            var httpDownloader = new HTTP();
            var headers = new Dictionary<HttpRequestHeader, string>();
            headers.Add(HttpRequestHeader.Accept, "*/*");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
            headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
            headers.Add(HttpRequestHeader.Host, "vip.stock.finance.sina.com.cn");
            headers.Add(HttpRequestHeader.Referer, "http://vip.stock.finance.sina.com.cn/quotes_service/view/cn_bill_all.php");
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");

            var volume = 100;///一手
            string pageCountUrl = string.Format("http://vip.stock.finance.sina.com.cn/quotes_service/api/json_v2.php/CN_Bill.GetBillListCount?num=100&page=1363&sort=ticktime&asc=0&volume={0}&type=0", volume);//(new String("283802"))
            string pageCountText = httpDownloader.GetGzip2(pageCountUrl, Encoding.GetEncoding("GBK"), headers);
            var pageCount = 0;
            var pageSize = 100;

            if (pageCountText.Contains("(new String("))
            {
                pageCountText = pageCountText.Replace("(new String(", string.Empty).Replace(")", string.Empty).Replace("\"", string.Empty).Trim('\0');
                pageCount = (0 < int.Parse(pageCountText) % volume) ? int.Parse(pageCountText) / pageSize + 1 : int.Parse(pageCountText) / pageSize;
            }

            return pageCount;
        }
        #endregion

        #region 获取大单数据
        /// <summary>
        /// 获取大单数据
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="volume"></param>
        /// <returns></returns>
        public string GetDaDan(object pageIndex)
        {
            int volume = 100;
            var httpDownloader = new HTTP();
            var headers = new Dictionary<HttpRequestHeader, string>();
            headers.Add(HttpRequestHeader.Accept, "*/*");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
            headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
            headers.Add(HttpRequestHeader.Host, "vip.stock.finance.sina.com.cn");
            headers.Add(HttpRequestHeader.Referer, "http://vip.stock.finance.sina.com.cn/quotes_service/view/cn_bill_all.php");
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
            var url = string.Format("http://vip.stock.finance.sina.com.cn/quotes_service/api/json_v2.php/CN_Bill.GetBillList?num=100&page={0}&sort=ticktime&asc=0&volume={1}&type=0", pageIndex, volume);
            var pageContent = httpDownloader.GetGzip2(url, Encoding.GetEncoding("GBK"), headers);

            return pageContent;
        }
        #endregion

        #region 获取财务摘要
        /// <summary>
        /// 获取财务摘要
        /// http://vip.stock.finance.sina.com.cn/corp/go.php/vFD_FinanceSummary/stockid/601888.phtml
        /// </summary>
        /// <param name="stockCode"></param>
        public string GetCWZY(string stockCode)
        {
            var url = string.Format("http://vip.stock.finance.sina.com.cn/corp/go.php/vFD_FinanceSummary/stockid/{0}.phtml", stockCode);
            var httpDownloader = new HTTP();
            var headers = new Dictionary<HttpRequestHeader, string>();
            headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            headers.Add(HttpRequestHeader.Host, "money.finance.sina.com.cn");
            headers.Add(HttpRequestHeader.Referer, string.Format("http://money.finance.sina.com.cn/corp/go.php/vFD_FinanceSummary/stockid/{0}/displaytype/4.phtml", stockCode));
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0(Windows NT 10.0; Win64; x64) AppleWebKit/537.36(KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36");

            var strData = httpDownloader.GetGzip2(url, Encoding.GetEncoding("GBK"), headers);
            return strData;

        }
        #endregion

        #region 获取公司简介
        /// <summary>
        /// 获取公司简介
        /// http://vip.stock.finance.sina.com.cn/corp/go.php/vCI_CorpInfo/stockid/600698.phtml
        /// </summary>
        /// <returns></returns>
        public string GetGSJJ(string stockCode)
        {
            var url = string.Format("http://vip.stock.finance.sina.com.cn/corp/go.php/vCI_CorpInfo/stockid/{0}.phtml", stockCode);
            var httpDownloader = new HTTP();
            var headers = new Dictionary<HttpRequestHeader, string>();
            headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            headers.Add(HttpRequestHeader.Host, "vip.stock.finance.sina.com.cn");
            headers.Add(HttpRequestHeader.Referer, string.Format("http://vip.stock.finance.sina.com.cn/corp/go.php/vCI_CorpOtherInfo/stockid/{0}/menu_num/2.phtml", stockCode));
            headers.Add(HttpRequestHeader.UserAgent, CONST.UserAgent);

            var strData = httpDownloader.GetGzip2(url, Encoding.GetEncoding("GBK"), headers);
            return strData;

        }
        #endregion

        #region 获取板块概念
        /// <summary>
        /// 获取板块概念
        /// </summary>
        /// <param name="stockCode"></param>
        /// <returns></returns>
        public string GetBKGN(string stockCode)
        {
            var url = string.Format("http://vip.stock.finance.sina.com.cn/corp/go.php/vCI_CorpOtherInfo/stockid/{0}/menu_num/5.phtml", stockCode);
            var httpDownloader = new HTTP();
            var headers = new Dictionary<HttpRequestHeader, string>();
            headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            headers.Add(HttpRequestHeader.Host, "vip.stock.finance.sina.com.cn");
            headers.Add(HttpRequestHeader.Referer, string.Format("http://vip.stock.finance.sina.com.cn/corp/go.php/vCI_CorpXiangGuan/stockid/{0}.phtml", stockCode));
            headers.Add(HttpRequestHeader.UserAgent, CONST.UserAgent);

            var strData = httpDownloader.GetGzip2(url, Encoding.GetEncoding("GBK"), headers);
            return strData;
        }
        #endregion

        #region 股市雷达
        /// <summary>
        /// 股市雷达 用于发现异常股票
        /// http://finance.sina.com.cn/stockradar/stockradar16.html
        /// </summary>
        /// <returns></returns>
        public List<string> GetStockRadar()
        {
            var list = new List<string>();
            for (int k = 1; k <= 16; k++)
            {
                var url = string.Format("http://finance.sina.com.cn/stockradar/stockradar{0}.html", k);
                var httpDownloader = new HTTP();
                var headers = new Dictionary<HttpRequestHeader, string>();
                headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
                headers.Add(HttpRequestHeader.Host, "finance.sina.com.cn");
                headers.Add(HttpRequestHeader.Referer, string.Format("http://finance.sina.com.cn/stockradar/stockradar{0}.html", (k % 14) + 2));
                headers.Add(HttpRequestHeader.UserAgent, CONST.UserAgent);

                var strData = httpDownloader.GetGzip2(url, Encoding.GetEncoding("GBK"), headers);
                list.Add(strData);
                LOGGER.Log(string.Format("正在获取 股市雷达 {0}", k));
                ThreadManager.Pause(seconds: 5);
            }

            return list;

        }
        #endregion

        #region 融资融券
        /// <summary>
        /// SINA 融资融券
        /// </summary>
        /// <returns></returns>
        public string GetRZRQ(string stockCode)
        {
            var symbol = Convertor.AddStockCodePrefix(stockCode);
            var startDate = string.Format("{0}-01-01", DateTime.Now.Year);
            var endDate = string.Format("{0:yyyy-MM-dd}", DateTime.Now);
            var url = string.Format("http://vip.stock.finance.sina.com.cn/q/go.php/vInvestConsult/kind/rzrq/index.phtml?symbol={0}&bdate={1}&edate={2}", symbol, startDate, endDate);
            var httpDownloader = new HTTP();
            var headers = new Dictionary<HttpRequestHeader, string>();
            headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            headers.Add(HttpRequestHeader.Host, "vip.stock.finance.sina.com.cn");
            headers.Add(HttpRequestHeader.UserAgent, CONST.UserAgent);

            var strData = httpDownloader.GetGzip2(url, Encoding.GetEncoding("GBK"), headers);
            return strData;
        }
        #endregion

        #region DownloadExcel
        public void DownloadExcel(DateTime date, string stockCode, string stockName)
        {
            var httpDownloader = new HTTP();
            var url = string.Format("http://market.finance.sina.com.cn/downxls.php?date={0}&symbol={1}", string.Format("{0:yyyy-MM-dd}", date), Convertor.AddStockCodePrefix(stockCode));
            var filePath = string.Format(@"F:\Excel\{0}{1}{2:yyyyMMdd}.xls", stockCode, stockName, date);
            filePath = filePath.Replace("*", "[星]");
            if (!File.Exists(filePath))
            {
                var data = httpDownloader.GetFile(url);
                File.WriteAllBytes(filePath, data);
                ThreadManager.Pause(seconds: 3);

            }

        }

        #endregion
    }
}
