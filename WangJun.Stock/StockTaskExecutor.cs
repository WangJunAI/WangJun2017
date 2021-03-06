﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.AI;
using WangJun.DB;
using WangJun.Net;
using WangJun.NetLoader;
using WangJun.Utility;

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
            var mongo = DataStorage.GetInstance(DBType.MongoDB);
            var dataSource = new Dictionary<string, string>();

            Console.WriteLine("准备从网络获取股票代码");
            dataSource = WebDataSource.GetInstance().GetAllStockCode();

            var dbName = CONST.DB.DBName_StockService;
            var collectionName = CONST.DB.CollectionName_BaseInfo;
            foreach (var srcItem in dataSource)
            {
                var svItem = new { ContentType = "股票代码", StockCode = srcItem.Key,SortCode = Convert.ToInt32(srcItem.Key), StockName = srcItem.Value, CreateTime = DateTime.Now };
                var filter = "{\"ContentType\":\"" + svItem.ContentType + "\",\"StockCode\":\"" + svItem.StockCode + "\"}";
                mongo.Save3(dbName,collectionName,svItem,filter);
            }
            Console.WriteLine("已成功更新今日股票代码");
        }
        #endregion

        #region 更新页面[已过时]
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
            var webSource = DataSourceSINA.GetInstance();
            var pageCount = 0;// webSource.GetDaDanPageCount();
            if (0 == pageCount)
            {
                pageCount = int.Parse(WebDataSource.GetDataFromHttpAgent(string.Format("http://aifuwu.wang/API.ashx?c=WangJun.Stock.DataSourceSINA&m=GetDaDanPageCount")));
            }
                var dbName = CONST.DB.DBName_StockService;
                var collectionName = CONST.DB.CollectionName_DaDan;
                ///数据检查
                var lastTradingDate = Convertor.CalTradingDate(DateTime.Now, "00:00:00");
            ///删除根据日期创建的临时表格
            ///将数据写入表格
            ///将这个数据转移到大表中
            #region 存储临时数据
            var tempCollectionName = collectionName + string.Format("{0:yyyyMMdd}", DateTime.Now);
            mongo.Remove(dbName, tempCollectionName, "{}");
            #endregion

            var methodName = "GetDaDanData";
            var status = TaskStatusManager.Get(methodName);
            var startIndex = (status.ContainsKey("PageIndex")) ? int.Parse(status["PageIndex"].ToString()) : 0;
            var totalCount = 0;
            for (int i = startIndex; i < pageCount; i++)
            {
                var html = "";// webSource.GetDaDan(i);

                if(string.IsNullOrWhiteSpace(html))
                {
                    html = WebDataSource.GetDataFromHttpAgent(string.Format("http://aifuwu.wang/API.ashx?c=WangJun.Stock.DataSourceSINA&m=GetDaDan&p={0}", i));
                }

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
                    LOGGER.Log(string.Format("SINA大单保存 {0} {1}", i, pageCount));

                    var arrayList = item.PageData as ArrayList;
                    ///数据二维化，计算可以计算的，剩下的再想办法计算
                    for (int k = 0; k < arrayList.Count; k++)
                    {
                        totalCount++;
                        var arrItem = arrayList[k] as Dictionary<string, object>;
                        var svItem2D = new
                        {
                            StockCode = arrItem["StockCode"],
                            StockName = arrItem["StockName"],
                            TickTime = arrItem["交易时间"],
                            Price = arrItem["成交价"],
                            Turnover = arrItem["成交量"],
                            PrevPrice = arrItem["之前价格"],
                            Kind = arrItem["成交类型"],
                            TradingTime = Convertor.CalTradingDate(item.TradingDate, arrItem["交易时间"].ToString()),
                            TradingDate = item.TradingDate,
                            CreateTime = item.CreateTime,
                            PageIndex = i,
                            PageCount = pageCount,
                            RowIndex = k
                        };
                        mongo.Save3(dbName, tempCollectionName, svItem2D);
                        TaskStatusManager.Set(methodName, new { ID = methodName, PageIndex=i,PageCount=pageCount, Status = "已下载", CreateTime = DateTime.Now });

                        LOGGER.Log(string.Format("SINA大单2D保存 {0} {1} {2}", i, k, pageCount));

                    }
                }
                ThreadManager.Pause(seconds: 2);
            }
            TaskStatusManager.Set(methodName, new { ID = methodName, Status = "队列处理完毕", CreateTime = DateTime.Now });
            var countInDB = mongo.Count(CONST.DB.DBName_StockService, tempCollectionName, "{}");
            if (totalCount == countInDB)
            {
                LOGGER.Log(string.Format("准备转移数据"));
                ///数据检查是否完毕,完备后插入
                //DataStorage.MoveCollection(mongo, CONST.DB.DBName_StockService, tempCollectionName, "{}", mongo, CONST.DB.DBName_StockService, CONST.DB.CollectionName_DaDan, false);
            }
            else
            {
                LOGGER.Log(string.Format("数据对不上 {0} {1}", totalCount, countInDB));
            }
        }
        #endregion

        #region 数据转换[已作废]
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

        #region 数据转换 大单[已作废]
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
            if (StringChecker.IsDateTime(dateTime))
            {
                var formatDate = string.Format("{0:yyyyMMdd}", Convert.ToDateTime(dateTime));
                var mongo = DataStorage.GetInstance(DBType.MongoDB);
                var webSrc = DataSourceTHS.CreateInstance();
                var newNewsList = new Dictionary<string,object>();
                var listHtml = webSrc.GetNewsListCJYW(Convert.ToDateTime(dateTime));
                var dbName = CONST.DB.DBName_StockService;
                var collectionNews = CONST.DB.CollectionName_News;

                if (!string.IsNullOrWhiteSpace(listHtml))
                {
                    var resDict = NodeService.Get(CONST.NodeServiceUrl, "同花顺", "GetDataFromHtml", new { ContentType = "THS财经要闻新闻列表", Page = listHtml }) as Dictionary<string, object>;
                    var resList = resDict["PageData"] as ArrayList;
                    if (null != resList)
                    {
                        foreach (Dictionary<string, object> listItem in resList)
                        {
                            var href = listItem["Href"].ToString().Trim();
                            var parentUrl = string.Format("http://news.10jqka.com.cn/today_list/{0}/", formatDate).Trim();
                            var newsHtml = webSrc.GetNewsArticle(href, parentUrl);
                            if (!string.IsNullOrWhiteSpace(newsHtml))
                            {
                                TaskStatusManager.Set("GetNewsListCJYW", new { ID = "GetNewsListCJYW", CreateTime = DateTime.Now, Status = "准备操作THS财经要闻新闻详细", Html= newsHtml });
                                var resItem = NodeService.Get(CONST.NodeServiceUrl, "同花顺", "GetDataFromHtml", new { ContentType = "THS财经要闻新闻详细", Page = newsHtml }) as Dictionary<string, object>;
                                var resDetail = resItem["PageData"] as Dictionary<string, object>;
                                if (null != resDetail)
                                {
                                    var newsCreateTimeString = resDetail["CreateTime"].ToString().Trim();
                                    var svItem = new
                                    {
                                        ContentType = "THS财经要闻",
                                        Title = resDetail["Title"].ToString().Trim(),
                                        Url = href,
                                        ParentUrl = parentUrl,
                                        SourceHref = resDetail["SourceHref"].ToString().Trim(),
                                        SourceName = resDetail["SourceName"].ToString().Trim(),
                                        NewsCreateTime = (StringChecker.IsDateTime(newsCreateTimeString))?Convert.ToDateTime(newsCreateTimeString):DateTime.MinValue,
                                        Content = resDetail["Content"].ToString().Trim(),
                                        Tag = Convert.ToInt32(formatDate),
                                        CreateTime = DateTime.Now,
                                        PageMD5 = "无"
                                    };
                                    newNewsList.Add(href,svItem);
                                    LOGGER.Log(string.Format("获取一个新闻正文 {0}", svItem.Title));
                                    ThreadManager.Pause(seconds: 5); ///5秒钟更新一次新闻         


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
                         
                        foreach (var svItem in newNewsList)
                        {
                            var filter = "{\"Url\":\"" + svItem.Key + "\"}";
                            mongo.Save3(dbName, collectionNews, svItem.Value, filter);
                        }
                    }
                }
                else
                {
                    LOGGER.Log(string.Format("获取的新闻列表为空白"));
                }
            }
            else
            {
                LOGGER.Log(string.Format("传入的时间字符串不对:{0}", dateTime));
                ThreadManager.Pause(minutes: 5);
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
                var dbName = CONST.DB.DBName_StockService;
                var collectionName = CONST.DB.CollectionName_FenCi;

                var res = FenCi.GetResult(content);
                var mongo = DataStorage.GetInstance(DBType.MongoDB);
 
                foreach (var item in res)
                {

                    var svItem = new {
                        Url=url,
                        Word=item.Key,
                        Count=item.Value,
                        CreateTime=DateTime.Now
                    };
                    var filter = "{\"Url\":\"" + url.Trim() + "\",\"Word\":\"" + svItem.Word + "\"}";

                    mongo.Save3(dbName,collectionName,svItem, filter);
                }
            }
        }

        #endregion



    }
}
