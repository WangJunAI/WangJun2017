using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.DB;

namespace WangJun.Doc
{
    public class CommentItem
    {
        public ObjectId _id { get; set; }
        public string RootID { get; set; }

        public string ParentID { get; set; }
        public string Mode { get; set; }///Form,Text,Like,Append

        public string Content { get; set; }

        public int LikeCount { get; set; }

        public DateTime CreateTime { get; set; }

        public string Status { get; set; }

        public string CreatorName { get; set; }

        public string CreatorID { get; set; }

        public string CreatorPic { get; set; }

        public void Save()
        {
            var task = new TaskFactory().StartNew(() => {
                try
                {
                    this._id = ObjectId.GenerateNewId();
                    this.CreateTime = DateTime.Now.AddDays(new Random().Next(-100, 100));
                    var dbName = CONST.DB.DBName_DocService;
                    var collectionName = CONST.DB.CollectionName_CommentItem;
                    var db = DataStorage.GetInstance(DBType.MongoDB);
                    db.Save3(dbName, collectionName, this);
                }
                catch (Exception e)
                {

                }
            });
        }

    }
}
