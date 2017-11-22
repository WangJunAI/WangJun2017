using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.BizCore;
using WangJun.DB;
using WangJun.Tools;

namespace WangJun.Stock
{
    public class StockTaskCreator
    {
        protected Dictionary<string, string> dict = new Dictionary<string, string>();
        protected string dbName = "StockTask";
        public static StockTaskCreator CreateInstance()
        {
            var inst = new StockTaskCreator();
            inst.PrepareData();
            return inst;
        }
        #region 初始化数据
        /// <summary>
        /// 初始化数据
        /// </summary>
        protected void PrepareData()
        {
            if (0 == this.dict.Count)
            {
                var db = DataStorage.GetInstance();
                var jsonFilter = string.Format("{{\"ContentType\":\"股票代码\"}}");
                var res = db.Find("StockTask", "BaseInfo", jsonFilter);
                foreach (var item in res)
                {
                    this.dict.Add(item["StockCode"].ToString(), item["StockName"].ToString());
                }
            }
        }
        #endregion

        #region 创建任务 - 更新股票代码
        /// <summary>
        /// 创建任务 - 更新股票代码
        /// </summary>
        public void CreateTaskUpdateStockCode()
        {
            var inst = new TaskItem();
            inst.ID = Guid.NewGuid();
            inst.Status = "待执行";
            inst.Description = "更新股票代码";
            inst.Type = "一次性任务";
            inst.CreateTime = DateTime.Now;
            inst.StartTime = DateTime.Now.AddDays(new Random().Next(0, 23));
            inst.ExpireTime = inst.StartTime.AddDays(2);

            inst.ExData = new Dictionary<string, object>();
            inst.ExData["MethodName"] = "UpdateAllStockCode";
            inst.ExData["TypeFullName"] = "WangJun.Stock.StockTaskExecutor";
            inst.ExData["DLLPath"] = @"E:\WangJun2017\WangJun.Stock\bin\Debug\WangJun.Stock.dll";
            inst.ExData["Param"] = new object[] { "THS" };
            inst.Save();

            Console.WriteLine("创建任务 - 更新股票代码");
        }
        #endregion



        #region 创建任务 - 下载指定股票的首页概览
        /// <summary>
        /// 创建任务 - 下载指定股票的首页概览
        /// </summary>
        public void CreateTaskSYGL()
        {
            foreach (var item in this.dict)
            {
                var inst = new TaskItem();
                inst.ID = Guid.NewGuid();
                inst.Status = "待执行";
                inst.Description = "下载指定股票的首页概览";
                inst.Type = "一次性任务";
                inst.CreateTime = DateTime.Now;
                inst.StartTime = DateTime.Now.AddDays(new Random().Next(0, 23));
                inst.ExpireTime = inst.StartTime.AddDays(2);
                inst.PauseSeconds = new Random().Next(1000, 3000);

                inst.ExData = new Dictionary<string, object>();
                inst.ExData["MethodName"] = "UpdatePage";
                inst.ExData["TypeFullName"] = "WangJun.Stock.StockTaskExecutor";
                inst.ExData["DLLPath"] = @"E:\WangJun2017\WangJun.Stock\bin\Debug\WangJun.Stock.dll";
                inst.ExData["Param"] = new object[] { item.Key, item.Value, "首页概览", string.Empty };
                inst.Save();
                Console.WriteLine(" 添加任务 {0} {1} {2}", "首页概览", item.Key, item.Value);
            }

        }
        #endregion

        #region 创建任务 - 下载指定股票的资金流向
        public void CreateTaskZJLX()
        {
            foreach (var item in this.dict)
            {
                var inst = new TaskItem();
                inst.ID = Guid.NewGuid();
                inst.Status = "待执行";
                inst.Description = "下载指定股票的资金流向";
                inst.Type = "一次性任务";
                inst.CreateTime = DateTime.Now;
                inst.StartTime = DateTime.Now.AddDays(new Random().Next(0, 23));
                inst.ExpireTime = inst.StartTime.AddDays(2);
                inst.PauseSeconds = new Random().Next(1000, 3000);

                inst.ExData = new Dictionary<string, object>();
                inst.ExData["MethodName"] = "UpdatePage";
                inst.ExData["TypeFullName"] = "WangJun.Stock.StockTaskExecutor";
                inst.ExData["DLLPath"] = @"E:\WangJun2017\WangJun.Stock\bin\Debug\WangJun.Stock.dll";
                inst.ExData["Param"] = new object[] { item.Key, item.Value, "资金流向", string.Empty };
                inst.Save();
                Console.WriteLine(" 添加任务 {0} {1} {2}", "资金流向", item.Key, item.Value);
            }
        }

