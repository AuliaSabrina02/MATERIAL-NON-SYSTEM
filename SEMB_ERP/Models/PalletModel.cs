using System.ComponentModel.DataAnnotations;
namespace SEMB_ERP.Models
{
    public class PalletModel
    {
        [Key]
        public int? id_pallet { get; set; }
        public string? pallet_no { get; set; }
        public int? id_request { get; set; }
        public string? request_no { get; set; }
        public string? status_pallet { get; set; }
        public DateTime? record_date { get; set; }
        public string? sesa_id { get; set; }
        public string? name { get; set; }
        public DateTime? receive_date { get; set; }
        public string? received_by { get; set; }
        public DateTime? supply_date { get; set; }
        public string? supplied_by { get; set; }
        public string? supply_comment { get; set; }
        public string? received_by_name { get; set; }
        public string? supplied_by_name { get; set; }
        public string? plant { get; set; }
    }
}
