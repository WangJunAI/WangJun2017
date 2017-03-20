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
                foreach (var item in items)
                {
                    q.Enqueue(item);
                }
            }

        }
        #endregion
    }
}
