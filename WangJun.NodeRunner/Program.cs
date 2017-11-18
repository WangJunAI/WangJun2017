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
using WangJun.Tools;

namespace WangJun.NodeRunner
{
    class Program
    { 
        static void Main(string[] args)
        {
   

            TaskManager taskMgr = new TaskManager();
            taskMgr.Run();
            //taskMgr.CreateTaskTemplate();

            Console.WriteLine("全部结束");
            Console.ReadKey();
        }

 
 

         
    }
}
