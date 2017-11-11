using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.Data;
using WangJun.Tools;
using M = MongoDB;

namespace WangJun.DB
{
    /// <summary>
    /// 数据库转换器
    /// </summary>
    public static class DBConvertor
    {
        #region 从MongoDB转换到SQLServer
        /// <summary>
        /// 从MongoDB转换到SQLServer
        /// </summary>
        public static void FromMongoDBToSQLServer(string mongodbConnectionString,string mongodbDBName , string mongodbCollectionName, string sqlserverConnectionString, string sqlserverDBName, string sqlserverCollectionName)
        {
            #region SQLServer 注册
            SQLServer.Register("140", @"Data Source=192.168.0.140\sql2016;Initial Catalog=WJStock;Persist Security Info=True;User ID=sa;Password=111qqq!!!");
            var mssql = SQLServer.GetInstance("140");
            #endregion

            #region   MongoDB注册
            MongoDB mongo = MongoDB.GetInst("mongodb://192.168.0.140:27017");
            #endregion
            var count = 0;
            mongo.EventTraverse += (object sender, EventArgs e)=>
            {
                var ee = e as EventProcEventArgs;
                var dict = ee.Default as Dictionary<string, object>;
                var list = dict["Data"] as Array;

                if (list is Array)
                {
                    var itemArrayIndex = 0;
                    string sql = "INSERT INTO  [DataDaDan2D]  ([dbItemID] ,[StockCode] ,[StockName] ,[TradingDate] ,[Price] ,[Volume] ,[PrevPrice] ,[Kind],[ItemArrayIndex])  VALUES (@dbItemID ,@StockCode ,@StockName ,@TradingDate ,@Price ,@Volume  ,@PrevPrice  ,@Kind,@ItemArrayIndex)";

                    foreach (var item in list)
                    {
                        var srcItem = item as Dictionary<string, object>;
                        var svItem = new Dictionary<string, object>();
                        svItem["dbItemID"] = dict["_id"].ToString();
                        svItem["StockCode"] = srcItem["symbol"].ToString().Substring(2);
                        svItem["StockName"] = srcItem["name"];
                        svItem["TradingDate"] = DBConvertor.CalTradingDate((DateTime)dict["CreateTime"], srcItem["ticktime"].ToString());
                        svItem["Price"] = float.Parse(srcItem["price"].ToString());
                        svItem["Volume"] = int.Parse(srcItem["volume"].ToString());
                        svItem["PrevPrice"] = float.Parse(srcItem["prev_price"].ToString());
                        svItem["Kind"] = srcItem["kind"];
                        svItem["ItemArrayIndex"] = itemArrayIndex++;
                        svItem["Source"] = "SINA";

                        mongo.Save("DataSource2D", "DaDan2D", svItem);

                        //var paramList = new List<KeyValuePair<string, object>>();
                        //paramList.Add(new KeyValuePair<string, object>("@dbItemID", svItem["dbItemID"]));
                        //paramList.Add(new KeyValuePair<string, object>("@StockCode", svItem["StockCode"]));
                        //paramList.Add(new KeyValuePair<string, object>("@StockName", svItem["StockName"]));
                        //paramList.Add(new KeyValuePair<string, object>("@TradingDate", svItem["TradingDate"]));
                        //paramList.Add(new KeyValuePair<string, object>("@Price", svItem["Price"]));
                        //paramList.Add(new KeyValuePair<string, object>("@Volume", svItem["Volume"]));
                        //paramList.Add(new KeyValuePair<string, object>("@PrevPrice", svItem["PrevPrice"]));
                        //paramList.Add(new KeyValuePair<string, object>("@Kind", svItem["Kind"]));
                        //paramList.Add(new KeyValuePair<string, object>("@ItemArrayIndex", svItem["ItemArrayIndex"]));

                        //mssql.Save(sql, paramList);
                    }


                    Console.WriteLine("成功插入" + dict["_id"] + " " + (++count));
                }

            };
            mongo.Find("DataSource", "DataDaDan", "{}");
        }

        public static DateTime CalTradingDate(DateTime createTime , string ticktime)
        {
            if(createTime.DayOfWeek== DayOfWeek.Saturday)
            {
                ///若是周六,收盘时间为周五
                createTime = createTime.AddDays(-1).Date;
            }
            else if(createTime.DayOfWeek == DayOfWeek.Sunday)
            {
                ///若是周日,收盘时间为周五
                createTime = createTime.AddDays(-2).Date;
            }
            else if(createTime.Hour<=15 && createTime.DayOfWeek == DayOfWeek.Monday)
            {
                ///周一收盘前,应该是周五
                createTime = createTime.AddDays(-3).Date;
            }
            else if (createTime.Hour <= 15)
            {
                ///其余日期收盘前,应该是前一天的
                createTime = createTime.AddDays(-1).Date;
            }
            else
            {
                createTime = createTime.Date;
            }
            createTime = DateTime.Parse(string.Format("{0}/{1}/{2} {3}", createTime.Year, createTime.Month, createTime.Day, ticktime));
            return createTime;
        }
         
        #endregion
    }
}
