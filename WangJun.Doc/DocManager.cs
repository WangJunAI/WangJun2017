using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WangJun.DB;
using WangJun.HumanResource;
using WangJun.Tools;
using WangJun.Utility;

namespace WangJun.Doc
{
    public class DocManager
    {

        public static DocManager GetInstance()
        {
            var inst = new DocManager();
            return inst;
        }

        public int Save(string title, string content, string categoryID, string publishTime,string status,string id,string plainText,string thumbnailSrc)
        {
            var session = SESSION.Current;

            var inst = new DocItem();
            var isNew = false;
            if (StringChecker.IsNotEmptyObjectId(id))
            {
                inst._id = ObjectId.Parse(id);
            }
            else
            {
                inst._id = ObjectId.GenerateNewId();
                //inst.ID = Guid.NewGuid();
                inst.CreateTime = DateTime.Now.AddDays(new Random().Next(-100, 100));
                inst.CreatorName = session.UserName;
                inst.CreatorID = session.UserID;
                isNew = true;
             }
            inst.Title = title;
            inst.Keyword = "暂空";
            inst.Summary = "暂空";
            inst.Content = content;
            inst.ContentLength = content.Length;
            //inst.CategoryID = categoryID;
            //inst.CategoryName = CategoryManager.GetInstance().Get(categoryID).Name;
            //inst.ContentType = "文档库模板";
             inst.PublishTime = DateTime.Parse(publishTime);
            inst.Status = status;
            inst.PlainText = plainText;
            inst.PlainTextLength = plainText.Length;
            inst.UpdateTime = DateTime.Now;
            if(StringChecker.IsHttpUrl(thumbnailSrc))
            {
                inst.ImageUrl = thumbnailSrc;
            }
            else
            {
                var pic = (DataSourceBaidu.GetInstance().GetPic(inst.Title)[0] as Dictionary<string, object>)["thumbURL"].ToString() ;
                inst.ImageUrl = pic;
            }

            inst.Save();

            ///添加记录
            //if (isNew)
            //{
            //    ModifyLogItem.LogAsNew(inst.id, CONST.DB.DBName_DocService, CONST.DB.CollectionName_DocItem);
            //}
            //else
            //{
            //    ModifyLogItem.LogAsModify(inst.id, CONST.DB.DBName_DocService, CONST.DB.CollectionName_DocItem);
            //}

            return 0;
        }

        public int SaveAs(string id=null,string title=null,string content=null,string categoryID=null,string status=null,string plainText=null
            ,string publishTime="1990-01-01",string thumbnailSrc=null,string bizMode=null)
        {
            var session = SESSION.Current;

            var inst = new DocItem();
            var isNew = false;
            if (StringChecker.IsNotEmptyObjectId(id))
            {
                inst._id = ObjectId.Parse(id);
            }
            else
            {
                inst._id = ObjectId.GenerateNewId();
                //inst.ID = Guid.NewGuid();
                inst.CreateTime = DateTime.Now.AddDays(new Random().Next(-100, 100));
                inst.CreatorName = session.UserName;
                inst.CreatorID = session.UserID;
                isNew = true;
            }
            inst.Title = title;
            inst.Keyword = "暂空";
            inst.Summary = "暂空";
            inst.Content = content;
            inst.ContentLength = content.Length;
            //inst.CategoryID = categoryID;
            //inst.CategoryName = CategoryManager.GetInstance().Get(categoryID).Name;
            //inst.ContentType = "文档库模板";
            inst.PublishTime = DateTime.Parse(publishTime);
            inst.Status = status;
            inst.PlainText = plainText;
            inst.PlainTextLength = plainText.Length;
            inst.UpdateTime = DateTime.Now;
            inst.BizMode = bizMode;
            if (StringChecker.IsHttpUrl(thumbnailSrc))
            {
                inst.ImageUrl = thumbnailSrc;
            }
            else
            {
                var pic = (DataSourceBaidu.GetInstance().GetPic(inst.Title)[0] as Dictionary<string, object>)["thumbURL"].ToString();
                inst.ImageUrl = pic;
            }

            inst.Save();

            ///添加记录
            //if (isNew)
            //{
            //    ModifyLogItem.LogAsNew(inst.id, CONST.DB.DBName_DocService, CONST.DB.CollectionName_DocItem);
            //}
            //else
            //{
            //    ModifyLogItem.LogAsModify(inst.id, CONST.DB.DBName_DocService, CONST.DB.CollectionName_DocItem);
            //}

            return 0;
        }

