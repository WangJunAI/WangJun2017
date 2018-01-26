using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.DB;
using WangJun.Tools;

namespace WangJun.Doc
{
    /// <summary>
    /// 
    /// </summary>
    public class DocRunner
    {
        /// <summary>
        /// 变更记录
        /// </summary>
        /// <param name="item"></param>
        public void LogChange(ChangeItem item)
        {

        }

        /// <summary>
        /// 数据维护
        /// </summary>
        public void MaintainData() {
            ///根据更改项检查修正数据
        }

        /// <summary>
        /// 热词分析 分词 发文/修改统计
        /// </summary>
        public void DataAnalyse()
        {
            var startTime = DateTime.Now;///开始运行时间
            Console.Title = "文档维护进程进程 启动时间：" + startTime;
            while (true)
            {
                ///遍历写入数据
                var dbName = CONST.DB.DBName_DocService;
                var collectionName = CONST.DB.CollectionName_DocItem;
                var db = DataStorage.GetInstance(DBType.MongoDB);
                db.EventTraverse += (object sender, EventArgs e) =>
                {
                    EventProcEventArgs ee = e as EventProcEventArgs;
                ///正文分析
                ///文章统计
                var data = ee.Default as Dictionary<string, object>;
                    if (data.ContainsKey("PlainText") && data["PlainText"] is string)
                    {
                        var content = data["PlainText"].ToString();
                        var title = data["Title"].ToString();
                        var res1 = FenCi.GetResult(content); 
  
                        foreach (var item in res1)
                        {
                            var svItem = new
                            {
                                id = data["id"],
                                Word = item.Key,
                                Count = item.Value,
                                CreateTime = DateTime.Now,
                                Source = "Content"
                            };
                            var filter = "{\"id\":\"" + svItem.id + "\",\"Word\":\"" + svItem.Word + "\",\"Source\":\"Content\"}";

                            db.Save3(dbName, "FenCi", svItem, filter);
                        }
                         
                    }

                    if (data.ContainsKey("Title") && data["Title"] is string)
                    { 
                        var title = data["Title"].ToString(); 
                        var res2 = FenCi.GetResult(title);
 
                        foreach (var item in res2)
                        {
                            var svItem = new
                            {
                                id = data["_id"].ToString(),
                                Word = item.Key,
                                Count = item.Value,
                                CreateTime = DateTime.Now,
                                Source = "Title"
                            };
                            var filter = "{\"id\":\"" + svItem.id + "\",\"Word\":\"" + svItem.Word + "\",\"Source\":\"Title\"}";

                            db.Save3(dbName, "FenCi", svItem, filter);
                        }
                    }
                };
                db.Traverse(dbName, collectionName, "{}");
                ThreadManager.Pause(minutes: 2);
            }
        }

        /// <summary>
        /// 性能测试 网络,数据库
        /// </summary>
        public void PerformanceTest() {

        }

        /// <summary>
        /// 自动优化
        /// </summary>
        public void AutoOptimize() {

        }

        /// <summary>
        /// 外部数据抓取
        /// </summary>
        public void WebDataSpider()
        {

        }

    }
}
