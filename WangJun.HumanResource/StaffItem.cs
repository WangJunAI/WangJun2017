using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.DB;
using WangJun.Entity;
using WangJun.Utility;

namespace WangJun.HumanResource
{
    public class StaffItem:BaseItem
    {
        public StaffItem()
        {
            this._DbName = CONST.DB.DBName_HumanResource;
            this._CollectionName = CONST.DB.CollectionName_StaffItem;
            this.GroupName = "人力资源模板类";
            this.BizMode = "人力资源服务";
            this.ClassFullName = this.GetType().FullName;

        }

        public string Sex { get; set; }

        public string StaffID { get; set; }
        public string Email { get; set; }

        public string QQ { get; set; }

        public string Phone { get; set; } 

        public string PositionID { get; set; }

        public string PositionName { get; set; }

        public string RoleID { get; set; }

        public string RoleName { get; set; }


        public string AreaID { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime DepartureTime { get; set; }
        public string Attachment { get; set; }

        /// <summary>
        /// [OK]
        /// </summary>
        public void Save()
        {
            EntityManager.GetInstance().Save<StaffItem>(this);
        }
        public static void Save(string jsonInput)
        {
            var dict = Convertor.FromJsonToDict2(jsonInput);
            var inst = new StaffItem();
            if (dict.ContainsKey("ID") && null != dict["ID"])
            {
                inst.ID = dict["ID"].ToString();
            }
            inst = EntityManager.GetInstance().Get<StaffItem>(inst);
            foreach (var kv in dict)
            {
                var property = inst.GetType().GetProperty(kv.Key);
                if(typeof(DateTime) == property.PropertyType)
                {
                    property.SetValue(inst, DateTime.Parse(kv.Value.ToString()));

                }
                else
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
