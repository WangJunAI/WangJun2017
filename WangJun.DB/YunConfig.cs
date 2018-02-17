using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.DB
{
    /// <summary>
    /// 数据域配置信息
    /// </summary>
    public class YunConfig
    {
        public const string CurrentGroupID = "阿里云按量计费";//"ubuntu开发环境";//联网端使用
        public Dictionary<string, string> Load(string keyName)
        {

            Dictionary<string, Dictionary<string, string>> dict = new Dictionary<string, Dictionary<string, string>>();
            dict["阿里云按量计费"] = new Dictionary<string, string>();
            dict["ubuntu开发环境"] = new Dictionary<string, string>();
            dict["docker开发环境"] = new Dictionary<string, string>();

            dict["阿里云按量计费"]["mongodb"] = "mongodb://101.200.49.75:27017";
            dict["阿里云按量计费"]["sqlserver"] = "Data Source=qds165298153.my3w.com;Initial Catalog=qds165298153_db;Persist Security Info=True;User ID=qds165298153;Password=75737573";


            dict["ubuntu开发环境"]["mongodb"] = "mongodb://192.168.0.170:27017";
            dict["ubuntu开发环境"]["sqlserver"] = "Data Source=qds165298153.my3w.com;Initial Catalog=qds165298153_db;Persist Security Info=True;User ID=qds165298153;Password=75737573";

            if (dict.ContainsKey(keyName))
            {
                return dict[keyName];
            }
            return new Dictionary<string, string>();
            
        }

    }
}
