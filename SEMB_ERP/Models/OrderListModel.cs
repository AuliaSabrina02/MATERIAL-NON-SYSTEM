using System.ComponentModel.DataAnnotations;
namespace SEMB_ERP.Models
{
    public class OrderListModel
    {
        [Key]
        public int? id_order { get; set; }
        public string? id_upload { get; set; }
        public string? material_type { get; set; }
        public string? partno { get; set; }
        public string? po_no { get; set; }
        public double? qty { get; set; }
        public string? uom { get; set; }
        public string? revision { get; set; }
        public string? project_name { get; set; }
        public string? storage_requirement { get; set; }
        public string? sbin { get; set; }
        public string? supplier_name { get; set; }
        public string? pic { get; set; }
        public string? order_type { get; set; }
        public double? length_mm { get; set; }
        public double? width_mm { get; set; }
        public double? height_mm { get; set; }
        public double? unit_price { get; set; }
        public string? file_support { get; set; }
        public string? status_code { get; set; }
        public string status_desc { get; set; }
        public double? spq { get; set; }
        public int? id_gr { get; set; }
        public string gr_no { get; set; }
        public int? total_box { get; set; }
    }
}
