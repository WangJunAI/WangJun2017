using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.HumanResource
{
    /// <summary>
    /// 全局会话服务
    /// </summary>
    public  class SESSION
    {
        public string UserID { get;set; }

        public string UserName { get; set; }


        public static SESSION Current
        {

            get
            {
                var inst = new SESSION();
                inst.UserID = "E10000";
                inst.UserName = "测试人员";
                return inst;
            }
        }
    }
}
