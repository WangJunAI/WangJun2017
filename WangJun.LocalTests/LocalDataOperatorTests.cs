using Microsoft.VisualStudio.TestTools.UnitTesting;
using WangJun.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WangJun.Data.Tests
{
    [TestClass()]
    public class LocalDataOperatorTests
    {
        [TestMethod()]
        public void TraverseTest()
        {
            var inst = LocalDataOperator.GetInst();
            //inst.Traverse(@"E:\下载");
        }
    }
}