using Microsoft.VisualStudio.TestTools.UnitTesting;
using WangJun.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.DB.Tests
{
    [TestClass()]
    public class MySQLTests
    {
        [TestMethod()]
        public void SaveTest()
        {
            MySQL.Register("140", @"server=192.168.0.140;user=root;database=WJBigData;port=3306;password=111qqq!!!");
            var inst = MySQL.GetInstance("140");
            inst.Save("INSERT INTO Page (Page) VALUES ('hhh')");
        }
    }
}