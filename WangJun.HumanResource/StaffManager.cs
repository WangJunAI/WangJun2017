using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.DB;
using WangJun.Utility;

namespace WangJun.HumanResource
{
    public class StaffManager
    {
        public static StaffManager GetInstance()
        {
            var inst = new StaffManager();
            return inst;

        }
        public void In()
        {
            var lines = File.ReadAllLines(@"E:\a.txt", Encoding.UTF8);
            var db = DataStorage.GetInstance(DBType.MongoDB);
            foreach (var item in lines)
            {
                var arr = item.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if(2 == arr.Length)
                {
                    db.Save3("Human", "Staff", new { ID = arr[0].Trim(), RealName = arr[1].Trim(),Password="1" });
                }
                else
                {
                    var o = 0;
                }
            }
        }

        public void Import()
        {
            var json = File.ReadAllText(@"F:\[2016]数据备份\人员.txt", Encoding.UTF8);
            var arr = Convertor.FromJsonToObject<ArrayList>(json);
            var db = DataStorage.GetInstance(DBType.MongoDB);
            foreach (Dictionary<string, object> item in arr)
            {
                var user = item["Item1"] as Dictionary<string, object>;
                var svItem = new
                {
                    PassportID = Guid.Parse(user["PassportID"].ToString()),
                    OrgID = Guid.Parse(user["OrgID"].ToString()),
                    OrgName = user["OrgName"],
                    Postion = user["Postion"],
                    Type = user["Type"],
                    CompanyName = user["CompanyName"],
                    Passport = user["Passport"],
                    Name = user["Name"],
                    NamePinyin = user["NamePinyin"],
                    EmployeeNo = user["EmployeeNo"],
                    IDCard = user["IDCard"],
                    Division = user["Division"],
                    EntryTime = user["EntryTime"],
                    Tel = user["Tel"],
                    Email = user["Email"],
                    MPhone = user["MPhone"],
                    LoginTime = user["LoginTime"],
                    OnlineMinutes = user["OnlineMinutes"],
                    LoginIP = user["LoginIP"],
                    LoginCount = user["LoginCount"],
                    LogoutUrl = user["LogoutUrl"],
                    LogoutTime = user["LogoutTime"],
                    LogoutIP = user["LogoutIP"],
                    HeadImage = user["HeadImage"],
                    iCalendarQRCode = user["iCalendarQRCode"],
                    LevelName = user["LevelName"],
                    ID = user["ID"],
                    AccountID = user["AccountID"],
                    LastUpdateTime = user["LastUpdateTime"],
                    CreateTime = user["CreateTime"],
                    GroupName = "奇瑞控股",
                    GroupID = "奇瑞控股",
                };
                db.Save3("HumanResource", "StaffItem", svItem);

            }
        }
    }
}
