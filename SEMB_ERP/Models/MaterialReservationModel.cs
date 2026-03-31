using System.ComponentModel.DataAnnotations;
namespace SEMB_ERP.Models
{
    public class MaterialReservationModel
    {
        [Key]
        public int? id_reservation { get; set; }
        public string? material_type { get; set; }
        public string? partno { get; set; }
        public string? po_no { get; set; }
        public decimal? reserved_qty { get; set; }
        public decimal? available_qty { get; set; }
        public string? uom { get; set; }
        public string? revision { get; set; }
        public string? project_name { get; set; }
        public string? storage_requirement { get; set; }
        public string? supplier_name { get; set; }
        public string? pic { get; set; }
        public string? pic_name { get; set; }
        public string? order_type { get; set; }
        public decimal? unit_price { get; set; }
        public string? priority_request { get; set; }
        public DateTime? reservation_date { get; set; }
        public string? file_support { get; set; }
        public decimal? length_mm { get; set; }
        public decimal? width_mm { get; set; }
        public decimal? height_mm { get; set; }
        public string? remark { get; set; }
        public int? status { get; set; }
        public string? status_desc { get; set; }

        public DateTime? order_date { get; set; }
        public string decline_reason { get; set; }

    }
}
