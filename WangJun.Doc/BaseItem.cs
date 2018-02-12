using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.Doc
{
    /// <summary>
    /// 基本类型
    /// </summary>
    public class BaseItem
    {
        public virtual string id { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }
        public DateTime DeleteTime { get; set; }

        public string Status { get; set; }

        public string ClassFullName { get; set; }

        public string CreatorID { get; set; }

        public string CreatorName { get; set; }

        public string ModifierID { get; set; }

        public string ModifierName { get; set; }

        public bool HasProc { get; set; } //是否处理

        public DateTime ProcTime { get; set; } ///处理时间

        public List<Dictionary<string, object>> ModifyLog { get; set; }

        public string _DbName{get;set;}

        public string _CollectionName { get; set; }

        public string _SourceID { get; set; }
    }
}
