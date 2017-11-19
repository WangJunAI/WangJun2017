using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.BizCore;
using WangJun.Data;
using WangJun.DB;

namespace WangJun.Stock
{
    /// <summary>
    /// 
    /// </summary>
    public class StockTaskRunner
    {
 

        #region 准备技术数据 股票代码
        public void PrepareData()
        {

        }
        #endregion

        #region 更新股票代码信息
        /// <summary>
        /// 更新股票代码信息
        /// </summary>
        public void UpdateAllStockCode(string dataSourceName="THS")
        {
            var db =   DataStorage.GetInstance();
            var dataSource = new Dictionary<string, string>();
            db.Delete("{\"ContentType\":\"股票代码\"}", "BaseInfo", "StockTask");
            if("THS".ToUpper() == dataSourceName.ToUpper())
            {
                dataSource = DataSourceTHS.CreateInstance().GetAllStockCode();
            }
            
            foreach (var srcItem in dataSource)
            {
                var item = new { ContentType = "股票代码", StockCode = srcItem.Key, StockName = srcItem.Value, CreateTime = DateTime.Now };

                db.Save(item, "BaseInfo", "StockTask");
            }
        }
        #endregion
         
        #region 更新页面
        /// <summary>
        /// 更新页面
        /// </summary>
        /// <param name="stockCode"></param>
        /// <param name="stockName"></param>
        /// <param name="contentType"></param>
        /// <param name="url"></param>
        /// <param name="date"></param>
        /// <param name="rid"></param>
        public void UpdatePage(string stockCode, string stockName,string contentType,string url=null)
        {
            var webSource = DataSourceTHS.CreateInstance();
            var db = DataStorage.GetInstance();
            var html = webSource.GetPage(contentType,stockCode,url);///获取页面
            var subLinkArray = new List<string>();
             
            var jsonFilter = string.Format("{{\"StockCode\":\"{0}\",\"ContentType\":\"{1}\"}}", stockCode, contentType);
            if ("个股龙虎榜" == contentType)
            {
                subLinkArray = webSource.GetUrlGGLHBMX(html); ///个股龙虎榜明细链接
                foreach (var subUrl in subLinkArray)
                {
                    this.UpdatePage(stockCode, stockName, "个股龙虎榜明细", subUrl);
                }
            }
            else if("个股龙虎榜明细" == contentType)
            {
                jsonFilter = string.Format("{{\"StockCode\":\"{0}\",\"ContentType\":\"{1}\",\"Url\":\"{2}\"}}", stockCode, contentType, url);
            }

  
            var list = db.Find("PageSource", "PageStock", jsonFilter);

            if (1 == list.Count) ///若已经存储
            {
                list[0]["UpdateTime"] = DateTime.Now;
                list[0]["Page"] = html;
                list[0]["MD5"] = Convertor.Encode_MD5(html);
                db.Save(list[0], "PageStock", "PageSource");
            }
            else if (0 == list.Count) ///若没有存储
            {
                var item = new
                {
                    StockCode = stockCode,
                    StockName = stockName,
                    ContentType = contentType,
                    Url = url,
                    CreateTime = DateTime.Now,
                    Page = html,
                    MD5 = Convertor.Encode_MD5(html),
                    LinkArray = subLinkArray
                };
                db.Save(item, "PageStock", "PageSource");
            }
        }
        #endregion
         


    }
}
