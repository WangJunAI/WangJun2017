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

        #region 是否可以遍历
        /// <summary>
        /// 是否可以遍历
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static  bool CanTraverse<T>(IEnumerable<T> source)
        {
            return (null != source) && (0 < source.Count());
        }
        #endregion

        #region 克隆一个对象
        public static Dictionary<string,object> CloneDict(Dictionary<string, object> source)
        {
            var target = new Dictionary<string, object>();
            if(null != source)
            {
                foreach (var key in source.Keys)
                {
                    target[key] = source[key];
                }
            }
            return target;
        }
        #endregion
    }
}
