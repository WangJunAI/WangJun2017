using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using WangJun.BizCore;
using WangJun.Data;
using WangJun.DB;
using WangJun.NetLoader;
using WangJun.Stock;
using WangJun.Tools;

namespace WangJun.NodeRunner
{
    class Program
    { 
        static void Main(string[] args)
        {
            #region 任务创建测试
            //StockTaskCreator creator = StockTaskCreator.CreateInstance();
            //creator.CreateTaskUpdateStockCode();///更新股票代码

            //creator.CreateTaskDownloadPageSYGL();///下载首页概览页面
            //creator.CreateTaskDownloadPageZJLX();///下载资金流向页面
            //creator.CreateTaskDwonloadPageGGLHB();//
            //creator.CreateTaskDownloadPageGGLHBMX();
            //creator.CreateTaskSINALSJY();///
            //creator.CreateTaskUpdatePageData("SINA个股历史交易");///更新日线信息
            //creator.CreateTaskUpdatePageDataDaDan();//更新大单数据
            //creator.CreateTaskDaDanTo2D();
            //creator.CreateTaskSINALSJYTo2D();
            //creator.CreateTaskEveryDay();
            //creator.CreateTaskAnalyseDaDan();
            #endregion

            //StockTaskExecutor exe = new StockTaskExecutor();
            //exe.UpdateData2D("5a19a9df487bdc2bf0e4ea71", "DataSource", "DataOfDaDan");
            //exe.UpdatePage("600521", "华海药业", "个股龙虎榜","");
            //exe.GetDataFromPageDaDan("59e87b23487bdc330458d069");
            //exe.GetDataFromPageDaDan("59e875da487bdc330458cd9a");
            //exe.GetNewsListCJYW("2017/12/01");
            //exe.SyncStockNews();

            //var analyser = StockAnalyser.GetInstance();
            //analyser.AnalyseDaDan();

            //TaskRunner runner = new TaskRunner();
            //runner.Run();

            #region 分词环境
            //var res = FenCi.GetResult("今年7月26日，魅族在广东珠海歌剧院发布了自己的年度旗舰Pro7系列，与去年每月一场发布会相比，今年的魅族只推出了Pro7一款产品。早先魅族CMO杨柘也曾在微博表示，魅族品牌年内将不会推出新产品，作为一个互联网品牌，今年的魅族显得较为“低调”。");
            #endregion


            ///MongoDB转移
            //DataStorage.MoveCollection("140", "PageSource", "PageStock", "{}", "140", "PageSource", "PageStock1", false);
            //DataStorage.MoveDataFromMongoToSQLServer("170", "StockData2D", "DaDan", "{}", "140", "StockData2D", "DaDan");
            //DataStorage.MoveCollection("170", "StockTask", "BaseInfo", "{}", "170", "StockService", "BaseInfo", false);
            //DataStorage.MoveDataFromMongoToSQLServer("170", "StockService", "BaseInfo", "{}", "aifuwu", "StockService", "BaseInfo");
            //DataStorage.MoveCollection("105", "TianTian", "Order2", "{}", "170", "TianTian", "Order", false);
            //DataStorage.MoveCollection("105", "WYGeQu", "ZhuanJi6", "{}", "170", "WYGeQu", "ZhuanJi", false);
            //DataStorage.MoveCollection("105", "WYGeQu", "GeQu6", "{}", "170", "WYGeQu", "GeQu", false);
            //DataStorage.MoveCollection("105", "Job51", "All2", "{}", "170", "Job51", "All", false);
            //$(trArray[10]).find("td").last().text().trim().replace("元","").replace(/,/g,"")

            //Convertor.CalTradingDate(new DateTime(2017, 12, 15, 1, 23, 0), "15:00:00");

            #region 宿主进程
            var json =Convertor.FromJsonToDict2( File.ReadAllText("config.js"));
            var serviceName = json["ServiceName"].ToString();
            StockSynchronizer sync = StockSynchronizer.GetInstance();
            var analysor = StockAnalyser.GetInstance();
            //sync.SyncStockCode();
            if("Test" == serviceName)
            {
                var mongo = MongoDB.GetInst("mongodb");
                var filter = "";
                //var res = mongo.Find2("StockService", "SINADaDan2D","{\"TradingDate\":new Date(\"2017/12/18\")}", "{}");
                //mongo.Distinct("StockService", "SINADaDan2D", "StockCode");
                //analysor.AnalyseDaDan();
                //var db = DataStorage.GetInstance(DBType.MongoDB);
                //var list =  db.Find2(CONST.DB.DBName_StockService, CONST.DB.CollectionName_Exception, "{}");
                //foreach (var item in list)
                //{
                //    var resItem = NodeService.Get(CONST.NodeServiceUrl, "同花顺", "GetDataFromHtml", new { ContentType = "THS财经要闻新闻详细", Page = (item["Args"] as Dictionary<string, object>)["Page"] }) as Dictionary<string, object>;

                //}

                //var list = DataSourceSINA.CreateInstance().GetLSCJMX("600360", "2017-12-27");
                var list = WebDataSource.GetInstance().GetStockRadar();
            }
            else if ("SyncStockNews" == serviceName)
            {
                sync.SyncStockNews();
            }
            else if("SyncSINADaDan" == serviceName)
            {
                sync.SyncSINADaDan();
            }
            else if ("SyncKLineDay" == serviceName)
            {
                sync.SyncKLineDay();
            }
            else if("SyncGSJJ" == serviceName)
            {
                sync.SyncGSJJ();
            }
            else if("SyncBKGN" == serviceName)
            {
                sync.SyncBKGN();
            }
            else if("SyncZJLX" == serviceName)
            {
                sync.SyncZJLX();
            }
            else if("SyncStockCode" == serviceName)
            {
                sync.SyncStockCode();
            }
            else if ("SyncCWZY" == serviceName)
            {
                sync.SyncCWZY();
            }
            else if ("SyncStockRardar" == serviceName)
            {
                sync.SyncStockRardar();
            }

            #endregion

            Console.WriteLine("全部结束");
            Console.ReadKey();
        }

 
 

         
    }
}
