using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WangJun.Data;
using WangJun.DB;

namespace WangJun.NodeRunner
{
    class Program
    {
        protected static DateTime StartTime = DateTime.Now; ///开始运行时间
        static void Main(string[] args)
        {
            SetConsoleInfo();


            var inst1 = MongoDB.GetInst("mongodb://192.168.0.140:27017");
            inst1.Save("x1", "x2", DateTime.Now);
            return;
            var inst = LocalDataOperator.GetInst();
            var list = inst.TraverseFiles(@"E:\下载");
            foreach (var item in list)
            {
                Console.WriteLine(item.Path);
            }
            Console.ReadKey();
        }

        #region 设置程序的基本信息
        static void SetConsoleInfo()
        {
            Console.Title = string.Format("本地文件遍历器  开始时间:{0}",Program.StartTime);
        }
        #endregion
    }
}
