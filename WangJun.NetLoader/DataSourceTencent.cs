using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WangJun.Net;

namespace WangJun.NetLoader
{
    /// <summary>
    /// 翻译君API
    /// </summary>
    public static class DataSourceTencent
    {
        private static HTTP http = new HTTP();
        private static string url = "http://fanyi.qq.com/api/translate";
        static string method = "post";
        static string formData = "source=auto&target=en&sourceText={0}";
        public static string Invoke(string input)
        {
            var res = http.PostGZip(url,  string.Format(formData, HttpUtility.UrlEncode(input)));
            return res;
        }
    }
}
