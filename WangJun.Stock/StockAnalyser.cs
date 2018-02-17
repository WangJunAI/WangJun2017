using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WangJun.DB;
using WangJun.Utility;

namespace WangJun.Stock
{
    /// <summary>
    /// 股票分析器
    /// </summary>
    public class StockAnalyser
    {

        public static StockAnalyser GetInstance()
        {
            var inst = new StockAnalyser();
            return inst;
        }


        #region 大单行为分析[暂时作废]
        /// <summary>
        /// 大单行为分析
        /// </summary>
        /// <param name="param"></param>
        public void AnalyseDaDan(object param=null) {

            var temp1 = 0;
            var temp2 = 0;
            var temp3 = 0;
            var temp4 = 0;
            var dbName = "StockService";
            var collectionName = "SINADaDan2D";
            var mongo = DataStorage.GetInstance(DBType.MongoDB);
            var listDict = new Dictionary<string, Dictionary<string,int>>();
                listDict["成交额500万到1000万"] = new Dictionary<string, int>();
                listDict["成交额1000万以上"] = new Dictionary<string, int>();
            mongo.EventTraverse += (object sender , EventArgs e)=>{
                var ee = e as EventProcEventArgs;
                var data = ee.Default as Dictionary<string, object>;
                var turnover = Convert.ToInt64(data["Turnover"]);
                var stockCode = data["StockCode"].ToString();
                var stockName = data["StockName"].ToString();
                var kind = data["Kind"].ToString();
                var key = stockCode + stockName + kind;
                if (turnover <= 100 * 10000)
                {
                    temp1 += 1;
                }
                else if (100 * 10000 < turnover && turnover <= 500 * 10000)
                {
                    temp2 += 1;
                }
                else if (500 * 10000 < turnover && turnover <= 1000 * 10000)
                {
                    temp3 += 1;
                    if(!listDict["成交额500万到1000万"].ContainsKey(key))
                    {
                        listDict["成交额500万到1000万"].Add(key,0) ;
                    }
                    listDict["成交额500万到1000万"][key] += 1; 

                }
                else if (1000 * 10000 < turnover && turnover <= int.MaxValue)
                {
                    temp4 += 1;
                    if (!listDict["成交额1000万以上"].ContainsKey(key))
                    {
                        listDict["成交额1000万以上"].Add(key,0);
                    }
                    listDict["成交额1000万以上"][key] += 1;
                }
            };

            mongo.Traverse(dbName, collectionName, "{\"TradingDate\":new Date('2017/12/18')}");

            ///保存结果
            ///
            var total =Convert.ToSingle( temp1 + temp2 + temp3 + temp4);
            var res = new { ContentType = "大单行为分析"
                , 交易日期 = "2017/12/18",
                成交额100万以下 = temp1/total
                , 成交额100万到500万 = temp2/total
                , 成交额500万到1000万 = temp3 / total
                , 成交额1000万以上 = temp4 / total
                , 股票列表= listDict
            };//成交额 1000为一个单位 太少就合并
            mongo.Save(res, "DataResult", "DataResult");

        }
        #endregion

        #region 日线分析

        #endregion

        #region 每日最新热词
        /// <summary>
        /// 每日最新热词
        /// </summary>
        public void AnalyseHotWords()
        {
            var date = DateTime.Now.AddDays(-3);
            var filter = "{\"CreateTime\":{$gt:new Date('"+string.Format("{0:yyyy/MM/dd}", date) + "'),$lt:new Date('" + string.Format("{0:yyyy/MM/dd}", date) + " 23:59:59')}}";
            var sort = "{\"Count\":-1}";
            var count = 20;
            var mongo = DataStorage.GetInstance(DBType.MongoDB);
            var dbName = CONST.DB.DBName_StockService;
            var sourceCollectionName = CONST.DB.CollectionName_FenCi;
            var targetCollectionName = CONST.DB.CollectionName_DataResult;
            var list = mongo.Find3(dbName, sourceCollectionName, filter, sort);

            var hotWords = new {
                ContentType = "每日热词",
                CreateTime = DateTime.Now,
                Top20 =(from item in list select new { Word=item["Word"],Count=item["Count"]}).ToList(),
                NewDate=int.Parse(string.Format("{0:yyyyMMdd}", date))
            };

            var svQuery = "{\"NewDate\":"+hotWords.NewDate+"}";
            mongo.Save3(dbName, targetCollectionName, hotWords, svQuery);
        }
        #endregion

        #region 板块概念
        /// <summary>
        /// 板块概念 每天晚上跑一次
        /// </summary>
        public void AnalyseBKGN()
        {
            var mongo = DataStorage.GetInstance(DBType.MongoDB);
            var dbName = CONST.DB.DBName_StockService;
            var sourceCollectionName = CONST.DB.CollectionName_BKGN;
            var targetCollectionName = CONST.DB.CollectionName_DataResult;
            var bkDict = new Dictionary<string, List<string>> ();
            var gnDict = new Dictionary<string, List<string>> ();
            mongo.EventTraverse += (object sender ,EventArgs e) => {
                var ee = e as EventProcEventArgs;
                var data = (ee.Default as Dictionary<string, object>);
                var pageData = data["PageData"] as Dictionary<string,object>;
                var stockCode = data["StockCode"].ToString();
                var stockName = data["StockName"].ToString();
                var bkData = pageData["所属板块"] as object[];
                var gnData = pageData["所属概念"] as object[];
                if(bkData is object[])
                {
                    foreach (var item in (bkData as object[]))
                    {
                        var bk = item.ToString().Replace(".","_");
                        if(!bkDict.ContainsKey(bk))
                        {
                            bkDict.Add(bk,new List<string>()) ;
                        }

                        bkDict[bk].Add(string.Format("{0}{1}", stockCode, stockName));
                    }
                }

                if (gnData is object[])
                {
                    foreach (var item in (gnData as object[]))
                    {
                        var gn= item.ToString().Replace(".", "_");
                        if (!gnDict.ContainsKey(gn))
                        {
                            gnDict.Add(gn, new List<string>());
                        }

                        gnDict[gn].Add(string.Format("{0}{1}", stockCode, stockName));

                    }
                }
            };
            var bkItem = new {ContentType="板块分析" ,Data=bkDict,CreateTime=DateTime.Now};
            var gnItem = new { ContentType = "概念分析", Data = gnDict, CreateTime = DateTime.Now };

            var filter = "{}";
            mongo.Traverse(dbName, sourceCollectionName, filter);

            mongo.Save3(dbName, targetCollectionName, bkItem);

            mongo.Save3(dbName, targetCollectionName, gnItem);
        }
        #endregion

        #region 好业绩公司
        /// <summary>
        /// 好业绩公司
        /// </summary>
        public void AnalyseGoodPerformanceCompany()
        {
            var filter = "{\"PageData.0.每股净资产\":{$gt: 1},\"PageData.0.每股收益\":{$gt: 1},\"PageData.0.每股现金含量\":{$gt: 1},\"PageData.0.每股资本公积金\":{$gt: 1},\"PageData.0.净利润\":{$gt: 10000000}}";
            var sort = "{}";
            var count = 20;
            var mongo = DataStorage.GetInstance(DBType.MongoDB);
            var dbName = CONST.DB.DBName_StockService;
            var sourceCollectionName = CONST.DB.CollectionName_CWZY;
            var targetCollectionName = CONST.DB.CollectionName_DataResult;
            var list = mongo.Find3(dbName, sourceCollectionName, filter, sort);

            var svItem = new
            {
                ContentType = "好业绩公司",
                Discription="最近一个季度业绩大于指定指标",
                CreateTime = DateTime.Now,
                Company = (from item in list select new { StockCode=item["StockCode"], StockName = item["StockName"] }).ToList(),
            };

            var svQuery = "{\"ContentType\":\"好业绩公司\"}";
            mongo.Save3(dbName, targetCollectionName, svItem, svQuery);
        }
        #endregion

