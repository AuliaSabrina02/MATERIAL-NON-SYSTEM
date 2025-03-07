using SEMB_ERP.Models;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Drawing2D;

namespace SEMB_ERP.Function
{
    public class DatabaseAccessLayer
    {
        public string ConnectionString = "Data Source=10.155.152.114;Initial Catalog=SEMB_ERP;Persist Security Info=True;User ID=dt;Password=Dt@123;MultipleActiveResultSets=true";

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
                            row.qty = Convert.ToDouble(reader["qty"]);
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
                            row.qty = Convert.ToDouble(reader["qty"]);
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


    }
}
