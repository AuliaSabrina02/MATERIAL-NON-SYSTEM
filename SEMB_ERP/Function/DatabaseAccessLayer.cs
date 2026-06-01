using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
using SEMB_ERP.Models;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace SEMB_ERP.Function
{
    public class DatabaseAccessLayer
    {
        public string ConnectionString = @"Server=localhost\SQLEXPRESS;Database=SEMB_ERP_QAS;Trusted_Connection=True;TrustServerCertificate=True;";
        public string ConnectionStringBLP = @"Server=localhost\SQLEXPRESS;Database=DBBLP;Trusted_Connection=True;TrustServerCertificate=True;";


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
                            row.priority_request = reader["priority_request"].ToString();
                            row.priority_request_msg = reader["priority_request_msg"].ToString();
                            row.length_mm = reader["length_mm"].ToString();
                            row.length_mm_msg = reader["length_mm_msg"].ToString();
                            row.width_mm = reader["width_mm"].ToString();
                            row.width_mm_msg = reader["width_mm_msg"].ToString();
                            row.height_mm = reader["height_mm"].ToString();
                            row.height_mm_msg = reader["height_mm_msg"].ToString();
                            row.remark = reader["remark"].ToString();
                            row.gatepass = reader["gatepass"].ToString();
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

        public string SaveOrderTemplate(OrderTemplateModel template)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    // Urutan kolom di bawah ini WAJIB sama dengan mapping di Bulk Insert
                    string query = @"
                INSERT INTO tbl_order_template (
                    material_type,       
                    partno,              
                    po_no,               
                    qty,                 
                    uom,                 
                    revision,            
                    project_name,        
                    storage_requirement, 
                    supplier_name,       
                    order_type,         
                    unit_price,         
                    priority_request,    
                    length_mm,          
                    width_mm,            
                    height_mm,           
                    remark,              
                    gatepass,            
                    status_code, 
                    created_date, 
                    created_by
                ) VALUES (
                    @material_type, @partno, @po_no, @qty, @uom, @revision,
                    @project_name, @storage_requirement, @supplier_name, @order_type,
                    @unit_price, @priority_request, @length_mm, @width_mm, @height_mm,
                    @remark, @gatepass, 'TEMPLATE', GETDATE(), @created_by
                )";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Mapping Parameter (Pastikan model OrderTemplateModel punya properti ini)
                        cmd.Parameters.AddWithValue("@material_type", template.material_type ?? "");
                        cmd.Parameters.AddWithValue("@partno", template.partno ?? "");
                        cmd.Parameters.AddWithValue("@po_no", template.po_no ?? "");
                        cmd.Parameters.AddWithValue("@qty", template.qty ?? "0");
                        cmd.Parameters.AddWithValue("@uom", template.uom ?? "");
                        cmd.Parameters.AddWithValue("@revision", template.revision ?? "");
                        cmd.Parameters.AddWithValue("@project_name", template.project_name ?? "");
                        cmd.Parameters.AddWithValue("@storage_requirement", template.storage_requirement ?? "");
                        cmd.Parameters.AddWithValue("@supplier_name", template.supplier_name ?? "");
                        cmd.Parameters.AddWithValue("@order_type", template.order_type ?? "");
                        cmd.Parameters.AddWithValue("@unit_price", template.unit_price ?? "0");

                        // Ini bagian krusial yang menyesuaikan Bulk Insert:
                        cmd.Parameters.AddWithValue("@priority_request", template.priority_request ?? ""); // Kolom 12
                        cmd.Parameters.AddWithValue("@length_mm", template.length_mm ?? "0");              // Kolom 13
                        cmd.Parameters.AddWithValue("@width_mm", template.width_mm ?? "0");                // Kolom 14
                        cmd.Parameters.AddWithValue("@height_mm", template.height_mm ?? "0");              // Kolom 15
                        cmd.Parameters.AddWithValue("@remark", template.remark ?? "");                    // Kolom 16
                        cmd.Parameters.AddWithValue("@gatepass", template.gatepass ?? "");                // Kolom 17

                        cmd.Parameters.AddWithValue("@created_by", template.created_by ?? "");

                        cmd.ExecuteNonQuery();
                    }
                }
                return "success;Template saved successfully!";
            }
            catch (Exception ex)
            {
                return "error;" + ex.Message;
            }
        }

        public List<OrderTemplateModel> GetOrderTemplates(string sesa_id)
        {
            List<OrderTemplateModel> templates = new List<OrderTemplateModel>();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = @"
                SELECT * FROM tbl_order_template 
                WHERE created_by = @sesa_id AND (is_deleted = 0 OR is_deleted IS NULL)
                ORDER BY created_date DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                templates.Add(new OrderTemplateModel
                                {
                                    id_template = Convert.ToInt32(reader["id_template"]),
                                    material_type = reader["material_type"].ToString(),
                                    partno = reader["partno"].ToString(),
                                    po_no = reader["po_no"].ToString(),
                                    qty = reader["qty"].ToString(),
                                    uom = reader["uom"].ToString(),
                                    revision = reader["revision"].ToString(),
                                    project_name = reader["project_name"].ToString(),
                                    storage_requirement = reader["storage_requirement"].ToString(),
                                    supplier_name = reader["supplier_name"].ToString(),
                                    order_type = reader["order_type"].ToString(),
                                    unit_price = reader["unit_price"].ToString(),
                                    priority_request = reader["priority_request"]?.ToString(),
                                    length_mm = reader["length_mm"].ToString(),
                                    width_mm = reader["width_mm"].ToString(),
                                    height_mm = reader["height_mm"].ToString(),
                                    remark = reader["remark"]?.ToString(),
                                    gatepass = reader["gatepass"]?.ToString(),
                                    pic = reader["pic"]?.ToString(),
                                    created_date = Convert.ToDateTime(reader["created_date"]),
                                    created_by = reader["created_by"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error
            }

            return templates;
        }

        public OrderTemplateModel GetOrderTemplateDetail(int id_template, string sesa_id)
        {
            OrderTemplateModel template = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = @"
                SELECT * FROM tbl_order_template 
                WHERE id_template = @id_template AND created_by = @sesa_id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id_template", id_template);
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                template = new OrderTemplateModel
                                {
                                    id_template = Convert.ToInt32(reader["id_template"]),
                                    material_type = reader["material_type"].ToString(),
                                    partno = reader["partno"].ToString(),
                                    po_no = reader["po_no"].ToString(),
                                    qty = reader["qty"].ToString(),
                                    uom = reader["uom"].ToString(),
                                    revision = reader["revision"].ToString(),
                                    project_name = reader["project_name"].ToString(),
                                    storage_requirement = reader["storage_requirement"].ToString(),
                                    supplier_name = reader["supplier_name"].ToString(),
                                    order_type = reader["order_type"].ToString(),
                                    unit_price = reader["unit_price"].ToString(),
                                    priority_request = reader["priority_request"]?.ToString(),
                                    length_mm = reader["length_mm"].ToString(),
                                    width_mm = reader["width_mm"].ToString(),
                                    height_mm = reader["height_mm"].ToString(),
                                    remark = reader["remark"]?.ToString(),
                                    gatepass = reader["gatepass"]?.ToString(),
                                    pic = reader["pic"]?.ToString(),
                                    created_date = Convert.ToDateTime(reader["created_date"]),
                                    created_by = reader["created_by"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error
            }

            return template;
        }


        public OrderTemplateModel GetOrderDetail(int id_order)
        {
            OrderTemplateModel template = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = @"
                SELECT * FROM tbl_order 
                WHERE id_order = @id_order";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id_order", id_order);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                template = new OrderTemplateModel
                                {
                                    id_template = Convert.ToInt32(reader["id_order"]),
                                    material_type = reader["material_type"]?.ToString(),
                                    partno = reader["partno"]?.ToString(),
                                    po_no = reader["po_no"]?.ToString(),
                                    qty = reader["qty"]?.ToString(),
                                    uom = reader["uom"]?.ToString(),
                                    revision = reader["revision"]?.ToString(),
                                    project_name = reader["project_name"]?.ToString(),
                                    storage_requirement = reader["storage_requirement"]?.ToString(),
                                    supplier_name = reader["supplier_name"]?.ToString(),
                                    order_type = reader["order_type"]?.ToString(),
                                    unit_price = reader["unit_price"]?.ToString(),
                                    priority_request = reader["priority_request"]?.ToString(),
                                    length_mm = reader["length_mm"]?.ToString(),
                                    width_mm = reader["width_mm"]?.ToString(),
                                    height_mm = reader["height_mm"]?.ToString(),
                                    remark = reader["remark"]?.ToString(),
                                    gatepass = reader["gatepass"]?.ToString(),
                                    pic = reader["pic"]?.ToString(),
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error
            }
            return template;
        }

        public string DeleteOrderTemplate(int id_template, string sesa_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = @"
                UPDATE tbl_order_template 
                SET is_deleted = 1 
                WHERE id_template = @id_template AND created_by = @sesa_id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id_template", id_template);
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                        cmd.ExecuteNonQuery();
                    }
                }
                return "success";
            }
            catch (Exception ex)
            {
                return "error;" + ex.Message;
            }
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


        public List<DiscussionModel> GetDiscussions(int id_order, string sesa_id)
        {
            List<DiscussionModel> discussions = new List<DiscussionModel>();

            try
            {
                string query = @"
            -- Cek apakah user berhak akses order ini
            IF NOT EXISTS (
                SELECT 1 FROM tbl_order o
                WHERE o.id_order = @id_order 
                AND (
                    o.pic = @sesa_id  -- User adalah PIC/Requestor
                    OR EXISTS (
                        SELECT 1 FROM mst_users_role ur
                        WHERE ur.sesa_id = @sesa_id
                        AND ur.id_role IN (2, 3, 99)  -- Receiver, Plant Receiver, atau Admin
                    )
                )
            )
            BEGIN
                RAISERROR('Access Denied: You do not have permission to view this discussion', 16, 1);
                RETURN;
            END
            
            -- Ambil discussions
            SELECT 
                d.id,
                d.id_order,
                d.sesa_id AS SesaId,
                u.name AS UserName,
                d.message AS Message,
                d.parent_id AS ParentId,
                d.created_date AS CreatedDate
            FROM tbl_discussion d
            LEFT JOIN mst_users u ON d.sesa_id = u.sesa_id
            WHERE d.id_order = @id_order
            ORDER BY d.created_date ASC";

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id_order", id_order);
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);

                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                discussions.Add(new DiscussionModel
                                {
                                    Id = reader.GetInt32(0),
                                    IdOrder = reader.GetInt32(1),
                                    SesaId = reader.GetString(2),
                                    UserName = reader.IsDBNull(3) ? reader.GetString(2) : reader.GetString(3),
                                    Message = reader.GetString(4),
                                    ParentId = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5),
                                    CreatedDate = reader.GetDateTime(6)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting discussions: " + ex.Message);
            }

            return discussions;
        }

        public void MarkDiscussionAsRead(int id_order, string sesa_id)
        {
            try
            {
                string query = @"
            MERGE tbl_discussion_read AS target
            USING (SELECT @sesa_id AS sesa_id, GETDATE() AS last_read_date) AS source
            ON target.sesa_id = source.sesa_id AND target.id_order = 0
            WHEN MATCHED THEN
                UPDATE SET last_read_date = source.last_read_date
            WHEN NOT MATCHED THEN
                INSERT (id_order, sesa_id, last_read_date)
                VALUES (0, source.sesa_id, source.last_read_date);";

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking discussion as read: {ex.Message}");
            }
        }

        public string InsertDiscussion(int id_order, string sesa_id, string message, int? parent_id)
        {
            try
            {
                string query = @"
                    INSERT INTO tbl_discussion (id_order, sesa_id, message, parent_id, created_date)
                    VALUES (@id_order, @sesa_id, @message, @parent_id, GETDATE())";

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id_order", id_order);
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                        cmd.Parameters.AddWithValue("@message", message);
                        cmd.Parameters.AddWithValue("@parent_id", (object)parent_id ?? DBNull.Value);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                return "OK";
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        public string CreateReturn(MaterialReturnModel model, IFormFile image_support_file, string sesa_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    string fileName = null;
                    if (image_support_file != null && image_support_file.Length > 0)
                    {
                        // FIX: path harus "uploads/returns" agar sesuai dengan view_image() di JS
                        string uploadFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "returns");
                        if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);
                        fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(image_support_file.FileName);
                        string filePath = System.IO.Path.Combine(uploadFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            image_support_file.CopyTo(stream);
                        }
                    }

                    string query = @"
                INSERT INTO material_return 
                (request_no, project_name, po_no, partno, qty_return, uom, condition, 
                 reason_return, storage_requirement, image_support, pic, return_date, status)
                VALUES 
                (@request_no, @project_name, @po_no, @partno, @qty_return, @uom, @condition, 
                 @reason_return, @storage_requirement, @image_support, @pic, GETDATE(), 1)";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@request_no", model.request_no ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@project_name", model.project_name ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@po_no", model.po_no ?? "");
                    cmd.Parameters.AddWithValue("@partno", model.partno ?? "");
                    cmd.Parameters.AddWithValue("@qty_return", model.qty_return);
                    cmd.Parameters.AddWithValue("@uom", model.uom ?? "");
                    cmd.Parameters.AddWithValue("@condition", model.condition ?? "");
                    cmd.Parameters.AddWithValue("@reason_return", model.reason_return ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@storage_requirement", model.storage_requirement ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@image_support", fileName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@pic", sesa_id ?? "");
                    cmd.ExecuteNonQuery();
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                return "error;" + ex.Message;
            }
        }
        public dynamic GetReturnList(IFormCollection form)
        {
            try
            {
                int.TryParse(form["draw"].ToString(), out int draw);
                int.TryParse(form["start"].ToString(), out int start);
                if (!int.TryParse(form["length"].ToString(), out int length))
                {
                    length = 10;
                }

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    string countQuery = "SELECT COUNT(*) FROM material_return";
                    SqlCommand countCmd = new SqlCommand(countQuery, conn);
                    int recordsTotal = (int)countCmd.ExecuteScalar();

                    string whereClause = " WHERE 1=1 ";
                    List<SqlParameter> parameters = new List<SqlParameter>();

                    // Urutan sesuai kolom view:
                    // 0=Status, 1=Request No, 2=PO Number, 3=Part Number, 4=Qty,
                    // 5=UOM, 6=Condition, 7=Remark, 8=PIC, 9=Return Date, 10=Image(skip), 11=Action(skip)
                    for (int i = 0; i <= 9; i++)
                    {
                        string colValue = form[$"columns[{i}][search][value]"];
                        if (!string.IsNullOrEmpty(colValue))
                        {
                            switch (i)
                            {
                                case 0: // Status
                                    whereClause += " AND CASE WHEN status = 1 THEN 'Sent' WHEN status = 2 THEN 'Approved' WHEN status = 3 THEN 'In Transit' WHEN status = 4 THEN 'Returned' ELSE 'Unknown' END LIKE @col0 ";
                                    parameters.Add(new SqlParameter("@col0", "%" + colValue + "%"));
                                    break;
                                case 1: // Request No
                                    whereClause += " AND request_no LIKE @col1 ";
                                    parameters.Add(new SqlParameter("@col1", "%" + colValue + "%"));
                                    break;
                                case 2: // PO Number
                                    whereClause += " AND po_no LIKE @col2 ";
                                    parameters.Add(new SqlParameter("@col2", "%" + colValue + "%"));
                                    break;
                                case 3: // Part Number
                                    whereClause += " AND partno LIKE @col3 ";
                                    parameters.Add(new SqlParameter("@col3", "%" + colValue + "%"));
                                    break;
                                case 4: // Qty
                                    whereClause += " AND CAST(qty_return AS VARCHAR) LIKE @col4 ";
                                    parameters.Add(new SqlParameter("@col4", "%" + colValue + "%"));
                                    break;
                                case 5: // UOM
                                    whereClause += " AND uom LIKE @col5 ";
                                    parameters.Add(new SqlParameter("@col5", "%" + colValue + "%"));
                                    break;
                                case 6: // Condition
                                    whereClause += " AND condition LIKE @col6 ";
                                    parameters.Add(new SqlParameter("@col6", "%" + colValue + "%"));
                                    break;
                                case 7: // Remark (reason_return)
                                    whereClause += " AND reason_return LIKE @col7 ";
                                    parameters.Add(new SqlParameter("@col7", "%" + colValue + "%"));
                                    break;
                                case 8: // PIC
                                    whereClause += " AND (SELECT name FROM mst_users WHERE sesa_id = material_return.pic) LIKE @col8 ";
                                    parameters.Add(new SqlParameter("@col8", "%" + colValue + "%"));
                                    break;
                                case 9: // Return Date
                                    whereClause += " AND CONVERT(VARCHAR, return_date, 23) LIKE @col9 ";
                                    parameters.Add(new SqlParameter("@col9", "%" + colValue + "%"));
                                    break;
                                    // case 10 = Image → skip
                                    // case 11 = Action → skip
                            }
                        }
                    }

                    string filteredCountQuery = "SELECT COUNT(*) FROM material_return " + whereClause;
                    SqlCommand filteredCountCmd = new SqlCommand(filteredCountQuery, conn);
                    filteredCountCmd.Parameters.AddRange(parameters.Select(p => (SqlParameter)((ICloneable)p).Clone()).ToArray());
                    int recordsFiltered = (int)filteredCountCmd.ExecuteScalar();

                    string query = $@"
SELECT 
    id_return, request_no, project_name, po_no, partno, 
    qty_return, uom, condition, image_support, pic,
    ISNULL((SELECT name FROM mst_users WHERE sesa_id = material_return.pic), '') as name,
    CONVERT(VARCHAR, return_date, 23) as return_date,
    status,
    CASE 
        WHEN status = 1 THEN 'Sent'
        WHEN status = 2 THEN 'Approved'
        WHEN status = 3 THEN 'In Transit'
        WHEN status = 4 THEN 'Returned'
        ELSE 'Unknown'
    END as status_desc,
    reason_return, storage_requirement
FROM material_return
{whereClause}
ORDER BY 
    CASE status 
        WHEN 1 THEN 0
        WHEN 2 THEN 1
        WHEN 3 THEN 2
        WHEN 4 THEN 3
        ELSE 4 
    END ASC,
    return_date DESC
OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddRange(parameters.ToArray());
                    cmd.Parameters.AddWithValue("@offset", start);
                    cmd.Parameters.AddWithValue("@pageSize", length);

                    SqlDataReader reader = cmd.ExecuteReader();
                    List<MaterialReturnModel> data = new List<MaterialReturnModel>();

                    while (reader.Read())
                    {
                        data.Add(new MaterialReturnModel
                        {
                            id_return = reader.GetInt32(0),
                            request_no = reader.IsDBNull(1) ? "" : reader.GetString(1),
                            project_name = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            po_no = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            partno = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            qty_return = reader.GetDecimal(5),
                            uom = reader.IsDBNull(6) ? "" : reader.GetString(6),
                            condition = reader.IsDBNull(7) ? "" : reader.GetString(7),
                            image_support = reader.IsDBNull(8) ? "" : reader.GetString(8),
                            pic = reader.IsDBNull(9) ? "" : reader.GetString(9),
                            name = reader.IsDBNull(10) ? "" : reader.GetString(10),
                            return_date = reader.IsDBNull(11) ? null : (DateTime?)DateTime.Parse(reader.GetString(11)),
                            status = reader.GetInt32(12),
                            status_desc = reader.IsDBNull(13) ? "" : reader.GetString(13),
                            reason_return = reader.IsDBNull(14) ? "" : reader.GetString(14),
                            storage_requirement = reader.IsDBNull(15) ? "" : reader.GetString(15)
                        });
                    }

                    return new { draw, recordsTotal, recordsFiltered, data };
                }
            }
            catch (Exception ex)
            {
                return new { draw = 0, recordsTotal = 0, recordsFiltered = 0, data = new List<MaterialReturnModel>(), error = ex.Message };
            }
        }


        public string DeleteMaterialReturn(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    // Opsional: Cek status dulu sebelum hapus untuk keamanan extra
                    // "Hanya boleh hapus jika status = 1 (Sent)"
                    string checkStatusQuery = "SELECT status FROM material_return WHERE id_return = @id";
                    SqlCommand checkCmd = new SqlCommand(checkStatusQuery, conn);
                    checkCmd.Parameters.AddWithValue("@id", id);
                    object currentStatus = checkCmd.ExecuteScalar();

                    if (currentStatus == null) return "Data not found.";
                    if (Convert.ToInt32(currentStatus) != 1) return "Cannot delete. Data is already processed (Approved/In Transit).";

                    // Eksekusi Delete
                    string query = "DELETE FROM material_return WHERE id_return = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0 ? "OK" : "No data deleted.";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public string UpdateReturn(MaterialReturnModel model, IFormFile image_support_file, string sesa_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    string fileName = null;
                    if (image_support_file != null && image_support_file.Length > 0)
                    {
                        // Menggunakan System.IO secara eksplisit untuk menghindari error 'Path'
                        string uploadFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "returns");
                        if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                        fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(image_support_file.FileName);
                        string filePath = System.IO.Path.Combine(uploadFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            image_support_file.CopyTo(stream);
                        }
                    }

                    // Query diperbarui: pic_updater dihapus agar tidak error
                    string query = @"
          UPDATE material_return 
          SET request_no = @request_no,
              project_name = @project_name,
              po_no = @po_no, 
              partno = @partno, 
              qty_return = @qty_return, 
              uom = @uom, 
              condition = @condition, 
              reason_return = @reason_return,
              storage_requirement = @storage_requirement,
              update_date = GETDATE() " +
                            (fileName != null ? ", image_support = @image_support " : "") +
                        " WHERE id_return = @id_return";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id_return", model.id_return);
                    cmd.Parameters.AddWithValue("@request_no", model.request_no ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@project_name", model.project_name ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@po_no", model.po_no ?? "");
                    cmd.Parameters.AddWithValue("@partno", model.partno ?? "");
                    cmd.Parameters.AddWithValue("@qty_return", model.qty_return);
                    cmd.Parameters.AddWithValue("@uom", model.uom ?? "");
                    cmd.Parameters.AddWithValue("@condition", model.condition ?? "");
                    cmd.Parameters.AddWithValue("@reason_return", model.reason_return ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@storage_requirement", model.storage_requirement ?? (object)DBNull.Value);

                    if (fileName != null) cmd.Parameters.AddWithValue("@image_support", fileName);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0 ? "OK" : "No changes made.";
                }
            }
            catch (Exception ex)
            {
                return "error;" + ex.Message;
            }
        }


        public string UpdateReturnStatus(int id_return, int status)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "UPDATE material_return SET status = @status, update_date = GETDATE() WHERE id_return = @id_return";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@status", status);
                    cmd.Parameters.AddWithValue("@id_return", id_return);
                    cmd.ExecuteNonQuery();
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }



        }

        public (List<PalletProblem> data, int totalRecords) GetPalletProblems(
    int start, int length,
    string searchPalletNo, string searchDate, string searchIssue,
    string searchCreatedBy, string searchUpdatedBy,
    string sortColumn, string sortDirection)
        {
            var palletProblems = new List<PalletProblem>();
            int totalRecords = 0;
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                string whereClause = @"WHERE 
      (@palletNo IS NULL OR pallet_no LIKE '%' + @palletNo + '%') AND
      (@date IS NULL OR CONVERT(VARCHAR, date, 103) LIKE '%' + @date + '%') AND
      (@issue IS NULL OR issue LIKE '%' + @issue + '%') AND
      (@createdBy IS NULL OR created_by LIKE '%' + @createdBy + '%') AND
      (@updatedBy IS NULL OR updated_by LIKE '%' + @updatedBy + '%')";

                string countQuery = $"SELECT COUNT(*) FROM pallet_problems {whereClause}";

                using (SqlCommand countCmd = new SqlCommand(countQuery, conn))
                {
                    countCmd.Parameters.AddWithValue("@palletNo", string.IsNullOrEmpty(searchPalletNo) ? DBNull.Value : (object)searchPalletNo);
                    countCmd.Parameters.AddWithValue("@date", string.IsNullOrEmpty(searchDate) ? DBNull.Value : (object)searchDate);
                    countCmd.Parameters.AddWithValue("@issue", string.IsNullOrEmpty(searchIssue) ? DBNull.Value : (object)searchIssue);
                    countCmd.Parameters.AddWithValue("@createdBy", string.IsNullOrEmpty(searchCreatedBy) ? DBNull.Value : (object)searchCreatedBy);
                    countCmd.Parameters.AddWithValue("@updatedBy", string.IsNullOrEmpty(searchUpdatedBy) ? DBNull.Value : (object)searchUpdatedBy);
                    totalRecords = (int)countCmd.ExecuteScalar();
                }

                string orderBy = "CASE status WHEN 'Active' THEN 0 ELSE 1 END ASC, created_date DESC";
                switch (sortColumn)
                {
                    case "1": orderBy = $"CASE status WHEN 'Active' THEN 0 ELSE 1 END ASC, pallet_no {sortDirection}"; break;
                    case "2": orderBy = $"CASE status WHEN 'Active' THEN 0 ELSE 1 END ASC, date {sortDirection}"; break;
                    case "3": orderBy = $"CASE status WHEN 'Active' THEN 0 ELSE 1 END ASC, issue {sortDirection}"; break;
                }

                string dataQuery = $@"
      SELECT * FROM (
          SELECT ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, *
          FROM pallet_problems
          {whereClause}
      ) AS RowConstrainedResult
      WHERE RowNum > @start AND RowNum <= @start + @length
      ORDER BY RowNum";

                using (SqlCommand dataCmd = new SqlCommand(dataQuery, conn))
                {
                    dataCmd.Parameters.AddWithValue("@palletNo", string.IsNullOrEmpty(searchPalletNo) ? DBNull.Value : (object)searchPalletNo);
                    dataCmd.Parameters.AddWithValue("@date", string.IsNullOrEmpty(searchDate) ? DBNull.Value : (object)searchDate);
                    dataCmd.Parameters.AddWithValue("@issue", string.IsNullOrEmpty(searchIssue) ? DBNull.Value : (object)searchIssue);
                    dataCmd.Parameters.AddWithValue("@createdBy", string.IsNullOrEmpty(searchCreatedBy) ? DBNull.Value : (object)searchCreatedBy);
                    dataCmd.Parameters.AddWithValue("@updatedBy", string.IsNullOrEmpty(searchUpdatedBy) ? DBNull.Value : (object)searchUpdatedBy);
                    dataCmd.Parameters.AddWithValue("@start", start);
                    dataCmd.Parameters.AddWithValue("@length", length);

                    using (SqlDataReader reader = dataCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            palletProblems.Add(new PalletProblem
                            {
                                id = reader.GetInt32(reader.GetOrdinal("id")),
                                pallet_no = reader.GetString(reader.GetOrdinal("pallet_no")),
                                req_no = reader.GetString(reader.GetOrdinal("req_no")),
                                date = reader.GetDateTime(reader.GetOrdinal("date")),
                                issue = reader.GetString(reader.GetOrdinal("issue")),
                                created_by = reader.IsDBNull(reader.GetOrdinal("created_by")) ? null : reader.GetString(reader.GetOrdinal("created_by")),
                                created_date = reader.GetDateTime(reader.GetOrdinal("created_date")),
                                updated_by = reader.IsDBNull(reader.GetOrdinal("updated_by")) ? null : reader.GetString(reader.GetOrdinal("updated_by")),
                                updated_date = reader.IsDBNull(reader.GetOrdinal("updated_date")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("updated_date")),
                                status = reader.GetString(reader.GetOrdinal("status"))
                            });
                        }
                    }
                }
            }
            return (palletProblems, totalRecords);
        }

        public bool CheckPalletNoExists(string palletNo, int? excludeId = null)
         {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT COUNT(*) FROM pallet_problems WHERE pallet_no = @pallet_no";

                 if (excludeId.HasValue)
                {
                    query += " AND id != @id";
                 }

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@pallet_no", palletNo);

                if (excludeId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@id", excludeId.Value);
                }

                conn.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        public bool AddPalletProblem(PalletProblem palletProblem)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"
                    INSERT INTO pallet_problems 
                    (pallet_no, req_no, date, issue, created_by, created_date, status)
                    VALUES 
                    (@pallet_no, @req_no, @date, @issue, @created_by, @created_date, @status)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@pallet_no", palletProblem.pallet_no);
                cmd.Parameters.AddWithValue("@req_no", palletProblem.req_no);
                cmd.Parameters.AddWithValue("@date", palletProblem.date);
                cmd.Parameters.AddWithValue("@issue", palletProblem.issue);
                cmd.Parameters.AddWithValue("@created_by", palletProblem.created_by ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@created_date", palletProblem.created_date);
                cmd.Parameters.AddWithValue("@status", palletProblem.status);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }


        public bool UpdatePalletProblem(PalletProblem palletProblem)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"
                    UPDATE pallet_problems 
                    SET pallet_no = @pallet_no,
                        req_no = @req_no,
                        date = @date,
                        issue = @issue,
                        updated_by = @updated_by,
                        updated_date = @updated_date
                    WHERE id = @id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", palletProblem.id);
                cmd.Parameters.AddWithValue("@pallet_no", palletProblem.pallet_no);
                cmd.Parameters.AddWithValue("@req_no", palletProblem.req_no);
                cmd.Parameters.AddWithValue("@date", palletProblem.date);
                cmd.Parameters.AddWithValue("@issue", palletProblem.issue);
                cmd.Parameters.AddWithValue("@updated_by", palletProblem.updated_by ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@updated_date", palletProblem.updated_date ?? (object)DBNull.Value);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        public bool DeletePalletProblem(int id)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "DELETE FROM pallet_problems WHERE id = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        public PalletProblem GetPalletProblemById(int id)
        {
            PalletProblem palletProblem = null;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT * FROM pallet_problems WHERE id = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        palletProblem = new PalletProblem
                        {
                            id = reader.GetInt32(reader.GetOrdinal("id")),
                            pallet_no = reader.GetString(reader.GetOrdinal("pallet_no")),
                            req_no = reader.GetString(reader.GetOrdinal("req_no")),
                            date = reader.GetDateTime(reader.GetOrdinal("date")),
                            issue = reader.GetString(reader.GetOrdinal("issue")),
                            created_by = reader.IsDBNull(reader.GetOrdinal("created_by")) ? null : reader.GetString(reader.GetOrdinal("created_by")),
                            created_date = reader.GetDateTime(reader.GetOrdinal("created_date")),
                            updated_by = reader.IsDBNull(reader.GetOrdinal("updated_by")) ? null : reader.GetString(reader.GetOrdinal("updated_by")),
                            updated_date = reader.IsDBNull(reader.GetOrdinal("updated_date")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("updated_date")),
                            status = reader.GetString(reader.GetOrdinal("status"))
                        };
                    }
                }
            }

            return palletProblem;
        }

        public (List<PalletProblem> data, int totalRecords) GetPalletList(
       int start, int length,
       string searchPalletNo, string searchDate, string searchIssue,
       string searchCreatedBy, string searchUpdatedBy,
       string sortColumn, string sortDirection)
        {
            var palletList = new List<PalletProblem>();
            int totalRecords = 0;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                string whereClause = @"WHERE 
            (@palletNo IS NULL OR pallet_no LIKE '%' + @palletNo + '%') AND
            (@date IS NULL OR CONVERT(VARCHAR, date, 103) LIKE '%' + @date + '%') AND
            (@issue IS NULL OR issue LIKE '%' + @issue + '%') AND
            (@createdBy IS NULL OR created_by LIKE '%' + @createdBy + '%') AND
            (@updatedBy IS NULL OR updated_by LIKE '%' + @updatedBy + '%')";

                string countQuery = $"SELECT COUNT(*) FROM pallet_problems {whereClause}";

                using (SqlCommand countCmd = new SqlCommand(countQuery, conn))
                {
                    countCmd.Parameters.AddWithValue("@palletNo", string.IsNullOrEmpty(searchPalletNo) ? DBNull.Value : (object)searchPalletNo);
                    countCmd.Parameters.AddWithValue("@date", string.IsNullOrEmpty(searchDate) ? DBNull.Value : (object)searchDate);
                    countCmd.Parameters.AddWithValue("@issue", string.IsNullOrEmpty(searchIssue) ? DBNull.Value : (object)searchIssue);
                    countCmd.Parameters.AddWithValue("@createdBy", string.IsNullOrEmpty(searchCreatedBy) ? DBNull.Value : (object)searchCreatedBy);
                    countCmd.Parameters.AddWithValue("@updatedBy", string.IsNullOrEmpty(searchUpdatedBy) ? DBNull.Value : (object)searchUpdatedBy);
                    totalRecords = (int)countCmd.ExecuteScalar();
                }

                string orderBy = "created_date DESC";
                switch (sortColumn)
                {
                    case "1": orderBy = $"pallet_no {sortDirection}"; break;
                    case "2": orderBy = $"date {sortDirection}"; break;
                    case "3": orderBy = $"issue {sortDirection}"; break;
                    case "4": orderBy = $"created_by {sortDirection}"; break;
                    case "5": orderBy = $"updated_by {sortDirection}"; break;
                }

                string dataQuery = $@"
            SELECT * FROM (
                SELECT ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum, *
                FROM pallet_problems
                {whereClause}
            ) AS RowConstrainedResult
            WHERE RowNum > @start AND RowNum <= @start + @length
            ORDER BY RowNum";

                using (SqlCommand dataCmd = new SqlCommand(dataQuery, conn))
                {
                    dataCmd.Parameters.AddWithValue("@palletNo", string.IsNullOrEmpty(searchPalletNo) ? DBNull.Value : (object)searchPalletNo);
                    dataCmd.Parameters.AddWithValue("@date", string.IsNullOrEmpty(searchDate) ? DBNull.Value : (object)searchDate);
                    dataCmd.Parameters.AddWithValue("@issue", string.IsNullOrEmpty(searchIssue) ? DBNull.Value : (object)searchIssue);
                    dataCmd.Parameters.AddWithValue("@createdBy", string.IsNullOrEmpty(searchCreatedBy) ? DBNull.Value : (object)searchCreatedBy);
                    dataCmd.Parameters.AddWithValue("@updatedBy", string.IsNullOrEmpty(searchUpdatedBy) ? DBNull.Value : (object)searchUpdatedBy);
                    dataCmd.Parameters.AddWithValue("@start", start);
                    dataCmd.Parameters.AddWithValue("@length", length);

                    using (SqlDataReader reader = dataCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            palletList.Add(new PalletProblem
                            {
                                id = reader.GetInt32(reader.GetOrdinal("id")),
                                pallet_no = reader.GetString(reader.GetOrdinal("pallet_no")),
                                req_no = reader.GetString(reader.GetOrdinal("req_no")),
                                date = reader.GetDateTime(reader.GetOrdinal("date")),
                                issue = reader.GetString(reader.GetOrdinal("issue")),
                                created_by = reader.IsDBNull(reader.GetOrdinal("created_by")) ? null : reader.GetString(reader.GetOrdinal("created_by")),
                                created_date = reader.GetDateTime(reader.GetOrdinal("created_date")),
                                updated_by = reader.IsDBNull(reader.GetOrdinal("updated_by")) ? null : reader.GetString(reader.GetOrdinal("updated_by")),
                                updated_date = reader.IsDBNull(reader.GetOrdinal("updated_date")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("updated_date")),
                                status = reader.GetString(reader.GetOrdinal("status"))
                            });
                        }
                    }
                }
            }
            return (palletList, totalRecords);
        }

        public string UpdatePalletProblemStatus(int id, string status, string updated_by)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = @"UPDATE pallet_problems 
                             SET status = @status, 
                                 updated_by = @updated_by, 
                                 updated_date = GETDATE() 
                             WHERE id = @id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@status", status);
                        cmd.Parameters.AddWithValue("@updated_by", updated_by);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                return "success";
            }
            catch (Exception ex)
            {
                return "error;" + ex.Message;
            }
        }
        public dynamic GetReservationList(IFormCollection form, string sesa_id)
        {
            try
            {
                int.TryParse(form["draw"].ToString(), out int draw);
                int.TryParse(form["start"].ToString(), out int start);
                if (!int.TryParse(form["length"].ToString(), out int length))
                {
                    length = 10;
                }
                string searchValue = form["search[value]"];

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    string countQuery = "SELECT COUNT(*) FROM material_reservation";
                    SqlCommand countCmd = new SqlCommand(countQuery, conn);
                    int recordsTotal = (int)countCmd.ExecuteScalar();

                    string whereClause = " WHERE 1=1 ";
                    List<SqlParameter> parameters = new List<SqlParameter>();

                    // ⭐ COLUMN-SPECIFIC FILTERS (dari filter_table)
                    for (int i = 0; i < 22; i++) // Jumlah kolom yang bisa difilter
                    {
                        string colValue = form[$"columns[{i}][search][value]"];
                        if (!string.IsNullOrEmpty(colValue))
                        {
                            switch (i)
                            {
                                case 0: // Status
                                    whereClause += " AND CASE WHEN ISNULL(status, 1) = 1 THEN 'Submission' WHEN status = 2 THEN 'Approved' WHEN status = 3 THEN 'Cancelled' ELSE 'Unknown' END LIKE @col0 ";
                                    parameters.Add(new SqlParameter("@col0", "%" + colValue + "%"));
                                    break;
                                case 1: // Material Type
                                    whereClause += " AND material_type LIKE @col1 ";
                                    parameters.Add(new SqlParameter("@col1", "%" + colValue + "%"));
                                    break;
                                case 2: // Part No
                                    whereClause += " AND partno LIKE @col2 ";
                                    parameters.Add(new SqlParameter("@col2", "%" + colValue + "%"));
                                    break;
                                case 3: // PO No
                                    whereClause += " AND po_no LIKE @col3 ";
                                    parameters.Add(new SqlParameter("@col3", "%" + colValue + "%"));
                                    break;
                                case 4: // Reserved Qty
                                    whereClause += " AND CAST(reserved_qty AS VARCHAR) LIKE @col4 ";
                                    parameters.Add(new SqlParameter("@col4", "%" + colValue + "%"));
                                    break;
                                case 5: // Available Qty
                                    whereClause += " AND CAST(available_qty AS VARCHAR) LIKE @col5 ";
                                    parameters.Add(new SqlParameter("@col5", "%" + colValue + "%"));
                                    break;
                                case 6: // UOM
                                    whereClause += " AND uom LIKE @col6 ";
                                    parameters.Add(new SqlParameter("@col6", "%" + colValue + "%"));
                                    break;
                                case 7: // Revision
                                    whereClause += " AND revision LIKE @col7 ";
                                    parameters.Add(new SqlParameter("@col7", "%" + colValue + "%"));
                                    break;
                                case 8: // Project Name
                                    whereClause += " AND project_name LIKE @col8 ";
                                    parameters.Add(new SqlParameter("@col8", "%" + colValue + "%"));
                                    break;
                                case 9: // Storage Requirement
                                    whereClause += " AND storage_requirement LIKE @col9 ";
                                    parameters.Add(new SqlParameter("@col9", "%" + colValue + "%"));
                                    break;
                                case 10: // Supplier Name
                                    whereClause += " AND supplier_name LIKE @col10 ";
                                    parameters.Add(new SqlParameter("@col10", "%" + colValue + "%"));
                                    break;
                                case 11: // PIC Name
                                    whereClause += " AND (SELECT name FROM mst_users WHERE sesa_id = material_reservation.pic) LIKE @col11 ";
                                    parameters.Add(new SqlParameter("@col11", "%" + colValue + "%"));
                                    break;
                                case 12: // Order Type
                                    whereClause += " AND order_type LIKE @col12 ";
                                    parameters.Add(new SqlParameter("@col12", "%" + colValue + "%"));
                                    break;
                                case 13: // Unit Price
                                    whereClause += " AND CAST(unit_price AS VARCHAR) LIKE @col13 ";
                                    parameters.Add(new SqlParameter("@col13", "%" + colValue + "%"));
                                    break;
                                case 14: // Priority
                                    whereClause += " AND priority_request LIKE @col14 ";
                                    parameters.Add(new SqlParameter("@col14", "%" + colValue + "%"));
                                    break;
                                case 15: // Reservation Date
                                    whereClause += " AND CONVERT(VARCHAR, reservation_date, 120) LIKE @col15 ";
                                    parameters.Add(new SqlParameter("@col15", "%" + colValue + "%"));
                                    break;
                                case 16: // Order Date
                                    whereClause += " AND CONVERT(VARCHAR, order_date, 23) LIKE @col16 ";
                                    parameters.Add(new SqlParameter("@col16", "%" + colValue + "%"));
                                    break;
                                case 18: // Length
                                    whereClause += " AND CAST(length_mm AS VARCHAR) LIKE @col18 ";
                                    parameters.Add(new SqlParameter("@col18", "%" + colValue + "%"));
                                    break;
                                case 19: // Width
                                    whereClause += " AND CAST(width_mm AS VARCHAR) LIKE @col19 ";
                                    parameters.Add(new SqlParameter("@col19", "%" + colValue + "%"));
                                    break;
                                case 20: // Height
                                    whereClause += " AND CAST(height_mm AS VARCHAR) LIKE @col20 ";
                                    parameters.Add(new SqlParameter("@col20", "%" + colValue + "%"));
                                    break;
                            }
                        }
                    }

                    // Global search (opsional, bisa dihapus jika hanya pakai column filter)
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        whereClause += " AND (partno LIKE @globalSearch OR po_no LIKE @globalSearch OR supplier_name LIKE @globalSearch) ";
                        parameters.Add(new SqlParameter("@globalSearch", "%" + searchValue + "%"));
                    }

                    string filteredCountQuery = "SELECT COUNT(*) FROM material_reservation " + whereClause;
                    SqlCommand filteredCountCmd = new SqlCommand(filteredCountQuery, conn);
                    filteredCountCmd.Parameters.AddRange(parameters.Select(p => (SqlParameter)((ICloneable)p).Clone()).ToArray());
                    int recordsFiltered = (int)filteredCountCmd.ExecuteScalar();

                    string orderBy = " ORDER BY reservation_date DESC ";

                    string query = $@"
SELECT 
    id_reservation, material_type, partno, po_no,
    ISNULL(reserved_qty, 0), ISNULL(available_qty, 0), uom, revision,
    project_name, storage_requirement, supplier_name, pic,
    ISNULL((SELECT name FROM mst_users WHERE sesa_id = material_reservation.pic), '') as pic_name,
    order_type, ISNULL(unit_price, 0), priority_request, reservation_date,
    order_date, file_support, ISNULL(length_mm, 0), ISNULL(width_mm, 0),
    ISNULL(height_mm, 0), remark, ISNULL(status, 1) as status,
    CASE 
        WHEN ISNULL(status, 1) = 1 THEN 'Submission'
        WHEN status = 2 THEN 'Approved'
        WHEN status = 3 THEN 'Cancelled'
        ELSE 'Unknown'
    END as status_desc,
    decline_reason
FROM material_reservation 
{whereClause} {orderBy}
OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddRange(parameters.ToArray());
                    cmd.Parameters.AddWithValue("@offset", start);
                    cmd.Parameters.AddWithValue("@pageSize", length);

                    SqlDataReader reader = cmd.ExecuteReader();
                    List<MaterialReservationModel> data = new List<MaterialReservationModel>();

                    while (reader.Read())
                    {
                        data.Add(new MaterialReservationModel
                        {
                            id_reservation = reader.GetInt32(0),
                            material_type = reader.IsDBNull(1) ? "" : reader.GetString(1),
                            partno = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            po_no = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            reserved_qty = reader.GetDecimal(4),
                            available_qty = reader.GetDecimal(5),
                            uom = reader.IsDBNull(6) ? "" : reader.GetString(6),
                            revision = reader.IsDBNull(7) ? "" : reader.GetString(7),
                            project_name = reader.IsDBNull(8) ? "" : reader.GetString(8),
                            storage_requirement = reader.IsDBNull(9) ? "" : reader.GetString(9),
                            supplier_name = reader.IsDBNull(10) ? "" : reader.GetString(10),
                            pic = reader.IsDBNull(11) ? "" : reader.GetString(11),
                            pic_name = reader.IsDBNull(12) ? "" : reader.GetString(12),
                            order_type = reader.IsDBNull(13) ? "" : reader.GetString(13),
                            unit_price = reader.GetDecimal(14),
                            priority_request = reader.IsDBNull(15) ? "" : reader.GetString(15),
                            reservation_date = reader.IsDBNull(16) ? DateTime.MinValue : reader.GetDateTime(16),
                            order_date = reader.IsDBNull(17) ? (DateTime?)null : reader.GetDateTime(17),
                            file_support = reader.IsDBNull(18) ? "" : reader.GetString(18),
                            length_mm = reader.GetDecimal(19),
                            width_mm = reader.GetDecimal(20),
                            height_mm = reader.GetDecimal(21),
                            remark = reader.IsDBNull(22) ? "" : reader.GetString(22),
                            status = reader.GetInt32(23),
                            status_desc = reader.IsDBNull(24) ? "" : reader.GetString(24),
                            decline_reason = reader.IsDBNull(25) ? "" : reader.GetString(25)
                        });
                    }
                    return new { draw, recordsTotal, recordsFiltered, data };
                }
            }
            catch (Exception ex)
            {
                return new { draw = 0, recordsTotal = 0, recordsFiltered = 0, data = new List<MaterialReservationModel>(), error = ex.Message };
            }
        }

        public DataTable GetUserSchedules(string sesaId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("dbo.get_user_schedules", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sesa_id", sesaId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }

            return dt;
        }


        public DataTable GetAllSchedules()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("dbo.get_user_schedules", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sesa_id", DBNull.Value);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            return dt;
        }
        public int CreateSchedule(string sesaId, string email, DateTime date, string title, string desc, bool sendEmail)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("dbo.create_schedule", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sesa_id", (object)sesaId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@user_email", (object)email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@scheduled_date", date);
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@description", (object)desc ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@send_email", sendEmail); // ← tambah ini
                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        public bool SendDiscussionEmailNotification(int id_order, string sender_sesa_id, string message)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SEND_EMAIL_DISCUSSION_NOTIFICATION", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 120;

                        cmd.Parameters.AddWithValue("@id_order", id_order);
                        cmd.Parameters.AddWithValue("@sender_sesa_id", sender_sesa_id);
                        cmd.Parameters.AddWithValue("@message", message);
                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending discussion email: {ex.Message}");
                return false;
            }
        }
        public List<string> GetStatus()
        {
            List<string> dataStatus = new List<string>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT status_desc FROM mst_status ORDER BY status_code", conn))
                {
                    using SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
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
                    using SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
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
                    cmd.Parameters.AddWithValue("@sesa_id", sesa_id);

                    string plant = (string)cmd.ExecuteScalar();

                    return plant;
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
                    return "success";
                }
            }
        }
        public List<string> GetPrinter(string loc = "")
        {
            string qry_printer = "SELECT printer_name FROM mst_printer WHERE loc='" + loc + "' ORDER BY printer_name";
            if (loc == "")
            {
                qry_printer = "SELECT printer_name FROM mst_printer ORDER BY printer_name";
            }
            List<string> listPrinter = new List<string>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(qry_printer, conn))
                {
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
             string storage_requirement, string supplier_name, string pic, string order_type, double unit_price, string priority_request, double length_mm, double width_mm, double height_mm,
             string remark, string gatepass, string file_support, string sesa_id)
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
                    cmd.Parameters.AddWithValue("@priority_request", priority_request ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@length_mm", length_mm);
                    cmd.Parameters.AddWithValue("@width_mm", width_mm);
                    cmd.Parameters.AddWithValue("@height_mm", height_mm);
                    cmd.Parameters.AddWithValue("@remark", remark ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@gatepass", gatepass ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@file_support", file_support);
                    cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                    cmd.ExecuteNonQuery();
                    return "success";
                }
            }
        }

        public int GetUnreadDiscussionCount(string sesa_id)
        {
            int count = 0;

            try
            {
                string query = @"
            -- Hitung JUMLAH DISCUSSION baru, bukan jumlah order
            SELECT COUNT(d.id)
            FROM tbl_discussion d
            INNER JOIN tbl_order o ON d.id_order = o.id_order
            WHERE d.sesa_id != @sesa_id  -- Bukan dari user sendiri
            AND d.created_date > ISNULL((
                -- Ambil tanggal terakhir user baca discussion
                SELECT MAX(last_read_date) 
                FROM tbl_discussion_read 
                WHERE sesa_id = @sesa_id
            ), '1900-01-01')
            AND (
                -- User harus punya akses
                o.pic = @sesa_id
                OR EXISTS (
                    SELECT 1 FROM mst_users_role ur
                    WHERE ur.sesa_id = @sesa_id
                    AND ur.id_role IN (2, 3, 99)
                )
                OR EXISTS (
                    SELECT 1 FROM tbl_discussion d2
                    WHERE d2.id_order = o.id_order 
                    AND d2.sesa_id = @sesa_id
                )
            )";

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                        conn.Open();

                        object result = cmd.ExecuteScalar();
                        count = result != null ? Convert.ToInt32(result) : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting unread discussion count: {ex.Message}");
            }

            return count;
        }


        public int GetOrderIdByUploadId(string id_upload)
        {
            int id = 0;
            try
            {
                System.Diagnostics.Debug.WriteLine($"[GetOrderIdByUploadId] START - id_upload: {id_upload}");

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    string q = "SELECT TOP 1 id_order FROM tbl_order WHERE id_upload = @id";
                    SqlCommand cmd = new SqlCommand(q, conn);
                    cmd.Parameters.AddWithValue("@id", id_upload);
                    conn.Open();

                    var res = cmd.ExecuteScalar();

                    if (res != null)
                    {
                        id = Convert.ToInt32(res);
                        System.Diagnostics.Debug.WriteLine($"[GetOrderIdByUploadId] Found id_order: {id}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[GetOrderIdByUploadId] NOT FOUND for id_upload: {id_upload}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetOrderIdByUploadId] ERROR: {ex.Message}");
            }

            return id;
        }

        public string UpdateOrder(string id_order, string id_upload, string material_type, string partno, string po_no, double qty, string uom, string revision, string project_name,
        string storage_requirement, string supplier_name, string order_type, double unit_price, string priority_request, double length_mm, double width_mm, double height_mm,
        string remark, string gatepass, string file_support, string sesa_id)
        {
            if (string.IsNullOrEmpty(file_support))
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
                    cmd.Parameters.AddWithValue("@priority_request", priority_request ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@length_mm", length_mm);
                    cmd.Parameters.AddWithValue("@width_mm", width_mm);
                    cmd.Parameters.AddWithValue("@height_mm", height_mm);
                    cmd.Parameters.AddWithValue("@remark", remark ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@gatepass", gatepass ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@file_support", file_support ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@sesa_id", sesa_id);

                    cmd.ExecuteNonQuery();

                    if (!string.IsNullOrEmpty(priority_request) && priority_request.Trim().Equals("High", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            int idOrderInt = int.Parse(id_order);
                            TriggerUrgentReceiverEmail(idOrderInt, sesa_id);

                            System.Diagnostics.Debug.WriteLine($"[UpdateOrder] Notifikasi High Priority terkirim untuk ID: {id_order}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[UpdateOrder] Gagal mengirim email: {ex.Message}");
                        }
                    }

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
                        return status ?? "NOK";
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

        public string ConfirmBinItem(string binId, string sesa_id, out List<int> orderIds)
        {
            orderIds = new List<int>();

            try
            {
                // ✅ STEP 1: Ambil id_order dari packagedetail SEBELUM SP dijalankan
                using (SqlConnection connBLP = new SqlConnection(ConnectionStringBLP))
                {
                    connBLP.Open();

                    string queryGetOrders = @"
                SELECT DISTINCT pd.erp_id_order
                FROM dbo.packagedetail pd
                WHERE pd.erp_id_order IS NOT NULL 
                AND pd.PKG_ID2 IN (
                    SELECT PartName 
                    FROM SEMB_ERP_QAS.dbo.tmp_bin_matrial 
                    WHERE DoneBy = @user
                )";

                    using (SqlCommand cmdGet = new SqlCommand(queryGetOrders, connBLP))
                    {
                        cmdGet.Parameters.AddWithValue("@user", sesa_id);

                        using (SqlDataReader reader = cmdGet.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (!reader.IsDBNull(0))
                                {
                                    orderIds.Add(reader.GetInt32(0));
                                }
                            }
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"✅ Found {orderIds.Count} orders before UPDATE_BIN_V1");
                    foreach (var id in orderIds)
                    {
                        System.Diagnostics.Debug.WriteLine($"   - id_order: {id}");
                    }
                }

                // ✅ STEP 2: Jalankan SP UPDATE_BIN_V1
                using (SqlConnection conn = new SqlConnection(ConnectionStringBLP))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("UPDATE_BIN_V1", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@input", binId);
                        cmd.Parameters.AddWithValue("@user", sesa_id);
                        cmd.ExecuteNonQuery();
                    }
                }

                return "OK";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ConfirmBinItem Error: {ex.Message}");
                return "NOK;" + ex.Message;
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

        public List<Tuple<int, int>> GetAffectedOrdersBeforeSubmit(string sesa_id)
        {
            List<Tuple<int, int>> orders = new List<Tuple<int, int>>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = @"
            SELECT DISTINCT 
                tr.id_order,
                tr.qty as picking_qty,
                ISNULL(vo.available_qty, 0) as available_qty,
                CASE 
                    WHEN (ISNULL(vo.available_qty, 0) - tr.qty) <= 0 THEN 5
                    ELSE 4
                END as new_status_code
            FROM temp_request tr
            INNER JOIN v_order vo ON tr.id_order = vo.id_order
            WHERE tr.added_by = @sesa_id
        ";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@sesa_id", sesa_id);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    int id_order = Convert.ToInt32(dr["id_order"]);
                    int new_status_code = Convert.ToInt32(dr["new_status_code"]);

                    Console.WriteLine($"[DAL] Order {id_order} → Status {new_status_code}");

                    orders.Add(new Tuple<int, int>(id_order, new_status_code));
                }

                dr.Close();
            }

            return orders;
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
        public string SubmitReqPicking(string remark, string sesa_id, string plant)
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
                        cmd.Parameters.AddWithValue("@plant", plant);
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
        public DataSet GetExportOrderList()
        {
            DataSet ds = new DataSet();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT status_desc, material_type, partno, po_no, qty, picked_qty, available_qty, uom, revision, project_name, storage_requirement, supplier_name, pic_name, order_type, unit_price, gatepass, record_date as order_date, length_mm, width_mm, height_mm " +
                    "from v_order WHERE status_desc = 'Putaway'";
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = conn;
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        sda.Fill(ds);
                    }
                }
            }

            return ds;

        }
        public DataSet GetExportRequestList()
        {
            DataSet ds = new DataSet();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT Request_No, Status_Desc, Requested_By, Partno, Qty, Picked_Qty, UoM, Material_Type, Status_Picking, Request_Date, Done_Picking_Date FROM v_request_download ORDER BY status_request";
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = conn;
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        sda.Fill(ds);
                    }
                }
            }

            return ds;

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
        public string UpdatePickingMaterialQty(int id_det, double value_data)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("UPDATE_PICKING_MATERIAL_QTY", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id_det", id_det);
                        cmd.Parameters.AddWithValue("@value_data", value_data);
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
        public string DeletePartnumber(int id_det, string sesa_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("DELETE_PARTNUMBER", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id_det", id_det);
                        //cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
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
                SqlCommand cmd = new SqlCommand("GET_DEPTS_LIST", conn);
                cmd.CommandType = CommandType.StoredProcedure;
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
                            lead_time = reader["lead_time"].ToString(),
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
        public List<TcodeModel> GetTcodeHistory(int id_request)
        {
            List<TcodeModel> dataList = new List<TcodeModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("GET_TCODE_HISTORY", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_request", id_request);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    dataList.Add(new TcodeModel
                    {
                        status_request = reader["status_request"].ToString(),
                        record_date = Convert.ToDateTime(reader["record_date"]),
                        name = reader["name"].ToString(),
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
        public List<BoxModel> GetTempBoxKitting(decimal total_qty, decimal spq)
        {
            List<BoxModel> boxs = new List<BoxModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("GEN_LIST_BOX_KITTING", conn);
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TotalQty", total_qty);
                    cmd.Parameters.AddWithValue("@QtyPerBox", spq);
                    using SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        boxs.Add(new BoxModel
                        {
                            BoxId = reader["box_no"].ToString(),
                            Qty = reader["qty"].ToString()
                        });
                    }
                }
            }
            return boxs;
        }
        public string CreateBoxKitting(string box_id, string qtys, string sesa_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("CREATE_BOX_KITTING", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@box_id", box_id);
                        cmd.Parameters.AddWithValue("@qtys", qtys);
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
        public DataSet GetExportStoragebinList()
        {
            DataSet ds = new DataSet();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT Status_Desc, Material_Type, Partno, PO_No, Storage_Bin, Box_ID, Qty, Picked_Qty, Available_Qty, UOM, Revision, Project_Name, Storage_Requirement, Supplier_Name, PIC_Name, Order_Type, Unit_Price, Gatepass, Record_Date, Length_mm, Width_mm, Height_mm FROM v_order_sbin";
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = conn;
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        sda.Fill(ds);
                    }
                }
            }

            return ds;

        }

        public DataSet GetExportRequestDetail()
        {
            DataSet ds = new DataSet();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                string query = "SELECT Request_No, Partno, Qty, Picked_Qty, UOM, Status_Picking FROM v_request_detail";
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = conn;
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        sda.Fill(ds);
                    }
                }
            }

            return ds;

        }
        public List<ChartModel> GetAgingMovementChart()
        {
            List<ChartModel> result = new List<ChartModel>();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("GET_AGING_MOVEMENT_CHART", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(new ChartModel()
                    {
                        label = reader["Label"].ToString(),
                        value1 = reader.IsDBNull("<= 3 Months") ? 0 : Convert.ToDouble(reader["<= 3 Months"]),
                        value2 = reader.IsDBNull("<= 6 Months") ? 0 : Convert.ToDouble(reader["<= 6 Months"]),
                        value3 = reader.IsDBNull("<= 12 Months") ? 0 : Convert.ToDouble(reader["<= 12 Months"]),
                        value4 = reader.IsDBNull("> 1 Year") ? 0 : Convert.ToDouble(reader["> 1 Year"])
                    });
                }

                conn.Close();
            }

            return result;
        }
        public List<ShipmentTempModel> GetTempShipment(string id_upload, string sesa_id)
        {
            List<ShipmentTempModel> dataList = new List<ShipmentTempModel>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("GET_TEMP_SHIPMENT", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_upload", id_upload);
                    cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                    using SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ShipmentTempModel row = new ShipmentTempModel();
                            row.project_name = reader["project_name"].ToString();
                            row.project_name_msg = reader["project_name_msg"].ToString();
                            row.stage_name = reader["stage_name"].ToString();
                            row.stage_name_msg = reader["stage_name_msg"].ToString();
                            row.wo_no = reader["wo_no"].ToString();
                            row.wo_no_msg = reader["wo_no_msg"].ToString();
                            row.partno = reader["partno"].ToString();
                            row.partno_msg = reader["partno_msg"].ToString();
                            row.revision = reader["revision"].ToString();
                            row.revision_msg = reader["revision_msg"].ToString();
                            row.qty = reader["qty"].ToString();
                            row.qty_msg = reader["qty_msg"].ToString();
                            row.is_coated = reader["is_coated"].ToString();
                            row.is_coated_msg = reader["is_coated_msg"].ToString();
                            row.ship_date = reader["ship_date"].ToString();
                            row.ship_date_msg = reader["ship_date_msg"].ToString();
                            row.is_error = Convert.ToInt32(reader["is_error"]);
                            dataList.Add(row);
                        }
                    }
                }

                conn.Close();
            }
            return dataList;
        }
        public string SubmitUploadShipment(string id_upload, string sesa_id)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SUBMIT_UPLOAD_SHIPMENT", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_upload", id_upload);
                    cmd.Parameters.AddWithValue("@sesa_id", sesa_id);
                    cmd.ExecuteNonQuery();
                    //cmd.ExecuteScalar();
                    return "success";
                }
            }
        }

        public string ReceiveShipment(string id_shipment, string sesa_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("RECEIVE_SHIPMENT", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id_shipment", id_shipment);
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);

                        SqlParameter returnValue = new SqlParameter();
                        returnValue.Direction = ParameterDirection.ReturnValue;
                        cmd.Parameters.Add(returnValue);

                        cmd.ExecuteNonQuery();

                        int result = (int)returnValue.Value;
                        if (result == 0)
                        {
                            return "success;Shipment(s) received successfully!";
                        }
                        else
                        {
                            return "error;Failed to receive shipment(s). Please try again.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"error;Failed to receive shipment(s): {ex.Message}";
            }
        }

        public string BinningShipment(string id_shipment, string storage_dest, string gatepass_no, string sesa_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("BINNING_SHIPMENT", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id_shipment", id_shipment);
                        cmd.Parameters.AddWithValue("@storage_dest", storage_dest);
                        cmd.Parameters.AddWithValue("@gatepass_no", gatepass_no ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);

                        SqlParameter returnValue = new SqlParameter();
                        returnValue.Direction = ParameterDirection.ReturnValue;
                        cmd.Parameters.Add(returnValue);

                        cmd.ExecuteNonQuery();

                        int result = (int)returnValue.Value;
                        if (result == 0)
                        {
                            return $"success;Shipment(s) assigned to {storage_dest} successfully!";
                        }
                        else
                        {
                            return "error;Failed to assign storage destination. Please try again.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"error;Failed to assign storage destination: {ex.Message}";
            }
        }

        public string InsertShipmentManual(string id_upload, string project_name, string stage_name, string wo_no, string partno,
            string revision, decimal qty, string is_coated, DateTime ship_date, string sesa_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("INSERT_SHIPMENT_MANUAL", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id_upload", id_upload);
                        cmd.Parameters.AddWithValue("@project_name", project_name);
                        cmd.Parameters.AddWithValue("@stage_name", stage_name);
                        cmd.Parameters.AddWithValue("@wo_no", wo_no);
                        cmd.Parameters.AddWithValue("@partno", partno);
                        cmd.Parameters.AddWithValue("@revision", revision);
                        cmd.Parameters.AddWithValue("@qty", qty);
                        cmd.Parameters.AddWithValue("@is_coated", is_coated);
                        cmd.Parameters.AddWithValue("@ship_date", ship_date);
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);

                        SqlParameter returnValue = new SqlParameter
                        {
                            Direction = ParameterDirection.ReturnValue
                        };
                        cmd.Parameters.Add(returnValue);

                        cmd.ExecuteNonQuery();

                        int result = (int)returnValue.Value;
                        if (result == 0)
                        {
                            return "success;Shipment added successfully!";
                        }
                        else
                        {
                            return "error;Failed to add shipment. Please try again.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"error;Failed to add shipment: {ex.Message}";
            }
        }

        public List<(int id, string priority)> GetAllOrderIdsByUploadId(string id_upload)
        {
            var result = new List<(int id, string priority)>();
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    string q = "SELECT id_order, priority_request FROM tbl_order WHERE id_upload = @id";
                    SqlCommand cmd = new SqlCommand(q, conn);
                    cmd.Parameters.AddWithValue("@id", id_upload);
                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        int id = Convert.ToInt32(dr["id_order"]);
                        string priority = dr["priority_request"] != DBNull.Value ? dr["priority_request"].ToString() : "";
                        System.Diagnostics.Debug.WriteLine($"[GetAllOrderIdsByUploadId] id={id}, priority='{priority}'");
                        result.Add((id, priority));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetAllOrderIdsByUploadId] ERROR: {ex.Message}");
            }
            return result;
        }

        public bool TriggerStatusUpdateEmail(int id_order, string receiver_id, int new_status_code)
        {
            try
            {
                Console.WriteLine($"      [DAL] ========================================");
                Console.WriteLine($"      [DAL] EMAIL TRIGGER START");
                Console.WriteLine($"      [DAL] ========================================");
                Console.WriteLine($"      [DAL] id_order: {id_order}");
                Console.WriteLine($"      [DAL] receiver_id: {receiver_id}");
                Console.WriteLine($"      [DAL] new_status_code: {new_status_code}");

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    Console.WriteLine($"      [DAL] Creating SP command...");
                    SqlCommand cmd = new SqlCommand("SEND_EMAIL_STATUS_UPDATE_NOTIFICATION", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    Console.WriteLine($"      [DAL] Adding parameters...");
                    cmd.Parameters.AddWithValue("@id_order", id_order);
                    cmd.Parameters.AddWithValue("@receiver_sesa_id", receiver_id);
                    cmd.Parameters.AddWithValue("@new_status_code", new_status_code);

                    Console.WriteLine($"      [DAL] Opening connection...");
                    conn.Open();
                    Console.WriteLine($"      [DAL] ✅ Connection opened! State: {conn.State}");

                    Console.WriteLine($"      [DAL] Executing SP: SEND_EMAIL_STATUS_UPDATE_NOTIFICATION");
                    cmd.ExecuteNonQuery();
                    Console.WriteLine($"      [DAL] ✅✅✅ SP EXECUTED SUCCESSFULLY!");

                    conn.Close();
                    Console.WriteLine($"      [DAL] Connection closed.");
                }

                Console.WriteLine($"      [DAL] ========================================");
                Console.WriteLine($"      [DAL] EMAIL TRIGGER END (SUCCESS)");
                Console.WriteLine($"      [DAL] ========================================");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"      [DAL] ❌❌❌❌❌❌❌❌❌❌❌❌❌❌❌❌❌");
                Console.WriteLine($"      [DAL] EMAIL TRIGGER ERROR!");
                Console.WriteLine($"      [DAL] ❌❌❌❌❌❌❌❌❌❌❌❌❌❌❌❌❌");
                Console.WriteLine($"      [DAL] Error Message: {ex.Message}");
                Console.WriteLine($"      [DAL] Error Source: {ex.Source}");
                Console.WriteLine($"      [DAL] Stack Trace:");
                Console.WriteLine($"      {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"      [DAL] Inner Exception: {ex.InnerException.Message}");
                }

                return false;
            }
        }

        public void TriggerUrgentReceiverEmail(int idOrder, string sesaId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[TriggerUrgentReceiverEmail] START - idOrder: {idOrder}, sesaId: {sesaId}");

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    System.Diagnostics.Debug.WriteLine("[TriggerUrgentReceiverEmail] Connection opened");

                    // Enable InfoMessage untuk tangkap PRINT dari SQL
                    conn.InfoMessage += (sender, e) =>
                    {
                        System.Diagnostics.Debug.WriteLine($"[SQL PRINT] {e.Message}");
                    };

                    using (SqlCommand cmd = new SqlCommand("SEND_EMAIL_URGENT_REQUEST_NOTIFICATION", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id_order", idOrder);
                        cmd.Parameters.AddWithValue("@sender_sesa_id", sesaId);

                        System.Diagnostics.Debug.WriteLine("[TriggerUrgentReceiverEmail] Executing SP...");
                        cmd.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine("[TriggerUrgentReceiverEmail] SP executed successfully");
                    }
                }

                System.Diagnostics.Debug.WriteLine("[TriggerUrgentReceiverEmail] COMPLETED");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TriggerUrgentReceiverEmail] ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[TriggerUrgentReceiverEmail] Stack: {ex.StackTrace}");
                throw;
            }
        }
        public string DeleteShipment(string id_shipment, string sesa_id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("DELETE_SHIPMENT", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id_shipment", id_shipment);
                        cmd.Parameters.AddWithValue("@sesa_id", sesa_id);

                        SqlParameter returnValue = new SqlParameter
                        {
                            Direction = ParameterDirection.ReturnValue
                        };
                        cmd.Parameters.Add(returnValue);

                        cmd.ExecuteNonQuery();

                        int result = (int)returnValue.Value;
                        if (result == 0)
                        {
                            // Count how many were deleted
                            string[] ids = id_shipment.Split(';');
                            return $"success;Successfully deleted {ids.Length} shipment(s)!";
                        }
                        else if (result == 1)
                        {
                            return "error;Cannot delete shipment that has been received. Please contact administrator.";
                        }
                        else
                        {
                            return "error;Failed to delete shipment. Please try again.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"error;Failed to delete shipment: {ex.Message}";
            }
        }
    }
}
