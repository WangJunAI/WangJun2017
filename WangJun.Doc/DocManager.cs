using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.Data;
using WangJun.DB;

namespace WangJun.Doc
{
    public class DocManager
    {

        public static DocManager GetInstance()
        {
            var inst = new DocManager();
            return inst;
        }

        public int Save(string title, string content, string categoryID, string publishTime,string status,string id,string plainText)
        {
            var inst = new DocItem();
            //inst._id = ObjectId.GenerateNewId();
            if (24 == id.Length && "000000000000000000000000" != id)
            {
                inst._id = ObjectId.Parse(id);
            }
            else
            {
                inst._id = ObjectId.GenerateNewId();
            }
            inst.Title = title;
            inst.Keyword = "暂空";
            inst.Summary = "暂空";
            inst.Content = content;
            inst.CategoryID = categoryID;
            inst.CategoryName = CategoryManager.GetInstance().Get(categoryID).Name;
            inst.CreateTime = DateTime.Now;
            inst.ContentType = "测试";
            inst.CreatorName = "测试员";
            inst.PublishTime = DateTime.Parse(publishTime);
            inst.Status = status;
            inst.PlainText = plainText;
            inst.Save();
            return 0;
        }

        public DocItem Get(string id)
        {
            var inst = DocItem.Load(id);
            return inst;
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
               
            }

            return 0;

        }
    }
}
