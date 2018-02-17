using System.Collections.Generic;
using System.Linq;
using WangJun.DB;

namespace WangJun.Stock
{
    /// <summary>
    /// 任务状态管理器
    /// </summary>
    public class TaskStatusManager
    {
        public static Dictionary<string,object> Get(string id)
        {
            var dbName = CONST.DB.DBName_StockService;
            var collectionName = CONST.DB.CollectionName_TaskStatus;
            var mongo = DataStorage.GetInstance(DBType.MongoDB);
            var filter = "{\"ID\":\"" + id + "\"}";
            var list = mongo.Find(dbName, collectionName, filter, 0, 1);
            return (0 < list.Count) ? list.First() : new Dictionary<string, object>{ { "ID",id} , { "Status", null } };
        }

        public static void Set(string id, object data)
        {
            var dbName = CONST.DB.DBName_StockService;
            var collectionName = CONST.DB.CollectionName_TaskStatus;
            var mongo = DataStorage.GetInstance(DBType.MongoDB);
            var filter = "{\"ID\":\"" + id + "\"}";
            mongo.Save3(dbName, collectionName, data, filter);
        }
    }
}
