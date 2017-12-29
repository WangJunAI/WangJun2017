using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WangJun.Data;
using WangJun.DB;
using WangJun.Debug;
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
            while(true)
            {               
                if (CONST.IsSafeUpdateTime(1)) ///非交易时间,且交易前1小时
                {
                    LOGGER.Log(string.Format("准备更新股票代码 {0}",DateTime.Now));
                    inst.UpdateAllStockCode();
                    LOGGER.Log(string.Format("股票代码更新完毕 {0} {1} 下一次更新在一天后后 已运行 ", DateTime.Now,DateTime.Now-startTime));
                    ThreadManager.Pause(days: 1); ///每日更新一次
                }
                else
                {
                    Console.WriteLine("交易时间或非安全可更新时间 {0}",DateTime.Now);
                    ThreadManager.Pause(hours: 1);

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
            while (true)
            {
                try
                {
                    if (CONST.IsSafeUpdateTime(1))
                    {
                        exe.GetNewsListCJYW(DateTime.Now.ToShortDateString());
                        ///非交易时间
                        if (DateTime.Now.Hour < 6 || DateTime.Now.Hour > 23) ///6点前,23点后
                        {
                            exe.GetNewsListCJYW(DateTime.Now.AddDays(-1).ToShortDateString());
                            exe.GetNewsListCJYW(DateTime.Now.AddDays(-2).ToShortDateString());
                            exe.GetNewsListCJYW(DateTime.Now.AddDays(-3).ToShortDateString());
                            exe.GetNewsListCJYW(DateTime.Now.AddDays(-4).ToShortDateString());
                            exe.GetNewsListCJYW(DateTime.Now.AddDays(-5).ToShortDateString());
                            exe.GetNewsListCJYW(DateTime.Now.AddDays(-6).ToShortDateString());
                            exe.GetNewsListCJYW(DateTime.Now.AddDays(-7).ToShortDateString());

                            ThreadManager.Pause(hours: 1); ///凌晨一小时更新一次   
                        }
                        else
                        {
                            ThreadManager.Pause(minutes: 10); ///10分钟更新一次新闻         
                        }
                    }
                    else
                    {
                        exe.GetNewsListCJYW(DateTime.Now.ToShortDateString());///交易时间
                        ThreadManager.Pause(minutes: 3); ///3分钟更新一次新闻         
                    }

                    LOGGER.Log(string.Format("自动新闻更新 已运行时间 {0}", DateTime.Now - startTime));
                }
                catch(Exception e)
                { 
                    LOGGER.Log(string.Format("异常：{0}",e.Message));
                    LOGGER.Log(string.Format("位置：{0}", e.StackTrace));
                    LOGGER.Beep();
                    ThreadManager.Pause(minutes: 1); ///停一分钟        

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
            Console.Title = "SINA财务摘要 更新进程 启动时间：" + startTime;

            var exe = StockTaskExecutor.CreateInstance();
            var mongo = DataStorage.GetInstance(DBType.MongoDB);
            var dbName = CONST.DB.DBName_StockService;
            var collectionName = CONST.DB.CollectionName_CWZY;
            var methodName = "SyncCWZY";
            while (true)
            {
                ///获取所有股票代码,遍历更新数据,二维化
                var q = this.PrepareData(methodName);
                while (0 < q.Count)
                {

                    var stockCode = q.Dequeue();

                    var resObj = WebDataSource.GetInstance().GetCWZY(stockCode);
                    var resDict = (resObj is Dictionary<string, object>) ? (resObj as Dictionary<string, object>)["PageData"]   : new Dictionary<string, object>();
                    ///保存到数据库中 二维化
                    var svItem = new
                    {
                        StockCode = stockCode,
                        StockName = this.stockCodeDict[stockCode],
                        Url = string.Format("http://vip.stock.finance.sina.com.cn/corp/go.php/vFD_FinanceSummary/stockid/{0}.phtml", stockCode),
                        CreateTime = DateTime.Now,
                        PageData =resDict
                    };

                    ///删除旧数据
                    var filter = "{\"ContentType\":\"SINA财务概要\",\"StockCode\":\"" + stockCode + "\"}";
                     ///添加新数据
                    LOGGER.Log(string.Format("更新{0} {1}的财务摘要 ",stockCode, svItem.StockName));
                    mongo.Save3(dbName, collectionName, svItem, filter);
                    ThreadManager.Pause(seconds: 5);
                    TaskStatusManager.Set(methodName, new {ID= methodName, StockCode = stockCode, StockName = svItem.StockName, Status = "已下载", CreateTime = DateTime.Now });
                }
                TaskStatusManager.Set(methodName, new { ID = methodName, Status = "队列处理完毕", CreateTime = DateTime.Now });

                ThreadManager.Pause(days: 2);
                q = this.PrepareData();
            }
        }
        #endregion

        #region SINA大单数据更新
        /// <summary>
        /// SINA大单数据更新
        /// </summary>
        public void SyncSINADaDan()
        {
            var startTime = DateTime.Now;///开始运行时间
            Console.Title = "SINA大单数据更新 更新进程 启动时间：" + startTime;
            var exe = StockTaskExecutor.CreateInstance();
            while (true)
            {
                if (CONST.IsSafeUpdateTime(1))
                {
                    exe.GetDaDanData();
                    LOGGER.Log(string.Format("大单数据已更新,下一次更新在一天后 {0}", DateTime.Now));
                    ThreadManager.Pause(days: 1);
                }
                else
                {
                    LOGGER.Log(string.Format("未在安全更新时间内 {0}", DateTime.Now));
                    ThreadManager.Pause(minutes: 5);
                }

                
            }
            
        }
        #endregion

        #region SINA日线数据同步
        /// <summary>
        /// 同步日线数据
        /// </summary>
        public void SyncKLineDay()
        {
            var mongo = DataStorage.GetInstance(DBType.MongoDB);
            var exe = WebDataSource.GetInstance();//
            var dbName = CONST.DB.DBName_StockService;
            var collectionName = CONST.DB.CollectionName_KLine;
            while (true)
            {
                var q = this.PrepareData();
                var year = DateTime.Now.Year;
                var jidu = Convertor.GetJidu(DateTime.Now); ///获取当前季度数据
                
                while (0 < q.Count)
                {
                    var stockCode = q.Dequeue();
                    var stockName = this.stockCodeDict[stockCode];

                    TaskStatusManager.Set("SyncKLineDay", new { ID = "SyncKLineDay",CreateTime =DateTime.Now, Status= "准备操作SINA历史交易", StockCode = stockCode, StockName = stockName,Year=year,JiDu=jidu });

                    var resDict = exe.GetSINAKLineDay(stockCode, year, jidu);
                    if (null != resDict && resDict.ContainsKey("Rows") && (resDict["Rows"] is ArrayList))
                    {

                        var rows = resDict["Rows"] as ArrayList;
                        if (rows is ArrayList)
                        {
                            for (var k = 0; k < rows.Count; k++)
                            {
                                var row = rows[k] as Dictionary<string, object>;
                                var prevClose = 0.0f;
                                if (1 <= k)
                                {
                                    prevClose = Convert.ToSingle((rows[k - 1] as Dictionary<string, object>)["收盘价"]);
                                }

                                var svItem2D = new
                                {
                                    StockCode = stockCode,
                                    StockName = this.stockCodeDict[stockCode],
                                    ContentType = "SINA历史交易",
                                    Year = year,
                                    CreateTime = DateTime.Now,
                                    UpdateTime = DateTime.Now,
                                    TradingDate = Convert.ToDateTime(row["日期"]),
                                    Open = row["开盘价"],
                                    High = row["最高价"],
                                    Close = row["收盘价"],
                                    Low = row["最低价"],
                                    Volume = row["交易量(股)"],
                                    Turnover = row["交易金额(元)"],
                                    Amplitude = (k == 0) ? -1 : (Convert.ToSingle(row["最高价"]) - Convert.ToSingle(row["最低价"])) / prevClose, ///振幅
                                    Increase = (k == 0) ? -1 : ((Convert.ToSingle(row["收盘价"]) - prevClose) / prevClose),///涨幅
                                    AveragePrice = Convert.ToSingle(row["交易金额(元)"]) / Convert.ToSingle(row["交易量(股)"]),///成交均价
                                    RowIndex = k,
                                    RowCount = rows.Count
                                };
                                ///状态保存
                                TaskStatusManager.Set("SyncKLineDay", new { ID= "SyncKLineDay", CreateTime = DateTime.Now, Status = "准备写入SINA历史交易", StockCode = stockCode, StockName = stockName, Year = year, JiDu = jidu,RowIndex=svItem2D.RowIndex,RowCount=svItem2D.RowCount });

                                var filter = "{\"StockCode\":\"" + stockCode + "\",\"TradingDate\":new Date('" + string.Format("{0:yyyy/MM/dd}", svItem2D.TradingDate) + "')}";
                                mongo.Save3(dbName, collectionName, svItem2D, filter);
                                LOGGER.Log(string.Format("保存 {0} {1} SINA历史交易2D", stockCode, stockName));
                            }
                        }

                    }

                    ThreadManager.Pause(seconds: 5);

                    if (0 == q.Count && jidu == Convertor.GetJidu(DateTime.Now) && 1 < jidu)//当年2,3,4季度 当季度
                    {
                        jidu = jidu - 1;
                        q = this.PrepareData();
                    }
                    else if (0 == q.Count && jidu == Convertor.GetJidu(DateTime.Now) && 1 == jidu)
                    {
                        jidu = 4;///上一年第四季度
                        year = year - 1;///上一年
                        q = this.PrepareData();
                    }


                    #region 补充计算缺失

                    #endregion
                }

                LOGGER.Log("历史交易获取完毕 ,下一次将在24小时后开始");
                ThreadManager.Pause(days: 1);
            }
        }
        #endregion

        #region SINA 公司简介
        /// <summary>
        /// SINA 公司简介
        /// </summary>
        public void SyncGSJJ()
        {
            var startTime = DateTime.Now;///开始运行时间
            Console.Title = "SINA公司简介 更新进程 启动时间：" + startTime;

            var exe = StockTaskExecutor.CreateInstance();
            var mongo = DataStorage.GetInstance(DBType.MongoDB);
            var dbName = CONST.DB.DBName_StockService;
            var collectionName = CONST.DB.CollectionName_GSJJ;
            while (true)
            {
                ///获取所有股票代码,遍历更新数据
                var q = this.PrepareData();
                while (0 < q.Count)
                {
                    var stockCode = q.Dequeue();
                    var resObj = WebDataSource.GetInstance().GetGSJJ(stockCode);
                    var resDict = (resObj is Dictionary<string, object>) ? (resObj as Dictionary<string, object>)["PageData"] : new Dictionary<string, object>();
                    ///保存到数据库中 二维化
                    var svItem = new
                    {
                        StockCode = stockCode,
                        StockName = this.stockCodeDict[stockCode],
                        Url = string.Format("http://vip.stock.finance.sina.com.cn/corp/go.php/vCI_CorpInfo/stockid/{0}.phtml", stockCode),
                        CreateTime = DateTime.Now,
                        PageData = resDict
                    };

                    ///删除旧数据
                    ///添加新数据
                    LOGGER.Log(string.Format("更新{0} {1}的公司简介 ", stockCode, svItem.StockName));
                    var filter = "{\"ContentType\":\"SINA公司简介\",\"StockCode\":\"" + stockCode + "\"}";
                    mongo.Save3(dbName, collectionName, svItem,filter);
                    ThreadManager.Pause(seconds: 5);
                }
                LOGGER.Log(string.Format("本次板块概念更新完毕，下一次一天以后更新 {0}", DateTime.Now));

                ThreadManager.Pause(days: 1);
                q = this.PrepareData();
            }

        }
        #endregion

        #region SINA 板块概念
        /// <summary>
        /// SINA 板块概念
        /// </summary>
        public void SyncBKGN()
        {
            var startTime = DateTime.Now;///开始运行时间
            Console.Title = "SINA板块概念 更新进程 启动时间：" + startTime;

            var exe = StockTaskExecutor.CreateInstance();
            var mongo = DataStorage.GetInstance(DBType.MongoDB);
            var dbName = CONST.DB.DBName_StockService;
            var collectionName = CONST.DB.CollectionName_BKGN;
            while (true)
            {
                ///获取所有股票代码,遍历更新数据
                var q = this.PrepareData();
                while (0 < q.Count)
                {
                    var stockCode = q.Dequeue();
                    var resObj = WebDataSource.GetInstance().GetBKGN(stockCode);
                    var resDict = (resObj is Dictionary<string, object>) ? (resObj as Dictionary<string, object>)["PageData"] : new Dictionary<string, object>();
                    ///保存到数据库中 二维化
                    var svItem = new
                    {
                        StockCode = stockCode,
                        StockName = this.stockCodeDict[stockCode],
                        Url = string.Format("http://vip.stock.finance.sina.com.cn/corp/go.php/vCI_CorpOtherInfo/stockid/{0}/menu_num/5.phtml", stockCode),
                        CreateTime = DateTime.Now,
                        PageData = resDict
                    };
                    
                    var filter = "{\"ContentType\":\"SINA板块概念\",\"StockCode\":\"" + stockCode + "\"}";
                     ///添加新数据
                    LOGGER.Log(string.Format("更新{0} {1}的板块概念 ", stockCode, svItem.StockName));
                    mongo.Save3(dbName, collectionName, svItem, filter);
                    ThreadManager.Pause(seconds: 5);

                }
                LOGGER.Log(string.Format("本次板块概念更新完毕，下一次两天以后更新 {0}",DateTime.Now));
                ThreadManager.Pause(days: 2);

                q = this.PrepareData();
            }

        }
        #endregion

        #region THS资金流向
        /// <summary>
        /// THS资金流向
        /// </summary>
        public void SyncZJLX()
        {
            var startTime = DateTime.Now;///开始运行时间
            Console.Title = "THS资金流向 更新进程 启动时间：" + startTime;

            var exe = StockTaskExecutor.CreateInstance();
            var mongo = DataStorage.GetInstance(DBType.MongoDB);
            var dbName = CONST.DB.DBName_StockService; 
            var collectionName = CONST.DB.CollectionName_ZJLX;
            while (true)
            {
                ///获取所有股票代码,遍历更新数据
                var q = this.PrepareData();
                while (0 < q.Count)
                {
                    var stockCode = q.Dequeue();
                    var resObj = WebDataSource.GetInstance().GetZJLX(stockCode);
                    var resList = (resObj is Dictionary<string, object>) ? (resObj as Dictionary<string, object>)["PageData"] : new ArrayList();
                    var stockName = this.stockCodeDict[stockCode];
 
                     ///二维化
                     if(resList is ArrayList)
                    {
                        var rows = (resList as ArrayList);
                        foreach (Dictionary<string,object> row in rows)
                        {
                            var svItem2D = new
                            {
                                StockCode = stockCode,
                                StockName = stockName,
                                Url = string.Format("http://stockpage.10jqka.com.cn/{0}/funds/", stockCode),
                                CreateTime = DateTime.Now,
                                TradingDate = row["日期"],
                                Close = row["收盘价"],
                                Increase = row["涨跌幅"],
                                NetInflow = row["资金净流入"],
                                NetLarge5Day = row["5日主力净额"],
                                NetLarge = row["大单(主力)净额"],
                                NetLargeProportion = row["大单(主力)净占比"],
                                NetMedium = row["中单净额"],
                                NetMediumProportion = row["中单净占比"],
                                NetSmall = row["小单净额"],
                                NetSmallProportion = row["小单净占比"],
                            };
 
                            var filter = "{\"ContentType\":\"THS资金流向\",\"StockCode\":\"" + stockCode + "\",\"TradingDate\":new Date('" + string.Format("{0:yyyy/MM/dd}", svItem2D.TradingDate) + "')}"; 
                            mongo.Save3(dbName, collectionName, svItem2D, filter);
                            LOGGER.Log(string.Format("THS资金流向 正在更新 {0} {1} {2}", svItem2D.StockCode, svItem2D.StockName, svItem2D.TradingDate));
                        }
                    }

                    ThreadManager.Pause(seconds: 5);

                    ///同步到SQL数据库中
                }

                ThreadManager.Pause(days: 1);
                q = this.PrepareData();
            }

        }
        #endregion

        #region SINA 大单详细

        #endregion

        #region 准备数据
        /// <summary>
        /// 准备数据
        /// </summary>
        /// <returns></returns>
        protected Queue<string> PrepareData(string methodName=null)
        {
            var mongo = DataStorage.GetInstance(DBType.MongoDB);
            var query = "{\"ContentType\":\"股票代码\",\"SortCode\":{$exists:true}}";
            var sort = "{\"SortCode\":1}";
            var resList = mongo.Find3("StockService", "BaseInfo", query,sort);
            if(null == this.stockCodeDict ||0==this.stockCodeDict.Count)
            {
                this.stockCodeDict = resList.ToDictionary(k=>k["StockCode"].ToString(), v => v["StockName"].ToString());
            }
            var codeList = from item in resList orderby (int)item["SortCode"] select item["StockCode"].ToString();
            var queue = CollectionTools.ToQueue<string>(codeList);

            if(!string.IsNullOrWhiteSpace(methodName))
            {
                var status = TaskStatusManager.Get(methodName);
                var from = string.Empty; ///上一次的起始位置
                if (status.ContainsKey("StockCode"))
                {
                    from = status["StockCode"].ToString();
                }
                while (!string.IsNullOrWhiteSpace(from) && 0 < queue.Count)
                {
                    var stockCode = queue.Dequeue();
                    if (stockCode == from) ///若有状态,则从上次的位置开始下载
                    {
                        break;
                    }
                }
            }

            return queue;
        }
        #endregion
    }
}
