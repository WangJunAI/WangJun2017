using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.Doc
{
    public class RecycleBinItem
    {
        public ObjectId _id { get; set; }

        public string id { get; set; }

        public string Title { get; set; }

        public DateTime DeleteTime { get; set; }

        public string Type { get; set; }
         
        public string CollectionName {get; set;  }
    }
}
