using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.Stock
{
    /// <summary>
    /// 常量信息
    /// </summary>
    public static class CONST
    {
        public static string DLLPath
        {
            get
            {
                return @"E:\WangJun2017\WangJun.Stock\bin\Debug1\WangJun.Stock.dll"; 
            }
        }
        /// <summary>
        /// Node服务Url
        /// </summary>
        public static string NodeServiceUrl
        {
            get
            {
                return "http://localhost:8990";
            }
        }

        /// <summary>
        /// 判断是否是交易日
        /// </summary>
        /// <returns></returns>
         public static bool IsTradingDay()
        {
            var res = false;
            if(DateTime.Now.DayOfWeek == DayOfWeek.Saturday|| DateTime.Now.DayOfWeek == DayOfWeek.Saturday) ///周六,周日非交易日
            {
                return false;
            }
            else
            {
                ///是否在中国的节假日
                return true;
            }

            return res;

        }

        /// <summary>
        /// 是否在交易时间
        /// </summary>
        /// <returns></returns>
        public static bool IsTradingTime()
        {
            if(CONST.IsTradingDay() && DateTime.Now.Date.AddHours(9.5)<=DateTime.Now&&DateTime.Now <=DateTime.Now.Date.AddHours(15)) ///若是在交易日内
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断是否是安全更新时间
        /// </summary>
        /// <param name="updateCost"></param>
        /// <returns></returns>
        public static bool IsSafeUpdateTime(int updateCost)
        {//非交易日 或交易日前1小时 或交易时间结束
            return true;

        }

        public static string UserAgent
        {
            get
            {
                return "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36";
            }
        }
    }
}
