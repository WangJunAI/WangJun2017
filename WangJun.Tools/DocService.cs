using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
 
namespace WangJun.OA
{
    /// <summary>
    /// 文档库服务
    /// </summary>
    public class DocService
    {
        /// <summary>
        /// 文档库服务
        /// </summary>
        public DocService()
        {
            //this.bizName = CONST.BizName.DocService;
        }

        #region 根据上下文获取一个实例
        /// <summary>
        /// 根据上下文获取一个实例
        /// </summary>
        /// <param name="userID">当前调用的用户</param>
        /// <returns></returns>
        public static DocService GetInstance(object hostObject, string userID)
        {
            var instance = new DocService(); 
            return instance;
        }
        #endregion

        /// <summary>
        /// 将Doc转换为图片
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="imageFolderName"></param>
        /// <param name="imageFolderRootPath"></param>
        public void ConvertToImage(string filepath,string imageFolderName, string imageFolderRootPath)
        {
            if(!Directory.Exists(imageFolderRootPath))
            {
                ///若路径不存在,创建路径
                Directory.CreateDirectory(imageFolderRootPath);
            }


            Aspose.Words.Document doc = new Aspose.Words.Document(filepath);
            for (var i = 0; i < doc.PageCount; i++)
            {
                Aspose.Words.Saving.ImageSaveOptions options = new Aspose.Words.Saving.ImageSaveOptions(Aspose.Words.SaveFormat.Png);
                options.DmlEffectsRenderingMode = Aspose.Words.Saving.DmlEffectsRenderingMode.Fine;
                options.PageIndex = i;

                string imageFilePath = string.Format(@"{0}\{1}\{2}.png",imageFolderRootPath, imageFolderName, i);
                doc.Save(imageFilePath, options);
            }
            string jsFileName = string.Format(@"{0}\{1}\{2}.js", imageFolderRootPath, imageFolderName, "info");
            var fileType = (filepath.EndsWith(".docx")) ? "docx" : "doc";
            File.WriteAllText(jsFileName, "var imageInfo={PageCount:" + doc.PageCount + ",FileType:'" + fileType + "'}");

        }
 
    }
}
