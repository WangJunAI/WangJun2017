using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.DB
{
    /// <summary>
    /// 数据库读写器
    /// 支持：MongoDB,MySQL，SQLServer，Redis，Mencache
    /// </summary>
    public class DataStorage
    {
        protected MongoDB mongo = null;
         public static DataStorage GetInstance()
        {
            DataStorage.Register();
            DataStorage inst = new DataStorage();
            inst.mongo = MongoDB.GetInst("140");
            return inst;
        }
        /// <summary>
        /// 数据注册
        /// </summary>
        public static void Register()
        {
            MongoDB.Register("140", "mongodb://192.168.0.140:27017");
            SQLServer.Register("140", @"Data Source=192.168.0.140\sql2016;Initial Catalog=WJStock;Persist Security Info=True;User ID=sa;Password=111qqq!!!");
            MySQL.Register("140", @"server=192.168.0.140;user=root;database=WJBigData;port=3306;password=111qqq!!!");

        }

        #region

        #endregion

        #region 存储一个数据，若数据存在，则更新
        /// <summary>
        /// 存储一个数据，若数据存在，则更新
        /// </summary>
        /// <param name="data"></param>
        public void Save(object data,string tableName , string dbName,string instanceName="140")
        {
            this.mongo.Save(dbName, tableName, data);
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

        #region 基于Linq的查询
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
