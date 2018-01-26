using Aspose.Cells;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WangJun.Data;

namespace WangJun.OA
{
    /// <summary>
    /// Excel 服务
    /// </summary>
    public class ExcelService
    {
        public static ExcelService GetInstance()
        {
            return new ExcelService();
        }

        #region 转换为图片
        /// <summary>
        /// 转换为图片
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="imageFolderName"></param>
        /// <param name="imageFolderRootPath"></param>
        public void ConvertToImage(string filepath, string imageFolderName, string imageFolderRootPath)
        {
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


            Aspose.Cells.Workbook workbook = new Aspose.Cells.Workbook(filepath);
            var count = 0;
            foreach (Aspose.Cells.Worksheet sheet in workbook.Worksheets)
            {

                if (sheet.IsVisible)
                {
                    //Define ImageOrPrintOptions
                    Aspose.Cells.Rendering.ImageOrPrintOptions imgOptions = new Aspose.Cells.Rendering.ImageOrPrintOptions();
                    //Specify the image format
                    imgOptions.AllColumnsInOnePagePerSheet = true;
                    imgOptions.IsCellAutoFit = true;
                    imgOptions.OnlyArea = false;
                    imgOptions.OnlyArea = true;
                    imgOptions.ImageFormat = System.Drawing.Imaging.ImageFormat.Png;
                    //Only one page for the whole sheet would be rendered
                    imgOptions.OnePagePerSheet = false;

                    //Render the sheet with respect to specified image/print options
                    Aspose.Cells.Rendering.SheetRender sr = new Aspose.Cells.Rendering.SheetRender(sheet, imgOptions);
                    for (int i = 0; i < sr.PageCount; i++)
                    {
                        count++;
                        string imageFilePath = string.Format(@"{0}\{1}\{2}.png", imageFolderRootPath, imageFolderName, i);
                        sr.ToImage(i, imageFilePath);
                    }
                    //Render the image for the sheet
                    
                    //System.Drawing.Bitmap bitmap = sr.ToImage(0,"D:\\1.jpg");

                    //Save the image file specifying its image format.
                    // bitmap.Save(@"d:\test\SheetImage.jpg");
                }
                 
            }

            string jsFileName = string.Format(@"{0}\{1}\{2}.js", imageFolderRootPath, imageFolderName,"info");
            var fileType = (filepath.EndsWith(".xlsx")) ? "xlsx" : "xls";
            File.WriteAllText(jsFileName, "var imageInfo={PageCount:" + count + ",FileType:'" + fileType + "'}");



        }
        #endregion

        #region 转换为Json数据
        public void ConvertToJson(string filepath, string imageFolderName, string imageFolderRootPath)
        {
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


            Aspose.Cells.Workbook workbook = new Aspose.Cells.Workbook(filepath);
            var count = 0;
            string json = "{}";
            string tableIndex = "{}";
            foreach (Aspose.Cells.Worksheet sheet in workbook.Worksheets)
            {

                if (sheet.IsVisible&& sheet.IsSelected) ///若可见
                {
                    List<Dictionary<string, object>> cellList = new List<Dictionary<string, object>>();
                    Dictionary<string, object> sheetData = new Dictionary<string, object>();
                    sheetData["Name"] = sheet.Name;
                    sheetData["Cells"] = cellList;
                    Dictionary<string, int> table = new Dictionary<string, int>();
                    foreach (Cell item in sheet.Cells)
                    {
                        Dictionary<string, object> itemDict = new Dictionary<string, object>();
                        itemDict["Value"] = item.Value;
                        itemDict["Name"] = item.Name;
                        itemDict["Row"] = item.Row;
                        if(item.Name.StartsWith("B") && null != item.Value && !table.ContainsKey(item.Value.ToString())) ///若是以工号打头
                        {
                            table.Add(item.Value.ToString(), item.Row);
                        }
                        cellList.Add(itemDict);
                    }
                    json = Convertor.FromObjectToJson(sheetData);
                    tableIndex = Convertor.FromObjectToJson(table);
                }

            }
            string jsWageIndex = string.Format(@"{0}\{1}\WageIndex{2}{3}.js", imageFolderRootPath, imageFolderName,DateTime.Now.Year,DateTime.Now.Month);
            File.WriteAllText(jsWageIndex, tableIndex);
            string jsFileName = string.Format(@"{0}\{1}\{2}json.js", imageFolderRootPath, imageFolderName, count);
            File.WriteAllText(jsFileName, json);



        }
        #endregion

        #region 转换为Json数据
        public void ConvertToJson(string filepath, string imageFolderName, string imageFolderRootPath,string keyColumn)
        {
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


            Aspose.Cells.Workbook workbook = new Aspose.Cells.Workbook(filepath);
            var count = 0;
            string data = "{}";
            string tableIndex = "{}";
            foreach (Aspose.Cells.Worksheet sheet in workbook.Worksheets)
            {

                if (sheet.IsVisible && sheet.IsSelected) ///若可见
                {
                    List<Dictionary<string, object>> cellList = new List<Dictionary<string, object>>();
                    Dictionary<string, object> sheetData = new Dictionary<string, object>();
                    sheetData["Name"] = sheet.Name;
                    sheetData["Cells"] = cellList;
                    Dictionary<string, int> table = new Dictionary<string, int>();
                    foreach (Cell item in sheet.Cells)
                    {
                        Dictionary<string, object> itemDict = new Dictionary<string, object>();
                        itemDict["Value"] = item.Value;
                        itemDict["Name"] = item.Name;
                        itemDict["Row"] = item.Row;
                        if (!string.IsNullOrWhiteSpace(keyColumn)&& item.Name.StartsWith(keyColumn) && null != item.Value && !table.ContainsKey(item.Value.ToString())) ///若是以工号打头
                        {
                            table.Add(item.Value.ToString(), item.Row);
                        }
                        cellList.Add(itemDict);
                    }
                    data = Convertor.FromObjectToJson(sheetData);
                    tableIndex = Convertor.FromObjectToJson(table);
                }

            }
            string jsWageIndex = string.Format(@"{0}\{1}\Index.js", imageFolderRootPath, imageFolderName);
            File.WriteAllText(jsWageIndex, tableIndex,Encoding.UTF8);
            string jsFileName = string.Format(@"{0}\{1}\data.js", imageFolderRootPath, imageFolderName);
            File.WriteAllText(jsFileName, data,  Encoding.UTF8);



        }
        #endregion
    }
}
