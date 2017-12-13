using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.Data;
using WangJun.Net;

namespace WangJun.NetLoader
{
    /// <summary>
    /// NodeJS Service 
    /// </summary>
    public static class NodeService
    {
        public static object Get(string url, string cmd, string method, object args)
        {
            var context = new
            {
                CMD = cmd,
                Method = method,
                Args = args
            };
            var httpdownloader = new HTTP();
            var resString = httpdownloader.Post(url, Encoding.UTF8, Convertor.FromObjectToJson(context));
            var res = Convertor.FromJsonToDict2(resString)["RES"];
            return res;
        }
    }
}
