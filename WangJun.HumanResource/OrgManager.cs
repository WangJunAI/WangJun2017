using MongoDB.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.DB;
using WangJun.Utility;

namespace WangJun.HumanResource
{
    public class OrgManager
    {
        public static OrgManager GetInstance()
        {
            var inst = new OrgManager();
            return inst;

        }

        #region
        public void Import()
        {
            var json = File.ReadAllText(@"F:\[2016]数据备份\组织.txt", Encoding.UTF8);
            var arr = Convertor.FromJsonToObject<ArrayList>(json);
            var db = DataStorage.GetInstance(DBType.MongoDB);
            foreach (Dictionary<string,object> item in arr)
            {
                var svItem = new {
                    GroupName="奇瑞控股",
                    GroupID= "奇瑞控股",
                    Title=item["title"],
                    Name=item["name"],
                    ID=Guid.Parse(item["id"].ToString()),
                    ParentID = Guid.Parse(item["pId"].ToString()),
                    CreateTime=DateTime.Now
                };
                db.Save3("HumanResource", "OrgItem", svItem);

            }
        }
        #endregion

        public List<OrgItem> Find(string query, string protection = "{}", string sort = "{}", int pageIndex = 0, int pageSize = 50)
        {
            var list = new List<OrgItem>();
            var dbName = CONST.DB.DBName_HumanResource;
            var collectionName = CONST.DB.CollectionName_OrgItem;
            if (!string.IsNullOrWhiteSpace(query))
            {
                var mongo = DataStorage.GetInstance(DBType.MongoDB);
                var resList = mongo.Find3(dbName, collectionName, query, sort, protection, pageIndex, pageSize);

                list = Convertor.FromDictionaryToObject<OrgItem>(resList);
            }

            return list;
        }

        public int Save(string name, string parentId, string id)
        {
            var session = SESSION.Current;
            var inst = new OrgItem();
            var isNew = false;
            if (StringChecker.IsObjectId(id))
            {
                //inst._id = Convertor.StringToObjectID(id);
            }
            else
            {
               // inst._id = ObjectId.GenerateNewId();
                //inst.ID = Guid.NewGuid();
                inst.CreateTime = DateTime.Now.AddDays(new Random().Next(-100, 100));
                inst.CreatorName = session.UserName;
                inst.CreatorID = session.UserID;
                isNew = true;
            }


            inst.Name = name;
            inst.ParentID = parentId;
            inst.GroupName = "文档库模板";
            inst.UpdateTime = DateTime.Now;
            inst.Status = CONST.Status.Normal;
            inst.Save();

            ///添加记录
            if (isNew)
            {
                //ModifyLogItem.LogAsNew(inst.id, CONST.DB.DBName_DocService, CONST.DB.CollectionName_CategoryItem);
            }
            else
            {
                //ModifyLogItem.LogAsModify(inst.id, CONST.DB.DBName_DocService, CONST.DB.CollectionName_CategoryItem);
            }

            return 0;
        }


    }
}
