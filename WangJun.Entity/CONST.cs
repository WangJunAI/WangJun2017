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
            public static string Normal  { get { return "已发布"; } }

            public static string Deleted { get { return "已删除"; } }
        }

 

        public static class APP
        {
            public static string GetString(int code)
            {
                if (code == APP.Staff)
                {
                    return "汪俊人员和组织管理";
                }
                else if (code == APP.YunNote)
                {
                    return "汪俊云笔记管理";
                }
                else if (code == APP.YunPan)
                {
                    return "汪俊云盘管理";
                }
                else if (code == APP.YunProject)
                {
                    return "汪俊云项目管理";
                }
                return "未定义";
            }

            public static int Staff = 10000000;
            public static int YunNote = 20000000;
            public static int YunProject = 30000000;
            public static int YunPan = 40000000;
        }





    }
}
