using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.Utility
{
    /// <summary>
    /// 日志
    /// </summary>
    public static class LOGGER
    {
        private static int count = 0;
        /// <summary>
        /// 输出
        /// </summary>
        public static void Log(object message,string type="调试")
        {
            if(message is string)
            {
                Console.WriteLine("{0}\t{1}\t{2}\t{3}", ++count, type, DateTime.Now, message);
            }
            else if(message is Exception)
            {
                Console.WriteLine("{0}\t{1}\t{2}\t{3}", ++count, "异常", DateTime.Now, (message as Exception).Message);
                Console.WriteLine("{0}\t{1}\t{2}\t{3}", ++count, "异常", DateTime.Now, (message as Exception).StackTrace);

            }
            else
            {
                Console.WriteLine("类型异常,无法打印.{0}", message);
            }
        }

        public static void Beep()
        {
            for (int k = 0; k < 1; k++)
            {
                Console.Beep(new Random().Next(5000, 10000), 1 * 1000);
            }
        }
         

         
    }
}
