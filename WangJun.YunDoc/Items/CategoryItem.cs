using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using WangJun.Config;
using WangJun.DB;
using WangJun.Entity;
using WangJun.Utility;

namespace WangJun.YunDoc
{
    /// <summary>
    /// 云笔记目录 
    /// </summary>
    public class CategoryItem:BaseItem
    {
        public CategoryItem()
        {
            this._DbName = CONST.APP.YunDoc.DB;
            this._CollectionName = CONST.APP.YunDoc.TableCategory;
            this.ClassFullName = this.GetType().FullName;
            this.Version = 1;
            this.AppCode = CONST.APP.YunDoc.Code;
            this.AppName = CONST.APP.YunDoc.Name;
            this.StatusCode = CONST.APP.YunDoc.Status.正常;
            this.Status = CONST.APP.YunDoc.Status.GetString(this.StatusCode);
        }
  

 
        public int ItemCount { get; set; }

        public int SubCategoryCount { get; set; }

  
        /// <summary>
        /// [OK]
        /// </summary>
        public void Save()
        {
            EntityManager.GetInstance().Save<CategoryItem>(this);
            ClientBehaviorItem.Save(this, ClientBehaviorItem.BehaviorType.修改, SESSION.Current);
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
                var property = inst.GetType().GetProperty(kv.Key);
                if (property.CanWrite)
                {
                    property.SetValue(inst, kv.Value);
                }
            }
            inst.Save();
        }
        public void Remove()
        {
            EntityManager.GetInstance().Remove(this);

        }

 
    }
}
