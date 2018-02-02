using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.DB
{
    public class DBItem
    {
        public string Host { get; set; }

        public string DatabaseName { get; set; }

        public string TableName { get; set; }

        public string ID { get; set; }

        public static DBItem Create(string dbName,string tableName,string id)
        {
            var inst = new DBItem();
            inst.DatabaseName = dbName;
            inst.TableName = tableName;
            inst.ID = id;
            return inst;
        }
    }
}
