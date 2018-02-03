using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using WangJun.BizCore;
using WangJun.Data;
using WangJun.DB;
using WangJun.Doc;
using WangJun.HumanResource;
using WangJun.NetLoader;
using WangJun.OA;
using WangJun.Stock;
using WangJun.Tools;

namespace WangJun.NodeRunner
{
    class Program
    { 
        static void Main(string[] args)
        {
  
            #region 宿主进程
            var json =Convertor.FromJsonToDict2( File.ReadAllText("config.js"));
            var serviceName = json["ServiceName"].ToString();
            StockSynchronizer sync = StockSynchronizer.GetInstance();
            var analysor = StockAnalyser.GetInstance();
 
            if("Test" == serviceName)
            {
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
            else if ("SyncRZRQ" == serviceName)
            {
                sync.SyncRZRQ();
            }
            else if ("AnalyseRisingStock" == serviceName)
            {
                analysor.AnalyseRisingStock(startTime: DateTime.Now.AddDays(-10), endTime: DateTime.Now.AddDays(-5));
            }
            else if("DocRunner" == serviceName)
            {
                //WeChatService.GetAllOrg();
                //new DocRunner().DataAnalyse();
                var res = new DataSourceBaidu().GetPic("骆驼管家告诉你投资P2P，分散也要懂策略！");
            }
            else if ("SyncExcel" == serviceName)
            {
                //DataSourceSINA.GetInstance().DownloadExcel();
                //new DocRunner().DataAnalyse();
                //sync.ProcHistory();
                //ExcelService.GetInstance().ConvertToJson(@"F:\【工程】2015年年终奖数据.xlsx", "Test", @"F:\Test");
            }
            #endregion

            Console.WriteLine("全部结束");
            Console.ReadKey();
        }

 
 

         
    }
}
