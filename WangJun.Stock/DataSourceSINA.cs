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
    /// 新浪数据源
    /// </summary>
    public  class DataSourceSINA
    {
        public static DataSourceSINA CreateInstance()
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
        public string GetLSJY(string stockcode,int year,int jidu)
        {
            string url = string.Format("http://vip.stock.finance.sina.com.cn/corp/go.php/vMS_MarketHistory/stockid/{0}.phtml?year={1}&jidu={2}",stockcode,year,jidu);
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
        /// </summary>
        public int GetLSCJMXCount(string stockcode, string date)
        {
            string url = string.Format("http://market.finance.sina.com.cn/transHis.php?date={0}&symbol={1}", date, stockcode);
            var httpdownloader = new HTTP();
            var headers = new Dictionary<HttpRequestHeader, string>();
            headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            headers.Add(HttpRequestHeader.Host, "market.finance.sina.com.cn");
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
            headers.Add(HttpRequestHeader.Cookie, "U_TRS1=000000bc.81346475.5a10967a.97e5dd54; U_TRS2=000000bc.813e6475.5a10967a.ecb434e5; FINANCE2=56e2a29d3a8fe026d3c022d0667dda04");

            var strData = httpdownloader.GetGzip2(url, Encoding.GetEncoding("GBK"), headers);

            var startIndex = strData.IndexOf("var detailPages=") + "var detailPages=".Length;
            var endIndex = strData.IndexOf("var detailDate =");
            var subString = strData.Substring(startIndex, endIndex - startIndex);
            var array = subString.Replace(";", string.Empty).Replace("[[",string.Empty).Replace("]]", string.Empty).Replace("\r\n", string.Empty).Split(new string[] { "],[" } , StringSplitOptions.RemoveEmptyEntries);
            return array.Length;
        }
        #endregion

        #region  历史成交明细
        /// <summary>
        /// 历史成交明细
        /// </summary>
        /// <param name="stockcode">类似于 sz300668</param>
        /// <param name="date"></param>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        public string GetLSCJMX(string stockcode, string date,int  pageNumber)
        {
            string url = string.Format("http://vip.stock.finance.sina.com.cn/quotes_service/view/vMS_tradehistory.php?symbol={0}&date={1}&page={2}", stockcode,date,pageNumber);
            var httpdownloader = new HTTP();
            var headers = new Dictionary<HttpRequestHeader, string>();
            headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            headers.Add(HttpRequestHeader.Host, "vip.stock.finance.sina.com.cn");
            headers.Add(HttpRequestHeader.Referer, string.Format("http://vip.stock.finance.sina.com.cn/quotes_service/view/vMS_tradehistory.php?symbol={0}&date={1}",stockcode,date));
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
            headers.Add(HttpRequestHeader.Cookie, "vjuids=-3d20ba9f8.150715b95a8.0.9e4d081a; SGUID=1445010838969_24869349; U_TRS1=00000077.eff21405.56211d98.e270c652; SCF=AiNSdX-kSf4x3r5s3OPpIKeILKce6Ob9lRa-jJt11Vh5r8kMpPU24v_1hNWF-ZDmw1aBetJmtrTPFQDDGaqUuMU.; SINAGLOBAL=123.138.24.104_1468410916.956202; sso_info=v02m6alo5qztKWRk5iljpSMpZCToKWRk5SlkJSQpY6TgKWRk5iljpSQpY6ElLGOk6SziaeVqZmDtLKNs4C2jJOct4yTlLA=; visited_uss=gb_wb; UOR=,,; SR_SEL=1_511; lxlrtst=1509781793_o; close_left_xraytg=1; ");

            var strData = httpdownloader.GetGzip2(url, Encoding.GetEncoding("GBK"), headers);
             return strData;
        }
        #endregion
    }
}
