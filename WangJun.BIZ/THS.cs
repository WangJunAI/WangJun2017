using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.Net;

namespace WangJun.BIZ
{
    /// <summary>
    /// 同花顺
    /// </summary>
    public class THS
    {


        protected HTTP http = new HTTP("gbk");

        #region 获取一个实例
        /// <summary>
        /// 获取一个实例
        /// </summary>
        /// <returns></returns>
        public static THS GetInst()
        {
            return new THS();
        }
        #endregion  


        /// <summary>
        /// 下载所有的股票代码
        /// </summary>
        /// <remarks>
        /// GET http://q.10jqka.com.cn/index/index/board/all/field/zdf/order/desc/page/{3:页码}/ajax/1/ 返回Html 片段
        /// </remarks>
        public void DownloadAllStockCode()
        {
            string url = "http://q.10jqka.com.cn/index/index/board/all/field/zdf/order/desc/page/3/ajax/1/";
            var res  = this.http.GetString(url);
        }

        #region 下载龙虎榜数据
        public void DownloadLHB()
        {
            var dayCount = 10;
            for (int i = 0; i < dayCount; i++)
            {
                string url = "http://data.10jqka.com.cn/ifmarket/lhbggxq/report/2017-03-27/";
                var res = this.http.GetString(url);
            }

        }

        #endregion
    }
}
