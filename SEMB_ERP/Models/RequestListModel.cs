using System.ComponentModel.DataAnnotations;
namespace SEMB_ERP.Models
{
    public class RequestListModel
    {
        [Key]
        public int? id_request { get; set; }
        public int? id_det { get; set; }
        public int? id_order { get; set; }
        public string? request_no { get; set; }
        public string? status_code { get; set; }
        public string? status_desc { get; set; }
        public string? remark { get; set; }
        public DateTime? record_date { get; set; }
        public string? requested_by { get; set; }
        public string? requested_by_name { get; set; }
        public string? partno { get; set; }
        public decimal? qty { get; set; }
        public string? uom { get; set; }
        public string? material_type { get; set; }
        public decimal? picked_qty { get; set; }
        public string? status_pick { get; set; }
        public string? department { get; set; }
        public string? status_request { get; set; }
        public string? lead_time { get; set; }

    }
}
