using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.Net
{
    public class URL
    {
        public string Url { get; set; }
        public int RetryCount { get; set; }
        public Dictionary<string, object> Data { get; set; }

        public string Message { get; set; }

        public URL(string url , int retryCount=0 , Dictionary<string,object> data=null,string message=null)
        {

            this.Url = url;
            this.RetryCount = 0;
            this.Data = new Dictionary<string, object>();
            this.Message = message;
        }
         
    }
}
