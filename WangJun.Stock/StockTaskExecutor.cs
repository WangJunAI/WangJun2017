using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WangJun.BizCore;
using WangJun.Data;
using WangJun.DB;
using WangJun.Debug;
using WangJun.Net;
using WangJun.NetLoader;
using WangJun.Tools;

namespace WangJun.Stock
{
    /// <summary>
    /// 
    /// </summary>
    public class StockTaskExecutor
    { 
        public static StockTaskExecutor CreateInstance()
        {
            var inst = new StockTaskExecutor();
            return inst;
        }

        #region 更新股票代码信息
        /// <summary>
        /// 更新股票代码信息
        /// </summary>
        public void UpdateAllStockCode()
        {
            var mongo = DataStorage.GetInstance("170");
            var dataSource = new Dictionary<string, string>();

            Console.WriteLine("准备从网络获取股票代码");
            dataSource = WebDataSource.GetInstance().GetAllStockCode();
            
            #region 删除旧数据
            mongo.Delete("{\"ContentType\":\"股票代码\"}","BaseInfo", "StockService");
            mongo.Delete("{\"ContentType\":\"股票代码\"}", "BaseInfo", "StockTask");
            #endregion

            foreach (var srcItem in dataSource)
            {
                var item = new { ContentType = "股票代码", StockCode = srcItem.Key, StockName = srcItem.Value, CreateTime = DateTime.Now };

                mongo.Save(item, "BaseInfo", "StockTask");
                mongo.Save(item, "BaseInfo", "StockService");
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
        public void UpdatePage(string stockCode, string stockName, string contentType, string url = null, Dictionary<string, object> exData = null)
        {
            var webSource = DataSourceTHS.CreateInstance();
            var db = DataStorage.GetInstance("170");
            var html = webSource.GetPage(contentType, stockCode, url, exData);///获取页面
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
            else if ("SINA个股历史交易" == contentType)
            {
                var year = exData["Year"];
                var jidu = exData["JiDu"];
                jsonFilter = string.Format("{{\"StockCode\":\"{0}\",\"ContentType\":\"{1}\",\"Year\":\"{2}\",\"JiDu\":\"{3}\"}}", stockCode, contentType, year, jidu);
            }
            else if ("SINA个股历史交易明细" == contentType)
            {
                //var year = exData["Year"];
                //var jidu = exData["JiDu"];
                //jsonFilter = string.Format("{{\"StockCode\":\"{0}\",\"ContentType\":\"{1}\",\"Year\":\"{2}\",\"JiDu\":\"{3}\"}}", stockCode, contentType, year, jidu);
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
        public void GetDaDanData()
        {
            ///获取页码码数
            ///下载每一页数据
            ///查找最近一个交易日的数据是否存在,若存在则不下载
            ///


            var mongo = DataStorage.GetInstance(DBType.MongoDB);
            var webSource = DataSourceSINA.CreateInstance();
            var pageCount = webSource.GetDaDanPageCount();
            var dbName = "StockService";
            var collectionName = "SINADaDan";
            ///数据检查
            var lastTradingDate = Convertor.CalTradingDate(DateTime.Now, "00:00:00");
            var checkFilter = "{\"ContentType\" :\"SINA大单\",\"TradingDate\":\"new Date('" + lastTradingDate + "')\"}";
            var checkRes = mongo.Find(dbName, collectionName, checkFilter);

            if (0 == checkRes.Count) ///若没有查找到数据 
            {
                for (int i = 1; i < pageCount; i++)
                {
                    var html = webSource.GetDaDan(i);

                    var res = NodeService.Get(CONST.NodeServiceUrl, "新浪", "GetDataFromHtml", new { ContentType = "SINA大单", Page = html });

                    if (res is Dictionary<string, object>)
                    {
                        var item = new
                        {
                            PageData = (res as Dictionary<string, object>)["PageData"],
                            ContentType = "SINA大单",
                            CreateTime = DateTime.Now,
                            TradingDate = Convertor.CalTradingDate(DateTime.Now, "00:00:00"),
                            PageIndex = i,
                            PageCount = pageCount,
                        };

                        mongo.Save(item, "SINADaDan", "StockService");
                        LOGGER.Log(string.Format("SINA大单保存 {0} {1}", i, pageCount));

                        var arrayList = item.PageData as ArrayList;
                        ///数据二维化，计算可以计算的，剩下的再想办法计算
                        for (int k = 0; k < arrayList.Count; k++)
                        {
                            var arrItem = arrayList[k] as Dictionary<string, object>;
                            var item2D = new
                            {
                                StockCode = arrItem["StockCode"],
                                StockName = arrItem["StockName"],
                                TickTime = arrItem["交易时间"],
                                Price = arrItem["成交价"],
                                Turnover = arrItem["成交量"],
                                PrevPrice = arrItem["之前价格"],
                                Kind = arrItem["成交类型"],
                                TradingTime = Convertor.CalTradingDate(item.TradingDate, arrItem["交易时间"].ToString()),
                                CreateTime = item.CreateTime,
                                PageIndex = i,
                                PageCount = pageCount,
                                RowIndex = k
                            };
                            mongo.Save(item2D, "SINADaDan2D", "StockService");
                            LOGGER.Log(string.Format("SINA大单2D保存 {0} {1} {2}", i, k, pageCount));

                        }
                    }
                    Thread.Sleep(new Random().Next(2000, 5000));
                }
            }
        }
        #endregion

        #region 数据转换
        /// <summary>
        /// 数据转换
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetDataFromPage(string _id)
        {
            var db = DataStorage.GetInstance("170");
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

        #region 数据转换 大单
        /// <summary>
        /// 数据转换 大单
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetDataFromPageDaDan(string _id)
        {
            var db = DataStorage.GetInstance("170");
            var filter = string.Format("{{\"_id\":\"{0}\"}}", _id);
            var src = db.Find("PageSource", "DaDan", filter, 0, 1);
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
                resData["TradingDate"] = Convertor.CalTradingDate(Convert.ToDateTime(src[0]["CreatTime"]).AddHours(8), "00:00:00");///注意:要添加时区
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
                var db = DataStorage.GetInstance("170");

                var id = Convertor.ObjectIDToString(svItem["PageID"] as Dictionary<string, object>);
                svItem["PageID"] = id;
                var filter = string.Format("{{\"PageID\":\"{0}\"}}", id);
                svItem["CreateTime"] = DateTime.Now;
                db.Remove("DataSource", "DataOfPage", filter);
                db.Save(srcData["RES"], "DataOfPage", "DataSource");

            }
        }

        #endregion

        #region 更新指定股票的数据
        /// <summary>
        /// 更新指定股票的数据
        /// </summary>
        /// <param name="_id"></param>

        public void UpdateDaDan(string _id)
        {
            var srcData = this.GetDataFromPageDaDan(_id);
            if (null != srcData)
            {
                var svItem = (srcData["RES"] as Dictionary<string, object>);
                svItem["TradingDate"] = srcData["TradingDate"];
                var db = DataStorage.GetInstance("170");

                var id = Convertor.ObjectIDToString(svItem["PageID"] as Dictionary<string, object>);
                svItem["PageID"] = id;
                var filter = string.Format("{{\"PageID\":\"{0}\"}}", id);
                svItem["CreateTime"] = DateTime.Now;
                db.Remove("DataSource", "DataOfDaDan", filter);
                db.Save(srcData["RES"], "DataOfDaDan", "DataSource");

            }
        }

        #endregion

        #region 将数据二维化
        /// <summary>
        /// 将数据二维化
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="srcDbName"></param>
        /// <param name="srcTableName"></param>
        public void UpdateData2D(string _id, string srcDbName, string srcTableName)
        {
            var srcdb = DataStorage.GetInstance("170");

            var list = srcdb.Find(srcDbName, srcTableName, string.Format("{{\"_id\":\"{0}\"}}", _id), 0, 1);
            if (1 == list.Count)
            {
                var srcData = list.First();
                var contentType = srcData["ContentType"].ToString();

                #region 首页概览
                if ("首页概览" == contentType)
                {
                    var company = srcData["公司概况"] as Dictionary<string, object>;
                    company["StockCode"] = srcData["StockCode"];
                    company["StockName"] = srcData["StockName"];

                    var jsonFilter = string.Format("{{\"StockCode\":\"{0}\"}}", company["StockCode"]);

                    srcdb.Save2("StockData", "Summary", jsonFilter, company);

                }
                #endregion

                #region 资金流向
                else if ("资金流向" == contentType)
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

                        srcdb.Save2("StockData", "Funds", jsonFilter, svItem);
                    }

                }
                #endregion

                #region 个股龙虎榜
                else if ("个股龙虎榜" == contentType)
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

                        srcdb.Save2("StockData", "GGLHB", jsonFilter, svItem);
                    }
                }
                #endregion

                #region 个股龙虎榜明细

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


                        srcdb.Save2("StockData", "GGLHBMX", jsonFilter, svItem);
                    }
                }
                #endregion

