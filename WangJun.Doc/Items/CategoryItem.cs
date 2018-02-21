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
    public class CategoryItem:BaseItem
    {
        public CategoryItem()
        {
            this._DbName = CONST.DB.DBName_DocService;
            this._CollectionName = CONST.DB.CollectionName_CategoryItem;
            this.GroupName = "文档模板类";
            this.BizMode = "目录服务";
            this.ClassFullName = this.GetType().FullName;

        }
  

        public  string id { get { return _id.ToString(); } }
 

        public int ItemCount { get; set; }

        public int SubCategoryCount { get; set; }

 
 

        public static CategoryItem Load(string id)
        {
            var _id = ObjectId.Parse(id);
            var query = CONST.DB.MongoDBFilterCreator_ByObjectId(id);
            var inst = CategoryManager.GetInstance().Find(query);
            
            return inst.First() ;
        }
         
        /// <summary>
        /// [OK]
        /// </summary>
        public void Save()
        {
            EntityManager.GetInstance().Save<CategoryItem>(this);
        }
        public static void Save(string jsonInput)
        {
            var dict = Convertor.FromJsonToDict2(jsonInput);
             var inst = new CategoryItem();
            if(dict.ContainsKey("ID") && null !=dict["ID"])
            {
                inst.ID = dict["ID"].ToString();
            }
            inst = EntityManager.GetInstance().Get<CategoryItem>(inst);
            foreach (var kv in dict)
            {
                inst.GetType().GetProperty(kv.Key).SetValue(inst, kv.Value);
            }
            inst.Save();
        }
        public void Remove()
        {
            EntityManager.GetInstance().Remove(this);

        }

 
    }
}