        #region 两个季度业绩变好公司
        /// <summary>
        /// 好业绩公司
        /// </summary>
        public void AnalyseBeBetterCompany()
        {
            var date = DateTime.Now.AddDays(-3);
 
            var filter = "{}";
            var sort = "{}";
            var mongo = DataStorage.GetInstance(DBType.MongoDB);
            var dbName = CONST.DB.DBName_StockService;
            var sourceCollectionName = CONST.DB.CollectionName_CWZY;
            var targetCollectionName = CONST.DB.CollectionName_DataResult;
            var svItem = new {ContentType="业绩连续两季度增长的股票",CreateTime=DateTime.Now,List=new List<string>() };
            mongo.EventTraverse += (object sender,EventArgs e) => {
                try
                {
                    var ee = e as EventProcEventArgs;
                    var data = ee.Default as Dictionary<string, object>;
                    var stockCode = data["StockCode"].ToString();
                    var stockName = data["StockName"].ToString();
                    var arrayList = data["PageData"] as object[];
                    var item1 = arrayList[0] as Dictionary<string, object>;
                    var item2 = arrayList[1] as Dictionary<string, object>;
                    var mgjzc1 = (string.IsNullOrWhiteSpace(item1["每股净资产"].ToString())) ? 0 : float.Parse(item1["每股净资产"].ToString());///最新季度的每股净资产
                    var mgjzc2 = (string.IsNullOrWhiteSpace(item2["每股净资产"].ToString())) ? 0 : float.Parse(item2["每股净资产"].ToString());///前第2季度的每股净资产
                    var mgsy1 = (string.IsNullOrWhiteSpace(item1["每股收益"].ToString())) ? 0 : float.Parse(item1["每股收益"].ToString());///最新季度的每股收益
                    var mgsy2 = (string.IsNullOrWhiteSpace(item2["每股收益"].ToString())) ? 0 : float.Parse(item2["每股收益"].ToString());///前第2季度的每股收益       
                    var mgxjhl1 = (string.IsNullOrWhiteSpace(item1["每股现金含量"].ToString())) ? 0 : float.Parse(item1["每股现金含量"].ToString());///最新季度的每股现金含量
                    var mgxjhl2 = (string.IsNullOrWhiteSpace(item2["每股现金含量"].ToString())) ? 0 : float.Parse(item2["每股现金含量"].ToString());///前第2季度的每股现金含量
                    var mgzbgjj1 = (string.IsNullOrWhiteSpace(item1["每股资本公积金"].ToString())) ? 0 : float.Parse(item1["每股资本公积金"].ToString());///最新季度的每股资本公积金
                    var mgzbgjj2 = (string.IsNullOrWhiteSpace(item2["每股资本公积金"].ToString())) ? 0 : float.Parse(item2["每股资本公积金"].ToString());///前第2季度的每股资本公积金
                    var jlr1 = (string.IsNullOrWhiteSpace(item1["净利润"].ToString())) ? 0 : float.Parse(item1["净利润"].ToString());///最新季度的净利润
                    var jlr2 = (string.IsNullOrWhiteSpace(item2["净利润"].ToString())) ? 0 : float.Parse(item2["净利润"].ToString());///前第2季度的净利润

                    if (1 <= mgjzc1 && 1 <= mgjzc2 && mgjzc2 * 1 <= mgjzc1 ///每股净资产 持续增加且增长10%以上
                && 1 <= mgsy1 && 1 <= mgsy2 && mgsy2 * 1<= mgsy1///每股收益 持续增加且增长10%以上
                && 1 <= mgxjhl1 && 1 <= mgxjhl2 && mgxjhl2 * 1 <= mgxjhl1///每股现金含量 持续增加且增长10%以上
                && 1 <= mgzbgjj1 && 1 <= mgzbgjj2 && mgzbgjj2 * 1 <= mgzbgjj1///每股资本公积金 持续增加且增长10%以上
                && 1 <= jlr1 && 1 <= jlr2 && jlr2 * 1 <= jlr1///净利润 持续增加且增长10%以上
                )
                    {
                        svItem.List.Add(string.Format("{0}{1}", stockCode, stockName));
                    }
                }
                catch(Exception ex)
                {
                    LOGGER.Log(string.Format("{0}",ex.Message));
                }
            };

            mongo.Traverse(dbName, sourceCollectionName, "{}");

            var svQuery = "{\"ContentType\":\"业绩连续两季度增长的股票\"}";
            mongo.Save3(dbName, targetCollectionName, svItem, svQuery);
        }
        #endregion

