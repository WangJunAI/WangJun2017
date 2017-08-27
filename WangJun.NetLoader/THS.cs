using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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





        #region 获取当日所有股票简要原始数据
        /// <summary>
        /// 获取当日所有股票简要原始数据
        /// 获取链接:http://q.10jqka.com.cn/index/index/board/all/field/zdf/order/desc/page/1/ajax/1/
        /// </summary>
        public void GetTodayStockSummaryInfo()
        {
            #region 获取分页数 
            var pageCount = 0;
            var pagerUrl = string.Format(@"http://q.10jqka.com.cn/index/index/board/all/field/zdf/order/desc/page/{0}/ajax/1/", 1);
            var pagerHtml = http.GetGzip(pagerUrl);
            if (100 < pagerHtml.Length)
            {
                pageCount = int.Parse(pagerHtml.Substring(pagerHtml.LastIndexOf("</span>") - 3, 3)); ///页码数
            }
            #endregion




            for (int i = 1; i <= pageCount; i++)
            {
                var url = string.Format(@"http://q.10jqka.com.cn/index/index/board/all/field/zdf/order/desc/page/{0}/ajax/1/",i);
                var html = http.GetGzip(url);


                #region 股票代码处理
                var subHtmlIndex = html.IndexOf("http://stockpage.10jqka.com.cn/");
                var stockCode = html.Substring(subHtmlIndex + "http://stockpage.10jqka.com.cn/".Length, 6);
                var stockName = html.Substring(html.IndexOf("<td class=\"c-rise\">") - 50, 30).Replace("target=\"_blank\">", string.Empty).Replace("</a></td>", string.Empty); //" target=\"_blank\">N昭衍</a></td>\n" string

                #endregion

                var item = new {
                    PageIndex = i,
                    CreateTime = DateTime.Now,
                    Data = html,
                    StockCode = stockCode,
                    StockName = stockName
                };
                mongo.Save("ths", "name", item);
            }
        }
        #endregion

        #region 获取所有的股票代码
        /// <summary>
        /// 获取所有的股票代码
        /// </summary>
        public void GetALLStockCode()
        {
            ///先检查本地或数据库是否有,若有就用,否则从网络获取
            ///
            if (File.Exists(@"E:\stockCode.txt"))
            {
                var lines = File.ReadLines(@"E:\stockCode.txt");
                this.stockCodeDict.Clear();
                foreach (var item in lines)
                {
                    var arr = item.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if(!this.stockCodeDict.ContainsKey(arr[0])) ///若不包含股票号
                    {
                        this.stockCodeDict.Add(arr[0], arr[1]);
                    }
                }
                Console.WriteLine("股票初始化完毕");
            }
            else
            {
                #region 获取分页数 
                var pageCount = 0;
                var pagerUrl = string.Format(@"http://q.10jqka.com.cn/index/index/board/all/field/zdf/order/desc/page/{0}/ajax/1/", 1);
                var pagerHtml = http.GetGzip(pagerUrl);
                if (100 < pagerHtml.Length)
                {
                    pageCount = int.Parse(pagerHtml.Substring(pagerHtml.LastIndexOf("</span>") - 3, 3)); ///页码数
                }
                #endregion
                var stringBuilder = new StringBuilder();

                for (int i = 1; i <= pageCount; i++)
                {
                    var url = string.Format(@"http://q.10jqka.com.cn/index/index/board/all/field/zdf/order/desc/page/{0}/ajax/1/", i);
                    var html = http.GetGzip(url);


                    #region 股票代码处理

                    string[] trArray = html.Substring(html.IndexOf("<tbody>") + "<tbody>".Length).Split(new string[] { "<tr>", "</tr>" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var tr in trArray)
                    {
                        var tdArray = tr.Split(new string[] { "<td>", "</td>" }, StringSplitOptions.RemoveEmptyEntries);
                        if (10 < tdArray.Length)
                        {
                            var stockCode = tdArray[3].Substring(tdArray[3].IndexOf("target=\"_blank\">") + "target=\"_blank\">".Length).Replace("</a>", string.Empty);
                            var stockName = tdArray[5].Substring(tdArray[5].IndexOf("target=\"_blank\">") + "target=\"_blank\">".Length).Replace("</a>", string.Empty);
                            stockCodeDict.Add(stockCode, stockName);
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
            }
        }
        #endregion

        #region 下载每个股票的页面
        /// <summary>
        /// 下载每个股票的页面 UTF8
        /// </summary>
        public void GetStockPage()
        {
            var httpdownloader = new HTTP(Encoding.UTF8);
            this.GetALLStockCode();
            if (null != this.stockCodeDict)
            {
                foreach (var item in this.stockCodeDict)
                {
                    var urlList = new List<KeyValuePair<string,string>>();
                    urlList.Add(new KeyValuePair<string, string>("首页概览", string.Format("http://stockpage.10jqka.com.cn/{0}/",item.Key.Trim()))); ///首页概览
                    urlList.Add(new KeyValuePair<string, string>("资金流向", string.Format("http://stockpage.10jqka.com.cn/{0}/funds/", item.Key.Trim()))); ///资金流向
                    urlList.Add(new KeyValuePair<string, string>("公司资料", string.Format("http://stockpage.10jqka.com.cn/{0}/company/", item.Key.Trim()))); ///公司资料
                    urlList.Add(new KeyValuePair<string, string>("新闻公告", string.Format("http://stockpage.10jqka.com.cn/{0}/news/", item.Key.Trim()))); ///新闻公告
                    urlList.Add(new KeyValuePair<string, string>("财务分析", string.Format("http://stockpage.10jqka.com.cn/{0}/finance/", item.Key.Trim()))); ///财务分析
                    urlList.Add(new KeyValuePair<string, string>("经营分析", string.Format("http://stockpage.10jqka.com.cn/{0}/operate/", item.Key.Trim()))); ///经营分析
                    urlList.Add(new KeyValuePair<string, string>("股东股本", string.Format("http://stockpage.10jqka.com.cn/{0}/holder/", item.Key.Trim()))); ///股东股本
                    urlList.Add(new KeyValuePair<string, string>("主力持仓", string.Format("http://stockpage.10jqka.com.cn/{0}/position/", item.Key.Trim()))); ///主力持仓
                    urlList.Add(new KeyValuePair<string, string>("公司大事", string.Format("http://stockpage.10jqka.com.cn/{0}/event/", item.Key.Trim()))); ///公司大事
                    urlList.Add(new KeyValuePair<string, string>("分红融资", string.Format("http://stockpage.10jqka.com.cn/{0}/bonus/", item.Key.Trim()))); ///分红融资
                    urlList.Add(new KeyValuePair<string, string>("价值分析", string.Format("http://stockpage.10jqka.com.cn/{0}/worth/", item.Key.Trim()))); ///价值分析
                    urlList.Add(new KeyValuePair<string, string>("行业分析",string.Format("http://stockpage.10jqka.com.cn/{0}/field/", item.Key.Trim()))); ///行业分析

                    foreach (var url in urlList)
                    {
                        var html = httpdownloader.GetString(url.Value);
                        var data = new {
                            StockCode=item.Key,
                            StockName = item.Value,
                            ContentType = url.Key,
                            CreatTime = DateTime.Now,
                            Page = html
                        };
                        mongo.Save("ths", "Page", data);
                        Console.WriteLine("已保存 " + url.Key+ " "+ url.Value );
                    }

                }
            }
        }

        #endregion

        #region 龙虎榜个股及明细
        /// <summary>
        /// 龙虎榜个股及明细
        /// </summary>
        public void LHB()
        {

            this.GetALLStockCode(); ///获取全部股票代码

            var httpdownloader = new HTTP("gbk");
            httpdownloader.EventException += (object sender,EventArgs e)=> {
                var ee = e as EventProcEventArgs;
                Console.WriteLine("下载失败 {0}",ee.Default);
                mongo.Save("ths", "exception", ee.Default); ///个股龙虎榜明细
                Console.ReadKey();
            };
            var count = 0;
            if (null != this.stockCodeDict)
            {
                foreach (var item in this.stockCodeDict)
                {
                    var url1 = string.Format("http://data.10jqka.com.cn/market/lhbgg/code/{0}/", item.Key);//
                    var pageHtml = httpdownloader.GetGzip(url1);

                    if (100 < pageHtml.Length)
                    {
                        if (true)
                        {
                            #region 分析明细
                            var tableStartIndex = pageHtml.IndexOf("<tbody>"); ///表格数据开始位置
                            var tableEndIndex = pageHtml.IndexOf("</tbody>");///表格数据结束位置
                            var tableHtml = pageHtml.Substring(tableStartIndex, tableEndIndex - tableStartIndex);
                            var tdArray = tableHtml.Split(new string[] { "<td>", "</td>" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var td in tdArray)
                            {
                                if (td.Contains("rid") && td.Contains("date"))
                                {
                                    var paramArray = td.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                    var date = "";
                                    var rid = "";
                                    var code = "";
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
                                            //Console.WriteLine("{3}code={0}\tdate={1}\trid={2}\t", code, date, rid,this.stockCodeDict[code]);
                                            var url2 = string.Format("http://data.10jqka.com.cn/ifmarket/getnewlh/code/{0}/date/{1}/rid/{2}/", code, date, rid);
                                            var subHtml = httpdownloader.GetGzip(url2);
                                            
                                            var subData = new
                                            {
                                                Code = code,
                                                Date = date,
                                                Rid = rid,
                                                ParentUrl = url1,
                                                Url = url2,
                                                CreateTime = DateTime.Now,
                                                Page = subHtml
                                            };
                                            mongo.Save("ths", "gglhbmx", subData); ///个股龙虎榜明细
                                            //Thread.Sleep(2000);
                                        }
                                    }
                                }
                            }

                            #endregion
                        }

                        #region 数据保存
                        var data = new
                        {
                            StockCode = item.Key,
                            StockName = item.Value,
                            Url = url1,
                            CreateTime = DateTime.Now,
                            Page = pageHtml
                        };

                        mongo.Save("ths", "gglhb2", data); ///个股龙虎榜
                        Console.WriteLine("龙虎榜数据保存 {0} {1} {2}", item.Key, item.Value,++count);
                        //Thread.Sleep(2000);
                        #endregion
                    }
                }
            }
        }

        private void Httpdownloader_EventException(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region 获取今日龙虎榜
        /// <summary>
        /// 获取今日龙虎榜
        /// </summary>

        public void GetTodayLHB()
        {
            var httpdownloader = new HTTP("gbk");
            var url = "http://data.10jqka.com.cn/market/longhu/";
            var html = httpdownloader.GetGzip(url);

            var data = new {
                CreateTime=DateTime.Now,
                Page = html
            };
            mongo.Save("ths", "jrlhb", data); ///个股龙虎榜

        }

        #endregion

        #region 新闻更新

        #endregion
    }
}