        #endregion

        #region 创建任务 - 下载指定股票的个股龙虎榜
        /// <summary>
        /// 创建任务 - 下载指定股票的个股龙虎榜
        /// </summary>
        /// <param name="_id"></param>
        public void CreateTaskGGLHB()
        {
            foreach (var item in this.dict)
            {
                var inst = new TaskItem();
                inst.ID = Guid.NewGuid();
                inst.Status = "待执行";
                inst.Description = "下载指定股票的个股龙虎榜";
                inst.Type = "一次性任务";
                inst.CreateTime = DateTime.Now;
                inst.StartTime = DateTime.Now.AddDays(new Random().Next(0, 23));
                inst.ExpireTime = inst.StartTime.AddDays(2);
                inst.PauseSeconds = new Random().Next(1000, 3000);

                inst.ExData = new Dictionary<string, object>();
                inst.ExData["MethodName"] = "UpdatePage";
                inst.ExData["TypeFullName"] = "WangJun.Stock.StockTaskExecutor";
                inst.ExData["DLLPath"] = @"E:\WangJun2017\WangJun.Stock\bin\Debug\WangJun.Stock.dll";
                inst.ExData["Param"] = new object[] { item.Key, item.Value, "个股龙虎榜", string.Empty };
                inst.Save();
                Console.WriteLine(" 添加任务 {0} {1} {2}", "个股龙虎榜", item.Key, item.Value);
            }
        }
        #endregion

        #region 创建任务 - 下载指定股票的个股龙虎榜明细
        /// <summary>
        /// 创建任务 - 下载指定股票的个股龙虎榜
        /// </summary>
        /// <param name="_id"></param>
        public void CreateTaskGGLHBMX()
        {
            var db = DataStorage.GetInstance();
            var jsonFilter = string.Format("{{\"ContentType\":\"{0}\"}}", "个股龙虎榜明细");
            db.EventTraverse += (object sender, EventArgs e) =>
            {
                var ee = e as EventProcEventArgs;
                var listItem = ee.Default as Dictionary<string, object>;

                var inst = new TaskItem();
                inst.ID = Guid.NewGuid();
                inst.Status = "待执行";
                inst.Description = "下载指定股票的个股龙虎榜明细";
                inst.Type = "一次性任务";
                inst.CreateTime = DateTime.Now;
                inst.StartTime = DateTime.Now.AddDays(new Random().Next(0, 23));
                inst.ExpireTime = inst.StartTime.AddDays(2);
                inst.PauseSeconds = new Random().Next(1000, 3000);

                inst.ExData = new Dictionary<string, object>();
                inst.ExData["MethodName"] = "UpdatePage";
                inst.ExData["TypeFullName"] = "WangJun.Stock.StockTaskExecutor";
                inst.ExData["DLLPath"] = @"E:\WangJun2017\WangJun.Stock\bin\Debug\WangJun.Stock.dll";
                inst.ExData["Param"] = new object[] { listItem["StockCode"], listItem["StockName"], "个股龙虎榜明细", string.Empty };
                inst.Save();
                Console.WriteLine(" 添加任务 {0} {1} {2}", "个股龙虎榜明细", listItem["StockCode"], listItem["StockName"]);

            };
            db.Traverse("PageSource", "PageStock", jsonFilter);
        } 
         
        #endregion

