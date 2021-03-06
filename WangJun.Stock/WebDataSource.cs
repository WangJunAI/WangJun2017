﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using WangJun.Net;
using WangJun.NetLoader;
using WangJun.Utility;

namespace WangJun.Stock
{
    public class WebDataSource
    {
        public static WebDataSource GetInstance()
        {
            var inst = new WebDataSource();

            return inst;
        }
        public Dictionary<string, string> GetAllStockCode(string source = "THS")
        {
            var res = new Dictionary<string, string>();
            if ("THS" == source.ToUpper())
            {
                res = DataSourceTHS.CreateInstance().GetAllStockCode();
            }

            return res;
        }

        public static string GetDataFromHttpAgent(string url)
        {
            var httpDownloader = new HTTP();
            var html = httpDownloader.GetGzip2(url, Encoding.UTF8);
            return html;
        }

        #region 获取SINA财务摘要数据
        /// <summary>
        /// 获取SINA财务摘要数据
        /// </summary>
        /// <param name="stockCode"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public object GetCWZY(string stockCode, string source = "SINA")
        {
            var res = new object();
            if ("SINA" == source.ToUpper() && !string.IsNullOrWhiteSpace(stockCode))//  //WebDataSource.GetDataFromHttpAgent("http://aifuwu.wang/API.ashx?c=WangJun.Stock.DataSourceSINA&m=GetCWZY&p=6018888");//
            {
                var html = DataSourceSINA.GetInstance().GetCWZY(stockCode);
                var resDict = NodeService.Get(CONST.NodeServiceUrl, "新浪", "GetDataFromHtml", new { ContentType = "SINA财务摘要", Page = html });
                return resDict;
            }

            return res;
        }

        #endregion

        #region 获取SINA公司简介数据
        /// <summary>
        /// 获取SINA公司简介数据
        /// </summary>
        /// <param name="stockCode"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public object GetGSJJ(string stockCode, string source = "SINA")
        {
            var res = new object();

            if ("SINA" == source.ToUpper() && !string.IsNullOrWhiteSpace(stockCode)) //
            {
                var html = DataSourceSINA.GetInstance().GetGSJJ(stockCode);
                if (string.IsNullOrWhiteSpace(html))
                {
                    html = WebDataSource.GetDataFromHttpAgent(string.Format("http://aifuwu.wang/API.ashx?c=WangJun.Stock.DataSourceSINA&m=GetGSJJ&p={0}", stockCode));
                }

                var resDict = NodeService.Get(CONST.NodeServiceUrl, "新浪", "GetDataFromHtml", new { ContentType = "SINA公司简介", Page = html });
                return resDict;
            }

            return res;
        }
        #endregion

        #region 获取SINA大单数据
        public object GetSINADaDan()
        {
            return "";
        }
        #endregion

        #region 获取SINA日线数据
        /// <summary>
        /// 获取SINA日线数据
        /// </summary>
        /// <param name="stockCode"></param>
        /// <param name="year"></param>
        /// <param name="jidu"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetSINAKLineDay(string stockCode, int year, int jidu, string source = "SINA")
        {
            var html = DataSourceSINA.GetInstance().GetLSJY(stockCode, year, jidu);
            var res = NodeService.Get(CONST.NodeServiceUrl, "新浪", "GetDataFromHtml", new { ContentType = "SINA历史交易", Page = html });// 
            var resDict = res as Dictionary<string, object>;
            return resDict["PageData"] as Dictionary<string, object>;
        }
        #endregion

