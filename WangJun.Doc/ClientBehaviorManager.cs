using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.Doc
{
    public static class ClientBehaviorManager
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
    }
}
