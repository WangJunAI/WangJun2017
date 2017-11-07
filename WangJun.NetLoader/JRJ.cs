using System;
using System.Collections.Generic;
using System.Linq;
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
        public void GetKLine()
        {
            var httpdownloader = new HTTP(Encoding.UTF8);
            var url = "http://d.10jqka.com.cn/v2/line/hs_600056/01/last.js ";
            var strData = httpdownloader.GetString(url);

        }
    }


}
