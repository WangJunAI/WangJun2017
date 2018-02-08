using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.Utility
{
    /// <summary>
    /// 字典工具
    /// </summary>
    public static class DictTools
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object GetValue(Dictionary<string,object> dict,string key)
        {
            if(null != dict && !string.IsNullOrWhiteSpace(key) && dict.ContainsKey(key))
            {
                return dict[key];
            }
            return null; 
        }

        /// <summary>
        /// 克隆一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static T Clone<T>(T t)where T : class
        {
            using (MemoryStream ms = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, t);
                byte[] buffer = ms.GetBuffer();
                var ms2 = new MemoryStream(buffer);
                object temp = formatter.Deserialize(ms2);
                ms2.Dispose();
                return temp as T;
            }
        }

        public static bool IsDictionary(object data)
        {
            var res = true;
            if(null != data)
            {
                var propertyArray = data.GetType().GetProperties();
                res = res && "KeyCollection" == data.GetType().GetProperty("KeyCollection").Name;
                res = res && "ValueCollection" == data.GetType().GetProperty("ValueCollection").Name;
                res = res && "Count" == data.GetType().GetProperty("Count").Name;
                res = res && "Item" == data.GetType().GetProperty("Item").Name;
            }
            return false;
        }
    }
}
