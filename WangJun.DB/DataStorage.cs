using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.Tools;

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
            }
            return inst;
        }
        /// <summary>
        /// 数据注册
        /// </summary>
        public static void Register()
        {
            MongoDB.Register("140", "mongodb://192.168.0.140:27017");
            MongoDB.Register("170", "mongodb://192.168.0.170:27017");
            SQLServer.Register("140", @"Data Source=192.168.0.140\sql2016;Initial Catalog=StockData2D;Persist Security Info=True;User ID=sa;Password=111qqq!!!");
            MySQL.Register("140", @"server=192.168.0.140;user=root;database=WJBigData;port=3306;password=111qqq!!!");

        }

        #region

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
            this.mongo.Save2(dbName, tableName, jsonFilter, data);
        } 
        #endregion

        #region 删除数据
        public void Delete(string jsonFilter , string tableName, string dbName, string instanceName = "140")
        {
            this.mongo.DeleteMany(dbName, tableName, jsonFilter);
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

        #region 基于Json的查询
        /// <summary>
        /// 基于Linq的查询
        /// </summary>
        /// <param name="filter">过滤器</param>
        /// <returns></returns>
        public List<Dictionary<string, object>> Find(string dbName, string tableName, string jsonString, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var res = mongo.Find(dbName, tableName, jsonString, pageIndex, pageSize);
            return res;
        }
        #endregion

        #region 遍历处理
        /// <summary>
        /// 遍历处理
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="tableName"></param>
        /// <param name="jsonString"></param>
        public void Traverse(string dbName, string tableName, string jsonString)
        {
            var pageSize = 1000;
            var index = 0;
            var list = mongo.Find(dbName, tableName, jsonString, ++index, pageSize);
            var hasData = (0 < list.Count) ? true : false;
            
            var queue = new Queue<List<Dictionary<string, object>>>();
            queue.Enqueue(list);
            while (0< queue.Count)
            {
                list = queue.Dequeue();
                foreach (var item in list)
                {
                    EventProc.TriggerEvent(this.EventTraverse, this, EventProcEventArgs.Create(item));
                }

                var task = Task.Factory.StartNew <object>(() => {
                    Console.WriteLine(" 准备查找第{0}页数据 ",index);
                    var resList = mongo.Find(dbName, tableName, jsonString, ++index, pageSize);
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

        #region 将2D数据从MongoDB转移到SQLServer
        /// <summary>
        /// 将2D数据从MongoDB转移到SQLServer
        /// </summary>
        public static void MoveDataFromMongoToSQLServer(string sourceKeyName, string sourceDbName, string sourceCollectionName, string sourceFilter, string targetKeyName, string targetDbName, string targetCollectionName, bool needDeleteSource = false)
        {
            var srcDB = DataStorage.GetInstance(sourceKeyName, "mongo");
            var targetDB = DataStorage.GetInstance(targetKeyName, "sqlserver");
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

        #region 自动优化
        /// <summary>
        /// 自动优化
        /// </summary>
        public void AutoOptimize()
        {

        }
        #endregion
    }
}
