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
    /// <summary>
    /// 文档实体 
    /// </summary>
    public class DocItem
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

        public string id { get { return _id.ToString(); } }
        public Guid ID{ get; set; }

        public string ShowMode { get; set; }

        public string Title { get; set; }

        public string Keyword { get; set; }

        public string Content { get; set; }

        public string PlainText { get; set; }

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

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public string Status { get; set; }

        public string CreatorName { get; set; }

        public string CreatorID { get; set; }

        public DateTime PublishTime { get; set; }

        public List<Dictionary<string, object>> ModifyLog { get; set; }

        public bool HasProc { get; set; } //是否处理

        public DateTime ProcTime { get; set; } ///处理时间

        public static DocItem Load(string id)
        {
            if (!string.IsNullOrWhiteSpace(id) && 24 == id.Length)
            {
                var _id = ObjectId.Parse(id);
                var query = "{\"_id\":new ObjectId('" + id + "')}";
                var inst = DocManager.GetInstance().Find(query);

                ///创建关联评论
                {
                    var dbName = "DocService";
                    var collectionName = "CommentItem";
                    var db = DataStorage.GetInstance(DBType.MongoDB);
                    var count = new Random().Next(10, 30);
                    for (int k = 0; k < count; k++)
                    {
                        try
                        {
                            var length = inst.First().Content.Length;
                            var commentLength = new Random().Next(10, 140);
                            var comment = new CommentItem();
                            comment.RootID = id;
                            comment.ParentID = id;
                            comment.LikeCount = new Random(k).Next(1, 1000);
                            comment.Mode = "Text";
                            comment.CreatorName = "创建人" + comment.LikeCount;
                            comment.CreatorID = "ID" + comment.LikeCount;
                            comment.CreateTime = DateTime.Now;
                            comment.Content = inst.First().Content.Substring(length - commentLength, commentLength - 1);
                            db.Save3(dbName, collectionName, comment);
                        }
                        catch (Exception e)
                        {

                        }

                    }

                }


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
