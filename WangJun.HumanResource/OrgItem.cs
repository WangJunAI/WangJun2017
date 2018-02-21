using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using WangJun.DB;
using WangJun.Entity;
using WangJun.Utility;

namespace WangJun.HumanResource
{
    /// <summary>
    /// 文档实体 
    /// </summary>
    public class OrgItem : BaseItem
    {
        public OrgItem()
        {
            this._DbName = CONST.DB.DBName_HumanResource;
            this._CollectionName = CONST.DB.CollectionName_OrgItem;
            this.GroupName = "组织结构模板类";
            this.BizMode = "组织结构服务";
            this.ClassFullName = this.GetType().FullName;

        }

        public static OrgItem Create(string title,string keyword,string summary,string content,DateTime dateTime,string creatorName,string creatorID)
        {
            var inst = new OrgItem();
            inst._id = ObjectId.GenerateNewId();
            inst.Name = title;
            inst.CreatorName = creatorName;
            inst.CreatorID = creatorID;
            return inst;
        }




        public static OrgItem Create(Dictionary<string,object> data)
        {
            var inst = Convertor.FromDictionaryToObject<OrgItem>(data);
            return inst;
        }
         

        public  string id { get { return _id.ToString(); } }
 

        public int ItemCount { get; set; }

        public int SubCategoryCount { get; set; }

  
        /// <summary>
        /// [OK]
        /// </summary>
        public void Save()
        {
            EntityManager.GetInstance().Save<OrgItem>(this);
        }

        public static void Save(string jsonInput)
        {
            var dict = Convertor.FromJsonToDict2(jsonInput);
             var inst = new OrgItem();
            if (dict.ContainsKey("ID") && null != dict["ID"])
            {
                inst.ID = dict["ID"].ToString();
            }
            inst = EntityManager.GetInstance().Get<OrgItem>(inst);
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
