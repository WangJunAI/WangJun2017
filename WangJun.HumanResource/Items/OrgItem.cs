using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using WangJun.Config;
using WangJun.DB;
using WangJun.Entity;
using WangJun.Utility;

namespace WangJun.HumanResource
{
    /// <summary>
    /// 组织实体 
    /// </summary>
    public class OrgItem : BaseItem
    {
        public OrgItem()
        {
            this._DbName = CONST.APP.OrgStaff.DB;
            this._CollectionName = CONST.APP.OrgStaff.TableOrg;
            this.ClassFullName = this.GetType().FullName;
            this.Version = 1;
            this.AppCode = CONST.APP.OrgStaff.Code;
            this.AppName = CONST.APP.OrgStaff.Name;
            this.StatusCode = CONST.APP.OrgStaff.OrgStatus.正常;
            this.Status = CONST.APP.OrgStaff.OrgStatus.GetString(this.StatusCode);

        } 
        public int ItemCount { get; set; }

        public int SubCategoryCount { get; set; }


        #region 实体保存
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
                var property = inst.GetType().GetProperty(kv.Key);

                if (typeof(DateTime) == property.PropertyType)
                {
                    property.SetValue(inst, DateTime.Parse(kv.Value.ToString()));
                }
                else if (null != kv.Value && typeof(string) == kv.Value.GetType())
                {
                    property.SetValue(inst, kv.Value.ToString().Trim());
                }
                else
                {
                    property.SetValue(inst, kv.Value);
                }

            }
            inst.Save();
        }
        #endregion

        #region 实体移除
        public void Remove()
        {
            EntityManager.GetInstance().Remove(this);

        }
        #endregion


    }
}
