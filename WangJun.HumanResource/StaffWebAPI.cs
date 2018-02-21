using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.Entity;
using WangJun.Utility;

namespace WangJun.HumanResource
{
    public class StaffWebAPI
    {

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
            var res = EntityManager.GetInstance().Find<OrgItem>(CONST.DB.DBName_HumanResource, CONST.DB.CollectionName_OrgItem, query, protection, sort, pageIndex, pageSize);
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
















         

        public List<StaffItem> LoadStaffList(string query, string protection = "{}", string sort = "{}", int pageIndex = 0, int pageSize = 50)
        {
            var res = StaffManager.GetInstance().Find(query, protection, sort, pageIndex, pageSize);

            return res;
        }

        public object StaffCount(string query)
        {
            var res = StaffManager.GetInstance().Count(query);
            return res;
        }


        public object RecycleBinCount(string query)
        {
            var res = RecycleBinManager.GetInstance().Count(query);
            return res;
        }

        public object SaveStaff(string staffStr)
        {
            var data = Convertor.FromJsonToDict2(staffStr);
            var staffData = Convertor.FromDictionaryToObject<StaffItem>(data);
            staffData.Save();
            return 0;
        }
 
    }
    
}
