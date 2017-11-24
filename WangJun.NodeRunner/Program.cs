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
            StockTaskCreator creator = StockTaskCreator.CreateInstance();
            //creator.CreateTaskUpdateStockCode();///更新股票代码

            //creator.CreateTaskDownloadPageSYGL();///下载首页概览页面
            //creator.CreateTaskDownloadPageZJLX();///下载资金流向页面
            //creator.CreateTaskDwonloadPageGGLHB();//
            //creator.CreateTaskDownloadPageGGLHBMX();
            creator.CreateTaskSINALSJY();
            #endregion

            StockTaskExecutor exe = new StockTaskExecutor();
            //exe.UpdateData2D("5a15b4f0487bdc5230dc0b5f");'
            //exe.UpdatePage("600521", "华海药业", "个股龙虎榜","");

            TaskRunner runner = new TaskRunner();
            runner.Run();


            ///MongoDB转移
            //DataStorage.MoveCollection("140", "DataSource", "DataOfPage", "{}", "140", "TestMove", "table1",true);


            Console.WriteLine("全部结束");
            Console.ReadKey();
        }

 
 

         
    }
}
