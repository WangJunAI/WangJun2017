using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WangJun.Tools
{
    /// <summary>
    /// 集合操作工具
    /// </summary>
    public static class CollectionTools
    {
        #region 将集合加入到队列中
        public static void AddToQueue<T>(Queue<T> q , IEnumerable<T> items)
        {
            if(null != q && null != items)
            {
                lock (q)
                {
                    foreach (var item in items)
                    {
                        q.Enqueue(item);
                    }
                }
            }
        }

        public static T DeleteFromQueue<T>(Queue<T> q)
        {
            T t = default(T);
            lock (q)
            {
                if (null != q&& 0<q.Count)
                {
                    t = q.Dequeue();
                }
            }
            return t;
        }
        #endregion
    }
}
