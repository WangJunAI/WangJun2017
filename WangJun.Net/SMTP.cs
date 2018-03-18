using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace WangJun.Net
{
    public class SMTP
    {
        protected SmtpClient client = null;
        public static SMTP GetInstance(string host,int port,string senderAddress,string password)
        {
            var inst = new SMTP();
            inst.client= new SmtpClient(host,port);
            inst.client.Credentials = new NetworkCredential(senderAddress, password);
            inst.client.EnableSsl = true;
            return inst;
        }

        public int SendMail(string senderAddress,string receiverAddress,string title,string content)
        {
            var mail = new MailMessage();
            mail.From = new MailAddress(senderAddress,"汪俊云企业注册系统");
            mail.To.Add(receiverAddress);
            mail.Subject = title;
            mail.SubjectEncoding = Encoding.UTF8;
            mail.Body = content;
            mail.BodyEncoding = Encoding.UTF8;
            this.client.Send(mail);
            return 0;
        }
    }
}
