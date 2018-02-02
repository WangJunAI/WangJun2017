using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.DB;

namespace WangJun.Doc
{
    /// <summary>
    /// 文档分析器
    /// </summary>
    public class DataAnalysor
    {
        /// <summary>
        /// 添加分析任务 - 分词服务,最热阅读数,
        /// </summary>
        public void AddAnalysisTask()
        {

        }

        public void ProcTask()
        {

        }

        protected void  SaveResult()
        {

        }

        #region 分词服务-分类
        protected void ClassifyFenCi()
        {
            ///分词classification and clustering
            ///更改结果
        }
        #endregion

        #region 聚类服务-
        protected void ClusterFenCi()
        {
            ///分词classification and clustering
            ///更改结果
        }
        #endregion

        public List<Dictionary<string,object>> GetHotWords()
        {

            var dbName = CONST.DB.DBName_DocService;
            var collectionName = CONST.DB.CollectionName_FenCi;
            var db = DataStorage.GetInstance(DBType.MongoDB);
            var query ="{}";
            var sort = "{'Count':-1}";
            var res = db.Find3(dbName, collectionName, query, sort, "{}", 0, 20);
            return res;
        }
    }
}
