using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.Entity
{
    public static class CONST
    {
        public static class DB
        {


            public static string MongoDBFilterCreator_ByObjectId(string id)
            {
                var filter = "{\"_id\":ObjectId('" + id + "')}";
                return filter;
            }

            public static string MongoDBFilterCreator_ByInc(string filedName, int incValue)
            {
                var filter = "{$inc:{'" + filedName + "':" + incValue + "}}";
                return filter;
            }
        }

        public static class Status {
            public static string Normal  { get { return "使用中"; } }

            public static string Deleted { get { return "已删除"; } }
        }

 


    }
}