        #region 上涨4%-8%股票特点分析
        /// <summary>
        /// 上涨4%-8%股票特点分析
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void AnalyseRisingStock(DateTime startTime ,DateTime endTime,int min=4,int max=8)
        {
            var minVal = float.Parse(min.ToString()) / 100.0;
            var maxVal = float.Parse(max.ToString()) / 100.0;

            var mongo = DataStorage.GetInstance(DBType.MongoDB);
            var dbName = CONST.DB.DBName_StockService;
            var collectionKLine = CONST.DB.CollectionName_KLine;
            var collecrionSINADaDan2D = CONST.DB.CollectionName_DaDan;
            var collectionCWZY = CONST.DB.CollectionName_CWZY;
            var collectionRadar = CONST.DB.CollectionName_Radar;
            var collectionZJLX = CONST.DB.CollectionName_ZJLX;
            var collectionLHB = CONST.DB.CollectionName_LHB;
            var collectionBKGN = CONST.DB.CollectionName_BKGN;
            var collectionDataResult = CONST.DB.CollectionName_DataResult;
            var collectionLHBMX = CONST.DB.CollectionName_LHBMX;
            var collectionRZRQ = CONST.DB.CollectionName_RZRQ;

            var svItem = new
            {
                涉及股票 =new Dictionary<string, int> {{ "总数" ,0} },
                BKGN = new Dictionary<string, int> { { "总数", 0 } },
                PrevKLine = new Dictionary<string, int> { { "总数",0},{ "上涨", 0 }, { "下跌", 0 }, { "4%以下", 0 }, { "4%以上", 0 }, { "-4%及内", 0 }, { "小于-4%", 0 },{"前几日比当前价高",0 },{"前几日比当前价低",0 }},
                CWZY = new Dictionary<string, int> { { "总数", 0 }, { "每股净资产小于1", 0 }, { "每股净资产大于1", 0 }, { "每股净资产减少", 0 }, { "每股净资产增加", 0 }, { "每股收益小于1", 0 }, { "每股收益大于1", 0 }, { "每股收益减少", 0 }, { "每股收益增加", 0 }, { "每股现金含量小于1", 0 }, { "每股现金含量大于1", 0 }, { "每股现金含量减少", 0 }, { "每股现金含量增加", 0 }, { "每股资本公积金小于1", 0 }, { "每股资本公积金大于1", 0 }, { "每股资本公积金减少", 0 }, { "每股资本公积金增加", 0 } },
                Radar = new Dictionary<string, int> { { "总数", 0 } },
                DaDan = new Dictionary<string, int> { { "总数", 0 } },
                ZJLX = new Dictionary<string, int> { { "总数", 0 }, { "流入", 0 }, { "流出", 0 }, { "大单占比正值", 0 }, { "大单占比负值", 0 }, { "中单占比正值", 0 }, { "中单占比负值", 0 }, { "小单占比正值", 0 }, { "小单占比负值", 0 } },
                LHB = new { SBLX = new Dictionary<string, int> { { "总数", 0 } }, JMR = new Dictionary<string, int> { { "总数", 0 }, { "正值", 0 }, { "负值", 0 } } },
                RZRQ = new { RZYE = new Dictionary<string, int> { { "总数", 0 }, { "增加", 0 }, { "减少", 0 } }, RZMRE = new Dictionary<string, int> { { "总数", 0 }, { "增加", 0 }, { "减少", 0 } }, RQYLJE = new Dictionary<string, int> { { "总数", 0 }, { "增加", 0 }, { "减少", 0 } }, RQYL = new Dictionary<string, int> { { "总数", 0 }, { "增加", 0 }, { "减少", 0 } }, RQMCL = new Dictionary<string, int> { { "总数", 0 }, { "增加", 0 }, { "减少", 0 } }, RQYE = new Dictionary<string, int> { { "总数", 0 }, { "增加", 0 }, { "减少", 0 } } },
                股票占比 = new Dictionary<string, string>(),
                板块概念占比 = new Dictionary<string, string>(),
                之前日线占比 = new Dictionary<string, string>(),
                财务摘要占比 = new Dictionary<string, string>(),
                异动雷达占比 = new Dictionary<string, string>(),
                资金流向占比 = new Dictionary<string, string>(),
                龙虎榜净买入占比 = new Dictionary<string, string>(),
                融资余额占比 = new Dictionary<string, string>(),
                融资买入额占比 = new Dictionary<string, string>(),
                融券余量金额占比 = new Dictionary<string, string>(),
                融券余量占比 = new Dictionary<string, string>(),
                融券卖出量占比 = new Dictionary<string, string>(),
                融券余额占比 = new Dictionary<string, string>(),
                
            };

            #region 找所有的上涨指定涨幅的日线数据
            var query1 = "{\"Increase\":{$gte:0.04,$lte:0.08},\"TradingDate\":{$gte:new Date('" + string.Format("{0:yyyy/MM/dd}",startTime)+ "'),$lte:new Date('" + string.Format("{0:yyyy/MM/dd}", endTime) + "')}}";
            var targetKLineList = mongo.Find3(dbName, collectionKLine, query1);
            #endregion

            foreach (var kline in targetKLineList)
            {
                LOGGER.Log(string.Format("正在分析 {0} {1}",kline["StockCode"], kline["StockName"]));
                

                var stockCode = kline["StockCode"].ToString();
                var stockName = kline["StockName"].ToString();
                var itemTradingDate = DateTime.Parse(kline["TradingDate"].ToString());
                svItem.涉及股票["总数"] += 1;
                if(!svItem.涉及股票.ContainsKey(stockCode+stockName))
                {
                    svItem.涉及股票[stockCode + stockName] = 0;
                }
                svItem.涉及股票[stockCode + stockName] += 1;
                #region 数据查找 
                ///获取这个股票所在的板块，概念
                var queryBKGN = "{\"StockCode\":\"" + stockCode + "\"}";
                var bkgnList = mongo.Find3(dbName, collectionBKGN, queryBKGN);
                ///获取这个股票所在的前7交易日KLine
                var days = -7;
                var queryKLinePrev = "{\"StockCode\":\"" + stockCode + "\",\"TradingDate\":{$gte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate.AddDays(days)) + "'),$lte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate) + "')}}";
                var prevKLineList = mongo.Find3(dbName, collectionKLine, queryKLinePrev);
                ///获取这个股票的财务摘要
                var queryCWZY = "{\"StockCode\":\"" + stockCode + "\"}";
                var cwzyList = mongo.Find3(dbName, collectionCWZY, queryCWZY);
                ///获取前7日的股票雷达
                var queryRadar = "{\"StockCode\":\"" + stockCode + "\",\"TradingDate\":{$gte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate.AddDays(days)) + "'),$lte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate) + "')}}";
                var radarList = mongo.Find3(dbName, collectionRadar, queryRadar);
                ///获取前7日的大单数据
                var querySINADaDan2D = "{\"StockCode\":\"" + stockCode + "\",\"TradingDate\":{$gte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate.AddDays(days)) + "'),$lte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate) + "')}}";
                var dadanList = new List<Dictionary<string,object>>();// mongo.Find3(dbName, collecrionSINADaDan2D, querySINADaDan2D);

                ///获取前7日资金流向

                var queryZJLX = "{\"StockCode\":\"" + stockCode + "\",\"TradingDate\":{$gte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate.AddDays(days)) + "'),$lte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate) + "')}}";
                var zjlxList = mongo.Find3(dbName, collectionZJLX, queryZJLX);

                ///获取前7日的龙虎榜信息
                var queryLHB = "{\"StockCode\":\"" + stockCode + "\",\"TradingDate\":{$gte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate.AddDays(days)) + "'),$lte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate) + "')}}"; ;
                var lhbList = mongo.Find3(dbName, collectionLHB, queryLHB);

                ///获取前7日的龙虎榜明细信息
                var queryLHBMX = "{\"StockCode\":\"" + stockCode + "\",\"TradingDate\":{$gte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate.AddDays(days)) + "'),$lte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate) + "')}}"; ;
                var lhbmxList = mongo.Find3(dbName, collectionLHBMX, queryLHBMX);

                ///获取前7日的融资融券信息
                var queryRZRQ = "{\"StockCode\":\"" + stockCode + "\",\"TradingDate\":{$gte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate.AddDays(days)) + "'),$lte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate) + "')}}"; ;
                var rzrqList = mongo.Find3(dbName, collectionRZRQ, queryRZRQ);



                ///获取前7日该股的全网新闻
                ///获取前7日该股的个股资讯
                #endregion
 
                #region 数据分析

                #region 分析所在的板块
                foreach (Dictionary<string, object> bkgn in bkgnList)
                {
                    var gnList = (bkgn["PageData"] as Dictionary<string, object>)["所属概念"] as object[];
                    foreach (string gn in gnList)
                    {
                        svItem.BKGN["总数"] += 1;
                        if (!svItem.BKGN.ContainsKey(gn))
                        {
                            svItem.BKGN[gn] = 0;
                        }
                        svItem.BKGN[gn] += 1;
                    }
                }
                #endregion

                #region 分析上涨幅度/振幅/成交量/成交均价
                for (var k = 0; k < prevKLineList.Count; k++)
                {
                    svItem.PrevKLine["总数"] += 1;

                    var prevKLine = prevKLineList[k];
                    var increase = float.Parse(prevKLine["Increase"].ToString());///涨幅
                    var amplitude = float.Parse(prevKLine["Amplitude"].ToString());///振幅
                    var averagePrice = float.Parse(kline["AveragePrice"].ToString());
                    var prevAveragePrice = float.Parse(prevKLine["AveragePrice"].ToString());

                    if (increase <= 0)
                    {
                        svItem.PrevKLine["下跌"] += 1;

                        if (increase <= -0.04)
                        {
                            svItem.PrevKLine["小于-4%"] += 1;
                        }
                        else if (-0.04 < increase && increase <= 0)
                        {
                            svItem.PrevKLine["-4%及内"] += 1;
                        }
                    }
                    else if (0 < increase)
                    {
                        svItem.PrevKLine["上涨"] += 1;
                        if (increase <= 0.04)
                        {
                            svItem.PrevKLine["4%以下"] += 1;
                        }
                        else if (0.04 < increase)
                        {
                            svItem.PrevKLine["4%以上"] += 1;
                        }
                    }

                    if(prevAveragePrice<=averagePrice)
                    {
                        svItem.PrevKLine["前几日比当前价高"] += 1;
                    }
                    else
                    {
                        svItem.PrevKLine["前几日比当前价低"] += 1;
                    }

                    if (k != 0)
                    {
                        ///判断成交量成交均价的增加比例
                        var prev = prevKLineList[k - 1];

                    }
                }
                #endregion

                #region 财务分析 每股净资产/每股收益/每股现金含量/每股公积金/净利润比上一季度增加的比例/是否大于1
                if (0 < cwzyList.Count)
                {
                    svItem.CWZY["总数"] += 1;
                    var cwzy = cwzyList.First()["PageData"] as object[];
                    var cwzy0 = cwzy[0] as Dictionary<string, object>; ///最新季度
                    var cwzy1 = cwzy[1] as Dictionary<string, object>;
                    var mgjzc0 = (string.IsNullOrWhiteSpace(cwzy0["每股净资产"].ToString())) ? 0 : float.Parse(cwzy0["每股净资产"].ToString()); 
                    var mgjzc1 = (string.IsNullOrWhiteSpace(cwzy1["每股净资产"].ToString())) ? 0 : float.Parse(cwzy1["每股净资产"].ToString()); 
                    var mgsy0 = (string.IsNullOrWhiteSpace(cwzy0["每股收益"].ToString())) ? 0 : float.Parse(cwzy0["每股收益"].ToString()); 
                    var mgsy1 = (string.IsNullOrWhiteSpace(cwzy1["每股收益"].ToString())) ? 0 : float.Parse(cwzy1["每股收益"].ToString()); 
                    var mgxjhl0 = (string.IsNullOrWhiteSpace(cwzy0["每股现金含量"].ToString())) ? 0 : float.Parse(cwzy0["每股现金含量"].ToString()); 
                    var mgxjhl1 = (string.IsNullOrWhiteSpace(cwzy1["每股现金含量"].ToString())) ? 0 : float.Parse(cwzy1["每股现金含量"].ToString()); 
                    var mgzbgjj0 = (string.IsNullOrWhiteSpace(cwzy0["每股资本公积金"].ToString())) ? 0 : float.Parse(cwzy0["每股资本公积金"].ToString()); 
                    var mgzbgjj1 = (string.IsNullOrWhiteSpace(cwzy1["每股资本公积金"].ToString())) ? 0 : float.Parse(cwzy1["每股资本公积金"].ToString());

                    if (1 <= mgjzc0)
                    {
                        svItem.CWZY["每股净资产大于1"] += 1;
                    }
                    else if (mgjzc0 < 1)
                    {
                        svItem.CWZY["每股净资产小于1"] += 1;
                    }

                    if(mgjzc1<mgjzc0)
                    {
                        svItem.CWZY["每股净资产增加"] += 1;
                    }
                    else if (mgjzc0 <mgjzc1 )
                    {
                        svItem.CWZY["每股净资产减少"] += 1;
                    }

                    if (1 <= mgsy0)
                    {
                        svItem.CWZY["每股收益大于1"] += 1;
                    }
                    else if (mgsy0 < 1)
                    {
                        svItem.CWZY["每股收益小于1"] += 1;
                    }

                    if (mgsy1 < mgsy0)
                    {
                        svItem.CWZY["每股收益增加"] += 1;
                    }
                    else if (mgsy0 < mgsy1)
                    {
                        svItem.CWZY["每股收益减少"] += 1;
                    }

                    if (1 <= mgxjhl0)
                    {
                        svItem.CWZY["每股现金含量大于1"] += 1;
                    }
                    else if (mgxjhl0 < 1)
                    {
                        svItem.CWZY["每股现金含量小于1"] += 1;
                    }

                    if (mgxjhl1 < mgxjhl0)
                    {
                        svItem.CWZY["每股现金含量增加"] += 1;
                    }
                    else if (mgxjhl0 < mgxjhl1)
                    {
                        svItem.CWZY["每股现金含量减少"] += 1;
                    }

                    if (1 <= mgzbgjj0)
                    {
                        svItem.CWZY["每股资本公积金大于1"] += 1;
                    }
                    else if (mgzbgjj0 < 1)
                    {
                        svItem.CWZY["每股资本公积金小于1"] += 1;
                    }

                    if (mgzbgjj1 < mgzbgjj0)
                    {
                        svItem.CWZY["每股资本公积金增加"] += 1;
                    }
                    else if (mgzbgjj0 < mgzbgjj1)
                    {
                        svItem.CWZY["每股资本公积金减少"] += 1;
                    }

                }
                #endregion

                #region 股市雷达 哪种类型的多
                if (null != radarList)
                {
                    foreach (var radar in radarList)
                    {
                        svItem.Radar["总数"] += 1;
                        var radarType = radar["AnomalyInfo"].ToString();
                        if (!svItem.Radar.ContainsKey(radarType))
                        {
                            svItem.Radar.Add(radarType, 0);
                        }

                        svItem.Radar[radarType] += 1;
                    }
                }
                #endregion

                #region 大单数据 哪种类型的多
                if (null != dadanList)
                {
                    foreach (var dadan in dadanList)
                    {
                        svItem.DaDan["总数"] += 1;
                        var kind = dadan["Kind"].ToString();
                        if (!svItem.DaDan.ContainsKey(kind))
                        {
                            svItem.DaDan.Add(kind, 0);
                        }

                        svItem.DaDan[kind] += 1;
                    }
                }
                #endregion

                #region 资金流向 大中小单占比 流入流出
                if(null != zjlxList)
                {
                    for (int k = 0; k < zjlxList.Count; k++)
                    {
                        svItem.ZJLX["总数"] += 1;
                        var zjlx = zjlxList[k];
                        var netInflow =float.Parse( zjlx["NetInflow"].ToString());///资金净流入
                        var netLargeProportion = float.Parse(zjlx["NetLargeProportion"].ToString());///大单占比
                        var netMediumProportion = float.Parse(zjlx["NetMediumProportion"].ToString());///中单占比
                        var netSmallProportion = float.Parse(zjlx["NetSmallProportion"].ToString());///小单占比

                        if (0 < netInflow)
                        {
                            svItem.ZJLX["流入"] += 1;
                        }
                        else if (netInflow < 0)
                        {
                            svItem.ZJLX["流出"] += 1;
                        }

                        if (0 < netLargeProportion)
                        {
                            svItem.ZJLX["大单占比正值"] += 1;
                        }
                        else if (netLargeProportion< 0 )
                        {
                            svItem.ZJLX["大单占比负值"] += 1;
                        }

                        if (0 < netMediumProportion)
                        {
                            svItem.ZJLX["中单占比正值"] += 1;
                        }
                        else if (netMediumProportion < 0)
                        {
                            svItem.ZJLX["中单占比负值"] += 1;
                        }

                        if (0 < netSmallProportion)
                        {
                            svItem.ZJLX["小单占比正值"] += 1;
                        }
                        else if (netSmallProportion < 0)
                        {
                            svItem.ZJLX["小单占比负值"] += 1;
                        }

                    }
                }
                #endregion

                #region  龙虎榜数据
                if (null != lhbList)
                {
                    for (int k = 0; k < lhbList.Count; k++)
                    {
                       
                        var lhb = lhbList[k]["Data"] as Dictionary<string,object>;
                        if(8 == lhb.Keys.Count)
                        {
                            var rq = lhb["C1"];///日期
                            var sblx = lhb["C2"].ToString(); ///上榜类型
                            var spj =("-" == lhb["C3"].ToString()) ?0: float.Parse(lhb["C3"].ToString());///收盘价
                            var crzd = ("-" == lhb["C4"].ToString()) ? 0 : float.Parse(lhb["C4"].ToString()); ///次日涨跌
                            var mr1 = ("-" == lhb["C5"].ToString()) ? 0 : float.Parse(lhb["C5"].ToString()); ///买入
                            var mr2 = ("-" == lhb["C6"].ToString()) ? 0 : float.Parse(lhb["C6"].ToString()); ///卖出
                            var jmr = ("-" == lhb["C7"].ToString()) ? 0 : float.Parse(lhb["C7"].ToString());///净买入

                            if (!svItem.LHB.SBLX.ContainsKey(sblx))
                            {
                                svItem.LHB.SBLX.Add(sblx, 0);
                            }
                            svItem.LHB.SBLX["总数"] += 1;
                            svItem.LHB.SBLX[sblx] += 1;

                            svItem.LHB.JMR["总数"] += 1;
                            if (0 < jmr)
                            {
                                svItem.LHB.JMR["正值"] += 1;
                            }
                            else
                            {
                                svItem.LHB.JMR["负值"] += 1;
                            }

                        }
                    }

                }
                #endregion

                #region 融资融券信息
                if(null != rzrqList)
                {
                    for (int k = 1; k < rzrqList.Count; k++)
                    {
                        
                        var rzrq0 = rzrqList[k];
                        var rzrq1 = rzrqList[k-1];
                        var rzye0 = float.Parse(rzrq0["RZYE"].ToString());///融资余额
                        var rzye1 = float.Parse(rzrq1["RZYE"].ToString());///前2日融资余额
                        var rzmre0 = float.Parse(rzrq0["RZMRE"].ToString());///融资买入额
                        var rzmre1 = float.Parse(rzrq1["RZMRE"].ToString());///前2日融资买入额
                        //var rzche0 = float.Parse(rzrq0["RZCHE"].ToString());///融资偿还额
                        //var rzche1 = float.Parse(rzrq1["RZCHE"].ToString());///前2日融资偿还额
                        var rqylje0 = float.Parse(rzrq0["RQYLJE"].ToString());///融券余量金额
                        var rqylje1 = float.Parse(rzrq1["RQYLJE"].ToString());///前2日融券余量金额
                        var rqyl0 = float.Parse(rzrq0["RQYL"].ToString());///融券余量
                        var rqyl1 = float.Parse(rzrq1["RQYL"].ToString());///前2日融券余量
                        var rqmcl0 = float.Parse(rzrq0["RQMCL"].ToString());///融券卖出量
                        var rqmcl1 = float.Parse(rzrq1["RQMCL"].ToString());///前2日融券卖出量
                        //var rqchl0 = float.Parse(rzrq0["RQCHL"].ToString());///融券偿还量
                        //var rqchl1 = float.Parse(rzrq1["RQCHL"].ToString());///前2日融券偿还量
                        var rqye0 = float.Parse(rzrq0["RQYE"].ToString());///融券余额
                        var rqye1 = float.Parse(rzrq1["RQYE"].ToString());///前2日融券余额

                        svItem.RZRQ.RQYE["总数"] += 1;
                        if ( rzye1<= rzye0) ///融资余额
                        {
                            svItem.RZRQ.RQYE["增加"] += 1;
                        }
                        else
                        {
                            svItem.RZRQ.RQYE["减少"] += 1;
                        }

                        svItem.RZRQ.RZMRE["总数"] += 1;
                        if (rzmre1 <= rzmre0) ///融资买入额
                        {
                            svItem.RZRQ.RZMRE["增加"] += 1;
                        }
                        else
                        {
                            svItem.RZRQ.RZMRE["减少"] += 1;
                        }

                        svItem.RZRQ.RQYLJE["总数"] += 1;
                        if (rqylje1 <= rqylje0) ///融券余量金额
                        {
                            svItem.RZRQ.RQYLJE["增加"] += 1;
                        }
                        else
                        {
                            svItem.RZRQ.RQYLJE["减少"] += 1;
                        }

                        svItem.RZRQ.RQYL["总数"] += 1;
                        if (rqyl1 <= rqyl0) ///融券余量
                        {
                            svItem.RZRQ.RQYL["增加"] += 1;
                        }
                        else
                        {
                            svItem.RZRQ.RQYL["减少"] += 1;
                        }

                        svItem.RZRQ.RQMCL["总数"] += 1;
                        if (rqmcl1 <= rqmcl0) ///融券卖出量
                        {
                            svItem.RZRQ.RQMCL["增加"] += 1;
                        }
                        else
                        {
                            svItem.RZRQ.RQMCL["减少"] += 1;
                        }

                        svItem.RZRQ.RQYE["总数"] += 1;
                        if (rqye1 <= rqye0) ///融券余额
                        {
                            svItem.RZRQ.RQYE["增加"] += 1;
                        }
                        else
                        {
                            svItem.RZRQ.RQYE["减少"] += 1;
                        }
                    }
                }
                #endregion

                #endregion


            }

            Func<Dictionary<string,int>,Dictionary<string,string>,string,int> CalRate = (Dictionary<string, int> inData1, Dictionary<string, string> inData2,string msg) => {
                var res =inData2;
                var totalCount = inData1["总数"];
                var src = from item in inData1 orderby item.Value descending select item;
                if (0 < totalCount)
                {
                    foreach (var item in src)
                    {
                        res.Add(item.Key + "占比", string.Format("共{0}计数，占比{1:P}",item.Value, float.Parse( item.Value.ToString()) / float.Parse(totalCount.ToString())));
                    }
                }
                return 0;
            };

            CalRate(svItem.涉及股票, svItem.股票占比, "0");
            CalRate(svItem.BKGN,svItem.板块概念占比,"1");
            CalRate(svItem.PrevKLine, svItem.之前日线占比,"2");
            CalRate(svItem.CWZY, svItem.财务摘要占比,"3");
            CalRate(svItem.Radar, svItem.异动雷达占比,"4");
            CalRate(svItem.ZJLX, svItem.资金流向占比,"5");
            CalRate(svItem.LHB.JMR, svItem.龙虎榜净买入占比,"6");
            CalRate(svItem.RZRQ.RZYE, svItem.融资余额占比,"7");
            CalRate(svItem.RZRQ.RZMRE, svItem.融资买入额占比,"8");
            CalRate(svItem.RZRQ.RQYLJE, svItem.融券余量金额占比,"9");
            CalRate(svItem.RZRQ.RQYL, svItem.融券余量占比,"10");
            CalRate(svItem.RZRQ.RQMCL, svItem.融券卖出量占比,"11");
            CalRate(svItem.RZRQ.RQYE, svItem.融券余额占比,"12");




            var dict = Convertor.FromObjectToDictionary3(svItem);
            var json = Convertor.FromObjectToJson(svItem);
            File.WriteAllText("E:\\股票结果.txt", json, Encoding.UTF8);
            Func<Dictionary<string, string>, string, string> OutPutHtml = (Dictionary<string, string> inData, string title) => {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendFormat("<h2>{0}</h2>",title);
                stringBuilder.AppendFormat("<ul>");
                foreach (var item in inData)
                {
                    stringBuilder.AppendFormat("<li>{0}&nbsp;&nbsp;&nbsp;&nbsp;{1}</li>", item.Key, item.Value);
                }
                stringBuilder.AppendFormat("</ul>");

                return stringBuilder.ToString();
            };
            var htmlBuilder = new StringBuilder();
            htmlBuilder.AppendFormat("<!DOCTYPE html><html><head><meta charset=\"utf-8\" /><title>汪德禄专用&nbsp;&nbsp;&nbsp;&nbsp;前{0}个交易日股票上涨{1}%-{2}%特征分析</title></head><body style=\"font-size: 20px;\">", (endTime-startTime).Days,min,max);
            htmlBuilder.AppendFormat("<h1>汪德禄专用&nbsp;&nbsp;&nbsp;&nbsp;前{0}个交易日股票上涨{1}%-{2}%特征分析&nbsp;&nbsp;起始时间：{3}&nbsp;&nbsp;&nbsp;&nbsp;{4}</h1>", (endTime - startTime).Days, min, max,startTime,endTime);
            htmlBuilder.AppendFormat("{0}",OutPutHtml(svItem.之前日线占比, "之前日线占比"));
            htmlBuilder.AppendFormat("{0}", OutPutHtml(svItem.财务摘要占比, "财务摘要占比"));
            htmlBuilder.AppendFormat("{0}", OutPutHtml(svItem.异动雷达占比, "异动雷达占比"));
            htmlBuilder.AppendFormat("{0}", OutPutHtml(svItem.资金流向占比, "资金流向占比"));
            htmlBuilder.AppendFormat("{0}", OutPutHtml(svItem.龙虎榜净买入占比, "龙虎榜净买入占比"));
            htmlBuilder.AppendFormat("{0}", OutPutHtml(svItem.融资余额占比, "融资余额占比"));
            htmlBuilder.AppendFormat("{0}", OutPutHtml(svItem.融资买入额占比, "融资买入额占比"));
            htmlBuilder.AppendFormat("{0}", OutPutHtml(svItem.融券余量金额占比, "融券余量金额占比"));
            htmlBuilder.AppendFormat("{0}", OutPutHtml(svItem.融券余量占比, "融券余量占比"));
            htmlBuilder.AppendFormat("{0}", OutPutHtml(svItem.融券卖出量占比, "融券卖出量占比"));
            htmlBuilder.AppendFormat("{0}", OutPutHtml(svItem.融券余额占比, "融券余额占比"));
            htmlBuilder.AppendFormat("{0}", OutPutHtml(svItem.股票占比, "股票占比"));
            htmlBuilder.AppendFormat("{0}", OutPutHtml(svItem.板块概念占比, "板块概念占比"));
            htmlBuilder.AppendFormat("</body></html>");
            File.WriteAllText("E://gupiao.html", htmlBuilder.ToString(), Encoding.UTF8);
            mongo.Save3(dbName, collectionDataResult, svItem);
        }
        #endregion

        #region 获取所有股票指定日期
        public List<Dictionary<string,object>> GetRisingStock(DateTime startTime,DateTime endTime)
        {
            var mongo = DataStorage.GetInstance(DBType.MongoDB);
            var dbName = CONST.DB.DBName_StockService;
            var collectionKLine = CONST.DB.CollectionName_KLine;
            var days = (endTime - startTime).Days;
            #region 找所有的上涨指定涨幅的日线数据
            var query1 = "{\"TradingDate\":{$gte:new Date('" + string.Format("{0:yyyy/MM/dd}", startTime) + "'),$lte:new Date('" + string.Format("{0:yyyy/MM/dd}", endTime) + "')}}";
            var sourceKLineList = mongo.Find3(dbName, collectionKLine, query1);
            var sourceKLineDict = new Dictionary<string, List<Dictionary<string, object>>>();
            foreach (var item in sourceKLineList)
            {
                var stockCode = item["StockCode"].ToString();
                if (!sourceKLineDict.ContainsKey(stockCode))
                {
                    sourceKLineDict.Add(stockCode, new List<Dictionary<string, object>>());
                }
                sourceKLineDict[stockCode].Add(item);


            }
            var targetKLineList = new List<Dictionary<string, object>>();
            foreach (var srcItem in sourceKLineDict)
            {
                var sort = (from item in srcItem.Value orderby DateTime.Parse(item["TradingDate"].ToString()) ascending select item).ToList();
                for (int k = 0; k < sort.Count; k++)
                {
                    var averagePriceFirst = float.Parse(sort.First()["AveragePrice"].ToString());
                    var averagePriceLast = float.Parse(sort.Last()["AveragePrice"].ToString());
                    if ((averagePriceFirst * (100.0f + days) / 100.0f) < averagePriceLast)
                    {
                        targetKLineList.Add(sort[k]);
                    }
                }
            }
            #endregion

            return targetKLineList;
        }
        #endregion

        #region 上涨特征分析
        /// <summary>
        /// 上涨4%-8%股票特点分析
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void AnalyseStockSymbol(List<Dictionary<string,object>> targetKLineList,string title)
        {
            var mongo = DataStorage.GetInstance(DBType.MongoDB);
            var dbName = CONST.DB.DBName_StockService;
            var collectionKLine = CONST.DB.CollectionName_KLine;
            var collecrionSINADaDan2D = CONST.DB.CollectionName_DaDan;
            var collectionCWZY = CONST.DB.CollectionName_CWZY;
            var collectionRadar = CONST.DB.CollectionName_Radar;
            var collectionZJLX = CONST.DB.CollectionName_ZJLX;
            var collectionLHB = CONST.DB.CollectionName_LHB;
            var collectionBKGN = CONST.DB.CollectionName_BKGN;
            var collectionDataResult = CONST.DB.CollectionName_DataResult;
            var collectionLHBMX = CONST.DB.CollectionName_LHBMX;
            var collectionRZRQ = CONST.DB.CollectionName_RZRQ;

            var svItem = new
            {
                涉及股票 = new Dictionary<string, List<string>> { { "总数",new List<string>() } },
                BKGN = new Dictionary<string,  List<string>> { { "总数", new List<string>() } },
                PrevKLine = new Dictionary<string,  List<string>> { { "总数", new List<string>() }, { "上涨", new List<string>() }, { "下跌", new List<string>() }, { "4%以下", new List<string>() }, { "4%以上", new List<string>() }, { "-4%及内", new List<string>() }, { "小于-4%", new List<string>() }, { "前几日比当前价高", new List<string>() }, { "前几日比当前价低", new List<string>() } },
                CWZY = new Dictionary<string,  List<string>> { { "总数", new List<string>() }, { "每股净资产小于1", new List<string>() }, { "每股净资产大于1", new List<string>() }, { "每股净资产减少", new List<string>() }, { "每股净资产增加", new List<string>() }, { "每股收益小于1", new List<string>() }, { "每股收益大于1", new List<string>() }, { "每股收益减少", new List<string>() }, { "每股收益增加", new List<string>() }, { "每股现金含量小于1", new List<string>() }, { "每股现金含量大于1", new List<string>() }, { "每股现金含量减少", new List<string>() }, { "每股现金含量增加", new List<string>() }, { "每股资本公积金小于1", new List<string>() }, { "每股资本公积金大于1", new List<string>() }, { "每股资本公积金减少", new List<string>() }, { "每股资本公积金增加", new List<string>() } },
                Radar = new Dictionary<string,  List<string>> { { "总数", new List<string>() } },
                DaDan = new Dictionary<string,  List<string>> { { "总数", new List<string>() }, { "成交额大于500万", new List<string>() }, { "成交额小于500万", new List<string>() } },
                ZJLX = new Dictionary<string,  List<string>> { { "总数", new List<string>() }, { "流入", new List<string>() }, { "流出", new List<string>() }, { "大单占比正值", new List<string>() }, { "大单占比负值", new List<string>() }, { "中单占比正值", new List<string>() }, { "中单占比负值", new List<string>() }, { "小单占比正值", new List<string>() }, { "小单占比负值", new List<string>() } },
                LHB = new { SBLX = new Dictionary<string,  List<string>> { { "总数", new List<string>() } }, JMR = new Dictionary<string,  List<string>> { { "总数", new List<string>() }, { "正值", new List<string>() }, { "负值", new List<string>() } } },
                RZRQ = new { RZYE = new Dictionary<string,  List<string>> { { "总数", new List<string>() }, { "增加", new List<string>() }, { "减少", new List<string>() } }, RZMRE = new Dictionary<string,  List<string>> { { "总数", new List<string>() }, { "增加", new List<string>() }, { "减少", new List<string>() } }, RQYLJE = new Dictionary<string,  List<string>> { { "总数", new List<string>() }, { "增加", new List<string>() }, { "减少", new List<string>() } }, RQYL = new Dictionary<string,  List<string>> { { "总数", new List<string>() }, { "增加", new List<string>() }, { "减少", new List<string>() } }, RQMCL = new Dictionary<string,  List<string>> { { "总数", new List<string>() }, { "增加", new List<string>() }, { "减少", new List<string>() } }, RQYE = new Dictionary<string,  List<string>> { { "总数", new List<string>() }, { "增加", new List<string>() }, { "减少", new List<string>() } } },
                
            };

 

            foreach (var kline in targetKLineList)
            {
                LOGGER.Log(string.Format("正在分析 {0} {1}", kline["StockCode"], kline["StockName"]));


                var stockCode = kline["StockCode"].ToString();
                var stockName = kline["StockName"].ToString();
                var itemTradingDate = DateTime.Parse(kline["TradingDate"].ToString());
                svItem.涉及股票["总数"].Add(string.Format("{0}{1}",stockCode,stockName));
                if (!svItem.涉及股票.ContainsKey(stockCode + stockName))
                {
                    svItem.涉及股票[stockCode + stockName] = new List<string> ();
                }
                svItem.涉及股票[stockCode + stockName] .Add(string.Format("{0}{1}",stockCode,stockName));
                
                #region 数据查找 
                ///获取这个股票所在的板块，概念
                var queryBKGN = "{\"StockCode\":\"" + stockCode + "\"}";
                var bkgnList = mongo.Find3(dbName, collectionBKGN, queryBKGN);
                ///获取这个股票所在的前7交易日KLine
                var days = -7;
                var queryKLinePrev = "{\"StockCode\":\"" + stockCode + "\",\"TradingDate\":{$gte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate.AddDays(days)) + "'),$lte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate) + "')}}";
                var prevKLineList = mongo.Find3(dbName, collectionKLine, queryKLinePrev);
                ///获取这个股票的财务摘要
                var queryCWZY = "{\"StockCode\":\"" + stockCode + "\"}";
                var cwzyList = mongo.Find3(dbName, collectionCWZY, queryCWZY);
                ///获取前7日的股票雷达
                var queryRadar = "{\"StockCode\":\"" + stockCode + "\",\"TradingDate\":{$gte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate.AddDays(days)) + "'),$lte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate) + "')}}";
                var radarList = mongo.Find3(dbName, collectionRadar, queryRadar);
                ///获取前7日的大单数据
                var querySINADaDan2D = "{\"StockCode\":\"" + stockCode + "\",\"TradingDate\":{$gte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate.AddDays(days)) + "'),$lte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate) + "')}}";
                var dadanList = mongo.Find3(dbName, collecrionSINADaDan2D, querySINADaDan2D);

                ///获取前7日资金流向

                var queryZJLX = "{\"StockCode\":\"" + stockCode + "\",\"TradingDate\":{$gte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate.AddDays(days)) + "'),$lte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate) + "')}}";
                var zjlxList = mongo.Find3(dbName, collectionZJLX, queryZJLX);

                ///获取前7日的龙虎榜信息
                var queryLHB = "{\"StockCode\":\"" + stockCode + "\",\"TradingDate\":{$gte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate.AddDays(days)) + "'),$lte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate) + "')}}"; ;
                var lhbList = mongo.Find3(dbName, collectionLHB, queryLHB);

                ///获取前7日的龙虎榜明细信息
                var queryLHBMX = "{\"StockCode\":\"" + stockCode + "\",\"TradingDate\":{$gte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate.AddDays(days)) + "'),$lte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate) + "')}}"; ;
                var lhbmxList = mongo.Find3(dbName, collectionLHBMX, queryLHBMX);

                ///获取前7日的融资融券信息
                var queryRZRQ = "{\"StockCode\":\"" + stockCode + "\",\"TradingDate\":{$gte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate.AddDays(days)) + "'),$lte:new Date('" + string.Format("{0:yyyy/MM/dd}", itemTradingDate) + "')}}"; ;
                var rzrqList = mongo.Find3(dbName, collectionRZRQ, queryRZRQ);



                ///获取前7日该股的全网新闻
                ///获取前7日该股的个股资讯
                #endregion

                #region 数据分析

                #region 分析所在的板块
                foreach (Dictionary<string, object> bkgn in bkgnList)
                {
                    var gnList = (bkgn["PageData"] as Dictionary<string, object>)["所属概念"] as object[];
                    foreach (string gn in gnList)
                    {
                        svItem.BKGN["总数"] .Add(string.Format("{0}{1}",stockCode,stockName));
                        if (!svItem.BKGN.ContainsKey(gn))
                        {
                            svItem.BKGN[gn] = new List<string>();
                        }
                        svItem.BKGN[gn] .Add(string.Format("{0}{1}",stockCode,stockName));
                    }
                }
                #endregion

                #region 分析上涨幅度/振幅/成交量/成交均价
                for (var k = 0; k < prevKLineList.Count; k++)
                {
                    svItem.PrevKLine["总数"] .Add(string.Format("{0}{1}",stockCode,stockName));

                    var prevKLine = prevKLineList[k];
                    var increase = float.Parse(prevKLine["Increase"].ToString());///涨幅
                    var amplitude = float.Parse(prevKLine["Amplitude"].ToString());///振幅
                    var averagePrice = float.Parse(kline["AveragePrice"].ToString());
                    var prevAveragePrice = float.Parse(prevKLine["AveragePrice"].ToString());

                    if (increase <= 0)
                    {
                        svItem.PrevKLine["下跌"] .Add(string.Format("{0}{1}",stockCode,stockName));

                        if (increase <= -0.04)
                        {
                            svItem.PrevKLine["小于-4%"] .Add(string.Format("{0}{1}",stockCode,stockName));
                        }
                        else if (-0.04 < increase && increase <= 0)
                        {
                            svItem.PrevKLine["-4%及内"] .Add(string.Format("{0}{1}",stockCode,stockName));
                        }
                    }
                    else if (0 < increase)
                    {
                        svItem.PrevKLine["上涨"] .Add(string.Format("{0}{1}",stockCode,stockName));
                        if (increase <= 0.04)
                        {
                            svItem.PrevKLine["4%以下"] .Add(string.Format("{0}{1}",stockCode,stockName));
                        }
                        else if (0.04 < increase)
                        {
                            svItem.PrevKLine["4%以上"] .Add(string.Format("{0}{1}",stockCode,stockName));
                        }
                    }

                    if (prevAveragePrice <= averagePrice)
                    {
                        svItem.PrevKLine["前几日比当前价高"] .Add(string.Format("{0}{1}",stockCode,stockName));
                    }
                    else
                    {
                        svItem.PrevKLine["前几日比当前价低"] .Add(string.Format("{0}{1}",stockCode,stockName));
                    }

                    if (k != 0)
                    {
                        ///判断成交量成交均价的增加比例
                        var prev = prevKLineList[k - 1];

                    }
                }
                #endregion

                #region 财务分析 每股净资产/每股收益/每股现金含量/每股公积金/净利润比上一季度增加的比例/是否大于1
                if (0 < cwzyList.Count)
                {
                    svItem.CWZY["总数"] .Add(string.Format("{0}{1}",stockCode,stockName));
                    var cwzy = cwzyList.First()["PageData"] as object[];
                    var cwzy0 = cwzy[0] as Dictionary<string, object>; ///最新季度
                    var cwzy1 = cwzy[1] as Dictionary<string, object>;
                    var mgjzc0 = (string.IsNullOrWhiteSpace(cwzy0["每股净资产"].ToString())) ? 0 : float.Parse(cwzy0["每股净资产"].ToString());
                    var mgjzc1 = (string.IsNullOrWhiteSpace(cwzy1["每股净资产"].ToString())) ? 0 : float.Parse(cwzy1["每股净资产"].ToString());
                    var mgsy0 = (string.IsNullOrWhiteSpace(cwzy0["每股收益"].ToString())) ? 0 : float.Parse(cwzy0["每股收益"].ToString());
                    var mgsy1 = (string.IsNullOrWhiteSpace(cwzy1["每股收益"].ToString())) ? 0 : float.Parse(cwzy1["每股收益"].ToString());
                    var mgxjhl0 = (string.IsNullOrWhiteSpace(cwzy0["每股现金含量"].ToString())) ? 0 : float.Parse(cwzy0["每股现金含量"].ToString());
                    var mgxjhl1 = (string.IsNullOrWhiteSpace(cwzy1["每股现金含量"].ToString())) ? 0 : float.Parse(cwzy1["每股现金含量"].ToString());
                    var mgzbgjj0 = (string.IsNullOrWhiteSpace(cwzy0["每股资本公积金"].ToString())) ? 0 : float.Parse(cwzy0["每股资本公积金"].ToString());
                    var mgzbgjj1 = (string.IsNullOrWhiteSpace(cwzy1["每股资本公积金"].ToString())) ? 0 : float.Parse(cwzy1["每股资本公积金"].ToString());

                    if (1 <= mgjzc0)
                    {
                        svItem.CWZY["每股净资产大于1"] .Add(string.Format("{0}{1}",stockCode,stockName));
                    }
                    else if (mgjzc0 < 1)
                    {
                        svItem.CWZY["每股净资产小于1"] .Add(string.Format("{0}{1}",stockCode,stockName));
                    }

                    if (mgjzc1 < mgjzc0)
                    {
                        svItem.CWZY["每股净资产增加"] .Add(string.Format("{0}{1}",stockCode,stockName));
                    }
                    else if (mgjzc0 < mgjzc1)
                    {
                        svItem.CWZY["每股净资产减少"] .Add(string.Format("{0}{1}",stockCode,stockName));
                    }

                    if (1 <= mgsy0)
                    {
                        svItem.CWZY["每股收益大于1"] .Add(string.Format("{0}{1}",stockCode,stockName));
                    }
                    else if (mgsy0 < 1)
                    {
                        svItem.CWZY["每股收益小于1"] .Add(string.Format("{0}{1}",stockCode,stockName)); 
                    }

                    if (mgsy1 < mgsy0)
                    {
                        svItem.CWZY["每股收益增加"] .Add(string.Format("{0}{1}",stockCode,stockName)); 
                    }
                    else if (mgsy0 < mgsy1)
                    {
                        svItem.CWZY["每股收益减少"] .Add(string.Format("{0}{1}",stockCode,stockName));
                    }

                    if (1 <= mgxjhl0)
                    {
                        svItem.CWZY["每股现金含量大于1"] .Add(string.Format("{0}{1}",stockCode,stockName));
                    }
                    else if (mgxjhl0 < 1)
                    {
                        svItem.CWZY["每股现金含量小于1"] .Add(string.Format("{0}{1}",stockCode,stockName)); 
                    }

                    if (mgxjhl1 < mgxjhl0)
                    {
                        svItem.CWZY["每股现金含量增加"] .Add(string.Format("{0}{1}",stockCode,stockName));
                    }
                    else if (mgxjhl0 < mgxjhl1)
                    {
                        svItem.CWZY["每股现金含量减少"] .Add(string.Format("{0}{1}",stockCode,stockName));
                    }

                    if (1 <= mgzbgjj0)
                    {
                        svItem.CWZY["每股资本公积金大于1"] .Add(string.Format("{0}{1}",stockCode,stockName));
                    }
                    else if (mgzbgjj0 < 1)
                    {
                        svItem.CWZY["每股资本公积金小于1"] .Add(string.Format("{0}{1}",stockCode,stockName));
                    }

                    if (mgzbgjj1 < mgzbgjj0)
                    {
                        svItem.CWZY["每股资本公积金增加"] .Add(string.Format("{0}{1}",stockCode,stockName));
                    }
                    else if (mgzbgjj0 < mgzbgjj1)
                    {
                        svItem.CWZY["每股资本公积金减少"] .Add(string.Format("{0}{1}",stockCode,stockName));
                    }

                }
                #endregion

                #region 股市雷达 哪种类型的多
                if (null != radarList)
                {
                    foreach (var radar in radarList)
                    {
                        svItem.Radar["总数"] .Add(string.Format("{0}{1}",stockCode,stockName));
                        var radarType = radar["AnomalyInfo"].ToString();
                        if (!svItem.Radar.ContainsKey(radarType))
                        {
                            svItem.Radar.Add(radarType,new List<string>());
                        }

                        svItem.Radar[radarType] .Add(string.Format("{0}{1}",stockCode,stockName));
                    }
                }
                #endregion

                #region 大单数据 哪种类型的多
                if (null != dadanList)
                {
                    foreach (var dadan in dadanList)
                    {
                        svItem.DaDan["总数"] .Add(string.Format("{0}{1}",stockCode,stockName));
                        var kind = dadan["Kind"].ToString();
                        if (!svItem.DaDan.ContainsKey(kind))
                        {
                            svItem.DaDan.Add(kind,new List<string> ());
                        }

                        svItem.DaDan[kind] .Add(string.Format("{0}{1}",stockCode,stockName));
                    }
                }
                #endregion

                #region 资金流向 大中小单占比 流入流出
                if (null != zjlxList)
                {
                    for (int k = 0; k < zjlxList.Count; k++)
                    {
                        svItem.ZJLX["总数"] .Add(string.Format("{0}{1}",stockCode,stockName));
                        var zjlx = zjlxList[k];
                        if (zjlx.ContainsKey("NetLargeProportion"))
                        {
                            var netInflow = float.Parse(zjlx["NetInflow"].ToString());///资金净流入
                            var netLargeProportion = float.Parse(zjlx["NetLargeProportion"].ToString());///大单占比
                            var netMediumProportion = float.Parse(zjlx["NetMediumProportion"].ToString());///中单占比
                            var netSmallProportion = float.Parse(zjlx["NetSmallProportion"].ToString());///小单占比

                            if (0 < netInflow)
                            {
                                svItem.ZJLX["流入"] .Add(string.Format("{0}{1}",stockCode,stockName));
                            }
                            else if (netInflow < 0)
                            {
                                svItem.ZJLX["流出"] .Add(string.Format("{0}{1}",stockCode,stockName));
                            }

                            if (0 < netLargeProportion)
                            {
                                svItem.ZJLX["大单占比正值"] .Add(string.Format("{0}{1}",stockCode,stockName));
                            }
                            else if (netLargeProportion < 0)
                            {
                                svItem.ZJLX["大单占比负值"] .Add(string.Format("{0}{1}",stockCode,stockName));
                            }

                            if (0 < netMediumProportion)
                            {
                                svItem.ZJLX["中单占比正值"] .Add(string.Format("{0}{1}",stockCode,stockName));
                            }
                            else if (netMediumProportion < 0)
                            {
                                svItem.ZJLX["中单占比负值"] .Add(string.Format("{0}{1}",stockCode,stockName));
                            }

                            if (0 < netSmallProportion)
                            {
                                svItem.ZJLX["小单占比正值"] .Add(string.Format("{0}{1}",stockCode,stockName));
                            }
                            else if (netSmallProportion < 0)
                            {
                                svItem.ZJLX["小单占比负值"] .Add(string.Format("{0}{1}",stockCode,stockName));
                            }
                        }
                    }
                }
                #endregion

                #region  龙虎榜数据
                if (null != lhbList)
                {
                    for (int k = 0; k < lhbList.Count; k++)
                    {

                        var lhb = lhbList[k]["Data"] as Dictionary<string, object>;
                        if (8 == lhb.Keys.Count)
                        {
                            var rq = lhb["C1"];///日期
                            var sblx = lhb["C2"].ToString(); ///上榜类型
                            var spj = ("-" == lhb["C3"].ToString()) ? 0 : float.Parse(lhb["C3"].ToString());///收盘价
                            var crzd = ("-" == lhb["C4"].ToString()) ? 0 : float.Parse(lhb["C4"].ToString()); ///次日涨跌
                            var mr1 = ("-" == lhb["C5"].ToString()) ? 0 : float.Parse(lhb["C5"].ToString()); ///买入
                            var mr2 = ("-" == lhb["C6"].ToString()) ? 0 : float.Parse(lhb["C6"].ToString()); ///卖出
                            var jmr = ("-" == lhb["C7"].ToString()) ? 0 : float.Parse(lhb["C7"].ToString());///净买入

                            if (!svItem.LHB.SBLX.ContainsKey(sblx))
                            {
                                svItem.LHB.SBLX.Add(sblx, new List<string>());
                            }
                            svItem.LHB.SBLX["总数"] .Add(string.Format("{0}{1}",stockCode,stockName));
                            svItem.LHB.SBLX[sblx] .Add(string.Format("{0}{1}",stockCode,stockName));

                            svItem.LHB.JMR["总数"] .Add(string.Format("{0}{1}",stockCode,stockName));
                            if (0 < jmr)
                            {
                                svItem.LHB.JMR["正值"] .Add(string.Format("{0}{1}",stockCode,stockName));
                            }
                            else
                            {
                                svItem.LHB.JMR["负值"] .Add(string.Format("{0}{1}",stockCode,stockName));
                            }

                        }
                    }

                }
                #endregion

                #region 融资融券信息
                if (null != rzrqList)
                {
                    for (int k = 1; k < rzrqList.Count; k++)
                    {

                        var rzrq0 = rzrqList[k];
                        var rzrq1 = rzrqList[k - 1];
                        var rzye0 = float.Parse(rzrq0["RZYE"].ToString());///融资余额
                        var rzye1 = float.Parse(rzrq1["RZYE"].ToString());///前2日融资余额
                        var rzmre0 = float.Parse(rzrq0["RZMRE"].ToString());///融资买入额
                        var rzmre1 = float.Parse(rzrq1["RZMRE"].ToString());///前2日融资买入额
                        //var rzche0 = float.Parse(rzrq0["RZCHE"].ToString());///融资偿还额
                        //var rzche1 = float.Parse(rzrq1["RZCHE"].ToString());///前2日融资偿还额
                        var rqylje0 = float.Parse(rzrq0["RQYLJE"].ToString());///融券余量金额
                        var rqylje1 = float.Parse(rzrq1["RQYLJE"].ToString());///前2日融券余量金额
                        var rqyl0 = float.Parse(rzrq0["RQYL"].ToString());///融券余量
                        var rqyl1 = float.Parse(rzrq1["RQYL"].ToString());///前2日融券余量
                        var rqmcl0 = float.Parse(rzrq0["RQMCL"].ToString());///融券卖出量
                        var rqmcl1 = float.Parse(rzrq1["RQMCL"].ToString());///前2日融券卖出量
                        //var rqchl0 = float.Parse(rzrq0["RQCHL"].ToString());///融券偿还量
                        //var rqchl1 = float.Parse(rzrq1["RQCHL"].ToString());///前2日融券偿还量
                        var rqye0 = float.Parse(rzrq0["RQYE"].ToString());///融券余额
                        var rqye1 = float.Parse(rzrq1["RQYE"].ToString());///前2日融券余额

                        svItem.RZRQ.RQYE["总数"] .Add(string.Format("{0}{1}",stockCode,stockName));
                        if (rzye1 <= rzye0) ///融资余额
                        {
                            svItem.RZRQ.RQYE["增加"] .Add(string.Format("{0}{1}",stockCode,stockName));
                        }
                        else
                        {
                            svItem.RZRQ.RQYE["减少"] .Add(string.Format("{0}{1}",stockCode,stockName));
                        }

                        svItem.RZRQ.RZMRE["总数"] .Add(string.Format("{0}{1}",stockCode,stockName));
                        if (rzmre1 <= rzmre0) ///融资买入额
                        {
                            svItem.RZRQ.RZMRE["增加"] .Add(string.Format("{0}{1}",stockCode,stockName));
                        }
                        else
                        {
                            svItem.RZRQ.RZMRE["减少"] .Add(string.Format("{0}{1}",stockCode,stockName));
                        }

                        svItem.RZRQ.RQYLJE["总数"] .Add(string.Format("{0}{1}",stockCode,stockName));
                        if (rqylje1 <= rqylje0) ///融券余量金额
                        {
                            svItem.RZRQ.RQYLJE["增加"] .Add(string.Format("{0}{1}",stockCode,stockName));
                        }
                        else
                        {
                            svItem.RZRQ.RQYLJE["减少"] .Add(string.Format("{0}{1}",stockCode,stockName));
                        }

                        svItem.RZRQ.RQYL["总数"] .Add(string.Format("{0}{1}",stockCode,stockName));
                        if (rqyl1 <= rqyl0) ///融券余量
                        {
                            svItem.RZRQ.RQYL["增加"] .Add(string.Format("{0}{1}",stockCode,stockName));
                        }
                        else
                        {
                            svItem.RZRQ.RQYL["减少"] .Add(string.Format("{0}{1}",stockCode,stockName));
                        }

                        svItem.RZRQ.RQMCL["总数"] .Add(string.Format("{0}{1}",stockCode,stockName));
                        if (rqmcl1 <= rqmcl0) ///融券卖出量
                        {
                            svItem.RZRQ.RQMCL["增加"] .Add(string.Format("{0}{1}",stockCode,stockName));
                        }
                        else
                        {
                            svItem.RZRQ.RQMCL["减少"] .Add(string.Format("{0}{1}",stockCode,stockName));
                        }

                        svItem.RZRQ.RQYE["总数"] .Add(string.Format("{0}{1}",stockCode,stockName));
                        if (rqye1 <= rqye0) ///融券余额
                        {
                            svItem.RZRQ.RQYE["增加"] .Add(string.Format("{0}{1}",stockCode,stockName));
                        }
                        else
                        {
                            svItem.RZRQ.RQYE["减少"].Add(string.Format("{0}{1}", stockCode, stockName));
                        }
                    }
                }
                #endregion

                #endregion


            }

            Func<Dictionary<string, List<string>>, Dictionary<string, string>, string, int> CalRate = (Dictionary<string, List<string>> inData1, Dictionary<string, string> inData2, string msg) => {
                var res = inData2;
                var totalCount = inData1["总数"].Count;
                var src = (from item in inData1 orderby item.Value.Count descending select new { Key = item.Key, Count=item.Value.Count}).ToList();
                if (0 < totalCount)
                {
                    foreach (var item in src)
                    {
                        res.Add(item.Key + "占比", string.Format("共{0}计数，占比{1:P}", item.Count, float.Parse(item.Count.ToString()) / float.Parse(totalCount.ToString())));
                    }
                }
                return 0;
            };

            var svItem2 = new {
                股票占比 = new Dictionary<string, string>(),
                板块概念占比 = new Dictionary<string, string>(),
                之前日线占比 = new Dictionary<string, string>(),
                财务摘要占比 = new Dictionary<string, string>(),
                异动雷达占比 = new Dictionary<string, string>(),
                资金流向占比 = new Dictionary<string, string>(),
                龙虎榜净买入占比 = new Dictionary<string, string>(),
                融资余额占比 = new Dictionary<string, string>(),
                融资买入额占比 = new Dictionary<string, string>(),
                融券余量金额占比 = new Dictionary<string, string>(),
                融券余量占比 = new Dictionary<string, string>(),
                融券卖出量占比 = new Dictionary<string, string>(),
                融券余额占比 = new Dictionary<string, string>(),
                股票推荐 = new Dictionary<string, float> { },
                输出结果 = new Dictionary<string, float>()
            };

            CalRate(svItem.涉及股票, svItem2.股票占比, "0");
            CalRate(svItem.BKGN, svItem2.板块概念占比, "1");
            CalRate(svItem.PrevKLine, svItem2.之前日线占比, "2");
            CalRate(svItem.CWZY, svItem2.财务摘要占比, "3");
            CalRate(svItem.Radar, svItem2.异动雷达占比, "4");
            CalRate(svItem.ZJLX, svItem2.资金流向占比, "5");
            CalRate(svItem.LHB.JMR, svItem2.龙虎榜净买入占比, "6");
            CalRate(svItem.RZRQ.RZYE, svItem2.融资余额占比, "7");
            CalRate(svItem.RZRQ.RZMRE, svItem2.融资买入额占比, "8");
            CalRate(svItem.RZRQ.RQYLJE, svItem2.融券余量金额占比, "9");
            CalRate(svItem.RZRQ.RQYL, svItem2.融券余量占比, "10");
            CalRate(svItem.RZRQ.RQMCL, svItem2.融券卖出量占比, "11");
            CalRate(svItem.RZRQ.RQYE, svItem2.融券余额占比, "12");

            Func<Dictionary<string,List<string>>, double, List<string>, int> CalRecommendStock = (Dictionary<string, List<string>> inData,double rate,List<string> keys) => {
                foreach (var list in inData)
                {
                    var totalCount = inData["总数"].Count;
                    foreach (var item in list.Value)
                    {
                        if(( null == keys ||keys.Contains(list.Key)) &&!svItem2.股票推荐.ContainsKey(item))
                        {
                            svItem2.股票推荐[item] = 0.0f;
                        }
                        svItem2.股票推荐[item] += (float)rate*float.Parse(list.Value.Count.ToString()) / float.Parse(totalCount.ToString());
                    }
                }
                return 0;
            };


            CalRecommendStock(svItem.涉及股票,0.1, null);
            CalRecommendStock(svItem.BKGN, 0.05,null);
            CalRecommendStock(svItem.PrevKLine,0.2, new List<string> { "前几日比当前价高" , "下跌" });
            CalRecommendStock(svItem.CWZY,0.7, new List<string> { "每股净资产大于1", "每股收益小于1", "每股收益增加", "每股现金含量小于1", "每股净资产增加" });
            CalRecommendStock(svItem.Radar, 0.1, new List<string> { "大买单", "大单成交" });
            CalRecommendStock(svItem.ZJLX, 0.1, new List<string> { "中单占比负值", "流出", "大单占比负值" });
            //CalRecommendStock(svItem.LHB.JMR,0, new List<string> { });
            //CalRecommendStock(svItem.RZRQ.RZYE, 0, "");
            //CalRecommendStock(svItem.RZRQ.RZMRE, 0, "");
            //CalRecommendStock(svItem.RZRQ.RQYLJE, 0, "");
            CalRecommendStock(svItem.RZRQ.RQYL, 0.2, new List<string> { "增加" });
            CalRecommendStock(svItem.RZRQ.RQMCL, 0.2, new List<string> { "增加" });
            CalRecommendStock(svItem.RZRQ.RQYE, 0.2, new List<string> { "减少" });
            var outData =(from item in svItem2.股票推荐 orderby item.Value descending select item).ToList();
            //svItem2.股票推荐.Clear();
            foreach (var outItem in outData)
            {
                svItem2.输出结果.Add(outItem.Key, outItem.Value);
            }



            var dict = Convertor.FromObjectToDictionary3(svItem2);
            var json = Convertor.FromObjectToJson(svItem2);
            File.WriteAllText("E:\\股票结果.txt", json, Encoding.UTF8);
            Func<Dictionary<string, string>, string, string> OutPutHtml = (Dictionary<string, string> inData, string subTitle) => {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendFormat("<h2>{0}</h2>", subTitle);
                stringBuilder.AppendFormat("<ul>");
                foreach (var item in inData)
                {
                    stringBuilder.AppendFormat("<li>{0}&nbsp;&nbsp;&nbsp;&nbsp;{1}</li>", item.Key, item.Value);
                }
                stringBuilder.AppendFormat("</ul>");

                return stringBuilder.ToString();
            };
            var htmlBuilder = new StringBuilder();
            htmlBuilder.AppendFormat("<!DOCTYPE html><html><head><meta charset=\"utf-8\" /><title>汪德禄专用&nbsp;&nbsp;&nbsp;&nbsp;{0}特征分析</title></head><body style=\"font-size: 20px;\">", title);
            htmlBuilder.AppendFormat("<h1>汪德禄专用&nbsp;&nbsp;&nbsp;&nbsp;{0}特征分析&nbsp;&nbsp;</h1>",title);
            htmlBuilder.AppendFormat("{0}", OutPutHtml(svItem2.输出结果.ToDictionary(p=>p.Key,p=>p.Value.ToString()), "股票推荐"));
            htmlBuilder.AppendFormat("{0}", OutPutHtml(svItem2.之前日线占比, "之前日线占比"));
            htmlBuilder.AppendFormat("{0}", OutPutHtml(svItem2.财务摘要占比, "财务摘要占比"));
            htmlBuilder.AppendFormat("{0}", OutPutHtml(svItem2.异动雷达占比, "异动雷达占比"));
            htmlBuilder.AppendFormat("{0}", OutPutHtml(svItem2.资金流向占比, "资金流向占比"));
            htmlBuilder.AppendFormat("{0}", OutPutHtml(svItem2.龙虎榜净买入占比, "龙虎榜净买入占比"));
            htmlBuilder.AppendFormat("{0}", OutPutHtml(svItem2.融资余额占比, "融资余额占比"));
            htmlBuilder.AppendFormat("{0}", OutPutHtml(svItem2.融资买入额占比, "融资买入额占比"));
            htmlBuilder.AppendFormat("{0}", OutPutHtml(svItem2.融券余量金额占比, "融券余量金额占比"));
            htmlBuilder.AppendFormat("{0}", OutPutHtml(svItem2.融券余量占比, "融券余量占比"));
            htmlBuilder.AppendFormat("{0}", OutPutHtml(svItem2.融券卖出量占比, "融券卖出量占比"));
            htmlBuilder.AppendFormat("{0}", OutPutHtml(svItem2.融券余额占比, "融券余额占比"));
            htmlBuilder.AppendFormat("{0}", OutPutHtml(svItem2.股票占比, "股票占比"));
            htmlBuilder.AppendFormat("{0}", OutPutHtml(svItem2.板块概念占比, "板块概念占比"));

            htmlBuilder.AppendFormat("</body></html>");
            File.WriteAllText(string.Format("E://gupiao{0}.html",title), htmlBuilder.ToString(), Encoding.UTF8);
            mongo.Save3(dbName, collectionDataResult, svItem2);
        }
        #endregion

 

    }
}
