using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using WangJun.Tools;

namespace WangJun.Data
{
    /// <summary>
    /// 本地文件操作器
    /// </summary>
    public class LocalDataOperator
    {
        #region 数据域
        protected Queue<FolderFileInfo> folderQueue = new Queue<FolderFileInfo>();

        protected Queue<FolderFileInfo> fileQueue = new Queue<FolderFileInfo>();

        public Queue<FolderFileInfo> FolderQueue
        {
            get
            {
                return this.folderQueue;
            }
        }

        public Queue<FolderFileInfo> FileQueue
        {
            get
            {
                return this.fileQueue;
            }
        }

        #endregion

        #region 初始化
        /// <summary>
        /// 获取一个实例
        /// </summary>
        /// <returns></returns>
        public static LocalDataOperator GetInst()
        {
            return new LocalDataOperator();
        }
        #endregion

        #region 遍历一个根目录下的文件
        /// <summary>
        /// 遍历一个根目录下的文件
        /// </summary>
        /// <param name="rootPath"></param>
        public void TraverseFiles(string rootPath)
        {
            CollectionTools.AddToQueue(this.fileQueue,this.GetFiles(rootPath));///获取该目录下文件信息
            var subFolders = new Queue<string>(this.GetSubFolder(rootPath));
            while (0<subFolders.Count)
            {
                var folder = subFolders.Dequeue();
                var folderInfo = FolderFileInfo.GetInst(folder);
                this.folderQueue.Enqueue(folderInfo);
                var files = this.GetFiles(folder);
                CollectionTools.AddToQueue(this.fileQueue, files);
                var folders = this.GetSubFolder(folder);
                CollectionTools.AddToQueue<string>(subFolders, folders);
            }
        }
        #endregion

        #region 遍历一个根目录下所有文件夹
        /// <summary>
        /// 遍历一个根目录下所有文件夹
        /// </summary>
        /// <param name="rootPath"></param>
        /// <returns></returns>
        public void  TraverseFolder(string rootPath)
        {
            var subFolders = new Queue<string>(this.GetSubFolder(rootPath)); ///获取指定根目录下所有子文件夹


            while (0 < subFolders.Count)
            {
                var folder = subFolders.Dequeue();
                var folders = this.GetSubFolder(folder);
                CollectionTools.AddToQueue<string>(subFolders, folders);

                foreach (var path in folders)
                {
                    var folderInfo = new DirectoryInfo(path);
                }
            }
        }
        #endregion


       #region 获取一个文件的子文件夹
            /// <summary>
            /// 获取一个文件的子文件夹
            /// </summary>
            /// <returns></returns>
        public List<string> GetSubFolder(string currentPath)
        {
            var list = new List<string>();
            if(StringChecker.IsPhysicalPath(currentPath)) ///若路径符合要求
            {
                list = Directory.GetDirectories(currentPath).ToList();
            }
            return list;
        }
        #endregion

        #region 获取一个文件信息
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public FolderFileInfo GetFileInfo(string filePath)
        {
            FolderFileInfo item = new FolderFileInfo();
            if (StringChecker.IsPhysicalPath(filePath))
            {

                item = FolderFileInfo.GetInst(filePath);
            }

            return item;
        }
        #endregion

        #region 获取一个文件夹下的所有文件概要信息
        public List<FolderFileInfo> GetFiles(string folderPath)
        {
            var list = new List<FolderFileInfo>();
            if(StringChecker.IsPhysicalPath(folderPath))
            {
                var fileNames = Directory.GetFiles(folderPath);
                foreach (var fileName in fileNames)
                {
                    var info = this.GetFileInfo(fileName);
                    list.Add(info);
                }
            }

            return list;
        }
        #endregion

        #region 获取一个文件夹的基本信息
        public FolderFileInfo GetFolderInfo(string path)
        {
            var folderInfo = FolderFileInfo.GetInst(fileOrFolderPath);
            
            return folderInfo;
        }
        #endregion
    }



    #region 文件信息实体
    /// <summary>
    /// 文件信息实体
    /// </summary>
    public class FolderFileInfo
    {
        protected FileInfo fileInfo= null;

        protected DirectoryInfo directoryInfo = null;


        #region 初始化 
        ///<summary>
        ///
        /// </summary>
        public static FolderFileInfo GetInst(string fileOrFolderPath)
        {
            FolderFileInfo item = new Data.FolderFileInfo();
            item.fileInfo = (File.Exists(fileOrFolderPath)) ? new FileInfo(fileOrFolderPath) : null;
            item.directoryInfo = (Directory.Exists(fileOrFolderPath)) ? new DirectoryInfo(fileOrFolderPath) : null;
            return item;
        }
        #endregion

        #region 是否是文件
        ///<summary>
        ///是否是文件
        /// </summary>
        public bool IsFile
        {
            get
            {
                return null !=this.fileInfo && null == this.directoryInfo;
            }
        }
        #endregion

        #region 是否是文件夹
        /// <summary>
        /// 是否是文件夹
        /// </summary>
        public bool IsFolder
        {
            get
            {
                return null == this.fileInfo && null != this.directoryInfo;
            }
        }
        #endregion

        #region 无效数据
        /// <summary>
        /// 无效数据
        /// </summary>
        public bool IsUnknown
        {
            get
            {
                return (null == this.fileInfo && null == this.directoryInfo) || (null != this.fileInfo && null != this.directoryInfo);
            }
        }
        #endregion

        #region 文件信息
        /// <summary>
        /// 文件信息
        /// </summary>
        public FileInfo FileSummaryInfo
        {
            get
            {
                return this.fileInfo;
            }
        }
        #endregion
 
        #region 获取目录信息
        /// <summary>
        /// 获取目录信息
        /// </summary>
        public DirectoryInfo DirectorySummaryInfo
        {
            get
            {
                return this.directoryInfo;
            }
        }
        #endregion

        public string Path
        {
            get
            {
                string info = string.Empty;
                if(this.IsFile)
                {
                    info = this.FileSummaryInfo.FullName;
                }
                else if(this.IsFolder)
                {
                    info = this.DirectorySummaryInfo.FullName;
                }
                return info;
            }
        }

    }
    #endregion

    #region 事件参数
    /// <summary>
    /// 事件参数
    /// </summary>
    public class TraverseEventArg:EventArgs
    {

    }
    #endregion  
}
