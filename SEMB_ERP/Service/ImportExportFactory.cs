using System.Data;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Components;
using System;
using SEMB_ERP.Function;
using System.Threading.Tasks;

namespace SEMB_ERP.Service
{
    public class ImportExportFactory
    {
        //string dbString = new DatabaseAccessLayer().ConnectionString;

        public readonly string ConnectionString = new DatabaseAccessLayer().ConnectionString;
        //public readonly string ConnectionString = "Data Source=10.155.129.223;Initial Catalog=BLP_Material_Tracking;Persist Security Info=True;User ID=semb;Password=Semb@123;MultipleActiveResultSets=true";
        private readonly FileManagementService _fileManagement;
        private readonly ExcelServiceProvider _excelService;

        public ImportExportFactory(FileManagementService fileManagement,
            ExcelServiceProvider excelService)
        {
            _fileManagement = fileManagement;
            _excelService = excelService;
        }

        public ImportExportFactory() :
            this(new FileManagementService(),
                new ExcelServiceProvider())
        {
        }

        public void ImportOrder(IFormFile file, string id_login, string sesa_id)
        {
            string query = "DELETE FROM temp_order WHERE inserted_by='" + sesa_id + "'";
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }

            var uploadedFilePath = _fileManagement.UploadFile(file);

            if (uploadedFilePath == "Upload Fail")
            {
                return;
            }
            var fileInfo = new FileInfo(uploadedFilePath);

            var dataTable = _excelService.Excel_To_DataTable(fileInfo);

            BulkInsertAsset(dataTable, id_login, sesa_id);
        }
        public void BulkInsertAsset(DataTable tbl, string id_login, string sesa_id)
        {
            //string monthYear = month + "-" + year.Substring(year.Length - 2); // Jan-23
            //string sesa_id = HttpContext.Session.GetString("sesa_id");
            tbl.Columns.Add("id_upload", typeof(string));
            tbl.Columns.Add("inserted_by", typeof(string));

            // Set the value of the "Plant" column for each row in the DataTable
            foreach (DataRow row in tbl.Rows)
            {
                row["id_upload"] = id_login;
                row["inserted_by"] = sesa_id;
            }

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(conn))
                {
                    //Set the database table name.
                    sqlBulkCopy.DestinationTableName = "dbo.temp_order";

                    // Map the Excel columns with that of the database table, this is optional but good if you do (you have to do for all columns)
                    sqlBulkCopy.ColumnMappings.Add("Column 1", "material_type");
                    sqlBulkCopy.ColumnMappings.Add("Column 2", "partno");
                    sqlBulkCopy.ColumnMappings.Add("Column 3", "po_no");
                    sqlBulkCopy.ColumnMappings.Add("Column 4", "qty");
                    sqlBulkCopy.ColumnMappings.Add("Column 5", "uom");
                    sqlBulkCopy.ColumnMappings.Add("Column 6", "revision");
                    sqlBulkCopy.ColumnMappings.Add("Column 7", "project_name");
                    sqlBulkCopy.ColumnMappings.Add("Column 8", "storage_requirement");
                    sqlBulkCopy.ColumnMappings.Add("Column 9", "supplier_name");
                    sqlBulkCopy.ColumnMappings.Add("Column 10", "order_type");
                    sqlBulkCopy.ColumnMappings.Add("Column 11", "unit_price");
                    sqlBulkCopy.ColumnMappings.Add("Column 12", "length_mm");
                    sqlBulkCopy.ColumnMappings.Add("Column 13", "width_mm");
                    sqlBulkCopy.ColumnMappings.Add("Column 14", "height_mm");
                    sqlBulkCopy.ColumnMappings.Add("Column 15", "remark");
                    sqlBulkCopy.ColumnMappings.Add("id_upload", "id_upload");
                    sqlBulkCopy.ColumnMappings.Add("inserted_by", "inserted_by");

                    conn.Open();
                    sqlBulkCopy.WriteToServer(tbl);
                    conn.Close();
                }
            }

            //using (SqlConnection conn = new SqlConnection(ConnectionString))
            //{
            //    conn.Open();
            //    SqlCommand cmd = new SqlCommand("INSERT_ASSET_LIST", conn);
            //    cmd.CommandType = CommandType.StoredProcedure;
            //    cmd.Parameters.AddWithValue("@id_login", id_login);
            //    cmd.Parameters.AddWithValue("@uploaded_by", sesa_id);
            //    cmd.ExecuteNonQuery();
            //    conn.Close();
            //}
        }
        public string ImportFileRename(IFormFile file_doc, string filename, string subfolder)
        {
            var uploadedFilePath = _fileManagement.UploadFileRename(file_doc, filename, subfolder);
            return uploadedFilePath;
        }

    }
}
