using SEMB_ERP.Models;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;

namespace SEMB_ERP.Function
{
    public class DatabaseAccessLayer
    {
        public string ConnectionString = "Data Source=10.155.152.114;Initial Catalog=SEMB_ERP;Persist Security Info=True;User ID=dt;Password=Dt@123;MultipleActiveResultSets=true";
        public string ConnectionStringBLP = "Data Source=10.155.129.223;Initial Catalog=DBBLP;Persist Security Info=True;User ID=semb;Password=Semb@123;MultipleActiveResultSets=true";

        public List<OrderTempListModel> GetTempOrder(string id_upload, string sesa_id)
        {
            List<OrderTempListModel> dataList = new List<OrderTempListModel>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("GET_TEMP_ORDER", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_upload", id_upload);
                    cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                    using SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            OrderTempListModel row = new OrderTempListModel();
                            row.material_type = reader["material_type"].ToString();
                            row.material_type_msg = reader["material_type_msg"].ToString();
                            row.partno = reader["partno"].ToString();
                            row.partno_msg = reader["partno_msg"].ToString();
                            row.po_no = reader["po_no"].ToString();
                            row.po_no_msg = reader["po_no_msg"].ToString();
                            row.qty = reader["qty"].ToString();
                            row.qty_msg = reader["qty_msg"].ToString();
                            row.uom = reader["uom"].ToString();
                            row.uom_msg = reader["uom_msg"].ToString();
                            row.revision = reader["revision"].ToString();
                            row.revision_msg = reader["revision_msg"].ToString();
                            row.project_name = reader["project_name"].ToString();
                            row.project_name_msg = reader["project_name_msg"].ToString();
                            row.storage_requirement = reader["storage_requirement"].ToString();
                            row.storage_requirement_msg = reader["storage_requirement_msg"].ToString();
                            row.supplier_name = reader["supplier_name"].ToString();
                            row.supplier_name_msg = reader["supplier_name_msg"].ToString();
                            row.order_type = reader["order_type"].ToString();
                            row.order_type_msg = reader["order_type_msg"].ToString();
                            row.unit_price = reader["unit_price"].ToString();
                            row.unit_price_msg = reader["unit_price_msg"].ToString();
                            row.length_mm = reader["length_mm"].ToString();
                            row.length_mm_msg = reader["length_mm_msg"].ToString();
                            row.width_mm = reader["width_mm"].ToString();
                            row.width_mm_msg = reader["width_mm_msg"].ToString();
                            row.height_mm = reader["height_mm"].ToString();
                            row.height_mm_msg = reader["height_mm_msg"].ToString();
                            row.remark = reader["remark"].ToString();
                            row.is_error = Convert.ToInt32(reader["is_error"]);
                            dataList.Add(row);
                        }
                    }
                }

