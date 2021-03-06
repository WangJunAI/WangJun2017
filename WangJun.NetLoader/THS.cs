﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WangJun.Data;
using WangJun.DB;
using WangJun.Net;
using WangJun.Tools;

namespace WangJun.NetLoader
{
    /// <summary>
    /// 同花顺 数据获取
    /// </summary>
    public class THS
    {
         
        protected HTTP http = new HTTP("gbk");

        protected MongoDB mongo = MongoDB.GetInst("mongodb://192.168.0.140:27017");

        protected Dictionary<string, string> stockCodeDict = new Dictionary<string, string>();

 

        /// <summary>
        /// 数据库集合
        /// </summary>
        public static class CONST
        {
            public static string DBName { get { return "PageSource"; } }
            /// <summary>
            /// 股票基本信息
            /// </summary>
            public static string StockBaseInfo{get{return string.Format("StockBaseInfo");} }
            /// <summary>
            /// 个股龙虎榜和龙虎榜明细原始页面信息
            /// </summary>
            public static string PageGGLHB { get { return string.Format("PageGGLHB"); } } 

            /// <summary>
            /// 个股资金流资金流向 - 个股资金 原始页面信息
            /// </summary>
            public static string PageFundsStock { get { return string.Format("PageFundsStock"); } }

            /// <summary>
            /// 个股详细 原始页面信息
            /// </summary>
            public static string PageStock { get { return string.Format("PageStock"); } }

            /// <summary>
            /// 个股KLine 原始页面信息
            /// </summary>
            public static string PageKLine { get { return string.Format("PageKLine"); } }

            /// <summary>
            /// 异常信息集合
            /// </summary>
            public static string Exception { get { return string.Format("Exception"); } }//异常信息 

            /// <summary>
            /// 大单追踪集合
            /// </summary>
            public static string PageLargeFundsTracking { get { return string.Format("PageLargeFundsTracking"); } }

            public static string TaskID = DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day+Guid.NewGuid().ToString().Replace("-", string.Empty);
        }
        #region 获取一个实例
        /// <summary>
        /// 获取一个实例
        /// </summary>
        /// <returns></returns>
        public static THS GetInst()
        {
            return new THS();
        }
        #endregion
         

