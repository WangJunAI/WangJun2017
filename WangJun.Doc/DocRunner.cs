using MongoDB.Bson;
using System;
using System.Collections.Generic;
using WangJun.AI;
using WangJun.DB;
using WangJun.Utility;

namespace WangJun.Doc
{
    /// <summary>
    /// 
    /// </summary>
    public class DocRunner
    {

        public static DocRunner GetInstance()
        {
            var inst = new DocRunner();
            return inst;
        }

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
        /// 根据新增,修改情况 热词分析 分词 发文/修改统计
        /// </summary>
        public void DataAnalyse()
        {
            var startTime = DateTime.Now;///开始运行时间
            Console.Title = "文档维护进程进程 启动时间：" + startTime;
            while (true)
            {
                ///遍历写入数据
                var dbName = CONST.DB.DBName_DocService;
                var collectionName = CONST.DB.CollectionName_ModifyLogItem;
                var db = DataStorage.GetInstance(DBType.MongoDB);
                db.EventTraverse += (object sender, EventArgs e) =>
                {
                    EventProcEventArgs ee = e as EventProcEventArgs;
                    var dict = ee.Default as Dictionary<string, object>;
                    var targetDbName = dict["DatabaseName"].ToString();
                    var targetCollectionName = dict["CollectionName"].ToString();
                    var targetID = (ObjectId)dict["TargetID"];
                    var id = dict["_id"].ToString();

                    if(targetCollectionName == CONST.DB.CollectionName_DocItem)
                    {
                        ///重新分词,聚类
                        var doc = new DocItem();
                        var plainText = doc.PlainText;
                        var res = FenCi.GetResult(plainText);
                        var queryDel = "{'TargetID':ObjectId('" + targetID.ToString() + "')}";
                        db.Remove(dbName, collectionName, queryDel);
                        foreach (var item in res)
                        {
                            var svItem = new
                            {
                                DbName = targetDbName,
                                CollectionName = targetCollectionName,
                                TargetID = targetID,
                                Word = item.Key,
                                Count = item.Value,
                                CreateTime = DateTime.Now,
                                TargetCreateTime=dict["CreateTime"]

                            };

                            db.Save3(CONST.DB.DBName_DocService, CONST.DB.CollectionName_FenCi, svItem);
                        }

                    }
                    else if(targetCollectionName == CONST.DB.CollectionName_CategoryItem)
                    {
                        ///重新统计,当前目录的子目录数量 , 每一级文档数量
                        var subCategoryList = CategoryManager.GetInstance().GetSubCategory(targetID.ToString());
                        var category=CategoryItem.Load(targetID.ToString());
                        category.SubCategoryCount = subCategoryList.Count;
                        category.Save();

                    }

                    //ModifyLogItem.Remove(id);
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
