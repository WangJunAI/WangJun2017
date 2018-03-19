using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using WangJun.Utility;

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
                var dict =Convertor.FromObjectToDictionary(data);
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

        #region 保存一个数据实体
        /// <summary>
        /// 保存一个数据实体
        /// </summary>
        public void Save2(string dbName, string collectionName, string jsonfilter, object data)
        {
             if (null != data) ///若数据有效
            {
                var dict = Convertor.FromObjectToDictionary(data);
                var dat = new BsonDocument(dict);

                var db = this.client.GetDatabase(dbName);
                var collection = db.GetCollection<BsonDocument>(collectionName);
                var filter = this.FilterConvertor(jsonfilter);
                if (null != jsonfilter)
                {
                    FindOneAndReplaceOptions<BsonDocument, BsonDocument> option = new FindOneAndReplaceOptions<BsonDocument, BsonDocument>();
                    option.IsUpsert = true;///找不到就添加
                    var res = collection.FindOneAndReplace(jsonfilter, dat, option);
                }
                else
                {
                    collection.InsertOne(dat);
                }

            }
        }
        #endregion
        protected IMongoCollection<BsonDocument> GetCollection(string dbName,string collectionName)
        {
            var db = this.client.GetDatabase(dbName);
            var collection = db.GetCollection<BsonDocument>(collectionName);
            return collection;
        }

        #region 保存一个数据实体
        /// <summary>
        /// 保存一个数据实体
        /// </summary>
        public void Save3(string dbName, string collectionName, object data, string query=null,bool replace=true)
        {
            if (null != data) ///若数据有效
            {

                var dict = new Dictionary<string, object>();
                var dat = new BsonDocument();
                if(data is string&& !string.IsNullOrWhiteSpace(query) && query.ToString().Contains("_id")  && false ==replace)
                {
                    dict = Convertor.FromJsonToDict2(data.ToString());
                    dat = new BsonDocument(dict);
                }
                else
                {
                    dict = Convertor.FromObjectToDictionary3(data);
                    ////string jsonData = Convertor.FromObjectToJson(data);
                    //var dict = Convertor.FromObjectToDictionary2(data);
                    dat = new BsonDocument(dict);

                }
                 
                var collection = this.GetCollection(dbName, collectionName);
                if ((string.IsNullOrWhiteSpace(query) || "{}" == query) && (!dict.ContainsKey("_id") || string.IsNullOrWhiteSpace(dict["_id"].ToString())))
                {
                    collection.InsertOne(dat);
                }
                else
                {
                    if(string.IsNullOrWhiteSpace(query)&&dict.ContainsKey("_id")&&24 == dict["_id"].ToString().Length)
                    {
                        query = "{\"_id\":new ObjectId('"+dict["_id"]+"')}";
                    }


                    if (replace)
                    {
                        FindOneAndReplaceOptions<BsonDocument, BsonDocument> option = new FindOneAndReplaceOptions<BsonDocument, BsonDocument>();
                        option.IsUpsert = true;///找不到就添加
                        var res = collection.FindOneAndReplace(query, dat, option);
                    }
                    else
                    {
                        FindOneAndUpdateOptions<BsonDocument, BsonDocument> option = new FindOneAndUpdateOptions<BsonDocument, BsonDocument>();
                        option.IsUpsert = true;
                        var updateDefinition = "{ '$set': " + dat.ToJson() + " }";
                        if(data is string && data.ToString().Contains("$inc"))
                        {
                            updateDefinition = dat.ToJson();
                        }
                        else if(data is string && data.ToString().Contains("$set"))
                        {
                            updateDefinition = data.ToString();
                        } 
                        collection.FindOneAndUpdate(query, updateDefinition, option);
                    }
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
            var filterDict = Convertor.FromJsonToDict2(jsonString);
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
        public List<Dictionary<string, object>> Find2(string dbName, string collectionName, string query, string protection="{}", int pageIndex = 0, int pageSize = int.MaxValue, Dictionary<string, object> updateData=null)
        {
            List<Dictionary<string, object>> res = new List<Dictionary<string, object>>();
            //var filterDict = Convertor.FromJsonToDict2(jsonString);
            //var filterBuilder = Builders<BsonDocument>.Filter;
            //var filter = this.FilterConvertor(jsonString);
            FilterDefinition<BsonDocument> filter = query;
            ProjectionDefinition<BsonDocument> protectionD = protection;
            var db = this.client.GetDatabase(dbName);
            var collection = db.GetCollection<BsonDocument>(collectionName);
            var cursor = collection.Find(filter).Project(protectionD).Skip(pageIndex * pageSize).Limit(pageSize).ToCursor();
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
        public List<Dictionary<string, object>> Find3(string dbName, string collectionName, string query,string sort="{}", string protection = "{}", int pageIndex = 0, int pageSize = int.MaxValue, Dictionary<string, object> updateData = null)
        {
            List<Dictionary<string, object>> res = new List<Dictionary<string, object>>();
            FilterDefinition<BsonDocument> filter = query;
            ProjectionDefinition<BsonDocument> protectionD = protection;
            var db = this.client.GetDatabase(dbName);
            var collection = db.GetCollection<BsonDocument>(collectionName);
            var cursor = collection.Find(filter).Project(protectionD).Sort(sort).Skip(pageIndex * pageSize).Limit(pageSize).ToCursor();
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

        #region 移动数据
        /// <summary>
        /// 移动数据
        /// </summary>
        /// <param name="sourceKeyName"></param>
        /// <param name="sourceDbName"></param>
        /// <param name="sourceCollectionName"></param>
        /// <param name="sourceFilter"></param>
        /// <param name="targetKeyName"></param>
        /// <param name="targetDbName"></param>
        /// <param name="targetCollectionName"></param>
        /// <param name="needDeleteSource"></param>
        public static void MoveCollection(string sourceKeyName,string sourceDbName,string sourceCollectionName , string sourceFilter , string targetKeyName, string targetDbName, string targetCollectionName,bool needDeleteSource=false)
        {
            var sourceInst = MongoDB.GetInst(sourceKeyName);
            var targetInst = MongoDB.GetInst(targetKeyName);
            var count = 0;
            sourceInst.EventTraverse += (object sender, EventArgs e) =>
            {
                var ee = e as EventProcEventArgs;
                var dict = ee.Default as Dictionary<string,object>;
                var filter = string.Format("{{\"_id\":\"{0}\"}}", dict["_id"]);
                targetInst.Save2(targetDbName, targetCollectionName, filter, dict);
                Console.WriteLine("正在转移数据 {0} 已转移 {1}", dict["_id"],++count);
                if(true == needDeleteSource)
                {
                    sourceInst.Delete(sourceDbName, sourceCollectionName, filter);
                }
            };

            sourceInst.Find(sourceDbName, sourceCollectionName, sourceFilter);

        }
        #endregion
        #region 移动数据
        /// <summary>
        /// 移动数据
        /// </summary>
        /// <param name="sourceKeyName"></param>
        /// <param name="sourceDbName"></param>
        /// <param name="sourceCollectionName"></param>
        /// <param name="sourceFilter"></param>
        /// <param name="targetKeyName"></param>
        /// <param name="targetDbName"></param>
        /// <param name="targetCollectionName"></param>
        /// <param name="needDeleteSource"></param>
        public static void MoveCollection(MongoDB sourceInst, string sourceDbName, string sourceCollectionName, string sourceFilter, MongoDB targetInst, string targetDbName, string targetCollectionName, bool needDeleteSource = false)
        {
            if (null != sourceInst && null != targetInst)
            {
                sourceInst.EventTraverse += (object sender, EventArgs e) =>
               {
                   var ee = e as EventProcEventArgs;
                   var dict = ee.Default as Dictionary<string, object>;
                   var filter = string.Format("{{\"_id\":ObjectId('{0}')}}", dict["_id"]);
                   targetInst.Save3(targetDbName, targetCollectionName, dict,filter);

                   LOGGER.Log(string.Format("正在转移数据 {0}", dict["_id"]));
;                   if (true == needDeleteSource)
                   {
                       sourceInst.Delete(sourceDbName, sourceCollectionName, filter);
                   }
               };

                sourceInst.Find(sourceDbName, sourceCollectionName, sourceFilter);
            }
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
        public void Delete(string dbName, string collectionName, string query)
        {
            var db = this.client.GetDatabase(dbName);
            var collection = db.GetCollection<BsonDocument>(collectionName);
            collection.DeleteOne(query); 
        }
        #endregion


        #region 全部删除
        public void DeleteMany(string dbName, string collectionName, string jsonString)
        {
            //var filter = this.FilterConvertor(jsonString);

            var db = this.client.GetDatabase(dbName);
            var collection = db.GetCollection<BsonDocument>(collectionName);
            var filter = jsonString;
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
            if (!string.IsNullOrWhiteSpace(jsonString) && jsonString.Contains("{") && jsonString.Contains("}")) ///若数据可以转换
            {
                var filterDict = Convertor.FromJsonToDict2(jsonString); ///转化成字典
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
                    else if (value is string && value.ToString().Contains("new Date"))
                    {
                        value = Convert.ToDateTime(value.ToString().Replace("new Date", string.Empty).Replace("(", string.Empty).Replace(")", string.Empty).Replace("'", string.Empty));
                    }


                    if (value is Dictionary<string, object> && (value as Dictionary<string, object>).ContainsKey("$gt"))
                    {
                        var temp = (value as Dictionary<string, object>)["$gt"];
                        var val = new object();
                        if(temp is string && temp.ToString().Contains("new Date"))
                        {
                            val = Convert.ToDateTime(temp.ToString().Replace("new Date", string.Empty).Replace("(", string.Empty).Replace(")", string.Empty).Replace("'", string.Empty));
                        }
                        filter &= filterBuilder.Gt(key, val);
                    }
                    else if (value is Dictionary<string, object> && (value as Dictionary<string, object>).ContainsKey("$lt"))
                    {
                        var temp = (value as Dictionary<string, object>)["$lt"];
                        var val = new object();
                        if (temp is string && temp.ToString().Contains("new Date"))
                        {
                            val = Convert.ToDateTime(temp.ToString().Replace("new Date", string.Empty).Replace("(", string.Empty).Replace(")", string.Empty).Replace("'", string.Empty));
                        }
                        filter &= filterBuilder.Lt(key, val);
                    }
                    else
                    {
                        filter &= filterBuilder.Eq(key, value);
                    }
                    
                }


                if ("{}" == jsonString)
                {
                    return Builders<BsonDocument>.Filter.Empty;
                }

                //filter = filterBuilder.Eq("Status", "待执行") & filterBuilder.Gt("StartTime", DateTime.Now);
                return filter;
            }
            return null;
        }

        #endregion

        #region 聚合计算
        /// <summary>
        /// 添加聚合计算
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="collectionName"></param>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> Aggregate(string dbName, string collectionName,string[] pipeline)
        {
            List<Dictionary<string, object>> res = new List<Dictionary<string, object>>();
            var pipelineDefinition = PipelineDefinition< BsonDocument, BsonDocument>.Create(pipeline);//"{$match:{}}", "{$group:{_id:'$CategoryName',total:{$sum:1}}}"
            var db = this.client.GetDatabase(dbName);
            var collection = db.GetCollection<BsonDocument>(collectionName);
            var cursor = collection.Aggregate(pipelineDefinition);
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

        #region Distinct
        /// <summary>
        /// Distinct
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="collectionName"></param>
        /// <param name="filed"></param>
        /// <returns></returns>
        public List<object> Distinct(string dbName, string collectionName,string filed,string queryFilter)
        {
            var resList = new List<object>();
            var db = this.client.GetDatabase(dbName);
            var collection = db.GetCollection<BsonDocument>(collectionName);
            FieldDefinition<BsonDocument> fd = filed;
            FilterDefinition < BsonDocument > filter = queryFilter;
            var res = collection.Distinct<object>(filed, filter);
            res.MoveNext();
            foreach (var item in res.Current)
            {
                resList.Add(item);
            }
            return resList;
        }
        #endregion

        #region Count
        public long Count(string dbName, string collectionName, string filter)
        {
            var collection = this.GetCollection(dbName, collectionName);
            var res = collection.Count(filter);
            return res;
        }
        #endregion

    }
}
