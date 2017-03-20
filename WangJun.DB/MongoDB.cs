using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using WangJun.Data;

namespace WangJun.DB
{
    /// <summary>
    /// MongoDB操作器
    /// </summary>
    public class MongoDB
    {
        protected  IMongoClient client;

        #region 初始化一个实例
        public static MongoDB GetInst(string url)
        {
            if(StringChecker.IsMongoDBConnectionString(url))
            {
                var db = new MongoDB();
                db.client = new MongoClient(url);
                return db;
            }
            return null;
        }
        #endregion

        #region 保存一个数据实体
        /// <summary>
        /// 保存一个数据实体
        /// </summary>
        public void Save(string dbName,string collectionName , object data)
        {
            if (null != data) ///若数据有效
            {
                var dict = Convertor.FromObjectToDictionary(data);
                var dat = new BsonDocument(dict);

                var db = this.client.GetDatabase(dbName);
                var collection = db.GetCollection<BsonDocument>(collectionName);

                collection.InsertOne(dat);
            }
        }
        #endregion
    }
}
