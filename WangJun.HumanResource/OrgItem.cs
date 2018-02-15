using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.HumanResource
{
    public class OrgItem
    {
        public string GroupName { get; set; }

        public string GroupID { get; set; }

        public string Title { get; set; }

        public string Name { get; set; }

        public Guid ID { get; set; }

        public Guid ParentID { get; set; }
    }
}
