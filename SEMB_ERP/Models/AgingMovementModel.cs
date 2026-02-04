using System.ComponentModel.DataAnnotations;
namespace SEMB_ERP.Models
{
    public class AgingMovementModel
    {
        [Key]
        public int? id_order { get; set; }
        public string? material_type { get; set; }
        public string? partno { get; set; }
        public string? po_no { get; set; }
        public string? project_name { get; set; }
        public string? storage_requirement { get; set; }
        public string? supplier_name { get; set; }
        public string? pic_name { get; set; }
        public string? order_type { get; set; }
        public string status_desc { get; set; }
        public DateTime? last_updated { get; set; }
        public int? aging { get; set; }

    }
}
