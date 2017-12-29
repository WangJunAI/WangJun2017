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
        public ObjectId _id { get; set; }
        public Guid ID{ get; set; }

        public string ShowMode { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

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
            var filter = "{\"_id\":ObjectId('"+this._id.ToString()+"')}";
            db.Save3(dbName, collectionName, this, filter);
        }

        public void Remove()
        {
            var dbName = "DocService";
            var collectionName = "DocItem";
            var db = DataStorage.GetInstance(DBType.MongoDB);
            var filter = "{\"_id\":ObjectId('" + this._id.ToString() + "')}";
            db.Remove(dbName, collectionName, filter);


        }

 
    }
}
