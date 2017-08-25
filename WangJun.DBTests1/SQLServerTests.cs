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
    public class SQLServerTests
    {
        [TestMethod()]
        public void SaveTest()
        {
            SQLServer.Register("140", @"Data Source=192.168.0.140\sql2016;Initial Catalog=WJBigData;Persist Security Info=True;User ID=sa;Password=111qqq!!!");
            var inst = SQLServer.GetInstance("140");
            inst.Save("INSERT INTO Page (Page) VALUES ('hhh')");
        }
    }
}