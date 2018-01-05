using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WangJun.Data;
using WangJun.Debug;
using WangJun.Net;
using WangJun.Tools;

namespace WangJun.Stock
{
    /// <summary>
    /// 头条
    /// </summary>
    public class DataSourceTouTiao
    {
        public static DataSourceTouTiao GetInstance()
        {
            var inst = new DataSourceTouTiao();
            return inst;
        }

        /// <summary>
        /// 获取搜索结果
        /// </summary>
        /// <returns></returns>
        public List<string> GetSearchResult(string keyword, int maxCount = 60)
        {
            var list = new List<string>();
            var count = 20;
            for (int offset = 0; offset <= 3*count; offset = (++offset) * count)
            {
                var url = string.Format("https://www.toutiao.com/search_content/?offset={0}&format=json&keyword={1}&autoload=true&count={2}&cur_tab=1&from=search_tab",offset, HttpUtility.UrlEncode(keyword),count);
                var httpDownloader = new HTTP();
                var headers = new Dictionary<string, string>();
                headers.Add("Accept", "application/json, text/javascript");
                headers.Add("Accept-Encoding", "gzip, deflate, br");
                headers.Add("Accept-Language", "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
                headers.Add("Host", "www.toutiao.com");
                headers.Add("Content-Type", "application/x-www-form-urlencoded");
                headers.Add("Referer", string.Format("https://www.toutiao.com/search/?keyword={0}",HttpUtility.UrlEncode(keyword)));
                headers.Add("User-Agent", CONST.UserAgent);
                headers.Add("X-Requested-With", "XMLHttpRequest");

                var strData = httpDownloader.GetGzip(url, Encoding.GetEncoding("GBK"), headers);
                list.Add(strData);
                LOGGER.Log(string.Format("正在获取 头条搜索结果 {0}", offset));
                ThreadManager.Pause(seconds: 5);
            }
            
            return list;

        }
    }
}