        //public DocItem Get(string id)
        //{
        //    var inst = DocItem.Load(id);
        //    try
        //    {
        //        var currentUser = SESSION.Current;

        //        this.UpdateValue(id, "{$inc:{'ReadCount':1}}");
        //        ClientBehaviorManager.Add(CONST.DB.DBName_DocService, CONST.DB.CollectionName_DocItem, id, "阅读", currentUser.UserID, currentUser.UserName);
        //    }
        //    catch
        //    {

        //    }
        //    return inst;
        //}

        public int UpdateStatus(string query, string newStatus)
        {
            var jsonData = Convertor.FromJsonToDict2(newStatus);
            var updateData = new {Status=jsonData["Status"] };
            var dbName = CONST.DB.DBName_DocService;
            var collectionName = CONST.DB.CollectionName_DocItem;
            if (!string.IsNullOrWhiteSpace(query))
            {
                var db = DataStorage.GetInstance(DBType.MongoDB);
                db.Save3(dbName, collectionName, updateData, query, false);
            }
            return 0;
        }

        public int UpdateValue(string id, object updateData)
        { 
            var dbName = CONST.DB.DBName_DocService;
            var collectionName = CONST.DB.CollectionName_DocItem;
            if (!string.IsNullOrWhiteSpace(id))
            {
                var query = "{'_id':ObjectId('" + id + "')}";
                var db = DataStorage.GetInstance(DBType.MongoDB);
                db.Save3(dbName, collectionName, updateData, query, false);
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

        public List<DocItem> Find(string query, string protection = "{}",string sort="{}", int pageIndex = 0,int pageSize=50)
        {
            var list = new List<DocItem>();
            var dbName = CONST.DB.DBName_DocService;
            var collectionName = CONST.DB.CollectionName_DocItem;
            if (!string.IsNullOrWhiteSpace(query))
            {
                var mongo = DataStorage.GetInstance(DBType.MongoDB);
                var resList = mongo.Find3(dbName, collectionName, query,sort,protection,pageIndex,pageSize);

                list = Convertor.FromDictionaryToObject<DocItem>(resList);
            }

            return list;
        }

        public List<DocItem> LoadAllDocInSubFolder(string categoryId, string protection = "{}", string sort = "{}", int pageIndex = 0, int pageSize = 50) {
            var list = RecycleBinManager.GetInstance().FindTarget(CONST.DB.DBName_DocService,CONST.DB.CollectionName_CategoryItem,categoryId);
            var source = list.Where((p) => { return p.TableName == CONST.DB.CollectionName_DocItem; });
            var query = "{'_id':{$in:[idArray]}}";
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in source)
            {
                stringBuilder.AppendFormat("ObjectId('{0}'),",item.ID);
            }
            query = query.Replace("idArray", stringBuilder.ToString());
            var res = this.Find(query, protection, sort, pageIndex, pageSize);
            return res;
        }

        public object Count(string query)
        {
            var res = new object();
            var dbName = CONST.DB.DBName_DocService;
            var collectionName = CONST.DB.CollectionName_DocItem;
            if (!string.IsNullOrWhiteSpace(query))
            {
                var mongo = DataStorage.GetInstance(DBType.MongoDB);
                var count = mongo.Count(dbName, collectionName, query);
                res = new { Count = count };
            }

            return res;
        }

        public object Remove(string query)
        {
            var res = new object();
            var dbName = CONST.DB.DBName_DocService;
            var collectionName = CONST.DB.CollectionName_DocItem;
            if (!string.IsNullOrWhiteSpace(query))
            {
                var mongo = DataStorage.GetInstance(DBType.MongoDB);
               mongo.Remove(dbName, collectionName, query);
                //ModifyLogItem.LogAsRemove(query.Replace("_id", string.Empty).Replace(":", string.Empty).Replace("ObjectId", string.Empty)
                //    .Replace("(", string.Empty).Replace(")", string.Empty).Replace(" ",string.Empty)
                //    , CONST.DB.DBName_DocService, CONST.DB.CollectionName_DocItem);
            }

            return 0;

        }

        public object Aggregate( string match, string group)
        {
            var db = DataStorage.GetInstance(DBType.MongoDB);
            return db.Aggregate(CONST.DB.DBName_DocService, CONST.DB.CollectionName_DocItem,   match,   group);
        }
    }
}
