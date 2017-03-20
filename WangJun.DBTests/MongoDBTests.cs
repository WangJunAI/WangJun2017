using Microsoft.VisualStudio.TestTools.UnitTesting;
using WangJun.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WangJun.DB.Tests
{
    [TestClass()]
    public class MongoDBTests
    {
        [TestMethod()]
        public void SaveTest()
        {
            var inst = MongoDB.GetInst("");
            inst.Save("");
        }
    }
}