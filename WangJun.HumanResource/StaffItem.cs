using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.DB;

namespace WangJun.HumanResource
{
    public class StaffItem
    {
        public string Name { get; set; }

        public string Sex { get; set; }

        public string StaffID { get; set; }
        public string Email { get; set; }

        public string QQ { get; set; }

        public string Phone { get; set; }

        public string DepartmentID { get; set; }

        public string PositionD { get; set; }

        public string RoleID { get; set; }
        public string AreaID { get; set; }
        public string EntryTime { get; set; }
        public string DepartureTime { get; set; }
        public string Attachment { get; set; }

        public string ID { get; set; }

        public string OrgName { get; set; }

        public void Save()
        {
            var db = DataStorage.GetInstance(DBType.MongoDB);
            db.Save3(CONST.DB.DBName_HumanResource, CONST.DB.CollectionName_StaffItem, this);
        }

    }
}
