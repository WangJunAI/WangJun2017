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
    public class DataSourceJRJ
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
            headers.Add(HttpRequestHeader.Cookie, "vjuids=-492f73280.15ef65296f7.0.ba8d7db09d1c2; bdshare_firstime=1508740094204; jrj_uid=1508741900394rUUgyFVcaF; Hm_lvt_1d0c58faa95e2f029024e79565404408=1508777391,1508807313,1509945290,1509994113; ADVS=35a84227fd0d6c; ASL=17480,ancbu,6f1256096f1256d06f1256746f1256b36f1256e9; jrj_z3_newsid=2096; jrj_z3_home_newsid=2096; Hm_lvt_0359dbaa540096117a1ec782fff9c43f=1509949335,1509950934,1510076377,1510317592; Hm_lpvt_0359dbaa540096117a1ec782fff9c43f=1510317592; JRJ_Click_Track=171110204004%3Bhttp%3A//www.jrj.com.cn/%3B%3B%3B; WT_FPC=id=20aa92fff4d755ceb821508740094103:lv=1510317607242:ss=1510317607242; channelCode=3763BEXX; ylbcode=24S2AZ96; vjlast=1507371161.1510317592.11; ADVC=3599e935b514c4; Hm_lvt_d654909655f2581e69361531a7850450=1509994114,1509994127,1510076527,1510317608; Hm_lpvt_d654909655f2581e69361531a7850450=1510317608; JRJ_TODAYREAD_SHARE_COOKIE=%2C600330-2017-11-10-0; JRJ_LASTEST_SHARE_COOKIE=600330%2C300676%2C601388%2C600887%2C603533%2C603535");

            var strData = httpdownloader.GetGzip2(url,Encoding.GetEncoding("GBK"), headers);
            return strData;
        }
  
    }


}
