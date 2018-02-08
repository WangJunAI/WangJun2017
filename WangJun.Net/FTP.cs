using System;
using System.Collections.Generic;
using System.Net;
using WangJun.Utility;

namespace WangJun.Net
{
    /// <summary>
    /// FTP操作类
    /// </summary>
    public class FTP
    {
        protected WebClient ftp = null; ///文件上传组件

        #region 创建一个新实例
        /// <summary>
        /// 创建一个新实例
        /// </summary>
        /// <param name="loginID"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static FTP CreateInstance(string loginID,string password)
        {
            var inst = new FTP();
            inst.ftp = new WebClient();
            inst.ftp.Credentials = new NetworkCredential(loginID, password);
            return inst;
        }
        #endregion
        
        #region 上传一个文件
        /// <summary>
        /// 上传一个文件
        /// </summary>
        /// <param name="ftpAdress"></param>
        /// <param name="localPath"></param>
        public void  UploadFile(string ftpAdress,string localFilePath)
        {
            var folderUrl = ftpAdress.Substring(0,ftpAdress.LastIndexOf('/'));
            this.CreateFolder(folderUrl);
            this.ftp.UploadFile(ftpAdress, localFilePath);
        }
        #endregion
 
        #region 创建一个文件夹
        /// <summary>
        /// 创建一个文件夹
        /// </summary>
        public void CreateFolder(string url)
        {
            var prefix = "ftp://";
            if (!string.IsNullOrWhiteSpace(url) && url.ToLower().StartsWith(prefix))
            {
                url = url.Substring(6); ///移除标记头
                var paramArr = url.Split(new char[] { '/' });
                url = paramArr[0];
                WebRequest req = null;
                for (int i = 1; i < paramArr.Length; i++)
                {
                    url += "/" + paramArr[i];
                    try
                    {
                        req = FtpWebRequest.Create("ftp://" + url);
                        req.Credentials = this.ftp.Credentials;// new NetworkCredential("qxw1146630116", "75737573");
                        req.Method = WebRequestMethods.Ftp.MakeDirectory;///创建目录
                        FtpWebResponse response = (FtpWebResponse)req.GetResponse();
                        response.Close();
                    }
                    catch (Exception e)
                    {
                        req.Abort();
                    }
                    finally
                    {
                        req.Abort();
                    }
                }
            }
        }
        #endregion

        #region 上传一个文件夹
        /// <summary>
        /// 上传一个文件夹
        /// </summary>
        /// <param name="ftpParentAdress">FTP上传的</param>
        /// <param name="localFolderPath"></param>
        public void UploadFolder(string ftpParentAdress,string localFolderPath)
        {
            var localDataOperator = LocalDataOperator.GetInst(localFolderPath);
            localDataOperator.EventOutput += LocalDataOperator_EventOutput;
            localDataOperator.OtherData = new Dictionary<string, object>();
            localDataOperator.OtherData.Add("relativePath", ftpParentAdress);
            localDataOperator.StartTraverse();
            
        }

        private void LocalDataOperator_EventOutput(object sender, EventArgs e)
        {
            var data = ((TraverseEventArg)e).Data;
            LocalDataOperator local = sender as LocalDataOperator;
            var relativePath = data.Path.Replace(local.RootPath,string.Empty);
            //this.UploadFile()
        }
        #endregion



        #region 文件夹详细
        public List<string> GetFolderFileList()
        {
            return null;
        }
        #endregion
    }
}