        #region 获取所有的股票代码
        /// <summary>
        /// 获取所有的股票代码 必须休市后再用 因为 股价变动造成排名变动
        /// </summary>
        /// <param name="needForceUpdate">是否强制更新</param>
        public void GetALLStockCode(bool needForceUpdate=false)
        {
            ///先检查本地或数据库是否有,若有就用,否则从网络获取
            Console.WriteLine("初始化股票代码\t{0}",DateTime.Now);
            if (0 == this.stockCodeDict.Count)
            {
                var list = mongo.Find(THS.CONST.DBName, THS.CONST.StockBaseInfo, "{}");
                if (0 < list.Count) ///若找得到数据
                {
                    this.stockCodeDict.Clear();
                    foreach (var item in list)
                    {
                        this.stockCodeDict.Add(item["StockCode"].ToString(), item["StockName"].ToString());
                    }

                    Console.WriteLine("股票基本信息从数据库初始化完毕");
                }
                else
                {
                    Console.WriteLine("重新获取股票信息");

                    var headers = new Dictionary<string, string>();
                    headers.Add("Accept","text/html,*/*; q=0.01");
                    headers.Add("Accept-Encoding", "gzip, deflate");
                    headers.Add("Accept-Language","zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4");
                    headers.Add("Host","q.10jqka.com.cn");
                    headers.Add("Referer","http://q.10jqka.com.cn/");
                    headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
                    headers.Add("X-Requested-With", "XMLHttpRequest");

                    #region 获取分页数 
                    var pageCount = 0;
                    var pagerUrl = string.Format(@"http://q.10jqka.com.cn/index/index/board/all/field/zdf/order/desc/page/{0}/ajax/1/", 1);
                    var pagerHtml = http.GetGzip(pagerUrl,Encoding.GetEncoding("GBK"), headers);
                    if (100 < pagerHtml.Length)
                    {
                        pageCount = int.Parse(pagerHtml.Substring(pagerHtml.LastIndexOf("</span>") - 3, 3)); ///页码数
                        Console.WriteLine("股票代码页数{0}\t{1}", pageCount, DateTime.Now);
                    }
                    #endregion

                    var stringBuilder = new StringBuilder();

                    for (int i = 1; i <= pageCount; i++)
                    {
                        var url = string.Format(@"http://q.10jqka.com.cn/index/index/board/all/field/zdf/order/desc/page/{0}/ajax/1/", i);
                        var html = http.GetGzip(url, Encoding.GetEncoding("GBK"), headers);


                        #region 股票代码处理

                        string[] trArray = html.Substring(html.IndexOf("<tbody>") + "<tbody>".Length).Split(new string[] { "<tr>", "</tr>" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var tr in trArray)
                        {
                            var tdArray = tr.Split(new string[] { "<td>", "</td>" }, StringSplitOptions.RemoveEmptyEntries);
                            if (10 < tdArray.Length)
                            {
                                var stockCode = tdArray[3].Substring(tdArray[3].IndexOf("target=\"_blank\">") + "target=\"_blank\">".Length).Replace("</a>", string.Empty).Replace("&#032;", "");
                                var stockName = tdArray[5].Substring(tdArray[5].IndexOf("target=\"_blank\">") + "target=\"_blank\">".Length).Replace("</a>", string.Empty).Replace("&#032;", "");
                                if (!stockCodeDict.ContainsKey(stockCode) && !stockCodeDict.ContainsValue(stockName))
                                {
                                    stockCodeDict.Add(stockCode, stockName);
                                }
                                else
                                {
                                    throw new Exception("数据重复");
                                }
                                Console.WriteLine("正在添加 {0}\t{1}", stockCode, stockName);
                            }
                        }
                        #endregion

                    }

                    #region 输出检查
                    foreach (var item in stockCodeDict)
                    {
                        Console.WriteLine(item.Key + "  " + item.Value.Replace("&#032;", string.Empty));
                        stringBuilder.AppendLine(item.Key + "\t" + item.Value.Replace("&#032;", string.Empty));
                    }
                    #endregion

                    #region 文件保存
                    File.WriteAllText(@"E:\stockCode.txt", stringBuilder.ToString(), Encoding.UTF8);
                    Console.WriteLine("stockCode.txt 保存完毕");
                    #endregion

                    #region 保存到数据库
                    foreach (var item in stockCodeDict)
                    {
                        mongo.Save(THS.CONST.DBName, THS.CONST.StockBaseInfo, new { StockCode = item.Key, StockName = item.Value, CreateTime = DateTime.Now, UpdateTime = DateTime.Now, TaskID = THS.CONST.TaskID });
                    }
                    #endregion
                    Console.WriteLine("股票代码添加完毕 {0}", DateTime.Now);


                }
            }
        }
        #endregion

        #region 下载每个股票的详细页面
        /// <summary>
        /// 下载每个股票的页面 UTF8
        /// </summary>
        public void GetPageStock()
        {
            this.GetALLStockCode(); ///准备所有股票代码
            var queueUrl = new Queue<KeyValuePair<string, string>>();

            var httpdownloader = new HTTP(Encoding.UTF8);
            httpdownloader.EventException += (object sender ,EventArgs e)=> {
                var ee = e as EventProcEventArgs;
                Console.WriteLine("下载失败 {0}", ee.Default);
                var item = ee.Default as Dictionary<string, object>;
                item["ContentType"] = "个股详细页面下载失败异常信息";
                item["TaskID"] = THS.CONST.TaskID;

                var url = item["Url"].ToString();
                var type = string.Empty;

                mongo.Save(THS.CONST.DBName, THS.CONST.Exception, item);
                #region 类型判断
                if(url.Contains("/funds/"))
                {
                    type = "资金流向";
                }
                else if(url.Length == "http://stockpage.10jqka.com.cn/000000/".Length)
                {
                    type = "首页概览";
                }
                #endregion

                queueUrl.Enqueue(new KeyValuePair<string, string>(type, url));
                Thread.Sleep(60 * 1000);
            };
             


            if (null != this.stockCodeDict)
            {
                foreach (var item in this.stockCodeDict)
                {
                    queueUrl.Enqueue(new KeyValuePair<string, string>(item.Key.Trim(), item.Value.Trim()));///Key：StockCode Value:StockName
                }
            }
             
            while(0<queueUrl.Count)
            {
                var queueItem = queueUrl.Dequeue();
                 {
                    var urlList = new List<KeyValuePair<string, string>>();
                    urlList.Add(new KeyValuePair<string, string>("首页概览", string.Format("http://stockpage.10jqka.com.cn/{0}/", queueItem.Key))); ///首页概览
                    urlList.Add(new KeyValuePair<string, string>("资金流向", string.Format("http://stockpage.10jqka.com.cn/{0}/funds/", queueItem.Key))); ///资金流向
                    //urlList.Add(new KeyValuePair<string, string>("公司资料", string.Format("http://stockpage.10jqka.com.cn/{0}/company/", queueItem.Key))); ///公司资料
                    //urlList.Add(new KeyValuePair<string, string>("新闻公告", string.Format("http://stockpage.10jqka.com.cn/ajax/code/{0}/type/news/", queueItem.Key))); ///新闻公告
                    //urlList.Add(new KeyValuePair<string, string>("财务分析", string.Format("http://stockpage.10jqka.com.cn/{0}/finance/", queueItem.Key))); ///财务分析
                    //urlList.Add(new KeyValuePair<string, string>("经营分析", string.Format("http://stockpage.10jqka.com.cn/{0}/operate/", queueItem.Key))); ///经营分析
                    //urlList.Add(new KeyValuePair<string, string>("股东股本", string.Format("http://stockpage.10jqka.com.cn/{0}/holder/", queueItem.Key))); ///股东股本
                    //urlList.Add(new KeyValuePair<string, string>("主力持仓", string.Format("http://stockpage.10jqka.com.cn/{0}/position/", queueItem.Key))); ///主力持仓
                    //urlList.Add(new KeyValuePair<string, string>("公司大事", string.Format("http://stockpage.10jqka.com.cn/{0}/event/", queueItem.Key))); ///公司大事
                    //urlList.Add(new KeyValuePair<string, string>("分红融资", string.Format("http://stockpage.10jqka.com.cn/{0}/bonus/", queueItem.Key))); ///分红融资
                    //urlList.Add(new KeyValuePair<string, string>("价值分析", string.Format("http://stockpage.10jqka.com.cn/{0}/worth/", queueItem.Key))); ///价值分析
                    //urlList.Add(new KeyValuePair<string, string>("行业分析", string.Format("http://stockpage.10jqka.com.cn/{0}/field/", queueItem.Key))); ///行业分析

                    var headers = new Dictionary<string, string>();
                    headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                    headers.Add("Accept-Encoding", "gzip,deflate");
                    headers.Add("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4");
                    headers.Add("Host", "stockpage.10jqka.com.cn");
                    headers.Add("Referer", "http://www.10jqka.com.cn/");
                    headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");

                    foreach (var url in urlList)
                    {
                        if ("新闻公告" == url.Key)
                        {
                            var html = httpdownloader.GetGzip(url.Value, Encoding.GetEncoding("gbk"), headers);
                            var item = new
                            {
                                StockCode = queueItem.Key,
                                StockName = queueItem.Value,
                                ContentType = url.Key,
                                CreatTime = DateTime.Now,
                                Url = url.Value,
                                Page = html,
                                MD5=Convertor.Encode_MD5(html),
                                TaskID = THS.CONST.TaskID
                            };
                            mongo.Save(THS.CONST.DBName, THS.CONST.PageStock, item);
                            Console.WriteLine("已保存 " + url.Key + " " + url.Value);
                        }
                        else
                        {
                            var html = httpdownloader.GetGzip(url.Value, Encoding.UTF8,headers);
                            var item = new
                            {
                                StockCode = queueItem.Key,
                                StockName = queueItem.Value,
                                ContentType = url.Key,
                                Url = url.Value,
                                CreatTime = DateTime.Now,
                                Page = html,
                                MD5 = Convertor.Encode_MD5(html),
                                TaskID = THS.CONST.TaskID
                            };
                            var collectionName =THS.CONST.PageStock;
                             mongo.Save(THS.CONST.DBName, collectionName, item);
                            Console.WriteLine("已保存 " + url.Key + " " + url.Value);
                        }
                    }

                }

            }

            
        }



        #endregion

        #region 下载每个股票的日线页面
        /// <summary>
        /// 下载每个股票的页面 UTF8
        /// </summary>
        public void GetPageKLine()
        {
            this.GetALLStockCode(); ///准备所有股票代码
            var queueUrl = new Queue<KeyValuePair<string, string>>();

            var httpdownloader = new HTTP(Encoding.UTF8);
            httpdownloader.EventException += (object sender, EventArgs e) => {
                var ee = e as EventProcEventArgs;
                Console.WriteLine("下载失败 {0}", ee.Default);
                var item = ee.Default as Dictionary<string, object>;
                item["ContentType"] = "个股日线页面下载失败异常信息";
                item["TaskID"] = THS.CONST.TaskID;

                var url = item["Url"].ToString();
                var type = string.Empty;

                mongo.Save(THS.CONST.DBName, THS.CONST.Exception, item);
                #region 类型判断

                if (url.Contains("/01/last.js"))
                {
                    type = "日线数据";
                }
                #endregion

                queueUrl.Enqueue(new KeyValuePair<string, string>(type, url));
                Thread.Sleep(60 * 1000);
            };
             
            if (null != this.stockCodeDict)
            {
                foreach (var item in this.stockCodeDict)
                {
                    queueUrl.Enqueue(new KeyValuePair<string, string>(item.Key.Trim(), item.Value.Trim()));///Key：StockCode Value:StockName
                }
            }

            while (0 < queueUrl.Count)
            {
                var queueItem = queueUrl.Dequeue();
                {
                    var urlList = new List<KeyValuePair<string, string>>();
                    urlList.Add(new KeyValuePair<string, string>("日线数据", string.Format("http://d.10jqka.com.cn/v2/line/hs_{0}/01/last.js", queueItem.Key))); ///日线数据


                    foreach (var url in urlList)
                    {
                        var headers = new Dictionary<HttpRequestHeader, string>();
                        headers.Add(HttpRequestHeader.Accept, "*/*");
                        headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                        headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.8,en;q=0.4");
                        headers.Add(HttpRequestHeader.Host, "d.10jqka.com.cn");
                        headers.Add(HttpRequestHeader.Referer, "http://data.10jqka.com.cn/funds/ggzjl/");
                        headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
                        headers.Add(HttpRequestHeader.Cookie, "_ga=GA1.3.1308637288.1480656705; Hm_lvt_f79b64788a4e377c608617fba4c736e2=1490269470; searchGuide=sg; spversion=20130314; __utma=156575163.1308637288.1480656705.1500515050.1510209522.23; __utmz=156575163.1510209522.23.17.utmcsr=10jqka.com.cn|utmccn=(referral)|utmcmd=referral|utmcct=/; Hm_lvt_78c58f01938e4d85eaf619eae71b4ed1=1509994430,1510022996,1510068180,1510300758; historystock=300201%7C*%7C601989%7C*%7C002156%7C*%7C600807; log=; v=AfoYrKP4d7j2x_gcqfWa-6RZSysYq39G8C3yOgTzpzFYPpSd7DvOlcC_QirU");


                        var html = httpdownloader.GetGzip2(url.Value,Encoding.GetEncoding("GBK"),headers);
                        var item = new
                        {
                            StockCode = queueItem.Key,
                            StockName = queueItem.Value,
                            ContentType = url.Key,
                            Url = url.Value,
                            CreatTime = DateTime.Now,
                            Page = html,
                            MD5 = Convertor.Encode_MD5(html),
                            TaskID = THS.CONST.TaskID
                        };
                        var collectionName =THS.CONST.PageKLine;
                        mongo.Save(THS.CONST.DBName, collectionName, item);
                        Console.WriteLine("已保存 " + url.Key + " " + url.Value);
                        Thread.Sleep(1000);

                    }

                }

            }


        }



        #endregion

        #region 下载金融街每个股票的日线页面
        /// <summary>
        /// 下载每个股票的页面 UTF8
        /// </summary>
        public void GetPageKLineJRJ()
        {
            this.GetALLStockCode(); ///准备所有股票代码
            var queueUrl = new Queue<string>();

            var httpdownloader = new HTTP(Encoding.UTF8);
            httpdownloader.EventException += (object sender, EventArgs e) => {
                var ee = e as EventProcEventArgs;
                Console.WriteLine("下载失败 {0}", ee.Default);
                var item = ee.Default as Dictionary<string, object>;
                item["ContentType"] = "JRJ个股日线页面下载失败异常信息";
                item["TaskID"] = THS.CONST.TaskID;

                var url = item["Url"].ToString();
                var type = string.Empty;

                mongo.Save(THS.CONST.DBName, THS.CONST.Exception, item);
                #region 类型判断

                if (url.Contains("/01/last.js"))
                {
                    type = "日线数据";
                }
                #endregion

                Thread.Sleep(60 * 1000);
            };

            if (null != this.stockCodeDict)
            {
                foreach (var item in this.stockCodeDict)
                {
                    queueUrl.Enqueue(item.Key);///Key：StockCode Value:StockName
                }
            }
            var jrj = new DataSourceJRJ();
            while (0 < queueUrl.Count)
            {
                var stockCode = queueUrl.Dequeue();


                var html = jrj.GetKLine(stockCode);
                var item = new
                {
                    StockCode = stockCode,
                    StockName = this.stockCodeDict[stockCode],
                    ContentType = "JRJ日线数据",
                    Url = string.Format("http://flashdata2.jrj.com.cn/history/js/share/{0}/other/dayk_ex.js?random=1510076545082", stockCode),
                    CreatTime = DateTime.Now,
                    Page = html,
                    MD5 = Convertor.Encode_MD5(html),
                    TaskID = THS.CONST.TaskID
                };
                var collectionName = THS.CONST.PageKLine;
                mongo.Save(THS.CONST.DBName, collectionName, item);
                Console.WriteLine("已保存 " + stockCode + " " + queueUrl.Count);

                Thread.Sleep(1000);


            }


        }



        #endregion

        #region 下载SINA每个股票的日线页面
        /// <summary>
        /// 下载每个股票的页面 UTF8
        /// </summary>
        public void GetPageKLineSINA()
        {
            this.GetALLStockCode(); ///准备所有股票代码
            var queueUrl = new Queue<string>();

            var httpdownloader = new HTTP(Encoding.UTF8);
            httpdownloader.EventException += (object sender, EventArgs e) => {
                var ee = e as EventProcEventArgs;
                Console.WriteLine("下载失败 {0}", ee.Default);
                var item = ee.Default as Dictionary<string, object>;
                item["ContentType"] = "SINA个股日线页面下载失败异常信息";
                item["TaskID"] = THS.CONST.TaskID;

                var url = item["Url"].ToString();
                var type = string.Empty;

                mongo.Save(THS.CONST.DBName, THS.CONST.Exception, item);
 

                Thread.Sleep(60 * 1000);
            };

            if (null != this.stockCodeDict)
            {
                foreach (var item in this.stockCodeDict)
                {
                    queueUrl.Enqueue(item.Key);///Key：StockCode Value:StockName
                }
            }
            var sina = new SinaFin();
            while (0 < queueUrl.Count)
            {
                var stockCode = queueUrl.Dequeue();
                var year = DateTime.Now.Year;
                for (int jidu = 1; jidu <= 4; jidu++)
                {
                    var html = sina.GetKLine(stockCode, year, jidu);
                    var item = new
                    {
                        StockCode = stockCode,
                        StockName = this.stockCodeDict[stockCode],
                        ContentType = "SINA日线数据",
                        Url = string.Format("http://vip.stock.finance.sina.com.cn/corp/go.php/vMS_MarketHistory/stockid/{0}.phtml?year={1}&jidu={2}", stockCode, year, jidu),
                        CreatTime = DateTime.Now,
                        Page = html,
                        MD5 = Convertor.Encode_MD5(html),
                        TaskID = THS.CONST.TaskID,
                        JiDu=jidu,
                        Year=year
                    };
                    var collectionName = THS.CONST.PageKLine;
                    mongo.Save(THS.CONST.DBName, collectionName, item);
                    Console.WriteLine("已保存 " + stockCode + " " + queueUrl.Count);

                    Thread.Sleep(1000);
                }
            }


        }



        #endregion

        #region 下载龙虎榜个股及明细页面
        /// <summary>
        /// 龙虎榜个股及明细
        /// </summary>
        public void GetStockLHB()
        {

            this.GetALLStockCode(); ///准备全部股票代码
            Console.WriteLine("准备下载龙虎榜个股及明细页面");
            var httpdownloader = new HTTP("gbk");
             
            #region 准备下载队列
            var queueUrl = new Queue<URL>(); ///key :url value 失败次数
            var queueMxUrl = new Queue<URL>();///龙虎榜明细队列
            foreach (var stock in this.stockCodeDict)
            {
                var urlgglhb = new URL(string.Format("http://data.10jqka.com.cn/market/lhbgg/code/{0}/", stock.Key), message : "个股龙虎榜");

                queueUrl.Enqueue(urlgglhb);
            }
            #endregion
             
            var excetionDict = new Dictionary<string, int>();
            while (0 < queueUrl.Count) ///下载个股龙虎榜准备龙虎榜明细队列
            {
                var qItem = queueUrl.Dequeue();
                var urlgglhb = qItem;///个股龙虎榜URL
 
                var stockCode = urlgglhb.Url.Substring(urlgglhb.Url.Length - 8).Replace("/", string.Empty);//http://data.10jqka.com.cn/market/lhbgg/code/600360/ //http://data.10jqka.com.cn/ifmarket/getnewlh/code/601228/date/2017-04-14/rid/7/
                var refID = Guid.NewGuid().ToString().Replace("-", string.Empty);

                var headers = new Dictionary<string, string>();
                headers.Add("Accept","text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                headers.Add("Accept-Encoding","gzip, deflate");
                headers.Add("Accept-Language","zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4");
                headers.Add("Host","data.10jqka.com.cn");
                headers.Add("Referer","http://data.10jqka.com.cn/market/longhu/");
                headers.Add("User-Agent","Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");

                 
                var pagegglhbHtml = httpdownloader.GetGzip(urlgglhb.Url,Encoding.GetEncoding("GBK"), headers);///下载的个股龙虎榜页面
                if (100 < pagegglhbHtml.Length) ///若下载的有数据
                {
                    #region 分析个股龙虎榜明细
                    var tableStartIndex = pagegglhbHtml.IndexOf("<tbody>"); ///个股龙虎榜表格数据开始位置
                    var tableEndIndex = pagegglhbHtml.IndexOf("</tbody>");///个股龙虎榜表格数据结束位置
                    var tableHtml = pagegglhbHtml.Substring(tableStartIndex, tableEndIndex - tableStartIndex);
                    var tdArray = tableHtml.Split(new string[] { "<td>", "</td>" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var td in tdArray)
                    {
                        if (td.Contains("rid") && td.Contains("date"))
                        {
                            var paramArray = td.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            var date = string.Empty;
                            var rid = string.Empty;
                            var code = string.Empty;
                            foreach (var param in paramArray)
                            {
                                if (param.Contains("code="))
                                {
                                    code = param.Substring(6, 6);
                                }
                                else if (param.Contains("date="))
                                {
                                    date = param.Substring(6, 10);
                                }
                                else if (param.Contains("rid="))
                                {
                                    rid = param.Substring(5, 2).Replace("\"", string.Empty);
                                }

                                if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(date) && !string.IsNullOrWhiteSpace(rid))
                                {
                                    var urlgglhbmx = string.Format("http://data.10jqka.com.cn/ifmarket/getnewlh/code/{0}/date/{1}/rid/{2}/", code, date, rid); ///个股龙虎榜明细


                                    var mxUrl = new URL(urlgglhbmx);
                                    mxUrl.Data["StockCode"] = code;
                                    mxUrl.Data["Date"] = date;
                                    mxUrl.Data["Rid"] = rid;
                                    mxUrl.Data["ParentUrl"] = urlgglhb.Url;
                                    mxUrl.Data["RefID"] = refID;
                                    mxUrl.Message = "个股龙虎榜明细";

                                    queueMxUrl.Enqueue(mxUrl);

                                }
                            }
                        }
                    }
                    #endregion


                    #region 个股龙虎榜数据保存
                    var gglhbData = new //个股龙虎榜 数据
                    {
                        StockCode = stockCode,
                        StockName = this.stockCodeDict[stockCode],
                        Url = urlgglhb,
                        CreateTime = DateTime.Now,
                        Page = pagegglhbHtml,
                        ContentType = "个股龙虎榜",
                        RefID = refID,///和明细保持一致
                        MD5 = Convertor.Encode_MD5(pagegglhbHtml),
                        TaskID = THS.CONST.TaskID
                    };

                    mongo.Save(THS.CONST.DBName, THS.CONST.PageGGLHB, gglhbData); ///个股龙虎榜
                    Console.WriteLine("个股龙虎榜 数据保存 {0} {1} {2}", stockCode, this.stockCodeDict[stockCode], queueUrl.Count);
                    #endregion
                }
                else
                {
                    if (excetionDict.ContainsKey(qItem.Url) && 3 <= excetionDict[qItem.Url])
                    {
                        ///写异常信息
                        Console.WriteLine("下载失败 {0}", qItem.Url);
                        var item = new Dictionary<string, object>();
                        item["ContentType"] = "龙虎榜下载失败异常信息";
                        item["TaskID"] = THS.CONST.TaskID;
                        item["CreateTime"] = DateTime.Now;
                        item["Data"] = Convertor.FromObjectToDictionary(qItem);

                        mongo.Save(THS.CONST.DBName, THS.CONST.Exception, item);
                        Thread.Sleep(60 * 1000);
                    }
                    else
                    {
                        excetionDict[qItem.Url] = (excetionDict.ContainsKey(qItem.Url)) ? excetionDict[qItem.Url] + 1 : 0;
                        queueUrl.Enqueue(qItem);
                    }

                }
            }


            var excetionMxDict = new Dictionary<string, int>();
            while (0 < queueMxUrl.Count)
            {
                var qItem = queueMxUrl.Dequeue();

                var headers = new Dictionary<string, string>();
                headers.Add("Accept","*/*");
                headers.Add("Accept-Encoding","gzip, deflate");
                headers.Add("Accept-Language","zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.4");
                headers.Add("Host","data.10jqka.com.cn");
                headers.Add("Referer",string.Format("http://data.10jqka.com.cn/market/lhbgg/code/{0}/", qItem.Data["StockCode"].ToString()));
                headers.Add("User-Agent","Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
                headers.Add("X-Requested-With", "XMLHttpRequest");

                ///下载明细
                var subHtml = httpdownloader.GetGzip(qItem.Url, Encoding.GetEncoding("GBK"), headers);
                if (50 < subHtml.Length)
                {
                    var stockCode = qItem.Data["StockCode"].ToString();
                    var date = qItem.Data["Date"].ToString();
                    var rid = qItem.Data["Rid"].ToString();
                    var parentUrl = qItem.Data["ParentUrl"].ToString();
                    var refID = qItem.Data["RefID"].ToString();

                    var gglhbmxData = new
                    {
                        StockCode = qItem.Data["StockCode"].ToString(),
                        StockName = this.stockCodeDict[stockCode],
                        Date = date,
                        Rid = rid,
                        ParentUrl = parentUrl,
                        Url = qItem.Url,
                        CreateTime = DateTime.Now,
                        Page = subHtml,
                        ContentType = "个股龙虎榜明细",
                        RefID = refID,
                        MD5 = Convertor.Encode_MD5(subHtml),
                        TaskID = THS.CONST.TaskID
                    };

                    mongo.Save(THS.CONST.DBName, THS.CONST.PageGGLHB, gglhbmxData);
                    Console.WriteLine("个股龙虎榜明细 数据保存 {0} {1} {2}", stockCode, this.stockCodeDict[stockCode], queueMxUrl.Count);
                    ///若能正常下载则删除
                    if (excetionMxDict.ContainsKey(qItem.Url))
                    {
                        ///正常下载完毕删除异常信息
                        excetionMxDict.Remove(qItem.Url);
                    }
                }
                else
                {
                    Console.WriteLine("下载异常 {0} ", qItem.Url);

                    if (excetionMxDict.ContainsKey(qItem.Url) && 3 <= excetionMxDict[qItem.Url])
                    {
                        ///写异常信息
                        Console.WriteLine("下载失败 {0}", qItem.Url);
                        var item = new Dictionary<string, object>();
                        item["ContentType"] = "龙虎榜明细下载失败异常信息";
                        item["TaskID"] = THS.CONST.TaskID;
                        item["CreateTime"] = DateTime.Now;
                        item["Data"] = qItem;
                        mongo.Save(THS.CONST.DBName, THS.CONST.Exception, item);
                        Thread.Sleep(60 * 1000);
                    }
                    else
                    {
                        ///重新加入队列
                        excetionMxDict[qItem.Url] = (excetionMxDict.ContainsKey(qItem.Url)) ? excetionMxDict[qItem.Url] + 1 : 0;
                        queueMxUrl.Enqueue(qItem);
                    }
                }

            }
 
        }


        #endregion

 


        /// <summary>
        /// 获取今日新数据
        /// </summary>
        public void GetTodayNewData()
        {
            //this.GetStockLHB();///获取个股龙虎榜数据
            //this.GetPageStock();///获取每个股票页面的数据
            //this.GetPageKLine();//获取日线信息
            //this.GetPageKLineJRJ();
            this.GetPageKLineSINA();
        }

    }
}
