using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.Entity
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
                var list = new List<SESSION>();
 
                for (int  k= 0;   k< 100;  k++)
                {
                    list.Add(new SESSION { UserID = string.Format("E{0:000}",k),UserName=string.Format("测试{0:000}",k) });
                }

                return list[new Random().Next(0, 100)];
            }
        }
    }
}
