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

        #region 业绩变好公司
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


    }
}
