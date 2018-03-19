using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.Net;
using WangJun.Utility;

namespace WangJun.DB
{
    /// <summary>
    /// 数据库读写器
    /// 支持：MongoDB,MySQL，SQLServer，Redis，Mencache
    /// </summary>
    public class DataStorage
    {
        protected MongoDB mongo = null;

        protected SQLServer sqlserver = null;

        public event EventHandler EventTraverse = null;

        private static DateTime initialTime = DateTime.MinValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="dbType">mongo,sqlserver,mysql,redis</param>
        /// <returns></returns>
        public static DataStorage GetInstance(string keyName="140",string dbType="mongo")
        {
            DataStorage.Register();
            DataStorage inst = new DataStorage();
            if ("mongo" == dbType)
            {
                inst.mongo = MongoDB.GetInst(keyName);
            }
            else if("sqlserver" == dbType)
            {
                inst.sqlserver = SQLServer.GetInstance(keyName);
                inst.sqlserver.UpdateSysObject(true);
            }
            return inst;
        }
        
        /// <summary>
        /// 获取一个可操作性的实例
        /// </summary>
        /// <param name="type"></param>
        /// <param name="exData"></param>
        /// <returns></returns>
        public static DataStorage GetInstance(DBType dbType, object exData=null)
        {
            DataStorage.Register();
            DataStorage inst = new DataStorage();
            if (DBType.MongoDB == dbType)
            {
                inst.mongo = MongoDB.GetInst("mongodb");
            }
            else if (DBType.SQLServer == dbType)
            {
                inst.sqlserver = SQLServer.GetInstance("sqlserver");
            }
            return inst;
        }

        #region 数据库注册
        /// <summary>
        /// 数据注册
        /// </summary>
        public static void Register()
        {
            if (DateTime.MinValue == DataStorage.initialTime)
            {
                var configJson = new HTTP().GetGzip2(string.Format("http://aifuwu.wang/API.ashx?c=WangJun.DB.YunConfig&m=Load&p0={0}",YunConfig.CurrentGroupID), Encoding.UTF8);
                if (!string.IsNullOrWhiteSpace(configJson))
                {
                    var configDict = Convertor.FromJsonToDict2(configJson); ///加载当前的配置信息
                    foreach (var item in configDict)
                    {
                        if (item.Value.ToString().Contains("mongodb://"))
                        {
                            MongoDB.Register(item.Key, item.Value.ToString());
                        }
                        else if (item.Value.ToString().Contains("Data Source="))
                        {
                            SQLServer.Register(item.Key, item.Value.ToString());
                        }
                        else if (item.Value.ToString().Contains("server="))
                        {
                            MySQL.Register(item.Key, item.Value.ToString());
                        }
                    }
                    DataStorage.initialTime = DateTime.Now;
                    LOGGER.Log("数据库组件配置信息初始化完毕 初始化为 "+YunConfig.CurrentGroupID);
                }
            }
        }
        #endregion  

        #region 存储一个数据，若数据存在，则更新
        /// <summary>
        /// 存储一个数据，若数据存在，则更新
        /// </summary>
        /// <param name="data"></param>
        public void Save(object data, string tableName, string dbName, string instanceName = "140", string key = null)
        {
            this.mongo.Save(dbName, tableName, data, key);
        }
        #endregion

        #region 存储一个数据，若数据存在，则更新
        /// <summary>
        /// 存储一个数据，若数据存在，则更新
        /// </summary>
        /// <param name="data"></param>
        public void Save2(string dbName, string tableName, string jsonFilter, object data, string instanceName = "140")
        {
            if (null != this.mongo)
            {
                this.mongo.Save2(dbName, tableName, jsonFilter, data);
            }
            else if (null != this.sqlserver)
            {
                var dict = Convertor.FromObjectToDictionary(data);
                this.sqlserver.Save(dbName, tableName, dict);
            }
        }
        #endregion

        #region 存储一个数据，若数据存在，则更新
        /// <summary>
        /// 存储一个数据，若数据存在，则更新
        /// </summary>
        /// <param name="data"></param>
        public void Save3(string dbName, string tableName, object data, string query=null,bool replace=true )
        {
            if (null != this.mongo)
            {
                this.mongo.Save3(dbName, tableName, data, query, replace);
            }
            else if (null != this.sqlserver)
            {
                var dict = Convertor.FromObjectToDictionary(data);
                this.sqlserver.Save(dbName, tableName, dict);
            }
        }
        #endregion

        #region 删除数据
        public void Delete(string jsonFilter , string tableName, string dbName, string instanceName = "140", object exParam = null)
        {
            if (null != this.mongo)
            {
                this.mongo.DeleteMany(dbName, tableName, jsonFilter);
            }
            else if (null != this.sqlserver)
            {
                if(this.sqlserver.IsExistUserTable(tableName))
                {
                    this.sqlserver.Delete(jsonFilter, exParam as List<KeyValuePair<string, object>>);
                }
               
            } 
        }
        #endregion

