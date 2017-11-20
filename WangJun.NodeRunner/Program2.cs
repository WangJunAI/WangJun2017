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
            creator.CreateTaskSYGL();

            Console.WriteLine("全部结束");
            Console.ReadKey();
        }

 
 

         
    }
}