                conn.Close();
            }
            return dataList;
        }
        public List<UserDetailModel> GetUserDetail(string sesa_id)
        {
            List<UserDetailModel> dataList = new List<UserDetailModel>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("GET_USER_DETAIL", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                    using SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            UserDetailModel row = new UserDetailModel();
                            row.sesa_id = reader["sesa_id"].ToString();
                            row.name = reader["name"].ToString();
                            row.email = reader["email"].ToString();
                            row.level = reader["level"].ToString();
                            row.role = reader["role"].ToString();
                            row.department = reader["department"].ToString();
                            row.manager_sesa_id = reader["manager_sesa_id"].ToString();
                            row.manager_name = reader["manager_name"].ToString();
                            row.manager_email = reader["manager_email"].ToString();
                            row.other_dept = reader["other_dept"].ToString();
                            dataList.Add(row);
                        }
                    }
                }

                conn.Close();
            }
            return dataList;
        }
        public List<UserDetailModel> GetUserRole(string sesa_id)
        {
            List<UserDetailModel> dataList = new List<UserDetailModel>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("GET_USER_ROLE", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                    using SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            UserDetailModel row = new UserDetailModel();
                            row.role = reader["role_name"].ToString();
                            dataList.Add(row);
                        }
                    }
                }

                conn.Close();
            }
            return dataList;
        }
        public List<CategoryModel> GetPIC(string search_value)
        {
            List<CategoryModel> listPIC = new List<CategoryModel>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT TOP 20 sesa_id, name FROM mst_users WHERE name LIKE '%" + search_value + "%' ORDER BY name ASC", conn))
                {
                    //cmd.CommandType = CommandType.StoredProcedure;
                    //cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                    using SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            CategoryModel row = new CategoryModel();
                            row.Id = reader["sesa_id"].ToString();
                            row.Text = reader["name"].ToString();
                            listPIC.Add(row);
                        }
                    }
                }

                conn.Close();
            }
            return listPIC;
        }
        public List<string> GetStatus()
        {
            List<string> dataStatus = new List<string>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT status_desc FROM mst_status ORDER BY status_code", conn))
                {
                    //cmd.CommandType = CommandType.StoredProcedure;
                    //cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                    using SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            //UserDetailModel row = new UserDetailModel();
                            //row.role = reader["role_name"].ToString();
                            //dataList.Add(row);
                            dataStatus.Add(reader["status_desc"].ToString() ?? "");
                        }
                    }
                }

                conn.Close();
            }
            return dataStatus;
        }
        public List<string> GetStatusRequest()
        {
            List<string> dataStatus = new List<string>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT status_desc FROM mst_status_request ORDER BY status_code", conn))
                {
                    //cmd.CommandType = CommandType.StoredProcedure;
                    //cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                    using SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            //UserDetailModel row = new UserDetailModel();
                            //row.role = reader["role_name"].ToString();
                            //dataList.Add(row);
                            dataStatus.Add(reader["status_desc"].ToString() ?? "");
                        }
                    }
                }

                conn.Close();
            }
            return dataStatus;
        }
        public string GetUserPlant(string sesa_id)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 ISNULL(plant,'-') as plant FROM mst_users WHERE sesa_id=@sesa_id", conn))
                {
                    //cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sesa_id", sesa_id);

                    string plant = (string)cmd.ExecuteScalar();

                    return plant;
                    //cmd.ExecuteScalar();
                }
            }
        }

        public string UpdateUser(string sesa_id, string full_name, string email, string manager_sesa_id, string manager_name)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("UPDATE_USER", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                    cmd.Parameters.AddWithValue("@full_name", full_name);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@manager_sesa_id", manager_sesa_id);
                    cmd.Parameters.AddWithValue("@manager_name", manager_name);
                    cmd.ExecuteNonQuery();
                    //cmd.ExecuteScalar();
                    return "success";
                }
            }
        }
        public List<string> GetPrinter()
        {
            List<string> listPrinter = new List<string>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT printer_name FROM mst_printer ORDER BY printer_name", conn))
                {
                    //cmd.CommandType = CommandType.StoredProcedure;
                    //cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                    using SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            listPrinter.Add(reader["printer_name"].ToString() ?? "");
                        }
                    }
                }

                conn.Close();
            }
            return listPrinter;
        }
        public string SubmitUploadOrder(string file_support, string id_upload, string sesa_id)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SUBMIT_UPLOAD_ORDER", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@file_support", file_support);
                    cmd.Parameters.AddWithValue("@id_upload", id_upload);
                    cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                    cmd.ExecuteNonQuery();
                    //cmd.ExecuteScalar();
                    return "success";
                }
            }
        }
        public string SubmitOrder(string id_upload, string material_type, string partno, string po_no, double qty, string uom, string revision, string project_name,
            string storage_requirement, string supplier_name, string pic, string order_type, double unit_price, double length_mm, double width_mm, double height_mm,
            string remark, string file_support, string sesa_id)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT_ORDER", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_upload", id_upload);
                    cmd.Parameters.AddWithValue("@material_type", material_type);
                    cmd.Parameters.AddWithValue("@partno", partno);
                    cmd.Parameters.AddWithValue("@po_no", po_no);
                    cmd.Parameters.AddWithValue("@qty", qty);
                    cmd.Parameters.AddWithValue("@uom", uom);
                    cmd.Parameters.AddWithValue("@revision", revision);
                    cmd.Parameters.AddWithValue("@project_name", project_name);
                    cmd.Parameters.AddWithValue("@storage_requirement", storage_requirement);
                    cmd.Parameters.AddWithValue("@supplier_name", supplier_name);
                    cmd.Parameters.AddWithValue("@pic", pic);
                    cmd.Parameters.AddWithValue("@order_type", order_type);
                    cmd.Parameters.AddWithValue("@unit_price", unit_price);
                    cmd.Parameters.AddWithValue("@length_mm", length_mm);
                    cmd.Parameters.AddWithValue("@width_mm", width_mm);
                    cmd.Parameters.AddWithValue("@height_mm", height_mm);
                    cmd.Parameters.AddWithValue("@remark", remark);
                    cmd.Parameters.AddWithValue("@file_support", file_support);
                    cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                    cmd.ExecuteNonQuery();
                    //cmd.ExecuteScalar();
                    return "success";
                }
            }
        }

        public string UpdateOrder(string id_order, string id_upload, string material_type, string partno, string po_no, double qty, string uom, string revision, string project_name,
    string storage_requirement, string supplier_name, string order_type, double unit_price, double length_mm, double width_mm, double height_mm,
    string remark, string file_support, string sesa_id)
        {
            if (file_support == "")
            {
                file_support = null;
            }
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("UPDATE_ORDER", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_order", id_order);
                    cmd.Parameters.AddWithValue("@id_upload", id_upload);
                    cmd.Parameters.AddWithValue("@material_type", material_type);
                    cmd.Parameters.AddWithValue("@partno", partno);
                    cmd.Parameters.AddWithValue("@po_no", po_no);
                    cmd.Parameters.AddWithValue("@qty", qty);
                    cmd.Parameters.AddWithValue("@uom", uom);
                    cmd.Parameters.AddWithValue("@revision", revision);
                    cmd.Parameters.AddWithValue("@project_name", project_name);
                    cmd.Parameters.AddWithValue("@storage_requirement", storage_requirement);
                    cmd.Parameters.AddWithValue("@supplier_name", supplier_name);
                    cmd.Parameters.AddWithValue("@order_type", order_type);
                    cmd.Parameters.AddWithValue("@unit_price", unit_price);
                    cmd.Parameters.AddWithValue("@length_mm", length_mm);
                    cmd.Parameters.AddWithValue("@width_mm", width_mm);
                    cmd.Parameters.AddWithValue("@height_mm", height_mm);
                    cmd.Parameters.AddWithValue("@remark", remark);
                    cmd.Parameters.AddWithValue("@file_support", file_support);
                    cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                    cmd.ExecuteNonQuery();
                    //cmd.ExecuteScalar();
                    return "success";
                }
            }
        }

        public List<OrderListModel> GetDataGR(string id_order_string)
        {
            List<OrderListModel> dataGR = new List<OrderListModel>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("GET_DATA_GR", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_order_string", id_order_string);
                    using SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            OrderListModel row = new OrderListModel();
                            row.id_order = Convert.ToInt32(reader["id_order"]);
                            row.material_type = reader["material_type"].ToString();
                            row.partno = reader["partno"].ToString();
                            row.po_no = reader["po_no"].ToString();
                            row.qty = Convert.ToDecimal(reader["qty"]);
                            row.uom = reader["uom"].ToString();
                            row.revision = reader["revision"].ToString();
                            row.supplier_name = reader["supplier_name"].ToString();
                            //row
                            dataGR.Add(row);
                        }
                    }
                }
            }
            return dataGR;
        }
        public string SubmitGR(string id_order_spq_string, string sesa_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SUBMIT_GR", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id_order_spq_string", id_order_spq_string);
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            return result.ToString() ?? ""; // Return the value as a string
                        }
                        else
                        {
                            return "No result returned from stored procedure."; // Handle no result
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Handle SQL Server-specific exceptions
                // Log the error for debugging purposes
                Console.WriteLine($"SQL Error: {ex.Message}");
                // Return an error message or throw a custom exception
                return $"SQL Error: {ex.Message}"; // Or throw new Exception($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                // Log the error
                Console.WriteLine($"General Error: {ex.Message}");
                // Return an error message or throw a custom exception
                return $"General Error: {ex.Message}"; // Or throw new Exception($"An error occurred: {ex.Message}");
            }
        }
        public List<OrderListModel> GetGRDetail(int id_gr)
        {
            List<OrderListModel> dataGR = new List<OrderListModel>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("GET_GR_DETAIL", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_gr", id_gr);
                    using SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            OrderListModel row = new OrderListModel();
                            row.id_order = Convert.ToInt32(reader["id_order"]);
                            row.material_type = reader["material_type"].ToString();
                            row.partno = reader["partno"].ToString();
                            row.po_no = reader["po_no"].ToString();
                            row.qty = Convert.ToDecimal(reader["qty"]);
                            row.uom = reader["uom"].ToString();
                            row.revision = reader["revision"].ToString();
                            row.supplier_name = reader["supplier_name"].ToString();
                            row.spq = Convert.ToDouble(reader["spq"]);
                            row.id_gr = Convert.ToInt32(reader["id_gr"]);
                            row.gr_no = reader["gr_no"].ToString();
                            row.total_box = Convert.ToInt32(reader["total_box"]);
                            //row
                            dataGR.Add(row);
                        }
                    }
                }
            }
            return dataGR;
        }

        public List<string> GET_CAT_NON_CONF()
        {
            List<string> catList = new List<string>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT DISTINCT category_name FROM mst_category_non_conf ORDER BY category_name ASC", conn))
                {
                    using SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            catList.Add(reader["category_name"].ToString() ?? "");
                        }
                    }
                }

                conn.Close();
            }
            return catList;
        }

        public string SUBMIT_NON_CONF(string material_type, string partno, string po_no, double qty, string uom,
            string supplier_name, string pic, string category_issue, string detail_issue, string file_support, string sesa_id)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT_NON_CONF", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@material_type", material_type);
                    cmd.Parameters.AddWithValue("@partno", partno);
                    cmd.Parameters.AddWithValue("@po_no", po_no);
                    cmd.Parameters.AddWithValue("@qty", qty);
                    cmd.Parameters.AddWithValue("@uom", uom);
                    cmd.Parameters.AddWithValue("@supplier_name", supplier_name);
                    cmd.Parameters.AddWithValue("@pic", pic);
                    cmd.Parameters.AddWithValue("@category_issue", category_issue);
                    cmd.Parameters.AddWithValue("@detail_issue", detail_issue);
                    cmd.Parameters.AddWithValue("@file_support", file_support);
                    cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                    cmd.ExecuteNonQuery();
                    //cmd.ExecuteScalar();
                    return "success";
                }
            }
        }

        public List<CategoryModel> GetCatNonConf(string search_value)
        {
            List<CategoryModel> listCAT = new List<CategoryModel>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"SELECT DISTINCT category_name FROM mst_category_non_conf WHERE category_name LIKE '%" + search_value + "%' ORDER BY category_name ASC", conn))
                {
                    //cmd.CommandType = CommandType.StoredProcedure;
                    //cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                    using SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            CategoryModel row = new CategoryModel();
                            row.Id = reader["category_name"].ToString();
                            row.Text = reader["category_name"].ToString();
                            listCAT.Add(row);
                        }
                    }
                }

                conn.Close();
            }
            return listCAT;
        }

        public string UPDATE_NON_CONF(string id_non_conf, string material_type, string partno, string po_no, double qty, string uom,
    string supplier_name, string pic, string category_issue, string detail_issue, string file_support, string sesa_id)
        {
            if (file_support == "")
            {
                file_support = null;
            }
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("UPDATE_NON_CONF", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_non_conf", id_non_conf);
                    cmd.Parameters.AddWithValue("@material_type", material_type);
                    cmd.Parameters.AddWithValue("@partno", partno);
                    cmd.Parameters.AddWithValue("@po_no", po_no);
                    cmd.Parameters.AddWithValue("@qty", qty);
                    cmd.Parameters.AddWithValue("@uom", uom);
                    cmd.Parameters.AddWithValue("@supplier_name", supplier_name);
                    cmd.Parameters.AddWithValue("@pic", pic);
                    cmd.Parameters.AddWithValue("@category_issue", category_issue);
                    cmd.Parameters.AddWithValue("@detail_issue", detail_issue);
                    cmd.Parameters.AddWithValue("@file_support", file_support ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                    cmd.ExecuteNonQuery();
                    //cmd.ExecuteScalar();
                    return "success";
                }
            }
        }
        //public List<PartModel> GetPartBin(string box_id)
        //{
        //    List<PartModel> listBin = new List<PartModel>();
        //    using (SqlConnection conn = new SqlConnection(ConnectionString))
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = new SqlCommand(@"SELECT sbin FROM mst_sbin", conn)) // store pro untuk cross check di DBBLP.dbo.packagedetail
        //        {
        //            //cmd.CommandType = CommandType.StoredProcedure;
        //            //cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
        //            using SqlDataReader reader = cmd.ExecuteReader();
        //            if (reader.HasRows)
        //            {
        //                while (reader.Read())
        //                {
        //                    PartModel row = new PartModel();
        //                    row.sbin = reader["sbin"].ToString();
        //                    listBin.Add(row);
        //                }
        //            }
        //        }

        //        conn.Close();
        //    }
        //    return listBin;
        //}

        public List<PartModel> GetPartBin(string box_id)
        {
            List<PartModel> listBin = new List<PartModel>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("GET_BIN_LIST", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@input", box_id);
                    using SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (reader["Storage_Bin"] != DBNull.Value)
                            {
                                PartModel row = new PartModel();
                                row.sbin = reader["Storage_Bin"].ToString();
                                listBin.Add(row);
                            }
                        }
                    }
                }
            }
            return listBin;
        }

        public List<PartModel> GetBinItem(string box_id)
        {
            List<PartModel> parts = new List<PartModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM tmp_bin_matrial", conn);
                //cmd.Parameters.AddWithValue("@box_id", box_id);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    parts.Add(new PartModel
                    {
                        partno = reader["PartNo"].ToString(),
                        partname = reader["PartName"].ToString(),
                        qty = reader["Qty"].ToString()
                    });
                }
            }

            return parts;
        }

        public string InsertBinItem(string box_id, string sesa_id)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT_BIN_ITEM", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@input", box_id);
                    cmd.Parameters.AddWithValue("@user", sesa_id);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string status = reader["Status"].ToString();
                        if (status == "OK")
                        {
                            return "OK";
                        }
                        else if (status == "NOK")
                        {
                            return "NOK";
                        }
                    }
                    return "NOK";
                }
            }
        }

        public string DeleteBinItem(string partName)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"DELETE FROM tmp_bin_matrial WHERE PartName = @partName", conn))
                {
                    // Use parameterized query to prevent SQL injection
                    cmd.Parameters.AddWithValue("@partName", partName);

                    // Execute the command
                    int rowsAffected = cmd.ExecuteNonQuery();

                    // Check if any rows were affected
                    if (rowsAffected > 0)
                    {
                        return "OK";
                    }
                    else
                    {
                        return "NOK";
                    }
                }
            }
        }

        public string ValidateBin(string anybinId)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("VALIDATE_BIN", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@input", anybinId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string status = reader["Status"].ToString();
                        if (status == "OK")
                        {
                            return "OK";
                        }
                        else if (status == "NOK")
                        {
                            return "NOK";
                        }
                    }
                    return "NOK";
                }
            }
        }

        public string ConfirmBinItem(string binId, string sesa_id)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionStringBLP))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("UPDATE_BIN", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@input", binId);
                    cmd.Parameters.AddWithValue("@user", sesa_id);
                    cmd.ExecuteNonQuery();
                    return "OK";
                }
            }
        }

        public string ClearBin(string sesa_id)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"DELETE FROM tmp_bin_matrial WHERE DoneBy = @sesa_id", conn))
                {
                    // Use parameterized query to prevent SQL injection
                    cmd.Parameters.AddWithValue("@sesa_id", sesa_id);

                    // Execute the command
                    int rowsAffected = cmd.ExecuteNonQuery();

                    return "OK";
                }
            }
        }
        public int GetTotalTempReq(string sesa_id)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(COUNT(*),0) as total_req FROM temp_request WHERE added_by=@sesa_id", conn))
                {
                    //cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sesa_id", sesa_id);

                    int total_req = (int)cmd.ExecuteScalar();

                    return total_req;
                    //cmd.ExecuteScalar();
                }
            }
        }
        public string AddReqPicking(int id_order, decimal qty, string sesa_id)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("ADD_REQ_PICKING", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_order", id_order);
                    cmd.Parameters.AddWithValue("@qty", qty);
                    cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                    cmd.ExecuteNonQuery();
                    //cmd.ExecuteScalar();
                    return "success";
                }
            }
        }
        public List<RequestTempModel> GetTempReqList(string sesa_id)
        {
            List<RequestTempModel> tempList = new List<RequestTempModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("GET_TEMP_REQ_LIST", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    tempList.Add(new RequestTempModel
                    {
                        id_temp = Convert.ToInt32(reader["id_temp"]),
                        id_order = Convert.ToInt32(reader["id_order"]),
                        partno = reader["partno"].ToString(),
                        qty = Convert.ToDecimal(reader["qty"]),
                        stock_qty = Convert.ToDecimal(reader["stock_qty"]),
                        picked_qty = Convert.ToDecimal(reader["picked_qty"]),
                        available_qty = Convert.ToDecimal(reader["available_qty"]),
                        status_stock = reader["status_stock"].ToString()
                    });
                }
            }

            return tempList;
        }
        public string DeleteTempReq(int id_temp)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"DELETE FROM temp_request WHERE id_temp = @id_temp", conn))
                {
                    // Use parameterized query to prevent SQL injection
                    cmd.Parameters.AddWithValue("@id_temp", id_temp);

                    // Execute the command
                    int rowsAffected = cmd.ExecuteNonQuery();

                    return "OK";
                }
            }
        }
        public string SubmitReqPicking(string remark, string sesa_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SUBMIT_REQ_PICKING", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@remark", remark);
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            return result.ToString() ?? ""; // Return the value as a string
                        }
                        else
                        {
                            return "ERROR;No result returned from stored procedure."; // Handle no result
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Handle SQL Server-specific exceptions
                // Log the error for debugging purposes
                Console.WriteLine($"SQL Error: {ex.Message}");
                // Return an error message or throw a custom exception
                return $"SQL Error: {ex.Message}"; // Or throw new Exception($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                // Log the error
                Console.WriteLine($"General Error: {ex.Message}");
                // Return an error message or throw a custom exception
                return $"General Error: {ex.Message}"; // Or throw new Exception($"An error occurred: {ex.Message}");
            }
        }
        public List<RequestListModel> GetReqDetail(int id_request)
        {
            List<RequestListModel> tempList = new List<RequestListModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("GET_REQ_DETAIL", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_request", id_request);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    tempList.Add(new RequestListModel
                    {
                        id_det = Convert.ToInt32(reader["id_det"]),
                        id_order = Convert.ToInt32(reader["id_order"]),
                        partno = reader["partno"].ToString(),
                        qty = Convert.ToDecimal(reader["qty"]),
                        picked_qty = Convert.ToDecimal(reader["picked_qty"]),
                        uom = reader["uom"].ToString(),
                        material_type = reader["material_type"].ToString(),
                        status_pick = reader["status_picking"].ToString()
                    });
                }
            }

            return tempList;
        }

        public string GetRequestInfo(int id_request)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT request_no, remark, status_desc FROM v_request WHERE id_request=@id_request", conn))
                {
                    cmd.Parameters.AddWithValue("@id_request", id_request);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var result = new
                            {
                                request_no = reader["request_no"]?.ToString(),
                                remark = reader["remark"]?.ToString(),
                                status_desc = reader["status_desc"]?.ToString()
                            };
                            return JsonConvert.SerializeObject(result);
                        }
                    }
                }
            }
            return JsonConvert.SerializeObject(new
            {
                request_no = (string)null,
                remark = (string)null,
                status_desc = (string)null
            });
        }
        public List<RequestListModel> GetReqList()
        {
            List<RequestListModel> reqList = new List<RequestListModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM v_request WHERE status_request IN (1,2,3) ORDER BY record_date", conn);
                //cmd.CommandType = CommandType.StoredProcedure;
                //cmd.Parameters.AddWithValue("@id_request", id_request);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    reqList.Add(new RequestListModel
                    {
                        id_request = Convert.ToInt32(reader["id_request"]),
                        request_no = reader["request_no"].ToString(),
                        status_desc = reader["status_desc"].ToString(),
                        requested_by_name = reader["requested_by_name"].ToString(),
                        remark = reader["remark"].ToString(),
                        record_date = Convert.ToDateTime(reader["record_date"])
                    });
                }
            }

            return reqList;
        }
        public string DeleteTempPicking(string sesa_id)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"DELETE FROM temp_picking WHERE sesa_id = @sesa_id", conn))
                {
                    // Use parameterized query to prevent SQL injection
                    cmd.Parameters.AddWithValue("@sesa_id", sesa_id);

                    // Execute the command
                    int rowsAffected = cmd.ExecuteNonQuery();

                    return "OK";
                }
            }
        }
        public string OpenBlockBin(string partno, string sbin, string sesa_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("UPDATE tbl_picking_variance SET open_bin=1, open_date=getdate(), open_by=@sesa_id WHERE open_bin=0 AND partno=@partno AND sbin=@sbin", conn))
                    {
                        //cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                        cmd.Parameters.AddWithValue("@partno", partno);
                        cmd.Parameters.AddWithValue("@sbin", sbin);
                        cmd.ExecuteScalar();
                        return "OK";
                        //object result = cmd.ExecuteScalar();

                        //if (result != null)
                        //{
                        //    return result.ToString() ?? "";
                        //}
                        //else
                        //{
                        //    return "ERROR;No result returned from stored procedure.";
                        //}
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                return $"SQL Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                return $"General Error: {ex.Message}";
            }
        }
        public string StartPicking(int id_request, string sesa_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("START_PICKING", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id_request", id_request);
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            return result.ToString() ?? "";
                        }
                        else
                        {
                            return "ERROR;No result returned from stored procedure.";
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                return $"SQL Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                return $"General Error: {ex.Message}";
            }
        }
        public List<RequestListModel> GetReqMaterial(int id_request, string sesa_id)
        {
            List<RequestListModel> reqList = new List<RequestListModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("GET_PICKING_MATERIAL", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_request", id_request);
                cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    reqList.Add(new RequestListModel
                    {
                        id_det = Convert.ToInt32(reader["id_det"]),
                        partno = reader["partno"].ToString(),
                        qty = Convert.ToDecimal(reader["qty"]),
                        picked_qty = Convert.ToDecimal(reader["picked_qty"]),
                        status_pick = reader["status_pick"].ToString()
                    });
                }
            }

            return reqList;
        }
        public string StartPickingMaterial(int id_det, int id_request, string sesa_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("START_PICKING_MATERIAL", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id_det", id_det);
                        cmd.Parameters.AddWithValue("@id_request", id_request);
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            return result.ToString() ?? "";
                        }
                        else
                        {
                            return "ERROR;No result returned from stored procedure.";
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                return $"SQL Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                return $"General Error: {ex.Message}";
            }
        }
        public string GetBinPicking(int id_request, int id_det, string sesa_id)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("GET_BIN_PICKING", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_request", id_request);
                    cmd.Parameters.AddWithValue("@id_det", id_det);
                    cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var result = new
                            {
                                partno = reader["partno"]?.ToString(),
                                sbin = reader["sbin"]?.ToString()
                            };
                            return JsonConvert.SerializeObject(result);
                        }
                    }
                }
            }
            return JsonConvert.SerializeObject(new
            {
                partno = (string)null,
                sbin = (string)null
            });
        }
        public List<PickingModel> GetPickingTempQty(int id_request, int id_det, string sbin, string sesa_id)
        {
            List<PickingModel> pickList = new List<PickingModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("GET_PICKING_TEMP_QTY", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_request", id_request);
                cmd.Parameters.AddWithValue("@id_det", id_det);
                cmd.Parameters.AddWithValue("@sbin", sbin);
                cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    pickList.Add(new PickingModel
                    {
                        partno = reader["partno"].ToString(),
                        sbin = reader["sbin"].ToString(),
                        req_qty = Convert.ToDecimal(reader["req_qty"]),
                        temp_qty = Convert.ToDecimal(reader["temp_qty"])
                    });
                }
            }

            return pickList;
        }
        public List<TempBoxModel> GetTempBox(int id_request, int id_det, string sbin, string sesa_id)
        {
            List<TempBoxModel> tempList = new List<TempBoxModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM temp_box WHERE id_request=@id_request AND id_det=@id_det AND sbin=@sbin AND picked_by=@sesa_id ORDER BY record_date", conn);
                //cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_request", id_request);
                cmd.Parameters.AddWithValue("@id_det", id_det);
                cmd.Parameters.AddWithValue("@sbin", sbin);
                cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    tempList.Add(new TempBoxModel
                    {
                        id_temp_box = Convert.ToInt32(reader["id_temp_box"]),
                        box_id = Convert.ToString(reader["box_id"]),
                        qty = Convert.ToDecimal(reader["qty"])
                    });
                }
            }

            return tempList;
        }
        public string CheckBox(int id_request, int id_det, string partno, string sbin, string box_id, decimal remain_qty, string sesa_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("CHECK_BOX", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id_request", id_request);
                        cmd.Parameters.AddWithValue("@id_det", id_det);
                        cmd.Parameters.AddWithValue("@partno", partno);
                        cmd.Parameters.AddWithValue("@sbin", sbin);
                        cmd.Parameters.AddWithValue("@box_id", box_id);
                        cmd.Parameters.AddWithValue("@remain_qty", remain_qty);
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            return result.ToString() ?? "";
                        }
                        else
                        {
                            return "ERROR;No result returned from database.";
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                return $"SQL Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                return $"General Error: {ex.Message}";
            }
        }
        public string DeleteTempBox(string sesa_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM temp_box WHERE picked_by=@sesa_id", conn))
                    {
                        //cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                        cmd.ExecuteScalar();
                        return "OK";
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                return $"SQL Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                return $"General Error: {ex.Message}";
            }
        }
        public string BoxKitting(string box_id, decimal kit_qty, string sesa_id, string name)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionStringBLP))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("KITTING_BOX_NON_SYSTEM", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@box_id", box_id);
                        cmd.Parameters.AddWithValue("@kit_qty", kit_qty);
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                        cmd.Parameters.AddWithValue("@name", name);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return "OK;" + reader["box_ids"].ToString() ?? "";
                            }
                            else
                            {
                                return "ERROR;No Response";
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                return $"SQL Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                return $"General Error: {ex.Message}";
            }
        }
        public string FinishPicking(int id_request, int id_det, string sbin, string sesa_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("FINISH_PICKING", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id_request", id_request);
                        cmd.Parameters.AddWithValue("@id_det", id_det);
                        cmd.Parameters.AddWithValue("@sbin", sbin);
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            return result.ToString() ?? "";
                        }
                        else
                        {
                            return "ERROR;No result returned from database.";
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                return $"SQL Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                return $"General Error: {ex.Message}";
            }
        }
        public string VariancePicking(int id_request, int id_det, string sbin, string sesa_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("VARIANCE_PICKING", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id_request", id_request);
                        cmd.Parameters.AddWithValue("@id_det", id_det);
                        cmd.Parameters.AddWithValue("@sbin", sbin);
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            return result.ToString() ?? "";
                        }
                        else
                        {
                            return "ERROR;No result returned from database.";
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                return $"SQL Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                return $"General Error: {ex.Message}";
            }
        }

        public List<BinModel> GetMstBin()
        {
            List<BinModel> parts = new List<BinModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM mst_sbin", conn);
                //cmd.Parameters.AddWithValue("@box_id", box_id);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    parts.Add(new BinModel
                    {
                        BinId = reader["id_sbin"].ToString(),
                        BinName = reader["sbin"].ToString(),
                        Length = Convert.ToDecimal(reader["length_mm"]),
                        Width = Convert.ToDecimal(reader["width_mm"]),
                        Height = Convert.ToDecimal(reader["height_mm"])
                    });
                }
            }

            return parts;
        }
        public string DeleteMstBins(List<string> binNames)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var parameterList = string.Join(",", binNames.Select((s, i) => $"@binName{i}"));
                var sql = $"DELETE FROM mst_sbin WHERE sbin IN ({parameterList})";

                using (var command = new SqlCommand(sql, connection))
                {
                    for (int i = 0; i < binNames.Count; i++)
                    {
                        command.Parameters.AddWithValue($"@binName{i}", binNames[i]);
                    }

                    int rowsAffected = command.ExecuteNonQuery();

                    return rowsAffected > 0 ? "OK" : "No rows affected.";
                }
            }
        }

        public string AddMstBin(string bin_name, string bin_length, string bin_width, string bin_height)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"INSERT INTO mst_sbin (sbin, length_mm, width_mm, height_mm) VALUES (@bin_name, @bin_length, @bin_width, @bin_height)", conn))
                {
                    cmd.Parameters.AddWithValue("@bin_name", bin_name);
                    cmd.Parameters.AddWithValue("@bin_length", bin_length);
                    cmd.Parameters.AddWithValue("@bin_width", bin_width);
                    cmd.Parameters.AddWithValue("@bin_height", bin_height);

                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0 ? "OK" : "No rows affected.";
                    }
                    catch (Exception ex)
                    {
                        return "Error: " + ex.Message;
                    }
                }
            }
        }

        public string UpdtMstBin(string bin_name, string bin_length, string bin_width, string bin_height)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"UPDATE mst_sbin SET length_mm = @bin_length, width_mm = @bin_width, height_mm = @bin_height WHERE sbin = @bin_name", conn))
                {
                    cmd.Parameters.AddWithValue("@bin_name", bin_name);
                    cmd.Parameters.AddWithValue("@bin_length", bin_length);
                    cmd.Parameters.AddWithValue("@bin_width", bin_width);
                    cmd.Parameters.AddWithValue("@bin_height", bin_height);

                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0 ? "OK" : "No rows affected.";
                    }
                    catch (Exception ex)
                    {
                        return "Error: " + ex.Message;
                    }
                }
            }
        }
        public string OpenVariance(int id_det, string sesa_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("UPDATE_OPEN_VARIANCE", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id_det", id_det);
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                        cmd.ExecuteScalar();
                        return "OK";
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                return $"SQL Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                return $"General Error: {ex.Message}";
            }
        }
        public string DeleteTempConsol(string sesa_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM temp_consol WHERE sesa_id=@sesa_id", conn))
                    {
                        //cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                        cmd.ExecuteScalar();
                        return "OK";
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                return $"SQL Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                return $"General Error: {ex.Message}";
            }
        }
        public List<RequestListModel> GetConsolList()
        {
            List<RequestListModel> reqList = new List<RequestListModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM v_request WHERE status_request IN (4) ORDER BY record_date", conn);
                //cmd.CommandType = CommandType.StoredProcedure;
                //cmd.Parameters.AddWithValue("@id_request", id_request);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    reqList.Add(new RequestListModel
                    {
                        id_request = Convert.ToInt32(reader["id_request"]),
                        request_no = reader["request_no"].ToString(),
                        status_desc = reader["status_desc"].ToString(),
                        requested_by_name = reader["requested_by_name"].ToString(),
                        remark = reader["remark"].ToString(),
                        record_date = Convert.ToDateTime(reader["record_date"])
                    });
                }
            }

            return reqList;
        }
        public string StartConsol(int id_request, string sesa_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("START_CONSOL", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id_request", id_request);
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            return result.ToString() ?? "";
                        }
                        else
                        {
                            return "ERROR;No result returned from stored procedure.";
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                return $"SQL Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                return $"General Error: {ex.Message}";
            }
        }
        public List<TempBoxModel> GetTempConsolBox(int id_request, string sesa_id)
        {
            List<TempBoxModel> tempList = new List<TempBoxModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("GET_TEMP_CONSOL_BOX", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_request", id_request);
                cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    tempList.Add(new TempBoxModel
                    {
                        id_temp_box = Convert.ToInt32(reader["id_temp_box"]),
                        box_id = Convert.ToString(reader["box_id"]),
                        box_id_consol = Convert.ToString(reader["box_id_consol"])
                    });
                }
            }

            return tempList;
        }
        public string CheckBoxConsol(int id_request, string box_id, string sesa_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("CHECK_BOX_CONSOL", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id_request", id_request);
                        cmd.Parameters.AddWithValue("@box_id", box_id);
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            return result.ToString() ?? "";
                        }
                        else
                        {
                            return "ERROR;No result returned from database.";
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                return $"SQL Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                return $"General Error: {ex.Message}";
            }
        }
        public List<PalletModel> GetPalletID(int id_request)
        {
            List<PalletModel> palletList = new List<PalletModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT id_pallet, pallet_no FROM tbl_pallet_header WHERE id_request=@id_request ORDER BY id_pallet", conn);
                //cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_request", id_request);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    palletList.Add(new PalletModel
                    {
                        id_pallet = Convert.ToInt32(reader["id_pallet"]),
                        pallet_no = Convert.ToString(reader["pallet_no"])
                    });
                }
            }

            return palletList;
        }
        public string CreatePallet(int id_request, int id_pallet, string sesa_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("CREATE_PALLET", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id_request", id_request);
                        cmd.Parameters.AddWithValue("@id_pallet", id_pallet);
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            return result.ToString() ?? "";
                        }
                        else
                        {
                            return "ERROR;No result returned from database.";
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                return $"SQL Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                return $"General Error: {ex.Message}";
            }
        }
        public List<BoxModel> GetBoxList(string orderId)
        {
            List<BoxModel> boxs = new List<BoxModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("GET_BOX_LIST", conn);
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@input", orderId);
                    using SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        boxs.Add(new BoxModel
                        {
                            BoxId = reader["PKG_ID2"].ToString(),
                            PartNo = reader["Part_NO"].ToString(),
                            Qty = reader["Qty"].ToString(),
                            Sbin = reader["Storage_Bin"].ToString(),
                            Unit = reader["Unit"].ToString(),
                            Pstats = reader["PickingStatus"].ToString()
                        });
                    }
                }                        
            }
            return boxs;
        }

        public string DeleteOrderList(string orderId)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"DELETE FROM tbl_order WHERE id_order = @orderId", conn))
                {
                    // Use parameterized query to prevent SQL injection
                    cmd.Parameters.AddWithValue("@orderId", orderId);

                    // Execute the command
                    int rowsAffected = cmd.ExecuteNonQuery();

                    // Check if any rows were affected
                    if (rowsAffected > 0)
                    {
                        return "OK";
                    }
                    else
                    {
                        return "NOK";
                    }
                }
            }
        }
        public List<RequestListModel> GetDeptList()
        {
            List<RequestListModel> depts = new List<RequestListModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT DISTINCT department FROM mst_users WHERE department is not NULL and department != 'NULL' ", conn);
                //cmd.Parameters.AddWithValue("@box_id", box_id);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    depts.Add(new RequestListModel
                    {
                        department = reader["department"].ToString(),                        
                    });
                }
            }

            return depts;
        }

        public List<RequestListModel> GetReqLists()
        {
            List<RequestListModel> reqs = new List<RequestListModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("GET_REQS_LIST", conn);
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        reqs.Add(new RequestListModel
                        {
                            id_request = reader["id_request"] != DBNull.Value ? (int?)Convert.ToInt32(reader["id_request"]) : null,
                            request_no = reader["request_no"].ToString(),
                            status_code = reader["status_request"].ToString(),
                            status_desc = reader["status_desc"].ToString(),
                            remark = reader["remark"].ToString(),
                            record_date = reader["record_date"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["record_date"]) : null,
                            requested_by = reader["requested_by"].ToString(),
                            requested_by_name = reader["name"].ToString(),
                            department = reader["department"].ToString(),
                        });
                    }
                }
            }
            return reqs;
        }
        public List<PickBoxModel> GetPickBox(int id_request)
        {
            List<PickBoxModel> dataList = new List<PickBoxModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM v_pick WHERE id_request=@id_request ORDER BY record_date", conn);
                //cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_request", id_request);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    dataList.Add(new PickBoxModel
                    {
                        id_pick = Convert.ToInt32(reader["id_pick"]),
                        box_id = reader["box_id"].ToString(),
                        partno = reader["partno"].ToString(),
                        sbin = reader["sbin"].ToString(),
                        qty = Convert.ToDecimal(reader["qty"]),
                        picked_by_name = reader["picked_by_name"].ToString(),
                        pallet_no = reader["pallet_no"].ToString(),
                        record_date = Convert.ToDateTime(reader["record_date"])
                    });
                }
            }

            return dataList;
        }
        public List<TempPalletModel> GetTempPallet(string sesa_id)
        {
            List<TempPalletModel> dataList = new List<TempPalletModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("GET_TEMP_PALLET_TRANSFER", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    dataList.Add(new TempPalletModel
                    {
                        id_temp = Convert.ToInt32(reader["id_temp"]),
                        pallet_no = reader["pallet_no"].ToString(),
                        request_no = reader["request_no"].ToString()
                    });
                }
            }

            return dataList;
        }
        public string InsertPalletTransfer(string pallet_no, string sesa_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("INSERT_PALLET_TRANSFER", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@pallet_no", pallet_no);
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            return result.ToString() ?? "";
                        }
                        else
                        {
                            return "ERROR;No result returned from database.";
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                return $"SQL Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                return $"General Error: {ex.Message}";
            }
        }
        public string RemovePalletTransfer(int id_temp)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"DELETE FROM temp_pallet_transfer WHERE id_temp = @id_temp", conn))
                {
                    // Use parameterized query to prevent SQL injection
                    cmd.Parameters.AddWithValue("@id_temp", id_temp);

                    // Execute the command
                    int rowsAffected = cmd.ExecuteNonQuery();

                    return "OK";
                }
            }
        }
        public string SubmitPalletTransfer(string sesa_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SUBMIT_PALLET_TRANSFER", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            return result.ToString() ?? "";
                        }
                        else
                        {
                            return "ERROR;No result returned from database.";
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                return $"SQL Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                return $"General Error: {ex.Message}";
            }
        }

        public string UpdateReceived(string id_pallet, string sesa_id)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("UPDATE_RECEIVE_PALLET", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_pallet", id_pallet);
                    cmd.Parameters.AddWithValue("@sesa_id", sesa_id);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return "OK";
                    }
                    else
                    {
                        return "NOK";
                    }
                }
            }
        }

        public string UpdateSupplied(string id_pallet, string sesa_id, string supplied_name)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("UPDATE_SUPPLY_PALLET", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_pallet", id_pallet);
                    cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                    cmd.Parameters.AddWithValue("@comment", supplied_name);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return "OK";
                    }
                    else
                    {
                        return "NOK";
                    }
                }
            }
        }

        public List<PickBoxModel> GetPalletDetail(string plt_id)
        {
            List<PickBoxModel> reqs = new List<PickBoxModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("GET_PALLET_DETAILS", conn);
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@plt_id", plt_id);
                    using SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        reqs.Add(new PickBoxModel
                        {
                            id_pick = reader["id_pick"] != DBNull.Value ? (int?)Convert.ToInt32(reader["id_request"]) : null,
                            id_request = reader["id_request"] != DBNull.Value ? (int?)Convert.ToInt32(reader["id_request"]) : null,
                            id_det = reader["id_det"] != DBNull.Value ? (int?)Convert.ToInt32(reader["id_request"]) : null,
                            box_id = reader["box_id"].ToString(),
                            partno = reader["partno"].ToString(),
                            qty = reader["qty"] != DBNull.Value ? (int?)Convert.ToInt32(reader["id_request"]) : null,
                            sbin = reader["sbin"].ToString(),
                            picked_by = reader["picked_by"].ToString(),
                            record_date = reader["record_date"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["record_date"]) : null,
                        });
                    }
                }
            }
            return reqs;
        }

        public string close_non_conf(string id_non_conf, string sesa_id, string cls_comment)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("CLOSE_NON_CONF", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_non_conf", id_non_conf);
                    cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                    cmd.Parameters.AddWithValue("@comment", cls_comment);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return "OK";
                    }
                    else
                    {
                        return "NOK";
                    }
                }
            }
        }
    }
}
