using System.ComponentModel.DataAnnotations;
namespace SEMB_ERP.Models
{
    public class PickBoxModel
    {
        public int? id_pick { get; set; }
        public int? id_request { get; set; }
        public int? id_det { get; set; }
        public string? box_id { get; set; }
        public string? partno { get; set; }
        public decimal? qty { get; set; }
        public string? sbin { get; set; }
        public string? picked_by { get; set; }
        public string? picked_by_name { get; set; }
        public string? pallet_no { get; set; }
        public DateTime? record_date { get; set; }
    }
}
