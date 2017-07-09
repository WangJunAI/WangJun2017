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
    public class MongoDBTests
    {
        [TestMethod()]
        public void FindTest()
        {
            var inst = MongoDB.GetInst("mongodb://192.168.0.140:27017");
            var res = inst.Find("f1", "f2", "{}");
        }
    }
}