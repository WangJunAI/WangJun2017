using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

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

        public static SQLServer GetInstance(string keyName)
        {
            if(null !=SQLServer.regDict && SQLServer.regDict.ContainsKey(keyName))
            {
                SQLServer inst = new SQLServer();
                inst.keyName = keyName;
                inst.connectionString = SQLServer.regDict[keyName];
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

        public void Delete()
        {


        }

        public void Update()
        {

        }

        public List<Dictionary<string,object>> Find()
        {
            throw new NotImplementedException();
        }
    }
}
