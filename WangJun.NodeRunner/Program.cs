using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WangJun.BIZ;
using WangJun.Data;
using WangJun.DB;
using WangJun.NetLoader;

namespace WangJun.NodeRunner
{
    class Program
    {
        protected static DateTime StartTime = DateTime.Now; ///开始运行时间
        protected static MongoDB  inst1 = MongoDB.GetInst("mongodb://192.168.0.140:27017");
        static void Main(string[] args)
        {
            SetConsoleInfo();




            //return;
            //var inst = LocalDataOperator.GetInst();
            //var list = inst.TraverseFiles(@"E:\下载");
            //foreach (var item in list)
            //{
            //    Console.WriteLine(item.Path);
            //}

            //THS ths = new THS();
            //ths.DownloadAllStockCode();

            //var inst = LocalDataOperator.GetInst();
            //inst.EventOutput += Inst_EventOutput;
            //inst.StartTraverse();


            //var inst = MongoDB.GetInst("mongodb://192.168.0.140:27017");
            //var res = inst.Find("f1", "f2", "{}");

            var loader = WebLoader.GetInstance();
            loader.Run();


            Console.ReadKey();
        }

        private static void Inst_EventOutput(object sender, EventArgs e)
        {
            var data = ((TraverseEventArg)e).Data;
            LocalDataOperator local = sender as LocalDataOperator;
            inst1.Save("f1", "gss2t", data);
            Console.WriteLine("文件夹队列长度:{0}\t文件队列长度:{1}\t{2}", local.FolderQueue.Count, local.FileCount, data.Path);

        }

        #region 设置程序的基本信息
        static void SetConsoleInfo()
        {
            Console.Title = string.Format("本地文件遍历器  开始时间:{0}",Program.StartTime);
        }
        #endregion
    }
}
