using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.DB;

namespace WangJun.Doc
{
    public  class ClientBehaviorManager
    {
        public static void Add(string dbName,string collectionName,string targetId,string behaviorType,string userID,string userName)
        {
            var inst = new ClientBehaviorItem();
            inst.DbName = dbName;
            inst.CollectionName = collectionName;
            inst.TargetID = targetId;
            inst.BehaviorType = behaviorType;
            inst.UserID = userID;
            inst.UserName = userName;
            inst.CreateTime = DateTime.Now;
            inst.Save();
        }

        public object Aggregate(string match, string group)
        {
            var db = DataStorage.GetInstance(DBType.MongoDB);
            return db.Aggregate(CONST.DB.DBName_DocService, CONST.DB.CollectionName_ClientBehaviorItem, match, group);
        }
    }
}
