using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.DB;

namespace WangJun.Doc
{
    public class ClientBehaviorItem
    {
        public string DbName { get; set; }

        public string CollectionName { get; set; }

        public string TargetID { get; set; }

        /// <summary>
        /// 阅读,点赞,评论,界面操作
        /// </summary>
        public string BehaviorType { get; set; }

        public string UserID { get; set; }

        public string UserName { get; set; }

        public DateTime CreateTime { get; set; }

        public string Status { get; set; }

        public void Save()
        {
            var task = new TaskFactory().StartNew(() => {
                try
                {
 
                    var dbName = CONST.DB.DBName_DocService;
                    var collectionName = CONST.DB.CollectionName_ClientBehaviorItem;
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
