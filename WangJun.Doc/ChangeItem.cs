using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.DB;

namespace WangJun.Doc
{
    /// <summary>
    /// 修改变更记录
    /// </summary>
    public class ChangeItem
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string Url { get; set; }

        public string Form { get; set; }

        public string Source { get; set; }

        public string HttpMethod { get; set; }
 
        public void Save()
        {
            var task = new TaskFactory().StartNew(() => {
                try
                {
                    var dbName = CONST.DB.DBName_DocService;
                    var collectionName = CONST.DB.CollectionName_LogItem;
                    var db = DataStorage.GetInstance(DBType.MongoDB);
                    db.Save3(dbName, collectionName, this);
                }
                catch(Exception e)
                {

                }
            });

        }
    }
}
