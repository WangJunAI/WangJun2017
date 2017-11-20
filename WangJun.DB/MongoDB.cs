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
        private static Dictionary<string, string> regDict = new Dictionary<string, string>(); ///数据注册中心


        protected IMongoClient client;

        public event EventHandler EventTraverse = null;

        #region 数据库注册
        public static void Register(string keyName, string connectionString)
        {
            if (null == MongoDB.regDict)
            {
                MongoDB.regDict = new Dictionary<string, string>();
            }
            MongoDB.regDict[keyName] = connectionString;
        }
        #endregion

        #region 初始化一个实例
        /// <summary>
        /// 获取一个实例
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public static MongoDB GetInst(string keyName)
        {
            if (MongoDB.regDict.ContainsKey(keyName))
            {
                var db = new MongoDB();
                db.client = new MongoClient(MongoDB.regDict[keyName]);
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
                    var res = collection.ReplaceOne(filter, dat);
                }



            }
        }
        #endregion

        #region 保存一个数据实体
        /// <summary>
        /// 保存一个数据实体
        /// </summary>
        public void Save(string dbName, string collectionName,object data, string key=null)
        {
            if(key == null) ///兼容旧版
            {
                this.Save(dbName, collectionName, data);
                return;
            }

            if (null != data) ///若数据有效
            {
                var dict = Convertor.FromObjectToDictionary(data);
                var dat = new BsonDocument(dict);

                var db = this.client.GetDatabase(dbName);
                var collection = db.GetCollection<BsonDocument>(collectionName);
 
                var value = dict[key];
                var filterBuilder = Builders<BsonDocument>.Filter;
                var filter = filterBuilder.Eq(key, value);
                FindOneAndReplaceOptions<BsonDocument, BsonDocument> option = new FindOneAndReplaceOptions<BsonDocument, BsonDocument>();
                option.IsUpsert = true;///找不到就添加

                var res = collection.FindOneAndReplace(filter, dat,option);
            }
        }
        #endregion

        #region 保存一组数据实体
        /// <summary>
        /// 保存一组数据实体
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="collectionName"></param>
        /// <param name="items"></param>
        public void Save(string dbName, string collectionName, IEnumerable<object> items)
        {
            if(null != items)
            {
                foreach (var item in items)
                {
                    this.Save(dbName, collectionName, item);
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
         
        #region 基于Json的查询
        /// <summary>
        /// 基于Linq的查询
        /// </summary>
        /// <param name="filter">过滤器</param>
        /// <returns></returns>
        public List<Dictionary<string, object>> Find2(string dbName, string collectionName, string jsonString, string protection="{}", Dictionary<string, object> updateData=null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            //List<Dictionary<string, object>> res = new List<Dictionary<string, object>>();
            //var filterDict = Convertor.FromJsonToDict(jsonString);
            //var filterBuilder = Builders<BsonDocument>.Filter;
            //var filter = this.FilterConvertor(jsonString);

            //var db = this.client.GetDatabase(dbName);
            //var collection = db.GetCollection<BsonDocument>(collectionName);
            //var cursor = collection.FindOneAndUpdate(filter).,
            //foreach (var document in cursor.ToEnumerable())
            //{
            //    if (null == this.EventTraverse)
            //    {
            //        res.Add(document.ToDictionary());
            //    }
            //    else
            //    {
            //        EventProc.TriggerEvent(this.EventTraverse, this, EventProcEventArgs.Create(document.ToDictionary()));
            //    }
            //}
            //return res;
            return null;
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


        #region 删除一个实体
        public void Delete(string dbName, string collectionName, string jsonString)
        {
             
            var filterDict = Convertor.FromJsonToDict(jsonString);
            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = this.FilterConvertor(jsonString);

            var db = this.client.GetDatabase(dbName);
            var collection = db.GetCollection<BsonDocument>(collectionName);
            collection.DeleteOne(filter); 
        }
        #endregion


        #region 全部删除
        public void DeleteMany(string dbName, string collectionName, string jsonString)
        {
            var filter = this.FilterConvertor(jsonString);

            var db = this.client.GetDatabase(dbName);
            var collection = db.GetCollection<BsonDocument>(collectionName);
            collection.DeleteMany(filter);
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
                if ("_id" == key.ToLower())
                {
                    value = ObjectId.Parse(value.ToString());
                }
                else if (StringChecker.IsGUID(value.ToString()))
                {
                    value = Guid.Parse(value.ToString());
                }
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
