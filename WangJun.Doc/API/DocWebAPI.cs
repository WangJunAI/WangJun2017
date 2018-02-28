using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.Entity;
using WangJun.HumanResource;
using WangJun.Utility;

namespace WangJun.Doc
{
    /// <summary>
    /// 
    /// </summary>
    public class DocWebAPI
    {
        #region 目录操作
        /// <summary>
        /// 保存一个目录
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parentId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public int SaveCategory(string jsonInput)
        {
            CategoryItem.Save(jsonInput);
            return 0;
        }

        /// <summary>
        /// 加载目录
        /// </summary>
        /// <param name="query"></param>
        /// <param name="protection"></param>
        /// <param name="sort"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<CategoryItem> LoadCategoryList(string query, string protection = "{}", string sort = "{}", int pageIndex = 0, int pageSize = 50)
        {
            var res = EntityManager.GetInstance().Find<CategoryItem>(CONST.DB.DBName_DocService, CONST.DB.CollectionName_CategoryItem, query, protection, sort, pageIndex, pageSize);
            return res;
        }


        /// <summary>
        /// 删除一个目录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int RemoveCategory(string id)
        {
            var inst = new CategoryItem();
            inst.ID = id;
            inst.Remove();
            return 0;
        }

        public CategoryItem GetCategory(string id)
        {
            var inst = new CategoryItem();
            inst.ID = id;
            inst = EntityManager.GetInstance().Get<CategoryItem>(inst);
            return inst;
        }
        #endregion

        #region 文档操作
        /// <summary>
        /// 保存一个目录
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parentId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public int SaveEntity(string jsonInput)
        {
            DocItem.Save(jsonInput);
            return 0;
        }

        /// <summary>
        /// 加载目录
        /// </summary>
        /// <param name="query"></param>
        /// <param name="protection"></param>
        /// <param name="sort"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<DocItem> LoadEntityList(string query, string protection = "{}", string sort = "{}", int pageIndex = 0, int pageSize = 50)
        {
            var res = EntityManager.GetInstance().Find<DocItem>(CONST.DB.DBName_DocService, CONST.DB.CollectionName_DocItem, query, protection, sort, pageIndex, pageSize);
            return res;
        }


        /// <summary>
        /// 删除一个目录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int RemoveEntity(string id)
        {
            var inst = new DocItem();
            inst.ID = id;
            inst.Remove();
            return 0;
        }

        public DocItem GetEntity(string id)
        {
            var inst = new DocItem();
            inst.ID = id;
            inst = EntityManager.GetInstance().Get<DocItem>(inst);
            return inst;
        }
        #endregion

        #region 统计操作
        /// <summary>
        /// 统计操作
        /// </summary>
        /// <returns></returns>
        public object Count(string json)
        {
            var item = new DocItem();
            var match = "{$match:{}}";
            var group = "{$group:{_id:'DocItem总数',Count:{$sum:1}}}";
            var res = EntityManager.GetInstance().Aggregate(item._DbName, item._CollectionName, match, group);
            return (res as List<Dictionary<string,object>>)[0];
        }
        #endregion



        ///// <summary>
        ///// 移除一份文档
        ///// </summary>
        ///// <param name="query"></param>
        ///// <returns></returns>
        //public object RemoveDoc(string query)
        //{
        //    var res = DocManager.GetInstance().Remove(query);
        //    return res;
        //}

        public object MoveToRecycleBin(string dbName, string collectionName, string id)
        {
            var res = new object();
            RecycleBinManager.GetInstance().MoveToRecycleBin(dbName, collectionName, id);
            return res;
        }
 

        public List<CategoryItem> Test()
        {
            var res = CategoryManager.GetInstance().Find("{}", "{}", "{}", 0, 1000);
            return res;
        }
        /// <summary>
        /// 加载文档列表
        /// </summary>
        /// <param name="query"></param>
        /// <param name="protection"></param>
        /// <param name="sort"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<DocItem> LoadDocList(string query, string protection = "{}", string sort = "{}", int pageIndex = 0, int pageSize = 50)
        {
            var res = DocManager.GetInstance().Find(  query,   protection , sort , pageIndex, pageSize);
            return res;
        }



        public object DocCount(string query)
        {
            var res = DocManager.GetInstance().Count(query);
            return res;
        }

        public object RecycleBinCount(string query)
        {
            var res = RecycleBinManager.GetInstance().Count(query);
            return res;
        }

        /// <summary>
        /// 加载回收站
        /// </summary>
        /// <param name="query"></param>
        /// <param name="protection"></param>
        /// <param name="sort"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<RecycleBinItem> LoadRecycleBinItem(string query, string protection = "{}", string sort = "{}", int pageIndex = 0, int pageSize = 50)
        {
            var res = RecycleBinManager.GetInstance().Find( query,  protection,  sort,  pageIndex,  pageSize);
            return res;
        }

        /// <summary>
        /// 文档聚合查询
        /// </summary>
        /// <param name="match"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public object Aggregate(string type ,string match, string group)
        {
            var res = new object();
            if ("Doc" == type)
            {
                res = DocManager.GetInstance().Aggregate(match, group);
            }
            else if("ClientBehavior" == type)
            {
                res = ClientBehaviorManager.GetInstance().Aggregate(match, group);
            }
            return res;
        }

        public List<DocItem> LoadAllDocInSubFolder(string categoryId, string protection = "{}", string sort = "{}", int pageIndex = 0, int pageSize = 50)
        {
            var res = DocManager.GetInstance().LoadAllDocInSubFolder(categoryId, protection, sort, pageIndex, pageSize);
            return res;
        }

        public int AddComment(string content, string targetId, string mode)
        {
            var res = CommentManager.GetInstance().Add(content, targetId, mode);
            return res;
        }

        public List<CommentItem> LoadCommentList(string query, string sort = "{}", string protection = "{}", int pageIndex = 0, int pageSize = 50) {
            var res = CommentManager.GetInstance().Find(query, sort, protection, pageIndex, pageSize);
            return res;
        }


    }
}
