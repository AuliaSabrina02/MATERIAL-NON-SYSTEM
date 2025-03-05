using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using OfficeOpenXml;

namespace SEMB_ERP.Service
{
    public class ExcelServiceProvider
    {
        public ExcelWorksheet Load_Excel_File_CSV(FileInfo fileInfo)
        {
            // csv option
            //set the formatting options
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            ExcelTextFormat format = new ExcelTextFormat();
            format.Delimiter = ',';
            format.Culture = new CultureInfo(Thread.CurrentThread.CurrentCulture.ToString());
            format.Culture.DateTimeFormat.ShortDatePattern = "dd-mm-yyyy";
            format.Encoding = new UTF8Encoding();
            ExcelPackage excelPackage = new ExcelPackage();
            //create a WorkSheet
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Sheet 1");

            //load the CSV data into cell A1
            worksheet.Cells["A1"].LoadFromText(fileInfo, format);
            return worksheet;
        }

        public ExcelWorksheet Load_Excel_File_XLSX(FileInfo fileInfo)
        {
            //xlsx option
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            var excelPackage_xlsx = new ExcelPackage(fileInfo);
            var excelWorkSheet = excelPackage_xlsx.Workbook.Worksheets.First();
            return excelWorkSheet;
        }
        public DataTable Excel_To_DataTable(FileInfo fileInfo, int row = 1, int col = 1, bool hasHeader = true)
        {
            var worksheet = (dynamic)null;
            if (fileInfo.ToString().EndsWith(".xlsx"))
            {
                worksheet = Load_Excel_File_XLSX(fileInfo);
            }
            else if (fileInfo.ToString().EndsWith(".csv"))
            {
                worksheet = Load_Excel_File_CSV(fileInfo);
            }
            else if (fileInfo.ToString().EndsWith(".XLSX"))
            {
                worksheet = Load_Excel_File_XLSX(fileInfo);
            }
            DataTable tbl = new DataTable();

            foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
            {
                //tbl.Columns.Add(hasHeader ? firstRowCell.Text : string.Format("Column {0}", firstRowCell.Start.Column));
                tbl.Columns.Add(string.Format("Column {0}", firstRowCell.Start.Column));
            }
            var startRow = hasHeader ? 2 : 1;
            for (int rowNum = startRow; rowNum <= worksheet.Dimension.End.Row; rowNum++)
            {
                var worksheetRow = worksheet.Cells[rowNum, 1, rowNum, worksheet.Dimension.End.Column];
                DataRow tblRow = tbl.Rows.Add();
                foreach (var cell in worksheetRow)
                {
                    tblRow[cell.Start.Column - 1] = cell.Text.Trim().Replace(",", "");
                }
            }
            return tbl;
        }
    }
}
