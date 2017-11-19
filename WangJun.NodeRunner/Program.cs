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
            //taskMgr.Run();
            //taskMgr.CreateTaskTemplate();
            DataSourceSINA sina = DataSourceSINA.CreateInstance();
            DataSourceTHS ths = DataSourceTHS.CreateInstance();
            
            var html = ths.GetGGLHBMX("002230","2013-06-26","3");

            Console.WriteLine("全部结束");
            Console.ReadKey();
        }

 
 

         
    }
}