        #region 获取SINA板块概念
        /// <summary>
        /// 获取SINA板块概念
        /// </summary>
        /// <param name="stockCode"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public object GetBKGN(string stockCode, string source = "SINA")
        {
            var res = new object();

            if ("SINA" == source.ToUpper() && !string.IsNullOrWhiteSpace(stockCode)) //
            {
                var html = DataSourceSINA.GetInstance().GetBKGN(stockCode);
                if (string.IsNullOrWhiteSpace(html))
                {
                    html = WebDataSource.GetDataFromHttpAgent(string.Format("http://aifuwu.wang/API.ashx?c=WangJun.Stock.DataSourceSINA&m=GetBKGN&p={0}", stockCode));
                }

                var resDict = NodeService.Get(CONST.NodeServiceUrl, "新浪", "GetDataFromHtml", new { ContentType = "SINA板块概念", Page = html });
                return resDict;
            }

            return res;
        }
        #endregion

        #region 获取THS资金流向
        /// <summary>
        /// 获取THS资金流向
        /// </summary>
        /// <param name="stockCode"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public object GetZJLX(string stockCode, string source = "THS")
        {
            var res = new object();

            if ("THS" == source.ToUpper() && !string.IsNullOrWhiteSpace(stockCode)) //
            {
                var html = DataSourceTHS.CreateInstance().GetJZLX(stockCode);
                if (string.IsNullOrWhiteSpace(html))
                {
                    html = WebDataSource.GetDataFromHttpAgent(string.Format("http://aifuwu.wang/API.ashx?c=WangJun.Stock.DataSourceTHS&m=GetZJLX&p={0}", stockCode));
                }

                var resDict = NodeService.Get(CONST.NodeServiceUrl, "同花顺", "GetDataFromHtml", new { ContentType = "THS资金流向", Page = html });
                return resDict;
            }

            return res;
        }
        #endregion

        #region 获取SINA股票雷达
        /// <summary>
        /// 获取SINA股票雷达
        /// </summary>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetStockRadar()
        {
            var tradingDate = Convertor.CalTradingDate(DateTime.Now, "00:00:00");
            var listHtml = DataSourceSINA.GetInstance().GetStockRadar();
            var listData = new List<Dictionary<string, object>>();
            foreach (var html in listHtml)
            {
                var resDict = NodeService.Get(CONST.NodeServiceUrl, "新浪", "GetDataFromHtml", new { ContentType = "SINA股市雷达", Page = html }) as Dictionary<string, object>;
                var pageData = resDict["PageData"] as ArrayList;
                if (pageData is ArrayList)
                {
                    var itemList = pageData as ArrayList;
                    foreach (Dictionary<string, object> item in itemList)
                    {
                        var dict = item;
                        dict["异动时间"] = Convertor.CalTradingDate(DateTime.Now, item["异动时间"].ToString());
                        listData.Add(dict);
                    }
                }
            }

            return listData;
        }
        #endregion

        #region 获取THS龙虎榜数据
        /// <summary>
        /// 获取THS龙虎榜数据
        /// </summary>
        /// <param name="stockCode"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public List<object> GetLHB(string stockCode, string source = "THS")
        {
            var resList = new List<object>();

            if ("THS" == source.ToUpper() && !string.IsNullOrWhiteSpace(stockCode)) //
            {
                var lhbHtml = DataSourceTHS.CreateInstance().GetGGLHB(stockCode);
                if (string.IsNullOrWhiteSpace(lhbHtml))
                {
                    lhbHtml = WebDataSource.GetDataFromHttpAgent(string.Format("http://aifuwu.wang/API.ashx?c=WangJun.Stock.DataSourceTHS&m=GetGGLHB&p={0}", stockCode));
                }

                var lhbDict = NodeService.Get(CONST.NodeServiceUrl, "同花顺", "GetDataFromHtml", new { ContentType = "THS个股龙虎榜", Page = lhbHtml });
                resList.Add((lhbDict as Dictionary<string, object>)["PageData"]);
                ThreadManager.Pause(seconds: 2);
                var lhbmxUrlList = DataSourceTHS.CreateInstance().GetUrlGGLHBMX(lhbHtml);
                foreach (var lhbMXUrl in lhbmxUrlList)
                {
                    var lhbMXHtml = DataSourceTHS.CreateInstance().GetGGLHBMX(stockCode, lhbMXUrl);
                    if (string.IsNullOrWhiteSpace(lhbMXHtml))
                    {
                        //lhbHtml = WebDataSource.GetDataFromHttpAgent(string.Format("http://aifuwu.wang/API.ashx?c=WangJun.Stock.DataSourceTHS&m=GetGGLHBMX&p={0}", stockCode));
                    }
                    var lhbMXDict = NodeService.Get(CONST.NodeServiceUrl, "同花顺", "GetDataFromHtml", new { ContentType = "THS个股龙虎榜明细", Page = lhbMXHtml });
                    resList.Add((lhbMXDict as Dictionary<string, object>)["PageData"]);
                    ThreadManager.Pause(seconds: 1);
                }
            }

            return resList;
        }
        #endregion

