using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WangJun.Data;

namespace WangJun.NetLoader
{
    /// <summary>
    /// API 请求上下文
    /// </summary>
    public class Context
    {
        public string CMD { get; set; }

        public string Method { get; set; }

        public object Args { get; set; }

        public string ToJson()
        {
            var res = Convertor.FromObjectToJson(this);
            return res;
        }
    }
}
