using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using WangJun.BizCore;
using WangJun.Stock;

namespace WangJun.NodeRunner
{
    class Program
    { 
        static void Main(string[] args)
        {
            StockTaskCreator creator = StockTaskCreator.CreateInstance();
            //creator.CreateTaskZJLX();
            //creator.CreateTaskSYGL();
            //creator.CreateTaskUpdatePageData();
            //creator.CreateTaskDataTo2D();
            //creator.CreateTaskGGLHB();
            //creator.CreateTaskGGLHBMX();

            StockTaskExecutor exe = new StockTaskExecutor();
            //exe.UpdateData2D("5a15b4f0487bdc5230dc0b5f");


            TaskRunner runner = new TaskRunner();
            runner.Run();

            Console.WriteLine("全部结束");
            Console.ReadKey();
        }

 
 

         
    }
}
