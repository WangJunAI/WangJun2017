using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.BizCore;
using WangJun.Data;
using WangJun.DB;
using WangJun.Net;

namespace WangJun.Stock
{
    /// <summary>
    /// 
    /// </summary>
    public class StockTaskExecutor
    {


        #region 更新股票代码信息
        /// <summary>
        /// 更新股票代码信息
        /// </summary>
        public void UpdateAllStockCode(string dataSourceName = "THS")
        {
            var db = DataStorage.GetInstance();
            var dataSource = new Dictionary<string, string>();
            db.Delete("{\"ContentType\":\"股票代码\"}", "BaseInfo", "StockTask");
            if ("THS".ToUpper() == dataSourceName.ToUpper())
            {
                dataSource = DataSourceTHS.CreateInstance().GetAllStockCode();
            }

            foreach (var srcItem in dataSource)
            {
                var item = new { ContentType = "股票代码", StockCode = srcItem.Key, StockName = srcItem.Value, CreateTime = DateTime.Now };

                db.Save(item, "BaseInfo", "StockTask");
            }
            Console.WriteLine("已成功更新今日股票代码");
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
        public void UpdatePage(string stockCode, string stockName, string contentType, string url = null)
        {
            var webSource = DataSourceTHS.CreateInstance();
            var db = DataStorage.GetInstance();
            var html = webSource.GetPage(contentType, stockCode, url);///获取页面
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
            else if ("个股龙虎榜明细" == contentType)
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

        #region 下载大单数据
        /// <summary>
        /// 下载大单数据
        /// </summary>
        public void GetDaDanPage()
        {
            ///获取页码码数
            ///下载每一页数据
            var db = DataStorage.GetInstance();
            var webSource = DataSourceSINA.CreateInstance();
            var pageCount = webSource.GetDaDanPageCount();
            for (int i = 1; i < pageCount; i++)
            {
                var html = webSource.GetDaDan(i);
                var item = new {
                    Page = html,
                    ContentType = "SINA大单页面",
                    CreateTime = DateTime.Now,
                    MD5 = Convertor.Encode_MD5(html),
                };

                db.Save(item, "PageDaDan", "PageSource");

            }
        }
        #endregion

        #region 数据转换
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public Dictionary<string,object> GetDataFromPage(string _id)
        {
            var db = DataStorage.GetInstance();
            var filter = string.Format("{{\"_id\":\"{0}\"}}", _id);
            var src = db.Find("PageSource", "PageStock", filter, 0, 1);
            if (1 == src.Count)
            {
                var context = new
                {
                    CMD = "同花顺",
                    Method = "GetDataFromHtml",
                    Args = src[0]
                };

                var httpdownloader = new HTTP();
                var resString = httpdownloader.Post("http://localhost:8990", Encoding.UTF8, Convertor.FromObjectToJson(context));

                var resData = Convertor.FromJsonToDict2(resString);
                return resData;
            }
            return null;
        }
        #endregion

        #region 更新指定股票的数据
        /// <summary>
        /// 更新指定股票的数据
        /// </summary>
        /// <param name="_id"></param>

        public void UpdateData(string _id)
        {
            var srcData = this.GetDataFromPage(_id);
            if (null != srcData)
            {
                var svItem = (srcData["RES"] as Dictionary<string, object>);
                var db = DataStorage.GetInstance();
                 
                var id = Convertor.ObjectIDToString(svItem["PageID"] as Dictionary<string, object>);
                svItem["PageID"] = id;
                var filter = string.Format("{{\"PageID\":\"{0}\"}}", id);
                svItem["CreateTime"] = DateTime.Now;
                db.Remove("DataSource", "DataOfPage", filter);
                db.Save(srcData["RES"], "DataOfPage", "DataSource");
                 
            }
        }

        #endregion

        #region 将数据二维化
        public void UpdateData2D(string _id)
        {
            var db = DataStorage.GetInstance();

            var srcData = db.Find("DataSource", "DataOfPage", string.Format("{{\"_id\":\"{0}\"}}", _id),0,1).First();
            var contentType = srcData["ContentType"].ToString();
            ///首页概览
            #region 首页概览
            if ("首页概览" == contentType){
                var company = srcData["公司概况"] as Dictionary<string,object>;
                company["StockCode"] = srcData["StockCode"];
                company["StockName"] = srcData["StockName"];

                var jsonFilter = string.Format("{{\"StockCode\":\"{0}\"}}", company["StockCode"]);
 
                db.Save2("StockData", "Summary", jsonFilter, company);

            }
            #endregion

             
            #region 资金流向
            else if("资金流向" == contentType)
            {
                var rows = (Array)srcData["Rows"];

                foreach (Dictionary<string, object> row in rows)
                {
                    var svItem = new Dictionary<string, object>();

                    svItem["StockCode"] = srcData["StockCode"];
                    svItem["StockName"] = srcData["StockName"];
                    svItem["ContentType"] = srcData["ContentType"];
                    foreach (var key in row.Keys)
                    {
                        svItem[key] = row[key];
                    }

                    var jsonFilter = string.Format("{{\"StockCode\":\"{0}\",\"C1\":\"{1}\"}}", svItem["StockCode"], svItem["C1"]);

                    db.Save2("StockData", "Funds", jsonFilter, svItem);
                }

            }
            #endregion

            ///个股龙虎榜
            #region 个股龙虎榜
            else if("个股龙虎榜" == contentType)
            {
                var rows = (Array)srcData["Data"];

                foreach (Dictionary<string, object> row in rows)
                {
                    var svItem = new Dictionary<string, object>();

                    svItem["StockCode"] = srcData["StockCode"];
                    svItem["StockName"] = srcData["StockName"];
                    svItem["ContentType"] = srcData["ContentType"];
                    foreach (var key in row.Keys)
                    {
                        svItem[key] = row[key];
                    }

                    var jsonFilter = string.Format("{{\"StockCode\":\"{0}\",\"C1\":\"{1}\"}}", svItem["StockCode"], svItem["C1"]);

                    db.Save2("StockData", "GGLHB", jsonFilter, svItem);
                }
            }
            #endregion
            ///龙虎榜明细
            else if ("个股龙虎榜明细" == contentType)
            {
                var rows = (Array)srcData["Rows"];

                foreach (Dictionary<string, object> row in rows)
                {
                    var svItem = new Dictionary<string, object>();

                    svItem["StockCode"] = srcData["StockCode"];
                    svItem["StockName"] = srcData["StockName"];
                    svItem["ContentType"] = srcData["ContentType"];
                    svItem["TradingDate"] = (srcData["Summary"] as Dictionary<string, object>)["日期"];
                    foreach (var key in row.Keys)
                    {
                        svItem[key] = row[key];
                    }

                    var jsonFilter = string.Format("{{\"StockCode\":\"{0}\",\"TradingDate\":\"{1}\",\"C1\":\"{2}\"}}", svItem["StockCode"], svItem["TradingDate"], svItem["C1"]);

                    db.Save2("StockData", "GGLHBMX", jsonFilter, svItem);
                }
            }
            #endregion
        }
 

    }
}