                #region 大单追踪实时数据

                else if ("大单追踪实时数据" == contentType)
                {
                    if (srcData.ContainsKey("Rows") && srcData["Rows"] is Array)
                    {
                        var rows = (Array)srcData["Rows"];
                        var rowIndex = 1;
                        var targetDb = DataStorage.GetInstance("170");
                        foreach (Dictionary<string, object> row in rows)
                        {
                            var svItem = new Dictionary<string, object>();
                            svItem["PageID"] = srcData["PageID"];
                            svItem["PageMD5"] = srcData["PageMD5"];
                            svItem["ContentType"] = srcData["ContentType"];
                            svItem["RowIndex"] = rowIndex++;
                            svItem["TradingDate"] = ((DateTime)srcData["TradingDate"]).AddHours(8); ///处理时区
                            svItem["TradingTime"] = DateTime.Parse(string.Format("{0} {1}", ((DateTime)svItem["TradingDate"]).ToShortDateString(), row["ticktime"]));
                            svItem["StockCode"] = row["symbol"].ToString().Substring(2);
                            svItem["StockName"] = row["name"];
                            svItem["Price"] = Convert.ToDecimal(row["price"]);
                            svItem["Volume"] = Convert.ToInt32(row["volume"]);
                            svItem["PrevPrice"] = Convert.ToDecimal(row["prev_price"]);
                            svItem["Kind"] = row["kind"];

                            //var jsonFilter = string.Format("{{\"StockCode\":\"{0}\",\"TradingTime\":\"{1}\",\"Volume\":\"{2}\"}}", svItem["StockCode"], svItem["TradingTime"], svItem["Volume"]);

                            targetDb.Save2("StockData2D", "DaDan", null, svItem);
                        }
                    }
                }
                #endregion