        #region 创建SINA大单任务
        public void CreateTaskSINADaDan()
        {
            var inst = new TaskItem();
            inst.ID = Guid.NewGuid();
            inst.Status = "待执行";
            inst.Description = "下载SINA大单";
            inst.Type = "一次性任务";
            inst.CreateTime = DateTime.Now;
            inst.StartTime = DateTime.Now.AddDays(new Random().Next(0, 23));
            inst.ExpireTime = inst.StartTime.AddDays(2);

            inst.ExData = new Dictionary<string, object>();
            inst.ExData["MethodName"] = "GetDaDanPage";
            inst.ExData["TypeFullName"] = "WangJun.Stock.StockTaskExecutor";
            inst.ExData["DLLPath"] = @"E:\WangJun2017\WangJun.Stock\bin\Debug\WangJun.Stock.dll";
            inst.ExData["Param"] = new object[] { };
            inst.Save();
        }
        #endregion

        #region 创建任务 - 获取页面数据 - 首页概览,资金流向,龙虎榜,龙虎榜明细
        public void CreateTaskUpdatePageData()
        {
            ///获取页面数据,创建任务
            var db = DataStorage.GetInstance();
            db.EventTraverse += (object sender,EventArgs e) => {
                var ee = e as EventProcEventArgs;
                var listItem = ee.Default as Dictionary<string,object>;
                var inst = new TaskItem();
                inst.ID = Guid.NewGuid();
                inst.Status = "待执行";
                inst.Description = "获取指定股票的页面数据 - 首页概览,资金流向,龙虎榜,龙虎榜明细";
                inst.Type = "一次性任务";
                inst.CreateTime = DateTime.Now;
                inst.StartTime = DateTime.Now.AddDays(new Random().Next(0, 23));
                inst.ExpireTime = inst.StartTime.AddDays(2);
                inst.PauseSeconds = 0;// new Random().Next(1000, 3000);

                inst.ExData = new Dictionary<string, object>();
                inst.ExData["MethodName"] = "UpdateData";
                inst.ExData["TypeFullName"] = "WangJun.Stock.StockTaskExecutor";
                inst.ExData["DLLPath"] = @"E:\WangJun2017\WangJun.Stock\bin\Debug\WangJun.Stock.dll";
                inst.ExData["Param"] = new object[] { listItem["_id"].ToString() };
                inst.Save();
                Console.WriteLine(" 添加任务 {0} {1} {2}", listItem["ContentType"], listItem["StockCode"], listItem["StockName"]);
            };
            db.Traverse("PageSource", "PageStock", "{\"ContentType\":\"个股龙虎榜明细\"}");
        }
        #endregion

        #region 创建任务 - 将页面数据二维化
        /// <summary>
        /// 创建任务 - 将页面数据二维化
        /// </summary>
        public void CreateTaskDataTo2D()
        {
            var db = DataStorage.GetInstance();
            db.EventTraverse += (object sender, EventArgs e) =>
            {
                var ee = e as EventProcEventArgs;
                var listItem = ee.Default as Dictionary<string, object>;

                var inst = new TaskItem();
                inst.ID = Guid.NewGuid();
                inst.Status = "待执行";
                inst.Description = "将指定的数据2维化";
                inst.Type = "一次性任务";
                inst.CreateTime = DateTime.Now;
                inst.StartTime = DateTime.Now.AddDays(new Random().Next(0, 23));
                inst.ExpireTime = inst.StartTime.AddDays(2);
                inst.PauseSeconds = 0;// new Random().Next(1000, 3000);

                inst.ExData = new Dictionary<string, object>();
                inst.ExData["MethodName"] = "UpdateData2D";
                inst.ExData["TypeFullName"] = "WangJun.Stock.StockTaskExecutor";
                inst.ExData["DLLPath"] = @"E:\WangJun2017\WangJun.Stock\bin\Debug\WangJun.Stock.dll";
                inst.ExData["Param"] = new object[] { listItem["_id"].ToString() };
                inst.Save();
                Console.WriteLine(" 添加任务 {0} {1} {2}", listItem["ContentType"], listItem["StockCode"], listItem["StockName"]);

            };
            db.Traverse("DataSource", "DataOfPage", "{}");

        }
        #endregion




    }
}