        #region 移除一个数据
        /// <summary>
        /// 移除一个数据
        /// </summary>
        /// <param name="id"></param>
        public void Remove(string dbName, string tableName,string jsonFilter)
        {

            this.mongo.DeleteMany(dbName, tableName, jsonFilter);
        }
        #endregion

        #region 基于Json/SQLServer的查询
        /// <summary>
        /// 基于Linq的查询
        /// </summary>
        /// <param name="filter">过滤器</param>
        /// <returns></returns>
        public List<Dictionary<string, object>> Find(string dbName, string tableName, string jsonString, int pageIndex = 0, int pageSize = int.MaxValue,object exParam=null)
        {
            if(null != this.mongo)
            {
                var res = mongo.Find(dbName, tableName, jsonString, pageIndex, pageSize);
                return res;
            }
            else if(null != this.sqlserver)
            {
                var res = this.sqlserver.Find(jsonString, exParam as List<KeyValuePair<string,object>>);
                return res;
            }

            return null;
        }
        #endregion
        #region 基于Json/SQLServer的查询
        /// <summary>
        /// 基于Linq的查询
        /// </summary>
        /// <param name="filter">过滤器</param>
        /// <returns></returns>
        public List<Dictionary<string, object>> Find2(string dbName, string collectionName, string query, string protection = "{}", int pageIndex = 0, int pageSize = int.MaxValue, Dictionary<string, object> updateData = null)
        {
            if (null != this.mongo)
            {
                var res = mongo.Find2(dbName, collectionName, query,protection, pageIndex, pageSize,updateData);
                return res;
            }
            else if (null != this.sqlserver)
            {
                //var res = this.sqlserver.Find(jsonString, exParam as List<KeyValuePair<string, object>>);
                //return res;
            }

            return null;
        }
        #endregion
        #region 基于Json/SQLServer的查询
        /// <summary>
        /// 基于Linq的查询
        /// </summary>
        /// <param name="filter">过滤器</param>
        /// <returns></returns>
        public List<Dictionary<string, object>> Find3(string dbName, string collectionName, string query, string sort="{}",string protection = "{}", int pageIndex = 0, int pageSize = int.MaxValue, Dictionary<string, object> updateData = null)
        {
            if (null != this.mongo)
            {
                var res = mongo.Find3(dbName, collectionName, query,sort, protection, pageIndex, pageSize, updateData);
                return res;
            }
            else if (null != this.sqlserver)
            {
                //var res = this.sqlserver.Find(jsonString, exParam as List<KeyValuePair<string, object>>);
                //return res;
            }

            return null;
        }
        #endregion

        #region 遍历处理
        /// <summary>
        /// 遍历处理
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="tableName"></param>
        /// <param name="jsonString"></param>
        public void Traverse(string dbName, string tableName, string query)
        {
            var pageSize = 1000;
            var index = 0;
            var startTime = DateTime.Now;
            var prevTime = startTime;
            var list = mongo.Find3(dbName, tableName, query,"{}","{}", index++, pageSize);
            var hasData = (0 < list.Count) ? true : false;
            
            var queue = new Queue<List<Dictionary<string, object>>>();
            queue.Enqueue(list);
            while (0< queue.Count)
            {
                var cost = DateTime.Now - prevTime;
                 list = queue.Dequeue();
                foreach (var item in list)
                {
                    EventProc.TriggerEvent(this.EventTraverse, this, EventProcEventArgs.Create(item));
                }

                var task = Task.Factory.StartNew <object>(() => {
                    prevTime=DateTime.Now;
                    Console.WriteLine(" 准备查找第{0}页数据 页面大小:{1} 上次耗时:{2}",index,pageSize,cost);
                    var resList = mongo.Find3(dbName, tableName, query, "{}", "{}", index++, pageSize);
                    return resList;
                });

                var nextList = task.Result as List<Dictionary<string, object>>;
                if (0 < nextList.Count)
                {
                    queue.Enqueue(nextList);
                    Console.WriteLine(" 队列深度 {0} ", queue.Count);
                }
                else
                {
                    Console.WriteLine(" 数据已经全部遍历完毕 ");
                }
            }
        }
        #endregion

        #region 转移集合
        /// <summary>
        /// 转移集合
        /// </summary>
        /// <param name="sourceKeyName"></param>
        /// <param name="sourceDbName"></param>
        /// <param name="sourceCollectionName"></param>
        /// <param name="sourceFilter"></param>
        /// <param name="targetKeyName"></param>
        /// <param name="targetDbName"></param>
        /// <param name="targetCollectionName"></param>
        /// <param name="needDeleteSource"></param>
        public static void MoveCollection(string sourceKeyName, string sourceDbName, string sourceCollectionName, string sourceFilter, string targetKeyName, string targetDbName, string targetCollectionName, bool needDeleteSource = false)
        {
            MongoDB.MoveCollection( sourceKeyName, sourceDbName,   sourceCollectionName,   sourceFilter,   targetKeyName,   targetDbName,   targetCollectionName,   needDeleteSource );
        }
        #endregion

