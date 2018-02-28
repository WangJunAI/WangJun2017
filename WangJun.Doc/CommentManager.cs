using System.Collections.Generic;
using WangJun.DB;
using WangJun.Entity;
using WangJun.HumanResource;
using WangJun.Utility;

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
            var currentUser = SESSION.Current;
            var inst = new CommentItem();
            inst.RootID = targetId;
            inst.Content = content;
            inst.CreatorID = currentUser.UserID;
            inst.CreatorName = currentUser.UserName;
            inst.Mode = mode;
            inst.Save();


            if(mode == "LikeCount")
            {
                DocManager.GetInstance().UpdateValue(targetId, "{$inc:{'LikeCount':1}}");
                ClientBehaviorManager.Add(CONST.DB.DBName_DocService, CONST.DB.CollectionName_CommentItem, targetId, "点赞", currentUser.UserID, currentUser.UserName);
            }
            else if(mode == "text")
            {
                DocManager.GetInstance().UpdateValue(targetId, "{$inc:{'CommentCount':1}}");
                ClientBehaviorManager.Add(CONST.DB.DBName_DocService, CONST.DB.CollectionName_CommentItem, targetId, "评论", currentUser.UserID, currentUser.UserName);

            }




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

        public List<CommentItem> Find(string query, string sort = "{}",string protection="{}", int pageIndex = 0,int pageSize=50)
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
