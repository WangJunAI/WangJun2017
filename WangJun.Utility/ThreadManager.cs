using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WangJun.Utility
{
    public class ThreadManager
    {
        public static void Pause(int days=0, int hours=0, int minutes=0, int seconds=0)
        {
            Thread.Sleep(new TimeSpan(days, hours, minutes, seconds));
            Thread.Sleep(new TimeSpan(0, 0, new Random().Next(1, 3)));
        }
    }
}
