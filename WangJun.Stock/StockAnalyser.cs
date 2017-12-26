using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.DB;
using WangJun.Tools;

namespace WangJun.Stock
{
    /// <summary>
    /// 股票分析器
    /// </summary>
    public class StockAnalyser
    {

        public static StockAnalyser GetInstance()
        {
            var inst = new StockAnalyser();
            return inst;
        }


        #region 大单行为分析
        /// <summary>
        /// 大单行为分析
        /// </summary>
        /// <param name="param"></param>
        public void AnalyseDaDan(object param=null) {

            var temp1 = 0;
            var temp2 = 0;
            var temp3 = 0;
            var temp4 = 0;
            var dbName = "StockService";
            var collectionName = "SINADaDan2D";
            var mongo = DataStorage.GetInstance(DBType.MongoDB);
            var listDict = new Dictionary<string, Dictionary<string,int>>();
                listDict["成交额500万到1000万"] = new Dictionary<string, int>();
                listDict["成交额1000万以上"] = new Dictionary<string, int>();
            mongo.EventTraverse += (object sender , EventArgs e)=>{
                var ee = e as EventProcEventArgs;
                var data = ee.Default as Dictionary<string, object>;
                var turnover = Convert.ToInt64(data["Turnover"]);
                var stockCode = data["StockCode"].ToString();
                var stockName = data["StockName"].ToString();
                var kind = data["Kind"].ToString();
                var key = stockCode + stockName + kind;
                if (turnover <= 100 * 10000)
                {
                    temp1 += 1;
                }
                else if (100 * 10000 < turnover && turnover <= 500 * 10000)
                {
                    temp2 += 1;
                }
                else if (500 * 10000 < turnover && turnover <= 1000 * 10000)
                {
                    temp3 += 1;
                    if(!listDict["成交额500万到1000万"].ContainsKey(key))
                    {
                        listDict["成交额500万到1000万"].Add(key,0) ;
                    }
                    listDict["成交额500万到1000万"][key] += 1; 

                }
                else if (1000 * 10000 < turnover && turnover <= int.MaxValue)
                {
                    temp4 += 1;
                    if (!listDict["成交额1000万以上"].ContainsKey(key))
                    {
                        listDict["成交额1000万以上"].Add(key,0);
                    }
                    listDict["成交额1000万以上"][key] += 1;
                }
            };

            mongo.Traverse(dbName, collectionName, "{\"TradingDate\":new Date('2017/12/18')}");

            ///保存结果
            ///
            var total =Convert.ToSingle( temp1 + temp2 + temp3 + temp4);
            var res = new { ContentType = "大单行为分析"
                , 交易日期 = "2017/12/18",
                成交额100万以下 = temp1/total
                , 成交额100万到500万 = temp2/total
                , 成交额500万到1000万 = temp3 / total
                , 成交额1000万以上 = temp4 / total
                , 股票列表= listDict
            };//成交额 1000为一个单位 太少就合并
            mongo.Save(res, "DataResult", "DataResult");

        }
        #endregion

        #region 日线分析

        #endregion

        #region 每日最新热词

        #endregion


    }
}
