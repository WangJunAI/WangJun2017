using Microsoft.VisualStudio.TestTools.UnitTesting;
using WangJun.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WangJun.Net.Tests
{
    [TestClass()]
    public class FTPTests
    {
        [TestMethod()]
        public void UploadFileTest()
        {
            var inst = FTP.CreateInstance("qxw1146630116", "75737573");
            inst.UploadFile("ftp://qxw1146630116.my3w.com//6666/9999/test.doc", @"F:\test.doc");
           // inst.CreateFolder();
        }
    }
}