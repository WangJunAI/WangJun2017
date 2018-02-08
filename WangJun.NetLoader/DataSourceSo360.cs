using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WangJun.Net;

namespace WangJun.NetLoader
{
    /// <summary>
    /// 
    /// </summary>
    public  class DataSourceSo360
    {
        /// <summary>
        /// 获取节日信息
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public  List<Dictionary<string,object>> GetHolidayArrangement()
        {
            var year =  DateTime.Now.Year;
            var res = new List<Dictionary<string, object>>();
            var url = "https://www.so.com/s?q="+year+"+%E6%94%BE%E5%81%87%E5%AE%89%E6%8E%92";
            var httpdownloader = new HTTP();
            var headers = new Dictionary<HttpRequestHeader, string>();
            headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7");
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36");
            headers.Add(HttpRequestHeader.Host, "www.so.com");

            var html = httpdownloader.GetGzip2(url, Encoding.UTF8, headers); 
            var tbodyIndex1 = html.IndexOf("<tbody>") + "<tbody>".Length;
            var tbodyIndex2 = html.IndexOf("</tbody>") - html.IndexOf("<tbody>") - "</tbody>".Length + 1;
            var trArray = html.Substring(tbodyIndex1, tbodyIndex2).Replace("</tr><tr>", "；").Split(new char[] { '；' });
            foreach (var tr in trArray)
            {
                //<tr>< td class="mh-col mh-col0"><span class="mh-txt">元旦</span></td><td class="mh-col mh-col1"><span class="mh-txt">12月30日 ~1月1日</span></td><td class="mh-col mh-col2"><span class="mh-txt">1月1日（周一）补休</span></td><td class="mh-col mh-col3 mh-col-last"><span class="mh-txt">共3天</span></td>
                var temp1 = tr.Replace("<br>"," ").Replace("<tr>", string.Empty).Replace("\"", string.Empty).Replace("<span class=mh-txt>", string.Empty).Replace("</span>", string.Empty)
                    .Replace("mh-col0", string.Empty).Replace("mh-col1", string.Empty).Replace("mh-col2", string.Empty).Replace("mh-col3", string.Empty).Replace("mh-col-last", string.Empty).Replace("mh-col", string.Empty).Replace("class=", string.Empty)
                    .Replace("<td", string.Empty).Replace("</td>", "；").Replace(">", string.Empty).Split(new char[] { '；' }, StringSplitOptions.RemoveEmptyEntries);
                var item = new Dictionary<string, object>();
                var temp2 = year + "/" + temp1[1].Trim().Split(new char[] { '~' })[0].Replace("月", "/").Replace("日", string.Empty);
                var temp3 = year + "/" + temp1[1].Trim().Split(new char[] { '~' })[1].Replace("月", "/").Replace("日", string.Empty);
                temp2 = (temp2.Contains("(")) ? temp2.Substring(0, temp2.IndexOf("(")) : temp2;
                temp3 = (temp3.Contains("(")) ? temp3.Substring(0, temp3.IndexOf("(")) : temp3;
                item["节日"] = temp1[0].Trim();
                item["放假开始时间"] = Convert.ToDateTime(temp2);
                item["放假结束时间"] = Convert.ToDateTime(temp3);
                item["调休上班时间"] = temp1[2].Trim();
                item["放假天数"] =Convert.ToInt32( temp1[3].Trim().Replace("共",string.Empty).Replace("天", string.Empty));
                res.Add(item);
            }
            return res;
        }
        
    }
}
