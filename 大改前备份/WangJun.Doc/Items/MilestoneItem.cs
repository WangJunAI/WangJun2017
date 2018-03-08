using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.Entity;

namespace WangJun.Doc
{
    public class MilestoneItem:BaseItem
    {
        public DateTime ExpectedEndTime { get; set; }
        public DateTime ActualEndTime { get; set; }
         
        public ArrayList TaskArray { get; set; }
    }
}
