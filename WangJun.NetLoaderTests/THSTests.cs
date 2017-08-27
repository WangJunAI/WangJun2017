using Microsoft.VisualStudio.TestTools.UnitTesting;
using WangJun.NetLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.NetLoader.Tests
{
    [TestClass()]
    public class THSTests
    {
        [TestMethod()]
        public void GetALLStockCodeTest()
        {
            var inst = THS.GetInst();
            inst.GetALLStockCode();
        }
    }
}