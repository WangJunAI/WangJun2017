using System;
using WangJun.HumanResource;
using WangJun.Net;

namespace WangJun.NodeRunner
{
    class Program
    { 
        static void Main(string[] args)
        {
            #region 发送邮件
            SMTP smtp = SMTP.GetInstance("smtp-mail.outlook.com", 587, "wangjun19850215@live.cn", "111qqq!!!W");
            smtp.SendMail("wangjun19850215@live.cn", "wangjun19850215@live.cn", "Test", "Test");
            #endregion

            //StaffManager.GetInstance().Import();
            Console.WriteLine("全部结束");
            Console.ReadKey();
        }

 
 

         
    }
}
