using Microsoft.VisualStudio.TestTools.UnitTesting;
using WangJun.BIZ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.BIZ.Tests
{
    [TestClass()]
    public class THSTests
    {
        [TestMethod()]
        public void DownloadLHBTest()
        {
            var inst = THS.GetInst();
            inst.DownloadLHB();
        }
    }
}