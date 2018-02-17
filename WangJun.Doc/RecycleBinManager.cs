using MongoDB.Bson;
using System.Collections.Generic;
using System.Linq;
using WangJun.DB;
using WangJun.Utility;

namespace WangJun.Doc
{
    public class RecycleBinManager
    {
 
        public static RecycleBinManager GetInstance()
        {
            var inst = new RecycleBinManager();

            return inst;
        }

        public void MoveToRecycleBin(string dbName,string collectionName , string id)
        {
            if(!string.IsNullOrWhiteSpace(dbName)&& !string.IsNullOrWhiteSpace(collectionName) && StringChecker.IsObjectId(id))
            {
                
                var res = this.FindTarget(dbName, collectionName, id);
                var db = DataStorage.GetInstance(DBType.MongoDB);
                foreach (var item in res)
                {
                    var query = "{'_id':ObjectId('" + item.ID + "')}";
                    var targetDataItem = new object();
                    if (item.TableName == CONST.DB.CollectionName_CategoryItem)
                    {
                        targetDataItem= CategoryManager.GetInstance().Find(query).First();
                    }
                    else if(item.TableName == CONST.DB.CollectionName_DocItem)
                    {
                        targetDataItem = DocManager.GetInstance().Find(query).First();
                    }

                    db.Save3(CONST.DB.DBName_DocService, CONST.DB.CollectionName_RecycleBin, targetDataItem);
                    db.Remove(item.DatabaseName, item.TableName, query);
                }
            }
        }

        /// <summary>
        /// 如果是删除 目录 ,查找该目录的所有子目录,查找所有的文件
        /// 若是删除文档,则直接删除
        /// </summary>
        /// <returns></returns>
        public List<DBItem> FindTarget(string dbName, string collectionName, string id)
        {
            List<DBItem> list = new List<DBItem>();
            var db = DataStorage.GetInstance(DBType.MongoDB);

            if(collectionName == CONST.DB.CollectionName_CategoryItem)
            {
                var categoryList = CategoryManager.GetInstance().GetSubCategory(id);
                categoryList.Add(CategoryItem.Load(id));
                foreach (var category in categoryList)
                {
                    var dbItem1 = DBItem.Create(CONST.DB.DBName_DocService, CONST.DB.CollectionName_CategoryItem, category.id);
                    list.Add(dbItem1);
                    var docList = DocManager.GetInstance().Find("{'CategoryID':'" + dbItem1.ID + "'}","{}","{}",0,int.MaxValue);
                    foreach (var doc in docList)
                    {
                        var dbItem2 = DBItem.Create(CONST.DB.DBName_DocService, CONST.DB.CollectionName_DocItem, doc.id);
                        list.Add(dbItem2);
                    }
                }
            }
            else
            {
                list.Add(DBItem.Create(dbName, collectionName, id));
            }
            return list;
        }
  
        public List<RecycleBinItem> Find(string query, string protection = "{}", string sort = "{}", int pageIndex = 0, int pageSize = 50)
        {
            var list = new List<RecycleBinItem> ();
            var dbName = CONST.DB.DBName_DocService;
            var collectionName = CONST.DB.CollectionName_RecycleBin;
            if (!string.IsNullOrWhiteSpace(query))
            {
                var mongo = DataStorage.GetInstance(DBType.MongoDB);
                var sourceList = mongo.Find3(dbName, collectionName, query, sort, protection, pageIndex, pageSize);
                foreach (Dictionary<string,object> item in sourceList)
                {
                    RecycleBinItem rItem = new RecycleBinItem();
                    if (item.ContainsKey("Name"))
                    {
                        rItem.Title = item["Name"].ToJson();
                        rItem.Type = "目录";
                    }
                    else if(item.ContainsKey("Title"))
                    {
                        rItem.Title = item["Title"].ToJson();
                        rItem.Type = "文档";
                    }
                    rItem.id = item["id"].ToString();
                    list.Add(rItem);
                }
             }
            return list;
        }

        public object Count(string query)
        {
            var res = new object();
            var dbName = CONST.DB.DBName_DocService;
            var collectionName = CONST.DB.CollectionName_RecycleBin;
            if (!string.IsNullOrWhiteSpace(query))
            {
                var mongo = DataStorage.GetInstance(DBType.MongoDB);
                var count = mongo.Count(dbName, collectionName, query);
                res = new { Count = count };
            }

            return res;
        }
        public void CleanRecycleBin()
        {

        }
    }
}
