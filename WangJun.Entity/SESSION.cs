using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using WangJun.DB;
using WangJun.Entity;
using WangJun.Utility;

namespace WangJun.Entity
{
    /// <summary>
    /// 全局会话服务
    /// </summary>
    public class SESSION 
    {
        public string ID { get; set; }

        public string LoginID { get; set; }

        public string UserID { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string CompanyID { get; set; }

        public string CompanyName { get; set; }

        public bool IsSuperAdmin { get; set; }

        public DateTime LoginTime { get; set; }

        public DateTime LastUpdateTime { get; set; }
 
 
        public static SESSION Current
        {

            get
            {
                string _SID = HttpContext.Current.Request.QueryString["_SID"];
                var staff = new BaseItem();
                staff.ID = _SID;
                staff._DbName = "WangJun";
                staff._CollectionName = "Staff";
                var res = EntityManager.GetInstance().Get<BaseItem>(staff);
                return new SESSION { UserID = res.ID, UserName = res.Name ,CompanyID=res.CompanyID,CompanyName=res.CompanyName};
            }
        }

        public static SESSION Login(string loginID, string password)
        {
            var inst = new SESSION();
            var query = "{'Email':'"+loginID+"'}";
            var res = EntityManager.GetInstance().Find<BaseItem>("WangJun","Staff",query,"{}","{}");
            if(1 == res.Count)
            {
                inst.UserID = res[0].ID;
                inst.UserName = res[0].Name;
                inst.CompanyID = res[0].CompanyID;
                inst.CompanyName = res[0].CompanyName;
                inst.IsSuperAdmin = (16 == res[0].Level) ? true : false;
            }
            return inst;

        }

        /// <summary>
        /// [OK]
        /// </summary>
        public void Save()
        {
        }
        public static void Save(string jsonInput)
        {

        }
        public void Remove()
        {

        }
    }
}
