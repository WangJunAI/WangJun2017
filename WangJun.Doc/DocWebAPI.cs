using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.HumanResource;

namespace WangJun.Doc
{
    /// <summary>
    /// 
    /// </summary>
    public class DocWebAPI
    {
        /// <summary>
        /// 保存一个文章
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="categoryID"></param>
        /// <param name="publishTime"></param>
        /// <param name="status"></param>
        /// <param name="id"></param>
        /// <param name="plainText"></param>
        /// <param name="thumbnailSrc"></param>
        /// <returns></returns>
        public int SaveDoc(string title, string content, string categoryID, string publishTime, string status, string id, string plainText, string thumbnailSrc)
        {
            var res = DocManager.GetInstance().Save(title,   content,   categoryID,   publishTime,   status,   id,   plainText,   thumbnailSrc);
            return res;
        }

        /// <summary>
        /// 获取一份文档
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DocItem GetDoc(string id)
        {
            var inst = DocItem.Load(id);
            try
            {
                var currentUser = SESSION.Current;

                DocManager.GetInstance().UpdateValue(id, CONST.DB.MongoDBFilterCreator_ByInc("ReadCount", 1));
                ClientBehaviorManager.Add(CONST.DB.DBName_DocService, CONST.DB.CollectionName_DocItem, id, CONST.ClientBehavior.Read, currentUser.UserID, currentUser.UserName);
            }
            catch
            {

            }
            return inst;
        }

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

        /// <summary>
        /// 获取一个目录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CategoryItem GetCategory(string id)
        {
            var inst = CategoryManager.GetInstance().Get(id);
            return inst;
        }

        /// <summary>
        /// 保存一个目录
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parentId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public int SaveCategory(string name, string parentId, string id)
        {
            var res = CategoryManager.GetInstance().Save(name, parentId, id);
            return res;
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
        public List<CategoryItem> LoadCategory(string query, string protection = "{}",string sort="{}", int pageIndex = 0, int pageSize = 50)
        {
            var res = CategoryManager.GetInstance().Find(query, protection,sort, pageIndex, pageSize);
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

        /// <summary>
        /// 删除一个目录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int RemoveCategory(string id)
        {
            var res = CategoryManager.GetInstance().Remove(id);
            return res;
        }

        public object DocCount(string query)
        {
            var res = DocManager.GetInstance().Count(query);
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
