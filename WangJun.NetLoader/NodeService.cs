using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WangJun.Data;
using WangJun.DB;
using WangJun.Debug;
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
                LOGGER.Log(string.Format("调用NodeService的时候,发生异常：{0} {1}",e.Message,DateTime.Now));
                LOGGER.Log(string.Format("参数：", json));
                var db = DataStorage.GetInstance(DBType.MongoDB);
                db.Save(new { Context = context, CreateTime = DateTime.Now,Message= e.Message },"Exception","StockService");

                Thread.Sleep(new TimeSpan(1, 0, 0));
            }
            return null;
        }
    }
}
