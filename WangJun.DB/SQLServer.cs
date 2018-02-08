using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using WangJun.Utility;

namespace WangJun.DB
{
    /// <summary>
    /// SQL Server 操作器
    /// </summary>
    public class SQLServer
    {
        private static Dictionary<string, string> regDict = new Dictionary<string, string>(); ///数据注册中心

        protected string keyName = string.Empty;
        protected string connectionString = string.Empty;
        protected List<Dictionary<string, object>> systemObjects = null;
        protected Dictionary<string, Dictionary<string, object>> userTableDict = null;

        #region 注册连接
        ///<summary>
        ///注册连接
        /// </summary>
        public static void Register(string keyName,string connectionString)
        {
            if(null == SQLServer.regDict)
            {
                SQLServer.regDict = new Dictionary<string, string>();
            }
            SQLServer.regDict[keyName] = connectionString;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public static SQLServer GetInstance(string keyName)
        {
            if(null !=SQLServer.regDict && SQLServer.regDict.ContainsKey(keyName))
            {
                SQLServer inst = new SQLServer();
                inst.keyName = keyName;
                inst.connectionString = SQLServer.regDict[keyName];
                inst.UpdateSysObject(true);
                return inst;
            }
            return null;
        }

        #region  获取可用连接
        /// <summary>
        /// 获取可用连接
        /// </summary>
        /// <returns></returns>
        protected SqlConnection GetConnection()
        {
            SqlConnection conn = new SqlConnection(this.connectionString);
            conn.Open();
            return conn;
        }
        #endregion

        #region 获取数据库系统信息
        /// <summary>
        /// 获取数据库系统信息
        /// </summary>
        /// <param name="forceUpdate">是否强制更新</param>
        public void UpdateSysObject(bool forceUpdate= false)
        {
            if (null == this.systemObjects || true == forceUpdate)
            {
                string sql = "SELECT * FROM sysobjects";
                var res = this.Find(sql);
                this.systemObjects = res;
                //this.userTableDict = this.systemObjects.ToDictionary(new Func<Dictionary<string, object>, string>((item)=> { return item["name"].ToString()}));
                this.userTableDict = this.systemObjects.ToDictionary(p => p["name"].ToString());
            }
        }
        #endregion  

        /// <summary>
        /// 检测
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public bool IsExistUserTable(string tableName,Dictionary<string,object> exampleData=null,bool forceCheck=false)
        {
            this.UpdateSysObject(forceCheck);///获取最新数据
            if(!string.IsNullOrWhiteSpace(tableName))
            {
                //var res = from dictItem in this.systemObjects where  dictItem["name"].ToString().ToUpper() == tableName.ToUpper()  select dictItem;
                //return 0 < res.Count();
                return this.userTableDict.ContainsKey(tableName);
            }
            return false;
        }


        /// <summary>
        /// 保存一个实体
        /// </summary>
        public void Save(string sql , List<KeyValuePair<string,object>> paramList = null)
        {
            var conn = this.GetConnection();
            if (conn.State == System.Data.ConnectionState.Open)
            {
                var com = conn.CreateCommand();
                com.CommandText = sql;
                com.CommandType = System.Data.CommandType.Text;

                if (null != paramList)
                {
                    foreach (var item in paramList)
                    {
                        com.Parameters.Add(new SqlParameter(item.Key, item.Value));
                    }
                }
                var res = com.ExecuteNonQuery();

                conn.Close();
            }
            else
            {

            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        public void Delete(string sql, List<KeyValuePair<string, object>> paramList = null)
        {
            this.CUD(sql, paramList);
        }

        /// <summary>
        /// 增删改
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        protected void CUD(string sql, List<KeyValuePair<string, object>> paramList = null)
        {
            var conn = this.GetConnection();
            if (conn.State == System.Data.ConnectionState.Open)
            {
                var com = conn.CreateCommand();
                com.CommandText = sql;
                com.CommandType = System.Data.CommandType.Text;

                if (null != paramList)
                {
                    foreach (var item in paramList)
                    {
                        com.Parameters.Add(new SqlParameter(item.Key, item.Value));
                    }
                }
                var res = com.ExecuteNonQuery();

                conn.Close();
            }
            else
            {

            }
        }

        public void Update()
        {

        }

        #region 创建一个数据库
        /// <summary>
        /// 创建一个数据库
        /// </summary>
        public void CreateDatabase(string databaseName)
        {
            string sql = string.Format("CREATE DATABASE {0}",databaseName);
            var conn = this.GetConnection();
            if (conn.State == System.Data.ConnectionState.Open)
            {
                var com = conn.CreateCommand();
                com.CommandText = sql;
                com.CommandType = System.Data.CommandType.Text;
                 var res = com.ExecuteNonQuery();

                conn.Close();
            }
            else
            {

            }

        }
        #endregion

        #region 根据样例数据 创建一个数据表
        /// <summary>
        /// 根据样例数据 创建一个数据表
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="exampleData"></param>
        public void CreateTable(string tableName , Dictionary<string,object> exampleData)
        {
            if(null != exampleData) ///若数据有效
            { 
                var stringBuilder = new StringBuilder();
                int temp1 = 0;
                var temp2 = DateTime.Now;
                float temp3 = 0.0f;
                foreach (var item in exampleData)
                {

                    if(int.TryParse(item.Value.ToString(),out temp1))
                    {
                        stringBuilder.AppendFormat(" {0} {1} ,", item.Key, "INT");
                    }
                    else if (float.TryParse(item.Value.ToString(),out temp3))
                    {
                        stringBuilder.AppendFormat(" {0} {1} ,", item.Key, "FLOAT");
                    }
                    else if (item.Value.ToString().Contains(":")&&DateTime.TryParse(item.Value.ToString(),out temp2))
                    {
                        stringBuilder.AppendFormat(" {0} {1} ,", item.Key, "DATETIME");
                    }
                    else if (typeof(ObjectId) == item.Value.GetType())
                    {
                        stringBuilder.AppendFormat(" {0} {1} ,", item.Key, "NVARCHAR(64)");
                    }
                    else if (typeof(string) ==item.Value.GetType())
                    {
                        stringBuilder.AppendFormat(" {0} {1} ,", item.Key, "NVARCHAR(MAX)");
                    }

                    
                }

                var sql = string.Format("CREATE TABLE {0} ({1})", tableName, stringBuilder.ToString());

                var conn = this.GetConnection();
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    var com = conn.CreateCommand();
                    com.CommandText = sql;
                    com.CommandType = System.Data.CommandType.Text;
                    var res = com.ExecuteNonQuery();

                    conn.Close();
                }
                else
                {

                }

                this.UpdateSysObject();
            }
        }
        #endregion

        #region 检查数据库是否存在

        #endregion

        #region 检查数据表是否存在

        #endregion

        #region 插入一个对象
        /// <summary>
        /// 插入一个对象
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="data"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public void Save(string dbName , string tableName , object data)
        {
            if(!this.IsExistUserTable(tableName))
            {
                this.CreateTable(tableName, Convertor.FromObjectToDictionary(data));
            }


            string sql = " INSERT INTO {0} ( {1} )  VALUES ( {2} ) ";
            var dict = data as Dictionary<string, object>;
            var paramList = new List<KeyValuePair<string, object>>();
            StringBuilder builderCol = new StringBuilder();
            StringBuilder builderParam = new StringBuilder();
            foreach (var item in dict)
            {
                builderCol.AppendFormat(" [{0}],",item.Key);
                builderParam.AppendFormat(" @{0},", item.Key);
                object value = null;
                if(typeof(ObjectId) == item.Value.GetType())
                {
                    value = item.Value.ToString();
                }
                else if (typeof(Decimal128) == item.Value.GetType())
                {
                    value =Convert.ToSingle( item.Value);
                }
                else
                {
                    value = item.Value;
                }

                paramList.Add(new KeyValuePair<string, object>("@" + item.Key, value));
            }
            sql = string.Format(sql, tableName, builderCol.ToString().TrimEnd(new char[] { ',' }), builderParam.ToString().TrimEnd(new char[] { ',' }));

            this.Save(sql, paramList);

        }
        #endregion

        /// <summary>
        /// 查找一个对象
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public List<Dictionary<string,object>> Find(string sql, List<KeyValuePair<string, object>> paramList = null)
        {
            var conn = this.GetConnection();
            if (conn.State == System.Data.ConnectionState.Open)
            {
                var com = conn.CreateCommand();
                com.CommandText = sql;
                com.CommandType = System.Data.CommandType.Text;
                if (null != paramList)
                {
                    foreach (var item in paramList)
                    {
                        com.Parameters.Add(new SqlParameter(item.Key, item.Value));
                    }
                }

                SqlDataAdapter adapter = new SqlDataAdapter(com);
                var tableSet = new DataSet();
                adapter.Fill(tableSet);
                adapter.Dispose();

                var res = Convertor.FromDataTableToList(tableSet);

                return res;

                
            }
            else
            {

            }
            return null;
        }
    }
}
