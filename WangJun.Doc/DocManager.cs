﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.Data;
using WangJun.DB;

namespace WangJun.Doc
{
    public class DocManager
    {

        public static DocManager GetInstance()
        {
            var inst = new DocManager();
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

        public List<DocItem> Find(string query, string protection = "{}", int pageIndex = 0,int pageSize=50)
        {
            var list = new List<DocItem>();
            var dbName = CONST.DB.DBName_DocService;
            var collectionName = CONST.DB.CollectionName_DocItem;
            if (!string.IsNullOrWhiteSpace(query))
            {
                var mongo = DataStorage.GetInstance(DBType.MongoDB);
                var resList = mongo.Find2(dbName, collectionName, query,protection,pageIndex,pageSize);

                list = Convertor.FromDictionaryToObject<DocItem>(resList);
            }

            return list;
        }
    }
}
