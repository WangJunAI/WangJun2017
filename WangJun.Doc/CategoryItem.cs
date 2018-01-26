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
    public class CategoryItem
    {
        public static CategoryItem Create(string title,string keyword,string summary,string content,DateTime dateTime,string creatorName,string creatorID)
        {
            var inst = new CategoryItem();
            inst._id = ObjectId.GenerateNewId();
            inst.Name = title;
            inst.CreatorName = creatorName;
            inst.CreatorID = creatorID;
            return inst;
        }




        public static CategoryItem Create(Dictionary<string,object> data)
        {
            var inst = Convertor.FromDictionaryToObject<CategoryItem>(data);
            return inst;
        }

        public ObjectId _id { get; set; }

        public string id { get { return _id.ToString(); } }
        public Guid ID{ get; set; }
 
        public string Name { get; set; }
        public string ParentID { get; set; }

        public string ParentName { get; set; }

        public string GroupName { get; set; }

        public string GroupID { get; set; }

        public int ItemCount { get; set; }

        public int SubCategoryCount { get; set; }
         

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public string Status { get; set; }

        public string CreatorName { get; set; }

        public string CreatorID { get; set; }

        public List<Dictionary<string, object>> ModifyLog { get; set; }

        public static CategoryItem Load(string id)
        {
            var _id = ObjectId.Parse(id);
            var query = "{\"_id\":new ObjectId('"+id+"')}";
            var inst = CategoryManager.GetInstance().Find(query);
            
            return inst.First() ;
        }

        public CategoryItem LoadInst(string id)
        {
            return CategoryItem.Load(id);
        }

        public void Save()
        {
            var dbName = "DocService";
            var collectionName = "CategoryItem";
            var db = DataStorage.GetInstance(DBType.MongoDB);
            //var filter = "{\"_id\":ObjectId('"+this._id.ToString()+"')}";
            db.Save3(dbName, collectionName, this);
        }

        public void Remove()
        {
            //var dbName = "DocService";
            //var collectionName = "CategoryItem";
            //var db = DataStorage.GetInstance(DBType.MongoDB);
            //var filter = "{\"_id\":ObjectId('" + this._id.ToString() + "')}";
            //db.Remove(dbName, collectionName, filter);


        }

 
    }
}
