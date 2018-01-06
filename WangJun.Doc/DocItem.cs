using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.DB;

namespace WangJun.Doc
{
    /// <summary>
    /// 文档实体 
    /// </summary>
    public class DocItem
    {
        public static DocItem Create(string title,string keyword,string summary,string content,DateTime dateTime)
        {
            var inst = new DocItem();
            //inst._id = ObjectId.GenerateNewId();
            inst.Title = title;
            inst.Keyword = keyword;
            inst.Summary = summary;
            inst.Content = content;
            inst.CreateTime = dateTime;
            inst.ContentType = "股票全网新闻";
            return inst;
        }
        //public ObjectId _id { get; set; }
        public Guid ID{ get; set; }

        public string ShowMode { get; set; }

        public string Title { get; set; }

        public string Keyword { get; set; }

        public string Content { get; set; }

        public string Summary { get; set; }

        public string ContentType { get; set; }

        public List<Dictionary<string,object>> Images { get; set; }

        public List<Dictionary<string, object>> Videos { get; set; }

        public List<Dictionary<string, object>> Votes { get; set; }

        public List<Dictionary<string, object>> Attachments { get; set; }

        public List<Dictionary<string, object>> AppendList { get; set; } ///

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public string Status { get; set; }

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
