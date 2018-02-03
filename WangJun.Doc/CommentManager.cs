using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.Data;
using WangJun.DB;
using WangJun.HumanResource;

namespace WangJun.Doc
{
    public class CommentManager
    {
        public SESSION CurrentUser
        {
            get
            {
                return SESSION.Current;
            }
        }

        public static CommentManager GetInstance()
        {
            var inst = new CommentManager();
            return inst;
        }

        public int Add(string content,string targetId,string mode)
        {
            var inst = new CommentItem();
            inst.RootID = targetId;
            inst.Content = content;
            inst.CreatorID = this.CurrentUser.UserID;
            inst.CreatorName = this.CurrentUser.UserName;
            inst.Mode = mode;
            inst.Save();
            return 0;
        }

        /// <summary>
        /// 根据
        /// </summary>
        /// <param name="query"></param>
        /// <param name="protection"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>

        public List<CommentItem> Find(string query, string protection = "{}",string sort="{}", int pageIndex = 0,int pageSize=50)
        {
            var list = new List<CommentItem>();
            var dbName = CONST.DB.DBName_DocService;
            var collectionName = CONST.DB.CollectionName_CommentItem;
            if (!string.IsNullOrWhiteSpace(query))
            {
                var mongo = DataStorage.GetInstance(DBType.MongoDB);
                
                var resList = mongo.Find3(dbName, collectionName, query,protection,sort,pageIndex,pageSize);

                list = Convertor.FromDictionaryToObject<CommentItem>(resList);
            }

            return list;
        }
    }
}
