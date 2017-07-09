using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.Debug
{
    /// <summary>
    /// 日志
    /// </summary>
    public static class LOGGER
    {
        /// <summary>
        /// 输出
        /// </summary>
        public static void Output(string message)
        {
            Console.WriteLine("[{0}]--{1}", DateTime.Now, message);
        }

        public static void Log(Exception e)
        {

        }
    }
}
