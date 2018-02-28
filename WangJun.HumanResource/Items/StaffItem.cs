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
             this.ClassFullName = this.GetType().FullName;
            this.Version = 1;
            this.AppCode = Entity.CONST.APP.Staff;
            this.AppName = Entity.CONST.APP.GetString(this.AppCode);
            this.Status = CONST.Status.Incumbency;
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
                else if (null != kv.Value && typeof(string) == kv.Value.GetType())
                {
                    inst.GetType().GetProperty(kv.Key).SetValue(inst, kv.Value.ToString().Trim());
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
