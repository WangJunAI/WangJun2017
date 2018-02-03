﻿using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.Data;
using WangJun.DB;

namespace WangJun.Doc
{
    public class CategoryManager
    {

        public static CategoryManager GetInstance()
        {
            var inst = new CategoryManager();
            return inst;
        }

        public int Save(string name, string parentId, string id, string creatorID)
        {
            var inst = new CategoryItem();
            var isNew = false;
            if (24 == id.Length && "000000000000000000000000" != id)
            {
                inst._id = ObjectId.Parse(id);
            }
            else
            {
                inst._id = ObjectId.GenerateNewId();
                isNew = true;
            }


            inst.Name = name;
            inst.ParentID = parentId;
            inst.GroupName = "文档库模板";
            inst.CreateTime = DateTime.Now;
            inst.UpdateTime = DateTime.Now;
            inst.CreatorName = "测试人员";
            inst.Status = CONST.Status.Normal;
            inst.CreatorID = creatorID;
            inst.Save();

            ///添加记录
            if (isNew)
            {
                ModifyLogItem.LogAsNew(inst.id, CONST.DB.DBName_DocService, CONST.DB.CollectionName_CategoryItem);
            }
            else
            {
                ModifyLogItem.LogAsModify(inst.id, CONST.DB.DBName_DocService, CONST.DB.CollectionName_CategoryItem);
            }

            return 0;
        }

        public CategoryItem Get(string id)
        {
            var inst = CategoryItem.Load(id);
            if ( !string.IsNullOrWhiteSpace(inst.ParentID)&& 24 == inst.ParentID.Length)
            {
                inst.ParentName = CategoryItem.Load(inst.ParentID).Name;
            }
            return inst;
        }

        /// <summary>
        /// 根据
        /// </summary>
        /// <param name="query"></param>
        /// <param name="protection"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>

        public List<CategoryItem> Find(string query, string protection = "{}", int pageIndex = 0,int pageSize=50)
        {
            var list = new List<CategoryItem>();
            var dbName = CONST.DB.DBName_DocService;
            var collectionName = CONST.DB.CollectionName_CategoryItem;
            if (!string.IsNullOrWhiteSpace(query))
            {
                var mongo = DataStorage.GetInstance(DBType.MongoDB);
                var resList = mongo.Find2(dbName, collectionName, query,protection,pageIndex,pageSize);

                list = Convertor.FromDictionaryToObject<CategoryItem>(resList);
            }

            return list;
        }

        public object Count(string query)
        {
            var res = new object();
            var dbName = CONST.DB.DBName_DocService;
            var collectionName = CONST.DB.CollectionName_CategoryItem;
            if (!string.IsNullOrWhiteSpace(query))
            {
                var mongo = DataStorage.GetInstance(DBType.MongoDB);
                var count = mongo.Count(dbName, collectionName, query);
                res = new { Count = count };
            }

            return res;
        }

        public int Remove(string id)
        {
            RecycleBinManager.GetInstance().MoveToRecycleBin(CONST.DB.DBName_DocService, CONST.DB.CollectionName_CategoryItem, id);
            return 0;
        }

        public List<CategoryItem> GetSubCategory(string id)
        {
            List<CategoryItem> list = new List<CategoryItem>();
            var db = DataStorage.GetInstance(DBType.MongoDB);
            var stack = new Stack<string>();
            stack.Push(id);

            while (0 < stack.Count)
            {
                var categoryID = stack.Pop();
                var query = "{'ParentID': '" + categoryID + "'}";
                
                var subCategoryList = this.Find(query);
                foreach (var item in subCategoryList)
                {
                    list.Add(item);
                    stack.Push(item.id);
                }

            }

                
 
 
            return list;
        }
 
    }
}