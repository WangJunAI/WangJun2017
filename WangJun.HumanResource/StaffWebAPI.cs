using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.Config;
using WangJun.Entity;
using WangJun.Utility;

namespace WangJun.HumanResource
{
    public class StaffWebAPI
    {
        public long AppCode = CONST.APP.OrgStaff.Code;


        #region 组织操作
        /// <summary>
        /// 保存一个目录
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parentId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public int SaveOrg(string jsonInput)
        {
            OrgItem.Save(jsonInput);
            return 0;
        }

        /// <summary>
        /// 加载目录
        /// </summary>
        /// <param name="query"></param>
        /// <param name="protection"></param>
        /// <param name="sort"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<OrgItem> LoadOrgList(string query, string protection = "{}", string sort = "{}", int pageIndex = 0, int pageSize = 50)
        {
            var dict = Convertor.FromJsonToDict2(query);

            query = "{$and:[" + query + ",{ 'AppCode':" + this.AppCode + ",'CompanyID':'" + SESSION.Current.CompanyID + "'},{'StatusCode':{$ne:" + CONST.APP.Status.删除 + "}}]}";
    
            var res = EntityManager.GetInstance().Find<OrgItem>( query, protection, sort, pageIndex, pageSize);
            return res;
        }


        /// <summary>
        /// 删除一个目录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int RemoveOrg(string id)
        {
            var inst = new OrgItem();
            inst.ID = id;
            inst.Remove();
            return 0;
        }

        public OrgItem GetOrg(string id)
        {
            var inst = new OrgItem();
            inst.ID = id;
            inst = EntityManager.GetInstance().Get<OrgItem>(inst);
            return inst;
        }
        #endregion

        #region 员工操作
        /// <summary>
        /// 保存一个目录
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parentId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public int SaveEntity(string jsonInput)
        {
            StaffItem.Save(jsonInput);
            return 0;
        }

        /// <summary>
        /// 加载目录
        /// </summary>
        /// <param name="query"></param>
        /// <param name="protection"></param>
        /// <param name="sort"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<StaffItem> LoadEntityList(string query, string protection = "{}", string sort = "{}", int pageIndex = 0, int pageSize = 50)
        {
            var dict = Convertor.FromJsonToDict2(query);
 
                query = "{$and:[" + query + ",{ 'AppCode':" + this.AppCode + "},{'StatusCode':{$ne:" + CONST.APP.Status.删除 + "}}]}";
 
            var res = EntityManager.GetInstance().Find<StaffItem>(query, protection, sort, pageIndex, pageSize);
            return res;
        }

        public List<object> LoadAll()
        {
            var list = new List<object>();
            var orgList = this.LoadOrgList("{}", "{}", "{}", 0, int.MaxValue);
            var staffList = this.LoadEntityList("{}", "{}", "{}", 0, int.MaxValue);
            list.AddRange(orgList);
            list.AddRange(staffList);
            return list;
        }


        /// <summary>
        /// 删除一个目录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int RemoveEntity(string id)
        {
            var inst = new StaffItem();
            inst.ID = id;
            inst.Remove();
            return 0;
        }

        public StaffItem GetEntity(string id)
        {
            var inst = new StaffItem();
            inst.ID = id;
            inst = EntityManager.GetInstance().Get<StaffItem>(inst);
            return inst;
        }
        #endregion

        #region 统计操作
        /// <summary>
        /// 统计操作
        /// </summary>
        /// <returns></returns>
        public object Count(string json)
        {
            var item = new StaffItem();
            var match = "{$match:" + json + "}";
            var group = "{$group:{_id:'StaffItem总数',Count:{$sum:1}}}";
            var res = EntityManager.GetInstance().Aggregate(item._DbName, item._CollectionName, match, group);
            return res;
        }
        #endregion


        #region SESSION操作
        public SESSION Login(string json)
        {
            var dict = Convertor.FromJsonToDict2(json);
            var inst = Convertor.FromDictionaryToObject<SESSION>(dict);
            var res = SESSION.Login(inst.UserID, null);
            return res;
        }
        #endregion


        #region 回收站
        public List<StaffItem> LoadRecycleBinEntityList(string query, string protection = "{}", string sort = "{}", int pageIndex = 0, int pageSize = 50)
        {
            query = "{$and:[" + "{}" + ",{ 'AppCode':" + this.AppCode + "},{'StatusCode':{$eq:" + CONST.APP.Status.删除 + "}}]}";

            var res = EntityManager.GetInstance().Find<StaffItem>(query, protection, sort, pageIndex, pageSize);
            return res;
        }

        /// <summary>
        /// 彻底删除实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int DeleteEntity(string id)
        {
            var inst = new StaffItem();
            inst.ID = id;
            inst.Delete();
            return 0;
        }

        public int EmptyRecycleBin()
        {
            var list = this.LoadRecycleBinEntityList("{}", "{}", "{}", 0, int.MaxValue);
            foreach (StaffItem item in list)
            {
                item.Delete();
            }
            return 0;
        }
        #endregion

        #region 聚合计算
        public object Aggregate(string itemType, string match, string group)
        {
            var item = new BaseItem();
            if ("Entity" == itemType)
            {
                item = new StaffItem();
            }
            else if ("Category" == itemType)
            {
                item = new OrgItem();
            }
            var res = EntityManager.GetInstance().Aggregate(item._DbName, item._CollectionName, match, group);
            return res;

        }
        #endregion


    }

}
