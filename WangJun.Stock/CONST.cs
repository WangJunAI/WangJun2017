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
        /// 判断是否是交易日
        /// </summary>
        /// <returns></returns>
         public static bool IsTradingDay()
        {
            var res = false;
            if(DateTime.Now.DayOfWeek == DayOfWeek.Saturday|| DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
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
        {
            return true;
        }
    }
}
