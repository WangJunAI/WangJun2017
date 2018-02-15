using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.HumanResource
{
    public class StaffWebAPI
    {
        public List<OrgItem> LoadOrgList(string query, string protection = "{}", string sort = "{}", int pageIndex = 0, int pageSize = 50)
        {
            var res = OrgManager.GetInstance().Find(query,  protection,  sort ,  pageIndex,  pageSize);

            return res;
        }

        public List<StaffItem> LoadStaffList(string query, string protection = "{}", string sort = "{}", int pageIndex = 0, int pageSize = 50)
        {
            var res = StaffManager.GetInstance().Find(query, protection, sort, pageIndex, pageSize);

            return res;
        }
    }
    
}
