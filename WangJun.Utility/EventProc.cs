using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.Utility
{
    /// <summary>
    /// 事件处理
    /// </summary>
    public static class EventProc
    {
        /// <summary>
        /// 事件触发
        /// </summary>
        public static void TriggerEvent(EventHandler handler, object sender, EventArgs e)
        {
            if (null != handler)
            {
                handler(sender, e);
            }
        }


    }

    #region 事件处理参数
    /// <summary>
    /// 事件处理参数
    /// </summary>
    public class EventProcEventArgs : EventArgs
    {
        public object Default { get; set; }
        public Dictionary<string, object> DataDict { get; set; }

        /// <summary>
        /// 携带的信息
        /// </summary>
        public string Message { get; set; }

        public static EventProcEventArgs Create(object data)
        {
            EventProcEventArgs e = new EventProcEventArgs();
            e.Default = data;
            return e;
        }

        public static EventProcEventArgs Create(string msg)
        {
            EventProcEventArgs e = new EventProcEventArgs();
            e.Message = msg;
            return e;
        }

    }
    #endregion
}
