using System.ComponentModel.DataAnnotations;
namespace SEMB_ERP.Models
{
    public class VarianceModel
    {
        [Key]
        public int? id_var { get; set; }
        public string? request_no { get; set; }
        public string? partno { get; set; }
        public string? sbin { get; set; }
        public decimal? qty { get; set; }
        public string? sesa_id { get; set; }
        public string? name { get; set; }
        public DateTime? record_date { get; set; }
    }
}
