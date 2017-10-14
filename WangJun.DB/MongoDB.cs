using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using WangJun.Data;
using WangJun.Tools;

namespace WangJun.DB
{
    /// <summary>
    /// MongoDB操作器
    /// </summary>
    public class MongoDB
    {
        protected IMongoClient client;

        public event EventHandler EventTraverse = null;

        #region 数据库注册

        #endregion

        #region 初始化一个实例
        public static MongoDB GetInst(string url)
        {
            if (StringChecker.IsMongoDBConnectionString(url))
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
        public void Save(string dbName, string collectionName, object data)
        {
            if (null != data) ///若数据有效
            {
                var dict = Convertor.FromObjectToDictionary(data);
                var dat = new BsonDocument(dict);

                var db = this.client.GetDatabase(dbName);
                var collection = db.GetCollection<BsonDocument>(collectionName);

                if(!dict.ContainsKey("_id")) ///若是新实体
                {
                    collection.InsertOne(dat);
                }
                else
                {
                    var id = dict["_id"];
                    var filterBuilder = Builders<BsonDocument>.Filter;
                    var filter = filterBuilder.Eq("_id", id);
                    //dat.Remove("_id");
                    collection.ReplaceOne(filter, dat);
                }



            }
        }
        #endregion

        #region 查找结果
        /// <summary>
        /// 根据条件查找结果
        /// </summary>
        /// <returns></returns>
        public List<Dictionary<string, object>> Find(string dbName, string collectionName, string jsonString,int pageIndex=0,int pageSize=int.MaxValue)
        {
            List<Dictionary<string, object>> res = new List<Dictionary<string, object>>();
            var filterDict = Convertor.FromJsonToDict(jsonString);
            var filterBuilder = Builders<BsonDocument>.Filter;
            //var filter = filterBuilder.Empty;
            //var filter = filterBuilder.Eq("ItemType", "PageArticle") & filterBuilder.Eq("Status", "NotDownloaded");
            var filter = this.FilterConvertor(jsonString);

            var db = this.client.GetDatabase(dbName);
            var collection = db.GetCollection<BsonDocument>(collectionName);
            var cursor = collection.Find(filter).Skip(pageIndex* pageSize).Limit(pageSize).ToCursor();
            foreach (var document in cursor.ToEnumerable())
            {
                if (null == this.EventTraverse)
                {
                    res.Add(document.ToDictionary());
                }
                else
                {
                    EventProc.TriggerEvent(this.EventTraverse, this, EventProcEventArgs.Create(document.ToDictionary()));
                }
            }
            return res;
        }
        #endregion



        #region 获取集合的统计信息
        /// <summary>
        /// 获取集合的统计信息
        /// </summary>
        /// <returns></returns>
        public Dictionary<string,object> GetCollectionStatistic(string dbName, string collectionName=null)
        {
            var db = this.client.GetDatabase(dbName);
            var collectionList = db.ListCollections();
            foreach (var collection in collectionList.ToEnumerable())
            {
                 var dict = collection.ToDictionary();
                var name = dict["name"];
            }
            return null;
        }
        #endregion

 


        #region  生成查询过滤器
        /// <summary>
        /// 生成查询过滤器
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        protected FilterDefinition<BsonDocument> FilterConvertor(string jsonString)
        {
            var filterDict = Convertor.FromJsonToDict(jsonString); ///转化成字典
            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = filterBuilder.Empty;
            foreach (var item in filterDict)
            {
                var key = item.Key;
                var value = item.Value;
                filter &= filterBuilder.Eq(key, value);
            }
            if("{}" == jsonString)
            {
                return Builders<BsonDocument>.Filter.Empty;
            }
            return filter;
        }

        #endregion


    }
}
