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

        #region 更新个股首页概览页面
        /// <summary>
        /// 更新个股首页概览页面
        /// </summary>
        /// <param name="stockCode"></param>
        public void UpdateSYGL(string stockCode,string stockName)
        {
            var webSource = DataSourceTHS.CreateInstance();
            var db = DataStorage.GetInstance();
            var html = webSource.GetSYGL(stockCode);

            var jsonFilter = string.Format("{{\"StockCode\":\"{0}\"}}", stockCode);
            var list = db.Find("PageSource", "PageStock", jsonFilter);
            
            if (1 == list.Count)
            { 
                list[0]["UpdateTime"] = DateTime.Now;
                list[0]["Page"] = html;
                list[0]["MD5"] = Convertor.Encode_MD5(html);
                db.Save(list[0], "PageStock", "PageSource");
            }
            else if(0 == list.Count)
            {
                var item = new
                {
                    StockCode = stockCode,
                    StockName = stockName,
                    ContentType = "首页概览",
                    Url = string.Format("http://stockpage.10jqka.com.cn/{0}/", stockCode),
                    CreateTime = DateTime.Now,
                    Page = html,
                    MD5 = Convertor.Encode_MD5(html),
                };
                db.Save(item, "PageStock", "PageSource");
            }
        }
        #endregion

        #region 更新个股资金流向页面
        /// <summary>
        /// 更新个股首页概览页面
        /// </summary>
        /// <param name="stockCode"></param>
        public void UpdateZJLX(string stockCode, string stockName)
        {
            var webSource = DataSourceTHS.CreateInstance();
            var db = DataStorage.GetInstance();
            var html = webSource.GetJZLX(stockCode);

            var jsonFilter = string.Format("{{\"StockCode\":\"{0}\",\"ContentType\":\"资金流向\"}}", stockCode);
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
                    ContentType = "资金流向",
                    Url = string.Format("http://stockpage.10jqka.com.cn/{0}/", stockCode),
                    CreateTime = DateTime.Now,
                    Page = html,
                    MD5 = Convertor.Encode_MD5(html),
                };
                db.Save(item, "PageStock", "PageSource");
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
        public void UpdatePage(string stockCode, string stockName,string contentType,string url,string date=null,string rid=null)
        {
            var webSource = DataSourceTHS.CreateInstance();
            var db = DataStorage.GetInstance();
            var html = webSource.GetPage(contentType,stockCode,date,rid);///获取页面

            var jsonFilter = string.Format("{{\"StockCode\":\"{0}\",\"ContentType\":\"{1}\"}}", stockCode, contentType);
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
                };
                db.Save(item, "PageStock", "PageSource");
            }
        }
        #endregion

        #region  判断是否可以更新

        #endregion



    }
}
