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

        public void AnalyseDaDan(Dictionary<string,object> param=null) {

            var temp1 = 0;
            var temp2 = 0;
            var temp3 = 0;
            var temp4 = 0;
            var db = DataStorage.GetInstance("170");
            db.EventTraverse += (object sender , EventArgs e)=>{
                var ee = e as EventProcEventArgs;
                var data = ee.Default as Dictionary<string, object>;
                var volume = Convert.ToInt64(data["Volume"]);
                var price = Convert.ToSingle(data["Price"]);
                var stockCode = data["StockCode"].ToString();
                var stockName = data["StockName"].ToString();
                var turnover = volume * price; ///成交额 100万 100万-500万 1000万 1000万以上
                if (turnover <= 100 * 10000)
                {
                    temp1 += 1;
                }
                else if(100 * 10000 < turnover && turnover <= 500 * 10000)
                {
                    temp2 += 1;
                }
                else if (500 * 10000 < turnover && turnover <= 1000 * 10000)
                {
                    temp3 += 1;
                }
                else if (1000 * 10000 < turnover && turnover <= int.MaxValue)
                {
                    temp4 += 1;
                }
            };

            db.Traverse("StockData2D", "DaDan", "{\"TradingDate\":\"new Date('2017/10/19 08:00:00')\"}");

            ///保存结果
            ///
            var total =Convert.ToSingle( temp1 + temp2 + temp3 + temp4);
            var res = new { ContentType = "大单行为分析",交易日期=20171019, 成交额100万以下 = temp1/total, 成交额100万到500万 = temp2/total, 成交额500万到1000万 = temp3 / total, 成交额1000万以上 = temp4 / total };//成交额 1000为一个单位 太少就合并
            db.Save(res, "DataResult", "DataResult");

        }

    }
}