        #region 转移集合
        /// <summary>
        /// 转移集合
        /// </summary>
        /// <param name="sourceKeyName"></param>
        /// <param name="sourceDbName"></param>
        /// <param name="sourceCollectionName"></param>
        /// <param name="sourceFilter"></param>
        /// <param name="targetKeyName"></param>
        /// <param name="targetDbName"></param>
        /// <param name="targetCollectionName"></param>
        /// <param name="needDeleteSource"></param>
        public static void MoveCollection(DataStorage sourceInst, string sourceDbName, string sourceCollectionName, string sourceFilter, DataStorage targetInst, string targetDbName, string targetCollectionName, bool needDeleteSource = false)
        {
            MongoDB.MoveCollection(sourceInst.mongo, sourceDbName, sourceCollectionName, sourceFilter,targetInst.mongo, targetDbName, targetCollectionName, needDeleteSource);
        }
        #endregion


        #region 移动一个数据库
        /// <summary>
        /// 
        /// </summary>
        public static void MoveDatabase(string sourceKeyName, string sourceDbName,string targetKeyName, string targetDbName, bool needDeleteSource = false)
        {

        }
        #endregion

        #region 将2D数据从MongoDB转移到SQLServer
        /// <summary>
        /// 将2D数据从MongoDB转移到SQLServer
        /// </summary>
        public static void MoveDataFromMongoToSQLServer(string sourceKeyName, string sourceDbName, string sourceCollectionName, string sourceFilter, string targetKeyName, string targetDbName, string targetCollectionName, bool needDeleteSource = false)
        {
            var srcDB = DataStorage.GetInstance(DBType.MongoDB);
            var targetDB = DataStorage.GetInstance(DBType.SQLServer);
            var srcList = srcDB.Find(sourceDbName, sourceCollectionName, sourceFilter, 0, 1);
            if (1 == srcList.Count)
            {
                var count = 0;
                var exampleData = srcList.First();
                ///检查标的存在和结构,不存在则创建
                if (!targetDB.sqlserver.IsExistUserTable(targetCollectionName))
                {
                    targetDB.sqlserver.CreateTable(targetCollectionName, exampleData);
                }

                srcDB.EventTraverse += (object sender , EventArgs e) => {
                    var ee = e as EventProcEventArgs;
                    var data = ee.Default as Dictionary<string, object>;
                    if(null != data)
                    {
                        targetDB.sqlserver.Save(targetDbName, targetCollectionName, data);
                        Console.WriteLine(" 成功转移一个数据 {0} {1}", count++,DateTime.Now);
                    }

                };

                srcDB.Traverse(sourceDbName, sourceCollectionName,sourceFilter);
                ///生成SQL
                ///插入数据
            }
        }
        #endregion

        #region 基于Json的查询
        /// <summary>
        /// 基于Linq的查询
        /// </summary>
        /// <param name="filter">过滤器</param>
        /// <returns></returns>
        public List<Dictionary<string, object>> Find(string dbName, string tableName, string jsonString,string protection,Dictionary<string,object> updateData, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var res = mongo.Find(dbName, tableName, jsonString, pageIndex, pageSize);
            return res;
        }
        #endregion

        #region Count
        public long Count(string dbName , string collectionName,string filter) {
            var res = this.mongo.Count(dbName, collectionName, filter);
            return res;
        }
        #endregion

        #region 自动优化
        /// <summary>
        /// 自动优化
        /// </summary>
        public void AutoOptimize()
        {

        }
        #endregion

        #region Log

        #endregion

        #region Get
        public Dictionary<string,object> Get(string dbName, string collectionName, string query, string sort= "{}", string protection = "{}", int pageIndex = 0, int pageSize = int.MaxValue, Dictionary<string, object> updateData = null)
        {

            var list = this.Find3(dbName, collectionName, query, sort, protection, 0, 1);
            if(1 == list.Count)
            {
                return list.First();
            }
            return null;
        }
        #endregion

        #region 聚合查询
         public List<Dictionary<string, object>> Aggregate(string dbName, string collectionName, string match,string group)
        {
            if (null != this.mongo)
            {
                var res = mongo.Aggregate(dbName, collectionName, new string[] { match, group });
                return res;
            }
            else if (null != this.sqlserver)
            {
                //var res = this.sqlserver.Find(jsonString, exParam as List<KeyValuePair<string, object>>);
                //return res;
            }

            return null;
        }
        #endregion
    }
}
