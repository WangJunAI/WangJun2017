using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using WangJun.Data;
using WangJun.DB;
using WangJun.NetLoader;
using WangJun.Tools;

namespace WangJun.NodeRunner
{
    class Program
    {
        protected static DateTime StartTime = DateTime.Now; ///开始运行时间
        protected static Stopwatch stopwatch = new Stopwatch();

         protected static WebLoader loader = WebLoader.GetInstance();
        static void Main(string[] args)
        {
            SetConsoleInfo();

            #region SQLServer 注册
            SQLServer.Register("140", @"Data Source=192.168.0.140\sql2016;Initial Catalog=WJBigData;Persist Security Info=True;User ID=sa;Password=111qqq!!!");
            var mssql = SQLServer.GetInstance("140");
            #endregion

            #region MySQL注册
            MySQL.Register("140", @"server=192.168.0.140;user=root;database=WJBigData;port=3306;password=111qqq!!!");
            var mysql = MySQL.GetInstance("140");
            #endregion

            #region   MongoDB注册
            MongoDB mongo = MongoDB.GetInst("mongodb://192.168.0.140:27017");
            #endregion

            #region 同花顺数据测试

            var inst = THS.GetInst();
            //inst.GetStockPage();
            //inst.UpdateToday();
            //inst.GetTodayLHB();
            #endregion

            #region 数据库测试

             var res = mongo.GetCollectionStatistic("ths");
            #endregion




            Console.WriteLine("全部结束");
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
