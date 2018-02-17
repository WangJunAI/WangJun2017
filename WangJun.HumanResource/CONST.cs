using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.HumanResource
{
    public static class CONST
    {
        public static class DB
        {
            public static string DBName_HumanResource { get { return "HumanResource"; } }

            public static string CollectionName_OrgItem { get { return "OrgItem"; } }

            public static string CollectionName_StaffItem { get { return "StaffItem"; } }

            public static string CollectionName_RecycleBin { get { return "RecycleBin"; } }


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

        public static class ClientBehavior {
            public static string Read { get { return "阅读"; } }

        }


    }
}
