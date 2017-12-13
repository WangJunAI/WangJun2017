﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WangJun.DB;
using WangJun.Tools;

namespace WangJun.Stock
{
    /// <summary>
    /// 股市同步器
    /// </summary>
    public  class StockSynchronizer
    {
        protected Dictionary<string, string> stockCodeDict = new Dictionary<string, string>();
        protected Dictionary<string, DateTime> syncTimeDict = new Dictionary<string, DateTime>();
        public static StockSynchronizer GetInstance()
        {
            var inst = new StockSynchronizer();
            inst.PrepareData();
            return inst;
        }

        #region 更新股票代码
        /// <summary>
        /// 更新股票代码
        /// </summary>
        public void SyncStockCode()
        {
            var startTime = DateTime.Now;///开始运行时间
            Console.Title = "股票代码更新进程 启动时间：" + startTime;
            var inst = StockTaskExecutor.CreateInstance();
            var mssql = DataStorage.GetInstance("aifuwu", "sqlserver");
            var mongo = DataStorage.GetInstance("170");
            var count = 0;
            while(true)
            {               
                if (CONST.IsSafeUpdateTime(1)) ///非交易时间,且交易前1小时
                {
                    Console.WriteLine("准备更新股票代码 {0} {1}",DateTime.Now,++count);
                    inst.UpdateAllStockCode();
                    mssql.Delete("DELETE FROM BaseInfo WHERE ContentType='股票代码'", "", "", null);
                    DataStorage.MoveDataFromMongoToSQLServer("170", "StockService", "BaseInfo", "{\"ContentType\":\"股票代码\"}", "aifuwu", "qds165298153_db", "BaseInfo");///数据同步
                    Console.WriteLine("股票代码更新完毕 {0} {1} 下一次更新在三小时后 已运行", DateTime.Now, count,DateTime.Now-startTime);
                    Thread.Sleep(3 * 60 * 60 * 1000);
                }
                else
                {
                    Console.WriteLine("交易时间或非安全可更新时间 {0}",DateTime.Now);
                    Thread.Sleep(1 * 60 * 60 * 1000);
                }
                
            }
        }
        #endregion

        #region 更新每日财经要闻 
        /// <summary>
        /// 不断更新最新的新闻
        /// </summary>
        public void SyncStockNews()
        {
            var startTime = DateTime.Now;///开始运行时间
            Console.Title = "同花顺财经要闻 更新进程 启动时间：" + startTime;

            var exe = StockTaskExecutor.CreateInstance();
            var tag = Convert.ToInt32(string.Format("{0:yyyyMMdd}", DateTime.Now));
            var count = 0;
            while (true)
            {
                try
                {
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday
                        || DateTime.Now.DayOfWeek == DayOfWeek.Sunday
                        || DateTime.Now.Hour < 9 || DateTime.Now.Hour > 15)
                    {
                        exe.GetNewsListCJYW(DateTime.Now.ToShortDateString());
                        ///非交易时间
                        if (DateTime.Now.Hour < 6 || DateTime.Now.Hour > 23) ///6点前,23点后
                        {
                            Thread.Sleep(new Random().Next(20 * 60 * 1000, 30 * 60 * 1000));///20-30分钟更新一次新闻         
                        }
                        else
                        {
                            Thread.Sleep(new Random().Next(5 * 60 * 1000, 10 * 60 * 1000));///5-10分钟更新一次新闻         
                        }
                    }
                    else
                    {
                        exe.GetNewsListCJYW(DateTime.Now.ToShortDateString());///交易时间
                        Thread.Sleep(new Random().Next(1 * 60 * 1000, 2 * 60 * 1000));///1-2分钟更新一次新闻     
                    }

                    Console.WriteLine("自动新闻更新 已运行时间 {0} 次数:{1}", DateTime.Now - startTime, ++count);
                }
                catch(Exception e)
                {
                    Console.WriteLine("异常：{0}",e.Message);
                    Console.WriteLine("位置：{0}",e.StackTrace);
                }

            }
        }
        #endregion

        #region 更新SINA的财务摘要
        /// <summary>
        /// 更新SINA的财务摘要
        /// </summary>
        public void SyncCWZY()
        {
            var startTime = DateTime.Now;///开始运行时间
            Console.Title = "同花顺财经要闻 更新进程 启动时间：" + startTime;

            var exe = StockTaskExecutor.CreateInstance();
            var mongo = DataStorage.GetInstance(DBType.MongoDB);
            var mssql = DataStorage.GetInstance(DBType.SQLServer);
            while (true)
            {
                ///获取所有股票代码,遍历更新数据,二维化
                var q = this.PrepareData();
                while (0 < q.Count)
                {
                    var stockCode = q.Dequeue();
                    var resDict = WebDataSource.GetInstance().GetCWZY(stockCode);
                    
                    ///保存到数据库中 二维化
                    var svItem = new
                    {
                        StockCode = stockCode,
                        StockName = this.stockCodeDict[stockCode],
                        Url = string.Format("http://vip.stock.finance.sina.com.cn/corp/go.php/vFD_FinanceSummary/stockid/{0}.phtml", stockCode),
                        CreateTime = DateTime.Now,
                        PageData = resDict
                    };

                    ///删除旧数据
                    var filter = "{\"ContentType\":\"SINA财务概要\",\"StockCode\":\"" + stockCode + "\"}";
                    mongo.Delete(filter, "SINACWZY", "StockService");
                    ///添加新数据

                    mongo.Save2("StockService", "SINACWZY", filter, svItem);

                    ///二维化
                    ///同步到SQL数据库中
                }
                Thread.Sleep(6 * 60 * 60 * 1000);
                q = this.PrepareData();
            }
        }
        #endregion

        #region 准备数据
        protected Queue<string> PrepareData()
        {
            var mongo = DataStorage.GetInstance(DBType.MongoDB);
            var filter = "{\"ContentType\":\"股票代码\"}";
            var resList = mongo.Find("StockService", "BaseInfo", filter);
            if(null == this.stockCodeDict ||0==this.stockCodeDict.Count)
            {
                this.stockCodeDict = resList.ToDictionary(k=>k["StockCode"].ToString(), v => v["StockName"].ToString());
            }

            var queue = CollectionTools.ToQueue<string>(this.stockCodeDict.Keys);
            return queue;
        }
        #endregion
    }
}
