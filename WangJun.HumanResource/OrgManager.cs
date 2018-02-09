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
    public class OrgManager
    {
        public static OrgManager GetInstance()
        {
            var inst = new OrgManager();
            return inst;

        }

        public void Import()
        {
            var json = File.ReadAllText(@"F:\[2016]数据备份\组织.txt", Encoding.UTF8);
            var arr = Convertor.FromJsonToObject<ArrayList>(json);
            var db = DataStorage.GetInstance(DBType.MongoDB);
            foreach (Dictionary<string,object> item in arr)
            {
                var svItem = new {
                    GroupName="奇瑞控股",
                    GroupID= "奇瑞控股",
                    Title=item["title"],
                    Name=item["name"],
                    ID=Guid.Parse(item["id"].ToString()),
                    ParentID = Guid.Parse(item["pId"].ToString()),
                    CreateTime=DateTime.Now
                };
                db.Save3("HumanResource", "OrgItem", svItem);

            }
        }


    }
}
