using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.Net
{
    public class WebClientWithCookie:WebClient
    {
        // Cookie 容器  
        public CookieContainer Cookies;

        /// <summary>  
        /// 创建一个新的 CookieWebClient 实例。  
        /// </summary>  
        public WebClientWithCookie()
        {
            this.Cookies = new CookieContainer();
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                //HttpWebRequest httpRequest = request as HttpWebRequest;
                //httpRequest.CookieContainer = Cookies;
            }
            return request;
        }
         



    }
}

