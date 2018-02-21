using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.DB;
using WangJun.Utility;

namespace WangJun.Entity
{
    public class EntityManager
    {

        public static EntityManager GetInstance()
        {
            var inst = new EntityManager();
            return inst;
        }

        public int Save<T>(BaseItem item)where T :class,new()
        {
            var inst = item as T;
            var db = DataStorage.GetInstance(DBType.MongoDB);
            var session = SESSION.Current;
            if(item._OID == ObjectId.Empty)
            {
                item._OID = ObjectId.GenerateNewId();
                item.CreateTime = DateTime.Now;
                item.UpdateTime = DateTime.Now;

                item.CreatorID = session.UserID;
                item.CreatorName = session.UserName;
                item.ModifierID = session.UserID;
                item.ModifierName = session.UserName;
                item.Status = CONST.Status.Normal;

                db.Save3(item._DbName, item._CollectionName, inst);
            }
            else
            {
                var query = CONST.DB.MongoDBFilterCreator_ByObjectId(item.ID);
                db.Save3(item._DbName, item._CollectionName, inst, query);
            }
            return 0;
        }

        public int Remove(BaseItem item)
        {
            if(StringChecker.IsNotEmptyObjectId(item.ID))
            {
                var db = DataStorage.GetInstance(DBType.MongoDB);
                var query = CONST.DB.MongoDBFilterCreator_ByObjectId(item.ID);
                db.Remove(item._DbName, item._CollectionName, query);
            }
            return 0;
        }

        public T Get<T>(BaseItem item) where T: class,new()
        {
            if (StringChecker.IsNotEmptyObjectId(item.ID))
            {
                var db = DataStorage.GetInstance(DBType.MongoDB);
                var query = CONST.DB.MongoDBFilterCreator_ByObjectId(item.ID);
                var data = db.Get(item._DbName, item._CollectionName, query);
                return Convertor.FromDictionaryToObject<T>(data);
            }
            return new T();
        }

        public List<T> Find<T>(string dbName , string collectionName, string query, string protection = "{}", string sort = "{}", int pageIndex = 0, int pageSize = 50) where T : class, new()
        {
            var list = new List<T>();
            if (!string.IsNullOrWhiteSpace(query))
            {
                var mongo = DataStorage.GetInstance(DBType.MongoDB);
                var resList = mongo.Find3(dbName, collectionName, query, sort, protection, pageIndex, pageSize);

                list = Convertor.FromDictionaryToObject<T>(resList);
            }

            return list;
        }

        
    }
}
