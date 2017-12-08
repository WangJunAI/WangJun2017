using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WangJun.Data;
using WangJun.DB;

namespace WangJun.BizCore
{
    /// <summary>
    /// 任务运行器
    /// </summary>
    public class TaskRunner
    {
         
        public List<TaskItem> GetNextTask(int count=1)
        {
            List<TaskItem> taskList = new List<TaskItem>();

            var db = DataStorage.GetInstance("170");
            //var filter = "{\"Status\":\"待执行\",\"StartTime\":{\"$gt\":\"new Date('"+DateTime.Now.Date+"')\" }}";
            var filter = "{\"Status\":\"待执行\"}";
            var res = db.Find("StockTask", "Task", filter, 0, count);

            taskList = Convertor.FromDictionaryToObject<TaskItem>(res);
 
            return taskList;
        }

        /// <summary>
        /// 运行任务
        /// </summary>
        public void Run()
        {
            while (true)
            {
                try
                {
                    var taskList = this.GetNextTask();

                    if (0 == taskList.Count)
                    {
                        Console.WriteLine("暂无新任务 {0}", DateTime.Now);
                        Thread.Sleep(10*1000);
                    }

                    foreach (var task in taskList)
                    {
                        this.ExecuteTask(task);
                        task.Remove();
                    }
                }
                catch (Exception e) {
                    Console.WriteLine("Task Runner 异常 {0} {1} ",e.Message,DateTime.Now);
                    Console.WriteLine("Task Runner 异常 {0} {1} ", e.InnerException.Message, DateTime.Now);
                    Console.WriteLine("Task Runner 异常 {0} {1} ", e.StackTrace, DateTime.Now);
                }
            }

        }

        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="task"></param>
        public void ExecuteTask(TaskItem task)
        {
            var startTime = DateTime.Now;
            Console.WriteLine("开始执行 {0} {1}",task.Description,startTime);
            var methodName = task.ExData["MethodName"].ToString();
            var typeFullName = task.ExData["TypeFullName"].ToString();
            var dllPath = task.ExData["DLLPath"].ToString();
            var param = task.ExData["Param"] as object[];
            var pauseSeconds = task.PauseSeconds;
            Assembly ass = Assembly.LoadFrom(dllPath);
            var obj = ass.CreateInstance(typeFullName);

            //obj.GetType().InvokeMember(methodName,BindingFlags.InvokeMethod,null,obj,null);
            var method = obj.GetType().GetMethod(methodName);
            method.Invoke(obj, param);
            ass = null;
            ///执行完毕,删除任务,并生成新任务

            Console.WriteLine("完成任务 {0} {1}", task.Description, DateTime.Now - startTime);
            if (0 < pauseSeconds)
            {
                Thread.Sleep(pauseSeconds);
            }
        }
         
    }
}
