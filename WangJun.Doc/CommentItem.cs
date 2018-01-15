using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.Doc
{
    public class CommentItem
    {

        public string RootID { get; set; }

        public string ParentID { get; set; }
        public string Mode { get; set; }///Form,Text

        public string Content { get; set; }

        public int LikeCount { get; set; }

        public DateTime CreateTime { get; set; }

        public string Status { get; set; }

        public string CreatorName { get; set; }

        public string CreatorID { get; set; }

        public string CreatorPic { get; set; }



    }
}
