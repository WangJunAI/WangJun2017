using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.DB;

namespace WangJun.Stock
{
    public class StockAPI
    {
        public static StockAPI GetInstance()
        {
            var inst = new StockAPI();

            return inst;
        }

        /// <summary>
        /// 获取股票代码列表
        /// </summary>
        /// <returns></returns>
        public object GetStockCodeList()
        {
            var db = DataStorage.GetInstance("aifuwu","sqlserver");
            var sql = "SELECT [_id] ,[ContentType],[StockCode] ,[StockName] ,[CreateTime] FROM [BaseInfo] WHERE ContentType=@ContentType";
            var paramList = new List<KeyValuePair<string, object>>();
            paramList.Add(new KeyValuePair<string, object> ("@ContentType","股票代码"));
            var res = db.Find("qds165298153_db", "BaseInfo", sql, exParam: paramList);
            return res;
        }

        /// <summary>
        /// 获取同花顺新闻正文列表
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public object GetTHSNewsList(int tag)
        {
            var db = DataStorage.GetInstance("aifuwu", "sqlserver");
            var sql = "SELECT [ContentType] ,[Title]  ,[SourceHref] ,[SourceName] ,[NewsCreateTime]   ,[Tag] ,[CreateTime] ,[PageMD5] FROM [News] WHERE Tag=@Tag";
            var paramList = new List<KeyValuePair<string, object>>();
            paramList.Add(new KeyValuePair<string, object>("@Tag",tag));
            var res = db.Find("qds165298153_db", "BaseInfo", sql, exParam: paramList);
            return res;
        }
    }
}
