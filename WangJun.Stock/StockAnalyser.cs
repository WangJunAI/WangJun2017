using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.DB;
using WangJun.Debug;
using WangJun.Tools;

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
                BKGN = new Dictionary<string, int>(),
                PrevKLine = new Dictionary<string, int> { { "上涨", 0 }, { "下跌", 0 }, { "4%以下", 0 }, { "4%以上", 0 }, { "-4%及内", 0 }, { "小于-4%", 0 } },
                CWZY = new Dictionary<string, int> { { "每股净资产小于1", 0 }, { "每股净资产大于1", 0 }, { "每股收益小于1", 0 }, { "每股收益大于1", 0 }, { "每股现金含量小于1", 0 }, { "每股现金含量大于1", 0 }, { "每股资本公积金小于1", 0 }, { "每股资本公积金大于1", 0 } },
                Radar = new Dictionary<string, int> { },
                DaDan = new Dictionary<string, int> { },
                ZJLX = new Dictionary<string, int> { { "流入", 0 }, { "流出", 0 }, { "大单占比", 0 }, { "中单占比", 0 }, { "小单占比", 0 } },
                LHB =new {SBLX=new Dictionary<string, int> { },JMR=new Dictionary<string, int> { { "正值", 0 },{"负值" ,0} } }
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
                    var prevKLine = prevKLineList[k];
                    var increase = float.Parse(prevKLine["Increase"].ToString());///涨幅
                    var amplitude = float.Parse(prevKLine["Amplitude"].ToString());///振幅

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

                    if (1 <= mgsy0)
                    {
                        svItem.CWZY["每股收益大于1"] += 1;
                    }
                    else if (mgsy0 < 1)
                    {
                        svItem.CWZY["每股收益小于1"] += 1;
                    }

                    if (1 <= mgxjhl0)
                    {
                        svItem.CWZY["每股现金含量大于1"] += 1;
                    }
                    else if (mgxjhl0 < 1)
                    {
                        svItem.CWZY["每股现金含量小于1"] += 1;
                    }

                    if (1 <= mgxjhl0)
                    {
                        svItem.CWZY["每股资本公积金大于1"] += 1;
                    }
                    else if (mgxjhl0 < 1)
                    {
                        svItem.CWZY["每股资本公积金小于1"] += 1;
                    }



                }
                #endregion

                #region 股市雷达 哪种类型的多
                if (null != radarList)
                {
                    foreach (var radar in radarList)
                    {
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
                        var zjlx = zjlxList[k];
                        var netInflow =float.Parse( zjlx["NetInflow"].ToString());///资金净流入
                        if (0 < netInflow)
                        {
                            svItem.ZJLX["流入"] += 1;
                        }
                        else if (netInflow < 0)
                        {
                            svItem.ZJLX["流出"] += 1;
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
                            svItem.LHB.SBLX[sblx] += 1;

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
                        var rzmre0 = float.Parse(rzrq0["RZMRE"].ToString());///融资买入额
                    }
                }
                #endregion

                #endregion


            }

            //mongo.Save3(dbName, collectionDataResult, svItem);
        }
        #endregion


    }
}
