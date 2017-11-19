using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.BizCore;
using WangJun.DB;

namespace WangJun.Stock
{
    public class StockTask
    {
        protected Dictionary<string, string> dict = new Dictionary<string, string>();
        protected string dbName = "StockTask";
        public static StockTask CreateInstance()
        {
            var inst =  new StockTask();
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

        #region 创建更新股票代码任务
        public void CreateTaskUpdateStockCode()
        {

        }
        #endregion

        #region 创建任务 - 下载指定股票的首页概览
        public void CreateTask()
        {
            this.dict.Clear();
            this.dict.Add("002230", "科大讯飞");
            foreach (var item in this.dict)
            {
                {
                    var inst = new TaskItem();
                    inst.ID = Guid.NewGuid();
                    inst.Status = "待执行";
                    inst.Description = "下载指定股票的首页概览";
                    inst.Type = "一次性任务";
                    inst.CreateTime = DateTime.Now;
                    inst.StartTime = DateTime.Now.AddDays(new Random().Next(0, 23));
                    inst.ExpireTime = inst.StartTime.AddDays(2);

                    inst.ExData = new Dictionary<string, object>();
                    inst.ExData["MethodName"] = "UpdatePage";
                    inst.ExData["TypeFullName"] = "WangJun.Stock.StockTaskRunner";
                    inst.ExData["DLLPath"] = @"E:\WangJun2017\WangJun.Stock\bin\Debug\WangJun.Stock.dll";
                    inst.ExData["Param"] = new object[] { item.Key, item.Value, "首页概览", string.Empty };
                    //inst.Save();
                }

                {
                    var inst = new TaskItem();
                    inst.ID = Guid.NewGuid();
                    inst.Status = "待执行";
                    inst.Description = "下载指定股票的资金流向";
                    inst.Type = "一次性任务";
                    inst.CreateTime = DateTime.Now;
                    inst.StartTime = DateTime.Now.AddDays(new Random().Next(0, 23));
                    inst.ExpireTime = inst.StartTime.AddDays(2);

                    inst.ExData = new Dictionary<string, object>();
                    inst.ExData["MethodName"] = "UpdatePage";
                    inst.ExData["TypeFullName"] = "WangJun.Stock.StockTaskRunner";
                    inst.ExData["DLLPath"] = @"E:\WangJun2017\WangJun.Stock\bin\Debug\WangJun.Stock.dll";
                    inst.ExData["Param"] = new object[] { item.Key, item.Value, "资金流向", string.Empty };
                    //inst.Save();
                }

                {
                    var inst = new TaskItem();
                    inst.ID = Guid.NewGuid();
                    inst.Status = "待执行";
                    inst.Description = "下载指定股票的个股龙虎榜及明细";
                    inst.Type = "一次性任务";
                    inst.CreateTime = DateTime.Now;
                    inst.StartTime = DateTime.Now.AddDays(new Random().Next(0, 23));
                    inst.ExpireTime = inst.StartTime.AddDays(2);

                    inst.ExData = new Dictionary<string, object>();
                    inst.ExData["MethodName"] = "UpdatePage";
                    inst.ExData["TypeFullName"] = "WangJun.Stock.StockTaskRunner";
                    inst.ExData["DLLPath"] = @"E:\WangJun2017\WangJun.Stock\bin\Debug\WangJun.Stock.dll";
                    inst.ExData["Param"] = new object[] { item.Key, item.Value, "个股龙虎榜", string.Empty };
                    inst.Save();
                }

                {

                }
            }

        }
        #endregion
          
    }
}
