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
            StockTaskCreator creator =   StockTaskCreator.CreateInstance();
            //creator.CreateTaskZJLX();

            StockTaskExecutor exe = new StockTaskExecutor();
            exe.UpdateData("5a12bde7487bdc4c40e57656");

            Console.WriteLine("全部结束");
            Console.ReadKey();
        }

 
 

         
    }
}
