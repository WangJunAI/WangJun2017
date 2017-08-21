using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web.Script.Serialization;

namespace WangJun.Data
{
    /// <summary>
    /// 字符串检查器
    /// </summary>
    public static class StringChecker
    {
        /// <summary>
        /// 是否是空字符串
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsEmpty(string input)
        {
            return (string.IsNullOrWhiteSpace(input));
        }

        /// <summary>
        /// 是否有值
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool HasValue(string input)
        {
            return !StringChecker.IsEmpty(input);
        }

        /// <summary>
        /// 判断是否是GUID
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsGUID(string input)
        {
            Guid guid = Guid.Empty;
            return (Guid.TryParse(input, out guid)) ? true : false;
        }

        /// <summary>
        /// 判断是否是Guid Empty
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsGuidEmpty(string input)
        {
            Guid guid = Guid.Empty;
            return (Guid.TryParse(input, out guid)) ? (guid == Guid.Empty) : false;
        }

        /// <summary>
        /// 判断是否是物理路径
        /// </summary>
        /// <returns></returns>
        public static bool IsPhysicalPath(string input)
        {
            return File.Exists(input) || Directory.Exists(input);
        }
        
        /// <summary>
        /// 判断是否是MongoDB字符串
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsMongoDBConnectionString(string url)
        {
            return (!string.IsNullOrEmpty(url) && url.ToLower().StartsWith("mongodb://"));
        }

        /// <summary>
        /// 判断是否是Http地址
        /// </summary>
        /// <returns></returns>
        public static bool IsHttpUrl(string input)
        {
            return (!string.IsNullOrWhiteSpace(input)) && input.ToLower().Trim().StartsWith("http://");
        }

        #region 判断是否是Json格式
        /// <summary>
        /// 判断是否是Json格式
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>

        public static bool IsJson(string input)
        {
            var convertor = new JavaScriptSerializer();
            try
            {
                convertor.Serialize(input);
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }
        #endregion
    }
}
