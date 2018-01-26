using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace WangJun.OA
{
    /// <summary>
    /// PPT服务
    /// </summary>
    public class PPTService
    {
        public static PPTService GetInstance()
        {
            return new PPTService();
        }

        public void ConvertToImage(string filepath, string imageFolderName, string imageFolderRootPath)
        {
            //Access the first slide
            if (!Directory.Exists(imageFolderRootPath))
            {
                ///若路径不存在,创建路径
                Directory.CreateDirectory(imageFolderRootPath);
            }

            if (!Directory.Exists(imageFolderRootPath + "\\" + imageFolderName))
            {
                ///若路径不存在,创建路径
                Directory.CreateDirectory(imageFolderRootPath + "\\" + imageFolderName);
            }

            Aspose.Slides.Presentation pres = new Aspose.Slides.Presentation(filepath);

            var i = 0;
            foreach (Aspose.Slides.Slide sld in pres.Slides)
            {
                //User defined dimension
                int desiredX = 1200;
                int desiredY = 800;

                //Getting scaled value  of X and Y
                float ScaleX = (float)(1.0 / pres.SlideSize.Size.Width) * desiredX;
                float ScaleY = (float)(1.0 / pres.SlideSize.Size.Height) * desiredY;

                //Create a full scale image
                //Image bmp = sld.GetThumbnail(ScaleX, ScaleY);
                Image bmp = sld.GetThumbnail(1,1);
                string imageFilePath = string.Format(@"{0}\{1}\{2}.png", imageFolderRootPath, imageFolderName, i++);

                //Save the image to disk in JPEG format
                bmp.Save(imageFilePath, System.Drawing.Imaging.ImageFormat.Png);
            }

            string jsFileName = string.Format(@"{0}\{1}\{2}.js", imageFolderRootPath, imageFolderName, "info");
            var fileType = (filepath.EndsWith(".pptx")) ? "pptx" : "ppt";
            File.WriteAllText(jsFileName, "var imageInfo={PageCount:" + pres.Slides.Count + ",FileType:'" + fileType + "'}");
        }
    }
}
