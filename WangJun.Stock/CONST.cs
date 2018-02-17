using System;
using System.IO;
using WangJun.Utility;

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

        public static class DB
        {
            public static string DBName_StockService { get { return "StockService"; } }
            public static string CollectionName_News { get { return "News"; } }

            public static string CollectionName_FenCi { get { return "FenCi"; } }

            public static string CollectionName_DaDan { get { return "SINADaDan2D"; } }

            public static string CollectionName_KLine { get { return "KLine"; } }

            public static string CollectionName_Exception { get { return "Exception"; } }

            public static string CollectionName_TaskStatus { get { return "TaskStatus"; } }

            /// <summary>
            /// 公司简介
            /// </summary>
            public static string CollectionName_GSJJ  {get { return "GSJJ"; } }

            /// <summary>
            /// 概念板块
            /// </summary>
            public static string CollectionName_BKGN { get { return "BKGN"; } }

            /// <summary>
            /// 股票异动信息
            /// </summary>
            public static string CollectionName_Radar { get { return "Radar"; } }

            /// <summary>
            /// 资金流向
            /// </summary>
            public static string CollectionName_ZJLX { get { return "ZJLX"; } }

            /// <summary>
            /// 财务摘要
            /// </summary>
            public static string CollectionName_CWZY { get { return "CWZY"; } }

            /// <summary>
            /// 基本信息
            /// </summary>
            public static string CollectionName_BaseInfo { get { return "BaseInfo"; } }

            /// <summary>
            /// 数据结果
            /// </summary>
            public static string CollectionName_DataResult { get { return "DataResult"; } }

            /// <summary>
            /// 龙虎榜
            /// </summary>
            public static string CollectionName_LHB { get { return "LHB"; } }

            /// <summary>
            /// 龙虎榜明细
            /// </summary>
            public static string CollectionName_LHBMX { get { return "LHBMX"; } }

            /// <summary>
            /// 融资融券
            /// </summary>
            public static string CollectionName_RZRQ { get { return "RZRQ"; } }

        }

        /// <summary>
        /// Node服务Url
        /// </summary>
        public static string NodeServiceUrl
        {
            get
            {
                var json = Convertor.FromJsonToDict2(File.ReadAllText("config.js"));
                var nodeServiceUrl = json["NodeServiceUrl"].ToString();

                return nodeServiceUrl;
            }
        }

        /// <summary>
        /// 判断是否是交易日
        /// </summary>
        /// <returns></returns>
         public static bool IsTradingDay()
        {
            var res = false;
            if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday|| DateTime.Now.DayOfWeek == DayOfWeek.Saturday) ///周六,周日非交易日
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
            if(!CONST.IsTradingDay()) ///不是交易日
            {
                return true;
            }
            else if(CONST.IsTradingDay() && (DateTime.Now<= DateTime.Now.Date.AddHours(7)|| DateTime.Now.Date.AddHours(15.5)<= DateTime.Now.AddHours(updateCost)))
            {
                ///交易日内早7点前,或交易结束后
                return true;
            }
            return false;
        }

        public static string UserAgent
        {
            get
            {
                return "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36";
            }
        }

        #region API Test

        #endregion
    }
}
