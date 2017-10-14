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

            mongo.EventTraverse += (object sender, EventArgs e)=> {
                var ee = e as EventProcEventArgs;
                var dict = ee.Default as Dictionary<string, object>;
                if ("涨幅在5%以上的股票日线信息" == dict["ContentType"].ToString())
                {
                    StringBuilder sql = new StringBuilder();
                    sql.Append("INSERT INTO [Target1]");
                    sql.Append("([ObjectId],[Date],[Opening],[Max],[Lowest],[Closing],[Volume],[Turnover],[Rate],[ContentType],[ArchorDate],[涨幅],[成交均价],[StockCode],[StockName],[CreateTime],[UpdateTime],[ItemStatus])");
                    sql.Append("VALUES(@ObjectId, @Date, @Opening, @Max, @Lowest, @Closing, @Volume, @Turnover, @Rate, @ContentType, @ArchorDate, @涨幅, @成交均价, @StockCode, @StockName, @CreateTime, @UpdateTime, @ItemStatus)");

                    var paramList = new List<KeyValuePair<string, object>>();

                    paramList.Add(new KeyValuePair<string, object>("@ObjectId", dict["_id"].ToString()));
                    paramList.Add(new KeyValuePair<string, object>("@Date", dict["Date"]));
                    paramList.Add(new KeyValuePair<string, object>("@Opening", dict["Opening"]));
                    paramList.Add(new KeyValuePair<string, object>("@Max", dict["Max"]));
                    paramList.Add(new KeyValuePair<string, object>("@Lowest", dict["Lowest"]));
                    paramList.Add(new KeyValuePair<string, object>("@Closing", dict["Closing"]));
                    paramList.Add(new KeyValuePair<string, object>("@Volume", dict["Volume"]));
                    paramList.Add(new KeyValuePair<string, object>("@Turnover", dict["Turnover"]));
                    paramList.Add(new KeyValuePair<string, object>("@Rate", dict["Rate"]));
                    paramList.Add(new KeyValuePair<string, object>("@ContentType", dict["ContentType"]));
                    paramList.Add(new KeyValuePair<string, object>("@ArchorDate", dict["ArchorDate"]));
                    paramList.Add(new KeyValuePair<string, object>("@涨幅", dict["涨幅"]));
                    paramList.Add(new KeyValuePair<string, object>("@成交均价", dict["成交均价"]));
                    paramList.Add(new KeyValuePair<string, object>("@StockCode", dict["StockCode"]));
                    paramList.Add(new KeyValuePair<string, object>("@StockName", dict["StockName"]));
                    paramList.Add(new KeyValuePair<string, object>("@CreateTime", dict["CreateTime"]));
                    paramList.Add(new KeyValuePair<string, object>("@UpdateTime", dict["UpdateTime"]));
                    paramList.Add(new KeyValuePair<string, object>("@ItemStatus", dict["ItemStatus"]));

                    mssql.Save(sql.ToString(), paramList);

                    Console.WriteLine("成功插入" + dict["_id"]);
                }

            };
            mongo.Find("ths", "THSDataAnalyse4", "{}");
        }
         
        #endregion
    }
}
