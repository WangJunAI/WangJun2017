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
   

            TaskManager taskMgr = new TaskManager();
            var task = StockTask.CreateInstance();
            task.CreateTask();
            taskMgr.Run();

            Console.WriteLine("全部结束");
            Console.ReadKey();
        }

 
 

         
    }
}
