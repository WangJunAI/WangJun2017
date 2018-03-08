using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.Entity
{
    public static class MongoDBFilterCreator
    {
        public static string SearchByObjectId(string id)
        {
            var filter = "{\"_id\":ObjectId('" + id + "')}";
            return filter;
        }

        public static string ByInc(string filedName, int incValue)
        {
            var filter = "{$inc:{'" + filedName + "':" + incValue + "}}";
            return filter;
        }
    }
}
