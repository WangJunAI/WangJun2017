using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.DB;

namespace WangJun.HumanResource
{
    public class StaffManager
    {
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
    }
}
