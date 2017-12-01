using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using WangJun.BizCore;
using WangJun.DB;
using WangJun.Stock;

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

            #endregion

            StockTaskExecutor exe = new StockTaskExecutor();
            //exe.UpdateData2D("5a19a9df487bdc2bf0e4ea71", "DataSource", "DataOfDaDan");
            //exe.UpdatePage("600521", "华海药业", "个股龙虎榜","");
            //exe.GetDataFromPageDaDan("59e87b23487bdc330458d069");
            //exe.GetDataFromPageDaDan("59e875da487bdc330458cd9a");
            exe.GetNewsListCJYW("2017/12/01");
            //TaskRunner runner = new TaskRunner();
            //runner.Run();


            ///MongoDB转移
            //DataStorage.MoveCollection("140", "PageSource", "PageStock", "{}", "140", "PageSource", "PageStock1", false);
            //DataStorage.MoveDataFromMongoToSQLServer("170", "StockData2D", "DaDan", "{}", "140", "StockData2D", "DaDan");

            Console.WriteLine("全部结束");
            Console.ReadKey();
        }

 
 

         
    }
}
