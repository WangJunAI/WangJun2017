using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace WangJun.Utility
{
    /// <summary>
    /// 本地文件操作器
    /// </summary>
    public class LocalDataOperator
    {
        #region 数据域
        protected Queue<FolderFileInfo> folderQueue = new Queue<FolderFileInfo>();

        protected Queue<FolderFileInfo> fileQueue = new Queue<FolderFileInfo>();

        protected Thread ThreadTraverseFile = null; ///文件遍历线程

        protected Thread ThreadOutput = null;///输出线程

        public event EventHandler EventOutput = null;

        /// <summary>
        /// 文件总数
        /// </summary>
        public int FileCount { get { return this.fileQueue.Count; } }

        /// <summary>
        /// 文件夹总数
        /// </summary>
        public int FolderCount { get { return this.folderQueue.Count; } } 

        protected string rootPath = @"G:\";

        /// <summary>
        /// 根目录
        /// </summary>
        public string RootPath
        {
            get
            {
                return this.rootPath;
            }
        }

        /// <summary>
        /// 其他参数
        /// </summary>
        public Dictionary<string, object> OtherData { get; set; }

        /// <summary>
        /// 遍历的子文件夹队列
        /// </summary>
        public Queue<FolderFileInfo> FolderQueue
        {
            get
            {
                return (null != this.folderQueue) ? this.folderQueue : new Queue<FolderFileInfo>();
            }
        }

        /// <summary>
        /// 遍历的文件队列
        /// </summary>
        public Queue<FolderFileInfo> FileQueue
        {
            get
            {
                return (null != this.fileQueue) ? this.fileQueue : new Queue<FolderFileInfo>();
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

        /// <summary>
        /// 获取一个实例
        /// </summary>
        /// <returns></returns>
        public static LocalDataOperator GetInst(string rootPath)
        {
            var inst = new LocalDataOperator();
            inst.rootPath = rootPath;
            return inst;
        }
        #endregion

        #region 遍历一个根目录下的文件
        /// <summary>
        /// 遍历一个根目录下的文件
        /// </summary>
        /// <param name="rootPath"></param>
        public void TraverseFiles()
        {
            LOGGER.Log("TraverseFiles遍历线程启动");
            CollectionTools.AddToQueue(this.fileQueue,this.GetFiles(this.rootPath));///获取该目录下文件信息
            var rootSubFolders = this.GetSubFolder(this.rootPath);
            CollectionTools.AddToQueue<FolderFileInfo>(this.folderQueue, rootSubFolders);
            while (0< this.folderQueue.Count)
            {
                var folder = this.folderQueue.Dequeue();
                if (null != folder)
                {
                    var files = this.GetFiles(folder.Path);
                    CollectionTools.AddToQueue(this.fileQueue, files);
                    var subFolders = this.GetSubFolder(folder.Path);
                    CollectionTools.AddToQueue<FolderFileInfo>(this.folderQueue, subFolders);
                    LOGGER.Log(string.Format("TraverseFiles遍历线程 文件夹队列长度:{0}\t文件队列长度:{1}", this.FolderCount, this.FileCount));
                }

 
                if (0 < this.FileCount && null == this.ThreadOutput)
                {
                    this.StartOutputThread();
                }
 
            }
            LOGGER.Log("TraverseFiles遍历线程结束");
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

            throw new Exception("未实现");
            //var subFolders = new Queue<FolderFileInfo>(this.GetSubFolder(rootPath)); ///获取指定根目录下所有子文件夹


            //while (0 < subFolders.Count)
            //{
            //    var folder = subFolders.Dequeue();
            //    var folders = this.GetSubFolder(folder);
            //    CollectionTools.AddToQueue<string>(subFolders, folders);

            //    foreach (var path in folders)
            //    {
            //        var folderInfo = new DirectoryInfo(path);
            //    }
            //}
        }
        #endregion


       #region 获取一个文件的子文件夹
            /// <summary>
            /// 获取一个文件的子文件夹
            /// </summary>
            /// <returns></returns>
        public List<FolderFileInfo> GetSubFolder(string currentPath)
        {
            var list = new List<FolderFileInfo>();
            if(StringChecker.IsPhysicalPath(currentPath)) ///若路径符合要求
            {
                try
                {
                    var subFolderArr = Directory.GetDirectories(currentPath);
                    foreach (var item in subFolderArr)
                    {
                        var folderInfo = this.GetFolderInfo(item);
                        if(null != folderInfo)
                        {
                            list.Add(folderInfo);
                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("异常信息：{0}", e.Message);
                }
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
        /// <summary>
        /// 获取一个文件夹下的所有文件概要信息
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public List<FolderFileInfo> GetFiles(string folderPath)
        {
            var list = new List<FolderFileInfo>();
            if(StringChecker.IsPhysicalPath(folderPath))
            {
               try
                {
                    var fileNames = Directory.GetFiles(folderPath);
                    foreach (var fileName in fileNames)
                    {
                        var info = this.GetFileInfo(fileName);
                        if(null != info)
                        {
                            list.Add(info);
                        }
                        
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine("异常信息：{0}", e.Message);
                }

            }
            return list;
        }
        #endregion

        #region 获取一个文件夹的基本信息
        /// <summary>
        ///  获取一个文件夹的基本信息
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public FolderFileInfo GetFolderInfo(string path)
        {
            var folderInfo = FolderFileInfo.GetInst(path);
            
            return folderInfo;
        }
        #endregion

        #region  输出函数
        /// <summary>
        /// 
        /// </summary>
        public void TargetOutput()
        {
            Console.WriteLine("TargetOutput输出线程启动 {0}", Thread.CurrentThread.ManagedThreadId);
            while (0 < this.fileQueue.Count)
            {
                var e = new TraverseEventArg();
                var data = CollectionTools.DeleteFromQueue<FolderFileInfo>(this.fileQueue);

                this.EventOutput(this, TraverseEventArg.Create(data));
            }
            Console.WriteLine("TargetOutput输出线程结束 {0}",Thread.CurrentThread.ManagedThreadId);
            this.ThreadOutput = null;
        }
        #endregion

   




        #region 开始遍历
        /// <summary>
        /// 开始遍历
        /// </summary>
        public void StartTraverse()
        {
            ///启动输入线程
            ///启动输出线程
            this.ThreadTraverseFile = new Thread(new ThreadStart( this.TraverseFiles));
            //this.ThreadOutput = new Thread(new ThreadStart(this.TargetOutput));

            this.ThreadTraverseFile.Start();
            //this.ThreadOutput.Start();
        }
        #endregion

        #region 启动输出线程
        public void StartOutputThread()
        {
            this.ThreadOutput = new Thread(new ThreadStart(this.TargetOutput));
            this.ThreadOutput.Start();
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
            FolderFileInfo item = new FolderFileInfo();
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

        /// <summary>
        /// 路径
        /// </summary>
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

        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize
        {
            get
            {
                return (null != this.fileInfo) ? this.fileInfo.Length : -1;
            }

        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime
        {
            get
            {
                return (null != this.fileInfo) ? this.fileInfo.CreationTime : DateTime.MinValue;
            }
        }

        /// <summary>
        /// 最后一次访问时间
        /// </summary>
        public DateTime LastAccessTime
        {
            get
            {
                return (null != this.fileInfo) ? this.fileInfo.LastAccessTime : DateTime.MinValue;
            }
        }

        /// <summary>
        /// 最后一次写入时间
        /// </summary>
        public DateTime LastWriteTime
        {
            get
            {
                return (null != this.fileInfo) ? this.fileInfo.LastWriteTime : DateTime.MinValue;
            }
        }

        /// <summary>
        /// 扩展名
        /// </summary>
        public string Extension
        {
            get
            {
                return (null != this.fileInfo) ? this.fileInfo.Extension : string.Empty;
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
        /// <summary>
        /// 参数
        /// </summary>
        public FolderFileInfo Data { get; set; }


        /// <summary>
        /// 创建一个新参数
        /// </summary>
        /// <returns></returns>
        public static TraverseEventArg Create(FolderFileInfo data)
        {
            var e = new TraverseEventArg();
            e.Data = data;
            return e;
        }
    }
    #endregion  
}
