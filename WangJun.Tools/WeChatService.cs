using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WangJun.Tools
{
    public static class WeChatService
    {
        private static string Str_AccessTokenUrl = "https://qyapi.weixin.qq.com/cgi-bin/gettoken?corpid={0}&corpsecret={1}";
        private static string Str_Corpid = "wx0b02c6e2d49dda91";
        private static string Str_Corpsecret = "LE_0oQSagHA9ywFVhAdvBzIPhWh7fZKUo3Do_qjrFc-h3Aa-21csCYQ-EGuVWI-Q";
        private static string Str_AccessToken = string.Empty;
        private static string Str_JSTicket = string.Empty;
        private static string Str_SendMessageUrl = "https://qyapi.weixin.qq.com/cgi-bin/message/send?access_token={0}";
        private static string Str_GetWeChatIDUrl = "https://qyapi.weixin.qq.com/cgi-bin/user/getuserinfo?access_token={0}&code={1}";
        private static string Str_GetUserInfoUrl = "https://qyapi.weixin.qq.com/cgi-bin/user/get?access_token={0}&userid={1}";
        private static string Str_GetAllOrgUrl = "https://qyapi.weixin.qq.com/cgi-bin/department/list?access_token={0}&id={1}";
        private static string Str_GetAllUserUrl = "https://qyapi.weixin.qq.com/cgi-bin/user/list?access_token={0}&department_id=1&fetch_child=1&status=0";
        private static string Str_JSAPI = "https://qyapi.weixin.qq.com/cgi-bin/get_jsapi_ticket?access_token={0}";

        private static System.Net.WebClient Http
        {
            get
            {
                var http =  new System.Net.WebClient();
                http.Encoding = Encoding.UTF8;
                return http;
            }
        }
        private static JavaScriptSerializer serializer = new JavaScriptSerializer();

        /// <summary>
        /// 获取AccessToken
        /// </summary>
        /// <example>
        /// {"access_token":"5XT_8bjQDo0M2SOZQBE1sddauEy8jgN0c-l3E6ASueLHRj21t8pqJV3260SBGFqbXKC2omfUeF50hF6qv99c2w","expires_in":7200}
        /// </example>
        /// <returns></returns>
        private static void GetAccessToken()
        {
            if (string.Empty == WeChatService.Str_AccessToken)
            {
                Http.Encoding = Encoding.UTF8;
                string url = string.Format(WeChatService.Str_AccessTokenUrl, WeChatService.Str_Corpid, WeChatService.Str_Corpsecret);
                string html = Http.DownloadString(url);
                WeChatService.Str_AccessToken = html.Replace("{\"access_token\":\"", string.Empty).Replace("\",\"expires_in\":7200}", string.Empty);
            }

        }


        /// <summary>
        /// 发送文章
        /// </summary>
        /// <param name="topicTitle"></param>
        /// <param name="subTitle"></param>
        /// <param name="picUrl"></param>
        /// <param name="articleUrl"></param>
        public static void SendArticle(string topicTitle , string subTitle , string picUrl , string articleUrl)
        {
            return;
            WeChatService.GetAccessToken();
            string url = string.Format(WeChatService.Str_SendMessageUrl,WeChatService.Str_AccessToken);
            //string data = "{\"touser\": \"@all\",\"toparty\": \" \",\"totag\": \"\",\"msgtype\": \"news\",\"agentid\": \"1\",\"news\": {\"articles\":[{\"title\": \"WX_TITLE_WX\",\"description\": \"WX_DESCRIPTION_WX\",\"url\":\"WX_URL_WX\",\"picurl\": \"WX_PIC_URL_WX\"}]}}".Replace("WX_TITLE_WX", topicTitle).Replace("WX_DESCRIPTION_WX", subTitle).Replace("WX_URL_WX", articleUrl).Replace("WX_PIC_URL_WX", picUrl);
            string data = "{\"touser\": \"@all\",\"toparty\": \" \",\"totag\": \"\",\"msgtype\": \"news\",\"agentid\": \"1\",\"news\": {\"articles\":[{\"title\": \"WX_TITLE_WX\",\"description\": \"WX_DESCRIPTION_WX\",\"url\":\"WX_URL_WX\",\"picurl\": \"WX_PIC_URL_WX\"}]}}".Replace("WX_TITLE_WX", topicTitle).Replace("WX_DESCRIPTION_WX", subTitle).Replace("WX_URL_WX", articleUrl).Replace("WX_PIC_URL_WX", picUrl);
            //string res = Http.UploadString(url,"POST", data);
        }

        /// <summary>
        /// 获取用户Code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string GetWeChatID(string code)
        {
            if (!string.IsNullOrWhiteSpace(code))
            {
                WeChatService.GetAccessToken();
                string url = string.Format(WeChatService.Str_GetWeChatIDUrl, WeChatService.Str_AccessToken, code);
                string res = Http.DownloadString(url);
                Dictionary<string, object> data = serializer.Deserialize<Dictionary<string, object>>(res);
                return data["UserId"].ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取单个用户信息
        /// </summary>
        /// <param name="weChatID"></param>
        /// <returns></returns>
        public static string GetUserInfo(string weChatID)
        {
            WeChatService.GetAccessToken();
            string url = string.Format(WeChatService.Str_GetUserInfoUrl, WeChatService.Str_AccessToken, weChatID);
            string res = Http.DownloadString(url);
            return res;
        }

        /// <summary>
        /// 获取所有组织信息
        /// </summary>
        /// <returns></returns>
        public static string GetAllOrg()
        {
            WeChatService.GetAccessToken();
            string url = string.Format(WeChatService.Str_GetAllOrgUrl, WeChatService.Str_AccessToken,"1");
            string data = Http.DownloadString(url);
            return data;
        }

        /// <summary>
        /// 获取所有人员
        /// </summary>
        /// <returns></returns>
        public static string GetAllUser()
        {
            WeChatService.GetAccessToken();
            string url = string.Format(WeChatService.Str_GetAllUserUrl, WeChatService.Str_AccessToken);
            string data = Http.DownloadString(url);
            return data;
        }

        /// <summary>
        /// 获取JSTicket
        /// </summary>
        /// <returns></returns>
        public static void GetJSTicket()
        {
            WeChatService.GetAccessToken();
            if (WeChatService.Str_JSTicket == string.Empty)
            {
                string url = string.Format(WeChatService.Str_JSAPI, WeChatService.Str_AccessToken);
                string data = Http.DownloadString(url);
                WeChatService.Str_JSTicket = data.Replace("{\"errcode\":0,\"errmsg\":\"ok\",\"ticket\":\"", string.Empty).Replace("\",\"expires_in\":7200}", string.Empty);
            }
        }

        /// <summary>
        /// 获取签名
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="noncestr"></param>
        /// <returns></returns>
        public static string GetSignature(string url, string timestamp, string noncestr)
        {
            string input = string.Format("jsapi_ticket={0}&noncestr={1}&timestamp={2}&url={3}", WeChatService.Str_JSTicket, noncestr, timestamp, url);
            string res = Encode_SHA1(input);
            return res;
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

            foreach (var item in list)
            {
                stringBuilder.AppendFormat("{0}", item);
            }
            return stringBuilder.ToString().Substring(0, 16);
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

    }
}
