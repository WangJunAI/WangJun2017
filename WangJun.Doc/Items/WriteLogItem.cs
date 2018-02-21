using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.DB;

namespace WangJun.Doc
{
    public class LogItem
    {
        public string ClassName { get; set; }

        public string MethodName { get; set; }

        public string HttpMethod { get; set; }

        public bool Success { get; set; }

        public Dictionary<string,string> FormalParameter { get; set; }

        public object ActualParameter { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public TimeSpan TimeCost { get; set; }

        public bool HasProc { get; set; }

        public DateTime ProcStartTime { get; set; }

        public DateTime ProcEndTime { get; set; }

        public TimeSpan ProcTimeCost { get; set; }


        public bool IsWrite { get; set; }

        public void Save() {
            var task = new TaskFactory().StartNew(() => {
                try
                {
                    this.IsWrite = true;
                    if(this.MethodName== "Find"|| this.MethodName == "Count" || this.MethodName == "Load" || this.MethodName == "Get")
                    {
                        this.IsWrite = false;
                    }

                    var dbName = CONST.DB.DBName_DocService;
                    var collectionName = CONST.DB.CollectionName_InvokeItem +( (this.IsWrite) ? "W" : "R");
                    var db = DataStorage.GetInstance(DBType.MongoDB);
                    db.Save3(dbName, collectionName, this);
                }
                catch (Exception e)
                {

                }
            });
        }
    }
}
