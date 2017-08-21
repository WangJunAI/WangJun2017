using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.Tools
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
    }
}
