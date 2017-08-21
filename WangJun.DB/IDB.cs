using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.DB
{
    /// <summary>
    /// DB数据库接口
    /// </summary>
    public interface IDB
    {
        #region 获取一个实例
        /// <summary>
        /// 获取一个实例
        /// </summary>
        /// <param name="instName">实例名称：若当前实例名称存在，则返回当前实例；否则，创建一个新实例</param>
        /// <param name="connectionString"></param>
        /// <param name="loginID"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        IDB GetInstance(string instName, string connectionString, string loginID, string password);
        #endregion


        #region 存储一个数据，若数据存在，则更新
        /// <summary>
        /// 存储一个数据，若数据存在，则更新
        /// </summary>
        /// <param name="data"></param>
        void Save(object data);
        #endregion

        #region 移除一个数据
        /// <summary>
        /// 移除一个数据
        /// </summary>
        /// <param name="id"></param>
        void Remove(object id);
        #endregion

        #region 基于Linq的查询
        /// <summary>
        /// 基于Linq的查询
        /// </summary>
        /// <param name="filter">过滤器</param>
        /// <returns></returns>
        List<object> Query(object filter);
        #endregion
    }
}
