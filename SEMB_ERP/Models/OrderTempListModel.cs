using System.ComponentModel.DataAnnotations;
namespace SEMB_ERP.Models
{
    public class OrderTempListModel
    {
        public string? id_upload { get; set; }
        public string? material_type { get; set; }
        public string? material_type_msg { get; set; }
        public string? partno { get; set; }
        public string? partno_msg { get; set; }
        public string? po_no { get; set; }
        public string? po_no_msg { get; set; }
        public string? qty { get; set; }
        public string? qty_msg { get; set; }
        public string? uom { get; set; }
        public string? uom_msg { get; set; }
        public string? revision { get; set; }
        public string? revision_msg { get; set; }
        public string? project_name { get; set; }
        public string? project_name_msg { get; set; }
        public string? storage_requirement { get; set; }
        public string? storage_requirement_msg { get; set; }
        public string? sbin { get; set; }
        public string? supplier_name { get; set; }
        public string? supplier_name_msg { get; set; }
        public string? pic { get; set; }
        public string? order_type { get; set; }
        public string? order_type_msg { get; set; }
        public string? length_mm { get; set; }
        public string? length_mm_msg { get; set; }
        public string? width_mm { get; set; }
        public string? width_mm_msg { get; set; }
        public string? height_mm { get; set; }
        public string? height_mm_msg { get; set; }
        public string? unit_price { get; set; }
        public string? unit_price_msg { get; set; }
        public string? priority_request { get; set; }
        public string? priority_request_msg { get; set; }
        public string? remark { get; set; }
        public string? gatepass { get; set; }
        public int? is_error { get; set; }
    }
}