                #region SINA 个股历史交易

                else if ("SINA个股历史交易" == contentType)
                {
                    if (srcData.ContainsKey("Rows") && srcData["Rows"] is Array)
                    {
                        var rows = (Array)srcData["Rows"];
                        var rowIndex = 1;
                        var targetDb = DataStorage.GetInstance("170");
                        foreach (Dictionary<string, object> row in rows)
                        {
                            var svItem = new Dictionary<string, object>();
                            svItem["PageID"] = srcData["PageID"];
                            svItem["PageMD5"] = srcData["PageMD5"];
                            svItem["ContentType"] = srcData["ContentType"];
                            svItem["RowIndex"] = rowIndex++;
                            svItem["日期"] =Convert.ToDateTime(row["日期"]);
                            svItem["开盘价"] = Convert.ToSingle(row["开盘价"]);
                            svItem["最高价"] = Convert.ToSingle(row["最高价"]);
                            svItem["收盘价"] = Convert.ToSingle(row["收盘价"]);
                            svItem["最低价"] = Convert.ToSingle(row["最低价"]);
                            svItem["交易量(股)"] = Convert.ToInt64(row["交易量(股)"]);
                            svItem["交易金额(元)"] = Convert.ToInt64(row["交易金额(元)"]);

                            //var jsonFilter = string.Format("{{\"StockCode\":\"{0}\",\"TradingTime\":\"{1}\",\"Volume\":\"{2}\"}}", svItem["StockCode"], svItem["TradingTime"], svItem["Volume"]);

                            targetDb.Save2("StockData2D", "GGLSJY", null, svItem);
                        }
                    }
                }
                #endregion


            }
        }
        #endregion

        #region 获取指定日期的新闻
        /// <summary>
        /// 获取指定日期的新闻
        /// </summary>
        public void GetNewsListCJYW(string dateTime)
        {
            var formatDate = string.Format("{0:yyyyMMdd}", Convert.ToDateTime(dateTime));
            var mongo= DataStorage.GetInstance(DBType.MongoDB);
            var webSrc = DataSourceTHS.CreateInstance();
            var newNewsList = new List<object>();
            var listHtml = webSrc.GetNewsListCJYW(Convert.ToDateTime(dateTime));
 
            var resList = NodeService.Get(CONST.NodeServiceUrl, "同花顺", "GetDataFromHtml", new { ContentType = "THS财经要闻新闻列表", Page = listHtml }) as Dictionary<string, object>;

            if (null != resList)
            {
                resList.Remove("ContentType");

                foreach (var listItem in resList)
                {
                    var href = (listItem.Value as Dictionary<string, object>)["Href"].ToString().Trim();
                    var parentUrl = string.Format("http://news.10jqka.com.cn/today_list/{0}/", formatDate).Trim();
                    var newsHtml = webSrc.GetNewsArticle(href, parentUrl);
                    if (!string.IsNullOrWhiteSpace(newsHtml))
                    {
          
                        var resDetail = NodeService.Get(CONST.NodeServiceUrl, "同花顺", "GetDataFromHtml", new { ContentType = "THS财经要闻新闻详细", Page = newsHtml }) as Dictionary<string,object>;
                        if (null != resDetail)
                        {
                            var svItem = new
                            {
                                ContentType = "THS财经要闻",
                                Title = resDetail["Title"].ToString().Trim(),
                                Url = href,
                                ParentUrl = parentUrl,
                                SourceHref = resDetail["SourceHref"].ToString().Trim(),
                                SourceName = resDetail["SourceName"].ToString().Trim(),
                                NewsCreateTime = Convert.ToDateTime(resDetail["CreateTime"].ToString().Trim()),
                                Content = resDetail["Content"].ToString().Trim(),
                                Tag = Convert.ToInt32(formatDate.Replace("/", string.Empty)),
                                CreateTime = DateTime.Now,
                                PageMD5 = "无"
                            };
                            newNewsList.Add(svItem);
                            LOGGER.Log(string.Format("获取一个新闻正文 {0}", svItem.Title));
                            Thread.Sleep(new Random().Next(1000, 3000));

                            ///分词
                            var task = Task.Factory.StartNew(() =>
                            {
                                var fcZStartTime = DateTime.Now;
                                this.SaveFenCi(svItem.Content, svItem.Url);
                                LOGGER.Log(string.Format("分词花费时间 开始时间：{0} 花费时间：{1}", fcZStartTime, DateTime.Now - fcZStartTime));
                            });
                        }
                    }
                }

                ///删除旧
                var deleteFilter = "{\"Tag\":" + Convert.ToInt32(formatDate.Replace("/", string.Empty)) + "}";
                mongo.Delete(deleteFilter, "News", "StockService");///删除旧数据


                #region 同步到服务器上
                var sql = "DELETE FROM News WHERE Tag=@Tag";
                var paramList = new List<KeyValuePair<string, object>>();
                paramList.Add(new KeyValuePair<string, object>("@Tag", Convert.ToInt32(formatDate.Replace("/", string.Empty))));
                var mssql = DataStorage.GetInstance(DBType.SQLServer);
                mssql.Delete(sql, "News", "StockService", exParam: paramList);
                #endregion
                var index = 0;
                foreach (var svItem in newNewsList)
                {
                    mongo.Save2("StockService", "News", null, svItem);
                    //mssql.Save2("StockService", "News", null, svItem);//数据库太小不存
                }
            }
        }
        #endregion
 

        #region 计算分词结果
        /// <summary>
        /// 计算分词结果
        /// </summary>
        /// <param name="content"></param>
        /// <param name="MD5"></param>
        public void SaveFenCi(string content,string url)
        {
            if(!string.IsNullOrWhiteSpace(content)&& !string.IsNullOrWhiteSpace(content))
            {
                var res = FenCi.GetResult(content);
                var mongo = DataStorage.GetInstance(DBType.MongoDB);
                var mssql = DataStorage.GetInstance(DBType.SQLServer);
                var filter = "{\"Url\":\""+ url.Trim() + "\"}";
                mongo.Delete(filter, "FenCi", "StockService");
                var sql = "DELETE FROM FenCi WHERE Url=@Url";
                var paramList = new List<KeyValuePair<string, object>>();
                paramList.Add(new KeyValuePair<string, object> ("@Url", url));
                mssql.Delete(sql, "", "",exParam:paramList);
                foreach (var item in res)
                {
                    var svItem = new {
                        Url=url,
                        Word=item.Key,
                        Count=item.Value,
                        CreateTime=DateTime.Now
                    };

                    mongo.Save2("StockService", "FenCi", null, svItem);
                    //mssql.Save2("", "FenCi", null, svItem);
                }
            }
        }

        #endregion



    }
}
