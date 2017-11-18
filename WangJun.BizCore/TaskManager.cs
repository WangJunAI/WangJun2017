using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WangJun.Data;
using WangJun.DB;

namespace WangJun.BizCore
{
    /// <summary>
    /// 系统任务 系统维护 定时下载 定时维护数据 等
    /// </summary>
    public class TaskManager
    {
        
        /// <summary>
        /// 创建一个新任务
        /// </summary>
        /// <param name="task">创建一个新任务</param>
        /// <returns></returns>
        public int CreateTask(TaskItem task)
        {
            if(null !=task)
            {
                //mongo.Save(TaskManager.dbName, TaskManager.collectionName, task);
            }
            return 0;
        }

        public List<TaskItem> GetTask(string jsonFilter)
        {
            List<TaskItem> tasklist = new List<TaskItem>();
            //var res = mongo.Find(TaskManager.dbName, TaskManager.collectionName, "{}",0,100);

            return tasklist;
        }

        public int CompleteTask(string _id)
        {
            var filter = string.Format("{{\"_id\":\"{0}\"}}", _id);
            //var res = mongo.Find(TaskManager.dbName, TaskManager.collectionName, filter, 0, 100);
            //if(1 == res.Count())
            //{
            //    var item = res.First();
            //    item["Status"] = "已完成";
            //    mongo.Save(TaskManager.dbName, TaskManager.collectionName, item);
            //}
            return 0;
        }

        public int UpdateTask(TaskItem task)
        {
            return 0;
        }

        public int RemoveTask(string _id)
        {
            var filter = string.Format("{{\"_id\":\"{0}\"}}", _id);
            ////mongo.Delete(TaskManager.dbName, TaskManager.collectionName, filter);

            return 0;
        }


        public List<TaskItem> GetTaskList()
        {
            List<TaskItem> taskList = new List<TaskItem>();

            var db = DataStorage.GetInstance();
            var res = db.Find("StockTask", "Task", "{\"Status\":\"待执行\"}");

            taskList = Convertor.FromDictionaryToObject<TaskItem>(res);
 


            return taskList;
        }

        public void Run()
        {
            var taskList = this.GetTaskList();
            foreach (var task in taskList)
            {
                this.ExecuteTask(task);
            }
        }

        public void ExecuteTask(TaskItem task)
        { 
            var methodName = task.ExData["MethodName"].ToString();
            var typeFullName = task.ExData["TypeFullName"].ToString();
            var dllPath = task.ExData["DLLPath"].ToString();
            var param = task.ExData["Param"] as object[];

            Assembly ass = Assembly.LoadFrom(dllPath);
            var obj = ass.CreateInstance(typeFullName);

            //obj.GetType().InvokeMember(methodName,BindingFlags.InvokeMethod,null,obj,null);
            var method = obj.GetType().GetMethod(methodName);
            method.Invoke(obj, param);

            ///执行完毕,删除任务,并生成新任务
            task.Remove();
            Console.WriteLine("完成一个任务 {0} {1}",typeFullName,methodName);
        }


        public void  CreateTaskTemplate()
        {
            #region 创建
            var inst = new TaskItem();
            inst.ID = Guid.NewGuid();
            inst.Status = "待执行";
            inst.Description = "下载指定股票的首页概览";
            inst.Type = "一次性任务";
            inst.CreateTime = DateTime.Now;
            inst.StartTime = DateTime.Now.AddDays(new Random().Next(0, 23));
            inst.ExpireTime = inst.StartTime.AddDays(2);

            inst.ExData = new Dictionary<string, object>();
            inst.ExData["MethodName"] = "UpdateSYGL";
            inst.ExData["TypeFullName"] = "WangJun.Stock.StockTaskRunner";
            inst.ExData["DLLPath"] = @"E:\WangJun2017\WangJun.Stock\bin\Debug\WangJun.Stock.dll";
            inst.ExData["Param"] = new object[] { "002592", "八菱科技" };
            inst.Save();
            #endregion
        }
    }
}
