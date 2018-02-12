using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web.Script.Serialization;
using MongoDB.Bson;

namespace WangJun.Utility
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

        #region 判断是否是汉字
        public static bool IsHanZi(string input,bool onlyHanZi=true)
        {
            var res =true;

            for (int k = 0; k < input.Length; k++)
            {
                var charItem = input[k];
                if (onlyHanZi) ///全都是汉字
                {
                    res = res && (0x4e00 <= charItem && charItem <= 0x9fbb);
                    if(!res) ///为负就返回不再计算
                    {
                        return res;
                    }
                }
                else ///包含汉字
                {
                    res = res || (0x4e00 <= charItem && charItem <= 0x9fbb); ///包含汉字
                }
            }
            return res;
        }
        #endregion

        #region 判断是否是英文
        /// <summary>
        /// 判断是否是英文
        /// </summary>
        /// <param name="input"></param>
        /// <param name="onlyEnglish"></param>
        /// <returns></returns>
        public static bool IsEnglish(string input, bool onlyEnglish = true)
        {
            var res = true;

            for (int k = 0; k < input.Length; k++)
            {
                var charItem = input[k];
                if (onlyEnglish) ///全都是英文
                {
                    res = res && ((0x0041 <= charItem && charItem <= 0x005A) || (0x0061 <= charItem && charItem <= 0x007A));
                    if (!res) ///为负就返回不再计算
                    {
                        return res;
                    }
                }
                else ///包含英文
                {
                    res = res || ((0x0041 <= charItem && charItem <= 0x005A) || (0x0061 <= charItem && charItem <= 0x007A)); ///包含英文
                }
            }
            return res;
        }
        #endregion

        #region 是否是日期
        /// <summary>
        /// 是否是日期
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsDateTime(string input)
        {
            var val = DateTime.MinValue;
            var res = DateTime.TryParse(input, out val);
            return res;
        }
        #endregion

        #region 判断是否是ObjectId
        public static bool IsObjectId(string id)
        {
            var oid = ObjectId.Empty;
            return   ObjectId.TryParse(id, out oid);
        }
        #endregion

        #region 判断是否是非空ObjectId
        public static bool IsNotEmptyObjectId(string id)
        {
            var oid = ObjectId.Empty;
            return ObjectId.TryParse(id, out oid) && oid != ObjectId.Empty;
        }
        #endregion

    }
}
