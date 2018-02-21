using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using WangJun.DB;
using WangJun.Entity;
using WangJun.Utility;

namespace WangJun.Doc
{
    /// <summary>
    /// 文档实体 
    /// </summary>
    public class DocItem: BaseItem
    {
        public static DocItem Create(string title,string keyword,string summary,string content,DateTime dateTime,string creatorName,string creatorID)
        {
            var inst = new DocItem();
            //inst._id = ObjectId.GenerateNewId();
            inst.Title = title;
            inst.Keyword = keyword;
            inst.Summary = summary;
            inst.Content = content;
            inst.CreateTime = dateTime;
            inst.ContentType = "股票全网新闻";
            inst.CreatorName = creatorName;
            inst.CreatorID = creatorID;
            return inst;
        }




        public static DocItem Create(Dictionary<string,object> data)
        {
            var inst = Convertor.FromDictionaryToObject<DocItem>(data);
            return inst;
        }

        public ObjectId _id { get; set; }

        public  string id { get { return _id.ToString(); } }
        public Guid ID{ get; set; }

        public string ShowMode { get; set; }

        public string Title { get; set; }

        public string Keyword { get; set; }

        public string Content { get; set; }

        public int ContentLength { get; set; }

        public string PlainText { get; set; }

        public int PlainTextLength { get; set; }

        public string Summary { get; set; }

        public string ContentType { get; set; }

        public string CategoryName { get; set; }

        public string CategoryID { get; set; }

        public int ReadCount { get; set; }

        public int LikeCount { get; set; }

        public int CommentCount { get; set; }

        public string ImageUrl { get; set; }

        public List<Dictionary<string,object>> Images { get; set; }

        public List<Dictionary<string, object>> Videos { get; set; }

        public List<Dictionary<string, object>> Votes { get; set; }

        public List<Dictionary<string, object>> Attachments { get; set; }

        public List<Dictionary<string, object>> AppendList { get; set; } ///

        public List<CommentItem> CommentList { get; set; }
 
        public DateTime PublishTime { get; set; }

        

 
             

        public static DocItem Load(string id)
        {
            if (!string.IsNullOrWhiteSpace(id) && 24 == id.Length)
            {
                var _id = ObjectId.Parse(id);
                var query = "{\"_id\":new ObjectId('" + id + "')}";
                var inst = DocManager.GetInstance().Find(query);

                return inst.First();
            }
            return new DocItem();
        }

        public DocItem LoadInst(string id)
        {
            return DocItem.Load(id);
        }

        public void Save()
        {
            var dbName = "DocService";
            var collectionName = "DocItem";
            var db = DataStorage.GetInstance(DBType.MongoDB);
            //var filter = "{\"_id\":ObjectId('"+this._id.ToString()+"')}";
            


            db.Save3(dbName, collectionName, this);
        }

        public void Remove()
        {
            //var dbName = "DocService";
            //var collectionName = "DocItem";
            //var db = DataStorage.GetInstance(DBType.MongoDB);
            //var filter = "{\"_id\":ObjectId('" + this._id.ToString() + "')}";
            //db.Remove(dbName, collectionName, filter);


        }

 
    }
}
