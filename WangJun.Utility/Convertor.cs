using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace WangJun.Utility
{
    public static class Convertor
    {
        private static JavaScriptSerializer js = new JavaScriptSerializer();

        public static T FromJsonToObject<T>(string jsonString) where T:class
        {
            try
            {
                T item = Convertor.js.Deserialize<T>(jsonString);
                return item;
            }
            catch (Exception e)
            {
            }
            return null;
        }

        public static T FromDictionaryToObject<T>(Dictionary<string,object> data) where T:class,new ()
        {
            if (null != data)
            {
                T item = new T();
                PropertyInfo[] properties = item.GetType().GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    string key1 = property.Name;
                    string key2 = property.Name.ToLower();
                    string key3 = property.Name.ToUpper();
                    string key = string.Empty;
                    if(data.ContainsKey(key1))
                    {
                        key = key1;
                    }
                    else if(data.ContainsKey(key2))
                    {
                        key = key2;
                    }
                    else if (data.ContainsKey(key3))
                    {
                        key = key3;
                    }


                    if (property.CanWrite && data.ContainsKey(key))
                    {
                        if (property.PropertyType == typeof(Guid))
                        {
                            property.SetValue(item, new Guid(data[key].ToString()), null);

                        }
                        else if (property.PropertyType.IsValueType)
                        {

                            if (property.PropertyType == typeof(int))
                            {
                                property.SetValue(item, int.Parse(data[key].ToString()), null);
                            }
                            else
                            {
                                property.SetValue(item, data[key], null);
                            }
                        }
                        else if (property.PropertyType == typeof(ArrayList) && null != data[key] && typeof(object[]) == data[key].GetType())
                        {
                            ArrayList arrayList = new ArrayList((object[])data[key]);
                            property.SetValue(item, arrayList, null);
                        }
                        else
                        {
                            property.SetValue(item, data[key], null);
                        }
                    }
                }
                return item;
            }
            return null;
        }

        public static List<T> FromDictionaryToObject<T>(List<Dictionary<string, object>> dataList) where T : class, new()
        {
            var list = new List<T>();
            foreach (var item in dataList)
            {
                list.Add(Convertor.FromDictionaryToObject<T>(item));
            }
            return list;
        }
        public static Dictionary<string, object> FromObjectToDictionary2(object data)
        {
            string json = Convertor.FromObjectToJson(data);
            var dict = Convertor.FromJsonToDict2(json);
            return dict;
        }

        public static Dictionary<string,object> FromObjectToDictionary3(object data)
        {
  
            Dictionary<string, object> res = new Dictionary<string, object>();
            if (null != data)
            {

                var propertyArray = data.GetType().GetProperties();
                foreach (var property in propertyArray)
                {
                    var name = property.Name;
                    var value = property.GetValue(data, null);
                    if (property.CanRead)
                    {

                        #region 若是单个实体
                        if (null != value && ( value.GetType().IsValueType || typeof(string) == value.GetType() || value.GetType().IsEnum)) ///若是基本类型或字符串
                        {
                            if (typeof(TimeSpan) == value.GetType())  ///解决TimeSpan无法映射到Bson对象的问题
                            {
                                var itemValue = Convertor.FromObjectToDictionary3(value);
                                res.Add(name, itemValue);
                            }
                            else
                            {
                                res.Add(name, value);
                            }
                        }
                        else if (null == value)
                        {
                            res.Add(name, value);
                        }
                        else if (null != value && value.GetType().IsClass && typeof(string) != value.GetType() && !(value is IEnumerable)) ///非集合类
                        {
                            var valueDict = Convertor.FromObjectToDictionary3(value);
                            res.Add(name, valueDict);
                        }
                        #endregion

                        #region 若字典值是集合,列表,字典等
                        else if (null != value && value.GetType().IsClass && (value is IDictionary))
                        {
                            var dict = new Dictionary<string, object>();
                            foreach (string key in (value as IDictionary).Keys)
                            {
                                var itemValue = (value as IDictionary)[key];
                                if (null != itemValue && (itemValue.GetType().IsValueType || typeof(string) == itemValue.GetType() || itemValue.GetType().IsEnum))
                                {
                                    dict.Add(key, itemValue);
                                }
                                else if(null != itemValue && itemValue.GetType().IsClass && (itemValue is IEnumerable))
                                {
                                    var list = new List<object>();
                                    foreach (var item in (value as IEnumerable))
                                    {
                                        if(null != item && item.GetType().IsValueType)
                                        {
                                            list.Add(item);
                                        }
                                        else
                                        {
                                            var itemDict = Convertor.FromObjectToDictionary3(item);
                                            list.Add(itemDict);
                                        }
                                    }
                                    dict.Add(key, list);
                                }
                                else
                                {
                                    var itemDict = Convertor.FromObjectToDictionary3(itemValue);
                                    dict.Add(key, itemDict);
                                }

                            }
                            res.Add(name, dict);
                        }
                        else if (null != value && value.GetType().IsClass && (value is IEnumerable))
                        {
                            var list = new List<Dictionary<string, object>>();
                            var listValueType = new List<object>();
                            bool isValueType = false;
                            foreach (var item in (value as IEnumerable))
                            {
                                if (!(item is string || item.GetType().IsValueType))
                                {
                                    var itemDict = Convertor.FromObjectToDictionary3(item);
                                    list.Add(itemDict);
                                }
                                else
                                {
                                    isValueType = true;
                                    listValueType.Add(item);
                                }
                            }

                            if(!isValueType)
                            {
                                res.Add(name, list);
                            }
                            else
                            {
                                res.Add(name, listValueType);
                            }
                            
                        }
                        #endregion
                    }
                }
            }
            return res;
        }

        public static Dictionary<string,object> FromObjectToDictionary(object data)
        {
            var dict = new Dictionary<string, object>();
            if(null != data&& (typeof(Dictionary<string,object>) != data.GetType()))
            {
                var propertyArray = data.GetType().GetProperties();
                foreach (var property in propertyArray)
                {
                    var name = property.Name;
                    var value = property.GetValue(data, null);
                    if(property.CanRead)
                    {
                        if(null !=value &&  typeof(TimeSpan) == value.GetType())
                        {
                            var timespanDict = Convertor.FromObjectToDictionary(value);
                            dict.Add(name, timespanDict);
                        }
                        else if(null != value && (value.GetType().IsValueType || typeof(string) == value.GetType()))
                        {
                            dict.Add(name, value);
                        }
                        else if (null != value && (  typeof(Dictionary<string,object>) == value.GetType()))
                        {
                            value = Convertor.FromObjectToDictionary(value);
                            dict.Add(name, value);
                        }
                        else if (null != value && (typeof(ArrayList) == value.GetType()))
                        {
                            var list = new List<object>();
                            foreach (var arrayItem in (value as ArrayList))
                            {
                                var arrayItemValue = Convertor.FromObjectToDictionary(arrayItem);
                                list.Add(arrayItemValue);
                            }
                            dict.Add(name, list);
                        }
                        else if (null != value && (value is IEnumerable) && !(value is IDictionary))
                        {
                            var list = new List<object>();
                            foreach (var arrayItem in (value as IEnumerable))
                            {
                                if (!arrayItem.GetType().IsValueType && arrayItem.GetType() != typeof(string))
                                {
                                    var arrayItemValue = Convertor.FromObjectToDictionary(arrayItem);
                                    list.Add(arrayItemValue);
                                }
                                else
                                {
                                    list.Add(arrayItem);
                                }

                            }
                            dict.Add(name, list);
                        }
                        else if(null != value)
                        {
                            dict.Add(name, value);
                        }

                    }
                }
            }
            else if(null != data && (typeof(Dictionary<string, object>) == data.GetType()))
            {
                return data as Dictionary<string, object>;
            }
            return dict;
        }

        public static List<T> FromDataTableToList<T>(DataSet dataSet) where T:class,new()
        {
            List<T> list = new List<T>();

            DataRowCollection rows = dataSet.Tables[0].Rows;
            foreach(DataRow row in rows)
            {
                T item = Convertor.FromDataRowToObject<T>(row);
                list.Add(item);
            }
            dataSet.Dispose();
            return list;
        }

        public static List<Dictionary<string,object>> FromDataTableToList(DataSet dataSet)
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();

            DataRowCollection rows = dataSet.Tables[0].Rows;
            foreach (DataRow row in rows)
            {
                Dictionary<string, object> item = Convertor.FromDataRowToDict(row);
                list.Add(item);
            }
            dataSet.Dispose();
            return list;
        }

        public static T FromDataRowToObject<T>(DataRow row) where T:class,new()
        {
            T item = new T();
            PropertyInfo[] properties = item.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.CanWrite && row.Table.Columns.Contains(property.Name) && !(row[property.Name] is DBNull))
                {
                    if (row.Table.Columns[property.Name].DataType == typeof(string) && property.PropertyType == typeof(IDictionary<string, string>))
                    {
                        Dictionary<string, string> dict = Convertor.FromJsonToObject<Dictionary<string, string>>(row[property.Name].ToString());
                        property.SetValue(item, dict,null);
                    }
                    else
                    {
                        property.SetValue(item, row[property.Name],null);
                    }
                }
            }
            return item;
        }

        public static Dictionary<string,object> FromDataRowToDict(DataRow row)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            DataTable table = row.Table;
            foreach(DataColumn column in table.Columns)
            {
                dict.Add(column.ColumnName, row[column.ColumnName]);
            }
            return dict;
        }

        /// <summary>
        /// 将对象转化为JSON
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string FromObjectToJson(object data)
        {
            js.MaxJsonLength = int.MaxValue;
            return js.Serialize(data);
        }

        public static ArrayList ResetValueType(ArrayList sourceDict)
        {
            if(null != sourceDict)
            {
                var list = new List<KeyValuePair<string, Dictionary<string, object>>>();
                foreach (Dictionary<string,object> itemDict in sourceDict)
                {
                    foreach (var e in itemDict)
                    {
                        if(null != e.Value &&typeof(decimal) == e.Value.GetType())
                        {
                            list.Add(new KeyValuePair<string, Dictionary<string, object>>( e.Key,itemDict));
                        }
                    }
                }

                foreach (var item in list)
                {
                    item.Value[item.Key] = Convert.ToSingle(item.Value[item.Key]);
                }
            }
            return sourceDict;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="AnonymousType">new {Pro1=Val1,Pro2=Val2}</param>
        /// <returns></returns>
        public static T FromObjectToAnonymous<T>(object source , T AnonymousType)
        {
            return (T)source;
        }

        public static Guid GetGuid(string input)
        {
            Guid guid = Guid.Empty;
            if (input.Contains("-"))
            {
                return (Guid.TryParse(input, out guid)) ? guid : Guid.Empty;
            }
            else if(32 == input.Length)
            {
                input = input.Insert(7, "-").Insert(12, "-").Insert(17, "-").Insert(22, "-");
                return (Guid.TryParse(input, out guid)) ? guid : Guid.Empty;
            }
            return guid;
        }

        public static Dictionary<string, object> FromJsonToDict(string jsonString)
        {
            js.MaxJsonLength = 16 * 1024 * 1024;
            
            Dictionary<string, object> data = js.Deserialize<Dictionary<string, object>>(jsonString);
            return data;
        }

        public static Dictionary<string, object> FromJsonToDict2(string jsonString)
        {
            JObject res = JsonConvert.DeserializeObject(jsonString) as JObject;
            var str = JsonConvert.SerializeObject(res);
            return Convertor.FromJsonToDict(str);
        }

        /// <summary>
        /// 从Unicode 字符串 转换为 UTF8 字符串
        /// 原始字符串类似于"\u6587\u4ef6\u59391":
        /// </summary>
        /// <param name="unicodeString"></param>
        /// <returns></returns>
        public static string FromUnicodeToUTF8(string unicodeString)
        {
            byte[] byteArray = Encoding.Unicode.GetBytes(unicodeString);
            var dat = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, byteArray);
            var res = Encoding.UTF8.GetString(dat);
            return res;
        }

        /// <summary>
        /// HMACSHA1  加密
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string EncodeHMACSHA1(string secret,string mk)
        {
            HMACSHA1 hmacsha1 = new HMACSHA1();
            hmacsha1.Key = Encoding.UTF8.GetBytes(secret);
            byte[] dataBuffer = Encoding.UTF8.GetBytes(mk);
            byte[] hashBytes = hmacsha1.ComputeHash(dataBuffer);
            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>  
        /// 获取时间戳  
        /// </summary>  
        /// <returns></returns>  
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        public static string GetNonce()
        {
            var list = DateTime.Now.Ticks.ToString().Reverse<char>();
            StringBuilder stringBuilder = new StringBuilder();

            foreach(var item in list)
            {
                stringBuilder.AppendFormat("{0}", item);
            }
            return stringBuilder.ToString().Substring(0,16);
        }

        public static string UrlEncoding(string inputString)
        {
            return HttpUtility.UrlEncode(inputString);
        }

        /// <summary>
        /// 将
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string DecodeBase64(string input)
        {
            if(!string.IsNullOrWhiteSpace(input)&& input.Contains(" "))
            {
                ///若包含空格
                input = input.Replace(" ", "+");
            }
            var str = Encoding.UTF8.GetString(Convert.FromBase64String(input));
            return str;
        }

        public static string GetInsertSQLFromObject(string tableName , Dictionary<string,object> data)
        {
            string sql = string.Empty;
            if (null != data)
            {
                StringBuilder stringBuilder_Columns = new StringBuilder("COLUMN_0");
                StringBuilder stringBuilder_Param = new StringBuilder("COLUMN_0");
                foreach (var item in data)
                {
                    int num = -1;
                    bool isInt = int.TryParse(item.Key, out num);
                    if(isInt)
                    {
                        stringBuilder_Columns.AppendFormat(",[C{0}]",item.Key);
                        stringBuilder_Param.AppendFormat(",@C{0}",item.Key);
                    }
                    else if ((null == item.Value) || item.Value.GetType().IsValueType || item.Value is string)
                    {
                        stringBuilder_Columns.AppendFormat(",[{0}]",item.Key);
                        stringBuilder_Param.AppendFormat(",@{0}", item.Key);
                    }
                }

                stringBuilder_Columns = stringBuilder_Columns.Replace("COLUMN_0,", string.Empty);
                stringBuilder_Param = stringBuilder_Param.Replace("COLUMN_0,", string.Empty);

                sql = string.Format(" INSERT INTO [{0}] ({1}) VALUES ({2}) ",tableName, stringBuilder_Columns.ToString(), stringBuilder_Param.ToString());
            }
            return sql;
        }

        public static string GetCreateTableSQLFromObject(string TableName , Dictionary<string, object> data)
        {
            string sql = string.Empty;
            if (null != data)
            {
                StringBuilder stringBuilder_Columns = new StringBuilder("COLUMN_0");
                foreach (var item in data)
                {
                    int num = -1;
                    bool isInt = int.TryParse(item.Key, out num);
                    if (isInt)
                    {
                        stringBuilder_Columns.AppendFormat(",[C{0}]  [nvarchar](max) NULL ", item.Key);
                    }
                    else if ((null == item.Value) || item.Value.GetType().IsValueType || item.Value is string)
                    {
                        stringBuilder_Columns.AppendFormat(",[{0}]  [nvarchar](max) NULL ", item.Key);
                    }
                }

                stringBuilder_Columns = stringBuilder_Columns.Replace("COLUMN_0,", string.Empty);

                sql = string.Format(" CREATE TABLE [{0}]({1})  ", TableName,stringBuilder_Columns.ToString());
            }
            return sql;
        }

        /// <summary>
        /// SHA1 加密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Encode_SHA1(string input)
        {
            byte[] cleanBytes = Encoding.Default.GetBytes(input);
            byte[] hashedBytes = System.Security.Cryptography.SHA1.Create().ComputeHash(cleanBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", string.Empty);
        }

        /// <summary>
        /// MD5 加密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Encode_MD5(string input)
        {
            byte[] cleanBytes = Encoding.Default.GetBytes(input);
            byte[] hashedBytes = System.Security.Cryptography.MD5.Create().ComputeHash(cleanBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", string.Empty);
        }

        /// <summary>
        /// Json对象转C#代码
        /// </summary>
        /// <returns></returns>
        public static string JsonToCSharpCode(string jsonString)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 添加股票代码前缀
        /// </summary>
        /// <param name="stockcode"></param>
        /// <returns></returns>
        public static string AddStockCodePrefix(string stockcode)
        {
            if (stockcode.StartsWith("600")
                || stockcode.StartsWith("601")
                || stockcode.StartsWith("603"))
            {
                return "sh" + stockcode;
            }  
            else if (stockcode.StartsWith("000") || stockcode.StartsWith("001") || stockcode.StartsWith("002") || stockcode.StartsWith("300"))
            {
                return "sz" + stockcode;
            }
            throw new Exception("股票代码转换出错");
        }

        /// <summary>
        /// 修正交易日
        /// </summary>
        /// <param name="createTime"></param>
        /// <param name="ticktime"></param>
        /// <returns></returns>
        public static DateTime CalTradingDate(DateTime createTime, string ticktime)
        {
            if (createTime.DayOfWeek == DayOfWeek.Saturday)
            {
                ///若是周六,收盘时间为周五
                createTime = createTime.AddDays(-1).Date;
            }
            else if (createTime.DayOfWeek == DayOfWeek.Sunday)
            {
                ///若是周日,收盘时间为周五
                createTime = createTime.AddDays(-2).Date;
            }
            else if (createTime.Hour <= 15 && createTime.DayOfWeek == DayOfWeek.Monday)
            {
                ///周一收盘前,应该是周五
                createTime = createTime.AddDays(-3).Date;
            }
            else if (createTime.Hour <= 15)
            {
                ///其余日期收盘前,应该是前一天的
                createTime = createTime.AddDays(-1).Date;
            }
            else
            {
                createTime = createTime.Date;
            }
            createTime = DateTime.Parse(string.Format("{0}/{1}/{2} {3}", createTime.Year, createTime.Month, createTime.Day, ticktime));
            return createTime;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ObjectIDToString(Dictionary<string,object> input)
        { 
            var timestamp = int.Parse(input["Timestamp"].ToString());
            var machine = int.Parse(input["Machine"].ToString());
            var pid = short.Parse(input["Pid"].ToString());
            var increment = int.Parse(input["Increment"].ToString());
            var id = new ObjectId(timestamp, machine, pid, increment);
            return id.ToString();//ObjectId.Parse(id);
        }

        public static ObjectId StringToObjectID(string objectId)
        {
            return ObjectId.Parse(objectId);
        }


        public static int GetJidu(DateTime dateTime)
        {
            var month = dateTime.Month;
            if(month<=3)
            {
                return 1;
            }
            else if ( 3<month && month<=6)
            {
                return 2;
            }
            else if (6 < month && month <=9)
            {
                return 3;
            }
            else if (9 < month && month <= 12)
            {
                return 4;
            }
            return -1;
        }

    }
}
