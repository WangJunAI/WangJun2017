using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace WangJun.DB
{
    /// <summary>
    /// MySQL 操作器
    /// </summary>
    public class MySQL
    {
        private static Dictionary<string, string> regDict = new Dictionary<string, string>(); ///数据注册中心

        protected string keyName = string.Empty;
        protected string connectionString = string.Empty;

        #region 注册连接
        ///<summary>
        ///注册连接
        /// </summary>
        public static void Register(string keyName, string connectionString)
        {
            if (null == MySQL.regDict)
            {
                MySQL.regDict = new Dictionary<string, string>();
            }
            MySQL.regDict[keyName] = connectionString;
        }
        #endregion

        public static MySQL GetInstance(string keyName)
        {
            if (null != MySQL.regDict && MySQL.regDict.ContainsKey(keyName))
            {
                MySQL inst = new MySQL();
                inst.keyName = keyName;
                inst.connectionString = MySQL.regDict[keyName];
                return inst;
            }
            return null;
        }

        #region  获取可用连接
        /// <summary>
        /// 获取可用连接
        /// </summary>
        /// <returns></returns>
        protected MySqlConnection GetConnection()
        {
            MySqlConnection conn = new MySqlConnection(this.connectionString);
            conn.Open();
            return conn;
        }
        #endregion


        /// <summary>
        /// 保存一个实体
        /// </summary>
        public void Save(string sql, List<KeyValuePair<string, object>> paramList = null)
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
                        com.Parameters.Add(new MySqlParameter(item.Key, item.Value));
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

        public List<Dictionary<string, object>> Find()
        {
            throw new NotImplementedException();
        }

        public void ExcuteScript()
        {

        }
    }
}
