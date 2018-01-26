using Aspose.Pdf;
using Aspose.Pdf.Devices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WangJun.OA
{
    public class PDFService
    {
        public static PDFService GetInstance()
        {
            return new PDFService();
        }

        public void ConvertToImage(string filepath, string imageFolderName, string imageFolderRootPath)
        {
            if (!Directory.Exists(imageFolderRootPath))
            {
                ///若路径不存在,创建路径
                Directory.CreateDirectory(imageFolderRootPath);
            }

            if (!Directory.Exists(imageFolderRootPath+"\\"+imageFolderName))
            {
                ///若路径不存在,创建路径
                Directory.CreateDirectory(imageFolderRootPath + "\\" + imageFolderName);
            }


            Document pdfDocument = new Document(filepath);

            for (int pageCount = 1; pageCount <= pdfDocument.Pages.Count; pageCount++)
            {
                string imageFilePath = string.Format(@"{0}\{1}\{2}.png", imageFolderRootPath, imageFolderName, pageCount);

                using (FileStream imageStream = new FileStream(imageFilePath, FileMode.Create))
                {
                    // Create Resolution object
                    Resolution resolution = new Resolution(300);
                    // Create PNG device with specified attributes
                    PngDevice pngDevice = new PngDevice(resolution);
                    // Convert a particular page and save the image to stream
                    pngDevice.Process(pdfDocument.Pages[pageCount], imageStream);
                    // Close stream
                    imageStream.Close();
                }
            }

            string jsFileName = string.Format(@"{0}\{1}\{2}.js", imageFolderRootPath, imageFolderName, "info");
            var fileType = (filepath.EndsWith(".docx")) ? "docx" : "doc";
            File.WriteAllText(jsFileName, "var imageInfo={PageCount:" + pdfDocument.Pages.Count + ",FileType:'" + fileType + "'}");
        }
    }
}
