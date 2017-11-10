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
    /// 金融街数据源
    /// </summary>
    public class JRJ
    {

        /// <summary>
        /// 获取日线数据
        /// </summary>
        public string GetKLine(string stockCode)
        {
            var httpdownloader = new HTTP();
            var url = string.Format("http://flashdata2.jrj.com.cn/history/js/share/{0}/other/dayk_ex.js?random=1510076545082",stockCode);
            var headers = new Dictionary<HttpRequestHeader, string>();
            headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
            headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4");
            headers.Add(HttpRequestHeader.Accept, "*/*");
            headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            headers.Add(HttpRequestHeader.Referer, string.Format("http://stock.jrj.com.cn/share,{0}.shtml",stockCode));
            headers.Add(HttpRequestHeader.Cookie, "vjuids=-492f73280.15ef65296f7.0.ba8d7db09d1c2; jrj_uid=1508741900394rUUgyFVcaF; jrj_z3_newsid=1943; jrj_z3_home_newsid=1943; ADVS=35a6108b05f425; ASL=17478,oancb,6f1256096f1256096f1256d06f1256746f1256b3; Hm_lvt_0359dbaa540096117a1ec782fff9c43f=1509945284,1509949335,1509950934,1510076377; Hm_lpvt_0359dbaa540096117a1ec782fff9c43f=1510076517; ADVC=3599e935b514c4; channelCode=3763BEXX; ylbcode=24S2AZ96; vjlast=1507371161.1510076376.11; JRJ_LASTEST_SHARE_COOKIE=300676%2C601388%2C600887%2C603533%2C603535");

            var strData = httpdownloader.GetGzip2(url,Encoding.GetEncoding("GBK"), headers);
            return strData;
        }
  
    }


}
