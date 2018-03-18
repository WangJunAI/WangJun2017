using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WangJun.Config;
using WangJun.DB;
using WangJun.Entity;
using WangJun.Utility;

namespace WangJun.HumanResource
{
    public class StaffItem:BaseItem
    {
        public StaffItem()
        {
            this._DbName = CONST.APP.OrgStaff.DB;
            this._CollectionName = CONST.APP.OrgStaff.TableStaff;
            this.ClassFullName = this.GetType().FullName;
            this.Version = 1;
            this.AppCode = CONST.APP.OrgStaff.Code;
            this.AppName = CONST.APP.OrgStaff.Name;
            this.StatusCode = CONST.APP.OrgStaff.StaffStatus.在职;
            this.Status = CONST.APP.OrgStaff.StaffStatus.GetString(this.StatusCode);
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

        public string Password { get; set; }


        public string AreaID { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime DepartureTime { get; set; }
        public string Attachment { get; set; }
 
        public bool IsAdmin { get; set; }

        public bool IsSuperAdmin { get { return 16 == this.Level; } }




        public static StaffItem Load(string id)
        {
            var inst = new StaffItem();
            inst.ID = id;
            inst = EntityManager.GetInstance().Get<StaffItem>(inst);
            return inst;
        }


        /// <summary>
        /// [OK]
        /// </summary>
        public void Save()
        {
            EntityManager.GetInstance().Save<StaffItem>(this);
            ClientBehaviorItem.Save(this, ClientBehaviorItem.BehaviorType.修改, SESSION.Current);
        }
        public static void Save(string jsonInput)
        {
            var dict = Convertor.FromJsonToDict2(jsonInput);
            var inst = new StaffItem();
            var isNew = true;
            if (dict.ContainsKey("ID") && null != dict["ID"])
            {
                inst.ID = dict["ID"].ToString();
                isNew = false;
            }
            inst = EntityManager.GetInstance().Get<StaffItem>(inst);
            foreach (var kv in dict)
            {
                var property = inst.GetType().GetProperty(kv.Key);
                if (property.CanWrite)
                {
                    if (typeof(DateTime) == property.PropertyType)
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
            }
            inst.Save();

            ///激活
            if (isNew)
            {
                new WebClient().DownloadString(string.Format("http://localhost:9990/API.ashx?_SID={0}&c=WangJun.Admin.AdminWebAPI&m=ActiveStaff&p0={0}", inst.ID));
            }
        }
 

    }
}