        #region 获取SINA融资融券
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stockCode"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public ArrayList GetRZRQ(string stockCode, string source = "SINA")
        {
            var res = new ArrayList();

            if ("SINA" == source.ToUpper() && !string.IsNullOrWhiteSpace(stockCode)) //
            {
                var rzrqHtml = DataSourceSINA.GetInstance().GetRZRQ(stockCode);
                var rzrqDict = NodeService.Get(CONST.NodeServiceUrl, "新浪", "GetDataFromHtml", new { ContentType = "SINA融资融券", Page = rzrqHtml });
                var pageData = (rzrqDict as Dictionary<string, object>)["PageData"] as Dictionary<string, object>;
                return pageData["Rows"] as ArrayList;
            }
            return res;
        }
        #endregion

        #region 头条新闻
        public List<Dictionary<string, object>> GetTouTiaoSearch(string keyword)
        {
            var list = new List<Dictionary<string, object>>();
            var htmlList = DataSourceTouTiao.GetInstance().GetSearchResult(keyword);
            if (null != htmlList && 0 < htmlList.Count)
            {
                foreach (var html in htmlList)
                {
                    var resDict = Convertor.FromJsonToDict2(html);
                    var dataList = resDict["data"] as ArrayList;

                    foreach (Dictionary<string, object> dataItem in dataList)
                    {
                        if (dataItem.ContainsKey("article_url") && dataItem.ContainsKey("title") && dataItem.ContainsKey("abstract") 
                            && dataItem.ContainsKey("create_time") && dataItem.ContainsKey("media_url") && dataItem.ContainsKey("media_avatar_url"))
                        {
                            var url = dataItem["article_url"].ToString();
                            var source = dataItem["source"];
                            var media_url = dataItem["media_url"];
                            var imageUrl = dataItem.ContainsKey("image_url") ? dataItem["image_url"] : "";
                            var media_avatar_url = dataItem["media_avatar_url"];
                            var title = dataItem["title"];
                            var summary = dataItem["abstract"];
                            var createTime = dataItem["create_time"];
                            if (url.StartsWith("http://toutiao.com/group/"))
                            {
                                var articleString = DataSourceTouTiao.GetInstance().GetArticle(url);
                                if (!string.IsNullOrWhiteSpace(articleString))
                                {
                                    var startIndex = articleString.IndexOf("content:") + "content:".Length;
                                    var endIndex = articleString.IndexOf("groupId:");
                                    articleString = articleString.Substring(startIndex, endIndex - startIndex);
                                    var item = new Dictionary<string, object> {
                                        { "Title",  title}
                                        , { "Summary",summary }
                                        , { "Content", articleString }
                                        , { "ImageUrl",  media_avatar_url}
                                        , { "CreateTime", new DateTime(1970,1,1).AddSeconds(long.Parse(createTime.ToString()))}
                                        , { "CreatorName", source }
                                        , { "CreatorID",media_url }
                                        ,{ "CreatorPic",media_avatar_url }
                                    };
                                    list.Add(item);
                                }
                            }
                        }
                    }

                }
            }

            return list;
        }
        #endregion
    }
}
