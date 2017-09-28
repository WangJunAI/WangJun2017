using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WangJun.Data;
using WangJun.DB;
using WangJun.Net;


namespace WangJun.NetLoader
{
    public class TouTiao
    {
        protected MongoDB mongo = MongoDB.GetInst("mongodb://192.168.0.140:27017");
        public void GetNewsSummary()
        {
            //var url = "http://www.toutiao.com/api/pc/feed/?category=news_tech&utm_source=toutiao&widen=1&max_behot_time=0&max_behot_time_tmp={0}&tadrequire=true&as=A135E92C7C07EA1&cp=59CC37BECAB15E1"; ///实时更新
            //var url = "http://www.toutiao.com/api/pc/feed/?max_behot_time={0}&category=__all__&utm_source=toutiao&widen=1&tadrequire=false&as=A1B5692C6C597C3&cp=59CC5997BC23FE1"; ///实时更新
            //var url = "http://www.toutiao.com/api/pc/feed/?category=news_entertainment&utm_source=toutiao&widen=1&max_behot_time=0&max_behot_time_tmp={0}&tadrequire=true&as=A1C5394CFCCC5BE&cp=59CCEC756BCECE1"; ///娱乐
            var url = "http://www.toutiao.com/api/pc/feed/?category=news_finance&utm_source=toutiao&widen=1&max_behot_time=0&max_behot_time_tmp={0}&tadrequire=true&as=A135296CBCFC67C&cp=59CCFCB6975C7E1";///财经

            var httpdownloader = new HTTP(Encoding.UTF8);
            var tick = "1506586665";
            while (true)
            {
                var str = httpdownloader.GetGzip2(string.Format(url,tick), Encoding.UTF8);
                //str = Convertor.FromUnicodeToUTF8(str);
                var type = new { has_more = false, message = string.Empty };
                var dict = Convertor.FromJsonToDict2(str);
                tick = (dict["next"]as Dictionary<string,object>)["max_behot_time"].ToString();
                mongo.Save("TouTiao", "money", dict);
                Console.WriteLine("财经" + tick);
                Thread.Sleep(5 * 1000);
            }
        }

    }
}
