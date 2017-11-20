using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.DB;

namespace WangJun.BizCore
{

    public class TaskItem
    { 
        public Guid ID { get; set; }
        public string Sender { get; set; }
        public string Recver { get; set; }
        public object SimpleData { get; set; }

        public string SimpleMessage { get; set; }

        public string Description { get; set; }

        public Dictionary<string,object> ExData { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime ExpireTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public string Status { get; set; } ///草稿,待执行,执行中,已完成,已作废,待删除,暂停,出错

        public string Type { get; set; } ///定时任务 , 一次性任务

        public List<Guid> NextTasks { get; set; }

        public int Level { get; set; }

        public int PauseSeconds { get; set; }

        /// <summary>
        /// 开启新线程数
        /// </summary>
        public int NewThreadCount { get; set; }

        public void Save()
        {
            var db = DataStorage.GetInstance();
            this.UpdateTime = DateTime.Now;
            db.Save(this, "Task", "StockTask", key: "ID");
        }

        public void Remove()
        {
            var db = DataStorage.GetInstance();
            string jsonFilter = string.Format("{{\"ID\":\"{0}\"}}", this.ID);
            db.Remove("StockTask", "Task", jsonFilter);
        }
 
    }
}
