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
    /// 修改记录
    /// </summary>
    public class ModifyLogItem
    {
        public ObjectId _id { get; set; }
        public DateTime CreateTime{get;set;}

        public ObjectId TargetID { get; set; }

        public string DatabaseName { get; set; }

        public string CollectionName { get; set; }

        public string Operate { get; set; }

        public string Description { get; set; }

        public static void  LogAsNew(string targetID,string dbName,string collectionName)
        {
            var inst = new ModifyLogItem();
            inst.CreateTime = DateTime.Now;
            inst.TargetID = ObjectId.Parse(targetID);
            inst.DatabaseName = dbName;
            inst.CollectionName = collectionName;
            inst.Operate = "新增";
            inst.Save();
        }

        public static void LogAsModify(string targetID, string dbName, string collectionName)
        {
            var inst = new ModifyLogItem();
            inst.CreateTime = DateTime.Now;
            inst.TargetID = ObjectId.Parse(targetID);
            inst.DatabaseName = dbName;
            inst.CollectionName = collectionName;
            inst.Operate = "修改";
            inst.Save();
        }

        public static void LogAsRemove(string targetID, string dbName, string collectionName)
        {
            var inst = new ModifyLogItem();
            inst.CreateTime = DateTime.Now;
            inst.TargetID = ObjectId.Parse(targetID);
            inst.DatabaseName = dbName;
            inst.CollectionName = collectionName;
            inst.Operate = "删除";
            inst.Save();
        }

        public void Remove()
        {
            var res = new object();
            var dbName = CONST.DB.DBName_DocService;
            var collectionName = CONST.DB.CollectionName_ModifyLogItem;
            var query = "{'_id':ObjectId('"+this._id.ToString()+"')}";
            if (!string.IsNullOrWhiteSpace(query))
            {
                var mongo = DataStorage.GetInstance(DBType.MongoDB);
                mongo.Remove(dbName, collectionName, query);

            }
            
        }

        public static void Remove(string id)
        {
            var res = new object();
            var dbName = CONST.DB.DBName_DocService;
            var collectionName = CONST.DB.CollectionName_ModifyLogItem;
            var query = "{'_id':ObjectId('" + id + "')}";
            if (!string.IsNullOrWhiteSpace(query))
            {
                var mongo = DataStorage.GetInstance(DBType.MongoDB);
                mongo.Remove(dbName, collectionName, query);

            }

        }

        protected void Save()
        {
            var task = new TaskFactory().StartNew(() => {
                try
                {
                    this._id = ObjectId.GenerateNewId();
                    var dbName = CONST.DB.DBName_DocService;
                    var collectionName = CONST.DB.CollectionName_ModifyLogItem;
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
