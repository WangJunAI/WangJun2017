using System;
using System.Text;
using System.Threading;
using WangJun.Net;
using WangJun.Utility;

namespace WangJun.NetLoader
{
    /// <summary>
    /// NodeJS Service 
    /// </summary>
    public static class NodeService
    {
        public static object Get(string url, string cmd, string method, object args)
        {
            var retryCount = 5;

            while (0 < retryCount)
            {
                retryCount--;
                LOGGER.Log(string.Format("调用NodeService 还剩余{0}次", retryCount));
                var json = string.Empty;
                var context = new
                {
                    CMD = cmd,
                    Method = method,
                    Args = args
                };
                try
                {
                    json = Convertor.FromObjectToJson(context);
                    var httpdownloader = new HTTP();
                    var resString = httpdownloader.Post(url, Encoding.UTF8, json);
                    var res = Convertor.FromJsonToDict2(resString)["RES"];
                    return res;
                }
                catch (Exception e)
                {
                    LOGGER.Log(string.Format("调用NodeService的时候,发生异常：{0} {1}", e.Message, DateTime.Now));
                    LOGGER.Log(string.Format("参数：", args));
                    //var db = DataStorage.GetInstance(DBType.MongoDB);
                    //db.Save(new { Args = Convertor.FromObjectToDictionary(args), CreateTime = DateTime.Now, Message = e.Message }, "Exception", "StockService");
                    LOGGER.Beep();
                    Thread.Sleep(new TimeSpan(0, 5, 0));
                }
            }
            return null;
        }
    }
}
